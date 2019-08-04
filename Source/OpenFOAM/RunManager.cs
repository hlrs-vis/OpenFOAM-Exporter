using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Windows.Forms;

namespace BIM.OpenFOAMExport.OpenFOAM
{
    /// <summary>
    /// Abstract base-class runmanager contains functions which have to be implemented for each OpenFOAM-Run-Environment itselfs.
    /// </summary>
    public abstract class RunManager
    {
        /// <summary>
        /// Path to openFoam-Case
        /// </summary>
        protected string m_CasePath;

        /// <summary>
        /// Path to install folder of the Environment
        /// </summary>
        protected string m_FOAMEnvPath;        
        
        /// <summary>
        /// Path to command.bat.
        /// </summary>
        protected string m_CommandBat;

        /// <summary>
        /// Environment selection.
        /// </summary>
        private readonly OpenFOAMEnvironment m_Env;

        /// <summary>
        /// DecomposPar-Dict for NumberOfSubdomains.
        /// </summary>
        private DecomposeParDict m_DecomposeParDict;

        /// <summary>
        /// Getter-Setter DecomposePar.
        /// </summary>
        public DecomposeParDict DecomposeParDict { set => m_DecomposeParDict = value; get => m_DecomposeParDict; }

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="casePath">Path to case.</param>
        public RunManager(string casePath, OpenFOAMEnvironment env)
        {
            m_Env = env;
            m_CasePath = casePath;
            //m_CommandBat = casePath + @"\Run.bat";
            CreateEnvConfig();
        }

        /// <summary>
        /// Interface for initial shell commands for starting openFOAM environment.
        /// </summary>
        /// <returns>List with batch commands.</returns>
        public abstract List<string> InitialEnvRunCommands();

        /// <summary>
        /// Interface for writing command into given streamwriter.
        /// </summary>
        /// <param name="sw">Streamwriter.</param>
        /// <param name="command">Command.</param>
        public abstract void WriteLine(StreamWriter sw, string command);

        /// <summary>
        /// Create bat and write given commands into batch file.
        /// </summary>
        public virtual bool WriteToCommandBat(List<string> command)
        {
            bool succeed = true;
            //create bat
            try
            {
                FileAttributes fileAttribute = FileAttributes.Normal;
                if (File.Exists(m_CommandBat))
                {
                    fileAttribute = File.GetAttributes(m_CommandBat);
                    FileAttributes tempAtt = fileAttribute & FileAttributes.ReadOnly;
                    if (FileAttributes.ReadOnly == tempAtt)
                    {
                        MessageBox.Show(OpenFOAMExportResource.ERR_FILE_READONLY, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
                              MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                    File.Delete(m_CommandBat);
                }

                using (StreamWriter commandBat = new StreamWriter(m_CommandBat))
                {
                    fileAttribute = File.GetAttributes(m_CommandBat) | fileAttribute;
                    File.SetAttributes(m_CommandBat, fileAttribute);
                    foreach (string com in command)
                    {
                        //diversify between environment
                        WriteLine(commandBat, com);
                    }
                }
            }
            catch (SecurityException)
            {
                MessageBox.Show(OpenFOAMExportResource.ERR_SECURITY_EXCEPTION, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                succeed = false;
            }
            catch (IOException)
            {
                MessageBox.Show(OpenFOAMExportResource.ERR_IO_EXCEPTION, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                succeed = false;
            }
            catch (Exception)
            {
                MessageBox.Show(OpenFOAMExportResource.ERR_EXCEPTION, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                succeed = false;
            }
            return succeed;
        }

        /// <summary>
        /// Running given commands in OpenFOAM-Environment.
        /// </summary>
        /// <param name="commands">Contains commands as string.</param>
        /// <returns>True if suceed and false if not.</returns>
        public virtual bool RunCommands(List<string> commands)
        {
            //create initial Environment commands.
            List<string> runCommands = new List<string>();
            List<string> envCommands = InitialEnvRunCommands();
            foreach (string s in envCommands)
            {
                runCommands.Add(s);
            }
            string log = " | tee " + @"log/";

            foreach (string command in commands)
            {
                if (DecomposeParDict != null)
                {
                    if (command.Equals("snappyHexMesh"))
                    {
                        runCommands.Add("decomposePar" + log + "decomposepar.log");
                        runCommands.Add("mpirun -np " + DecomposeParDict.NumberOfSubdomains + " " + command + " -overwrite -parallel " + log + command + ".log");
                        runCommands.Add("reconstructParMesh -constant" + log + "reconstructParMesh.log");
                        continue;
                    }
                    else if (command.Equals("simpleFoam"))
                    {
                        runCommands.Add("decomposePar");
                        runCommands.Add("mpirun -n " + DecomposeParDict.NumberOfSubdomains + " renumberMesh -overwrite -parallel");
                        runCommands.Add("mpirun -np " + DecomposeParDict.NumberOfSubdomains + " " + command + " -parallel " + log + command + ".log");
                        runCommands.Add("reconstructPar -latestTime");
                        continue;
                    }
                }

                //commands that have no log function.
                if (command.Equals("\"") || command.Contains("rm -r") 
                    || command.Contains("call") || /*USEFUL FOR DEBUGGING BATCH*/command.Contains("pause")
                    || command.Contains("scp") || command.Contains("ssh"))
                {
                    runCommands.Add(command);
                    continue;
                }
                runCommands.Add(command + log + command + ".log");
            }

            //write commands into batch file
            bool succeed = WriteToCommandBat(runCommands);

            if (succeed)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = m_CommandBat,
                    WorkingDirectory = m_CasePath
                };
                
                //start batch
                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    if(process.ExitCode != 0)
                    {
                        MessageBox.Show("Simulation isn't running properly. Please check the simulation parameter or openfoam environment." +
                            "\nC#-Process ExitCode: " + process.ExitCode,
                            OpenFOAMExportResource.MESSAGE_BOX_TITLE); ;
                    }
                }
            }
            else
            {
                return false;
            }
            return succeed;
        }

        /// <summary>
        /// Initialize config.
        /// </summary>
        public virtual void CreateEnvConfig()
        {
            string defaultEnvPath = "None";
            string envTag = "<" + m_Env + ">"; ;
            string assemblyDir = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8);
            string assemblyDirCorrect = assemblyDir.Remove(assemblyDir.IndexOf("OpenFOAMExport.dll"), 18).Replace("/", "\\");
            string configPath = assemblyDirCorrect + "openfoam_env_config.config";
            switch (m_Env)
            {
                case OpenFOAMEnvironment.blueCFD:
                    {
                        defaultEnvPath = @"C:\Program Files\blueCFD-Core-2017\setvars.bat";
                        break;
                    }
                case OpenFOAMEnvironment.wsl:
                    {
                        defaultEnvPath = @"C:\Windows\System32\bash.exe";
                        break;
                    }
                //case OpenFOAMEnvironment.docker:
                //    {
                //        //implement docker runmanger.
                //        break;
                //    }
                case OpenFOAMEnvironment.ssh:
                    {
                        defaultEnvPath = @"C:\Windows\System32\OpenSSH\ssh.exe";
                        break;
                    }
            }

            if (!File.Exists(configPath))
            {
                StreamWriter sw = new StreamWriter(configPath);
                sw.WriteLine("**********************Config for OpenFOAM-Environment**********************");

                if (File.Exists(defaultEnvPath))
                {
                    sw.WriteLine(envTag + " " + defaultEnvPath);
                    m_FOAMEnvPath = defaultEnvPath;
                }
                else
                {
                    ///TO-DO: NEW GUI FOR OPENFOAM-ENVIRONMENT USER INPUT
                    //StartOpenFOAMTextBoxForm();
                    m_FOAMEnvPath = string.Empty;
                }
                sw.Close();
            }
            else
            {
                using (var sr = File.OpenText(configPath))
                {
                    var configDone = false;
                    string s;
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (s.Contains(envTag))
                        {
                            configDone = true;
                            if (!s.Contains(defaultEnvPath))
                            {
                                m_FOAMEnvPath = s.Substring(s.IndexOf(" "));
                            }
                            else
                            {
                                m_FOAMEnvPath = defaultEnvPath;
                            }
                            break;
                        }
                    }
                    sr.Close();
                    if (configDone)
                    {
                        return;
                    }
                    using (var sw = File.AppendText(configPath))
                    {
                        if (File.Exists(defaultEnvPath))
                        {
                            sw.WriteLine(envTag + " " + defaultEnvPath);
                        }
                        else
                        {
                            ///TO-DO: NEW GUI FOR OPENFOAM-ENVIRONMENT USER INPUT
                            m_FOAMEnvPath = string.Empty;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Runmanager that is used for running OpenFOAM with BlueCFD.
    /// </summary>
    public class RunManagerBlueCFD : RunManager
    {
        /// <summary>
        /// Constructor creates RunManagerBlueCFD-Object.
        /// </summary>
        /// <param name="casePath">Path to case.</param>
        /// <param name="foamPath">Path to DOS_Mode.bat file.</param>
        public RunManagerBlueCFD(string casePath, OpenFOAMEnvironment env)
            : base(casePath, env)
        {
            m_CommandBat = casePath + @"\RunBlueCFD.bat";
        }

        /// <summary>
        /// Initial shell commands for running the blueCFD environment.
        /// </summary>
        /// <returns>List with shell commands as string.</returns>
        public override List<string> InitialEnvRunCommands()
        {
            //if casepath is on another drive than openfoam environment => additional tag /d
            if(m_CasePath.ToCharArray()[0] != m_FOAMEnvPath.ToCharArray()[0])
            {
                m_CasePath = "/d " + m_CasePath;
            }
            List<string> shellCommands = new List<string>
            {
                "call " + "\"" + m_FOAMEnvPath + "\"",
                "set " + @"PATH=%HOME%\msys64\usr\bin;%PATH%",
                "cd " + m_CasePath
            };
            return shellCommands;
        }

        /// <summary>
        /// The method writes command as a line into the streamWriter-object.
        /// </summary>
        /// <param name="sw">StreamWriter object.</param>
        /// <param name="command">Command as string.</param>
        public override void WriteLine(StreamWriter sw, string command)
        {
            sw.WriteLine(command);
        }
    }

    /// <summary>
    /// Runmanager that is used for running OpenFOAM in windows subsystem for linux.
    /// </summary>
    public class RunManagerWSL : RunManager
    {
        /// <summary>
        /// Constructor needs the casePath of the openFoam-case and environment.
        /// </summary>
        /// <param name="casePath">Path to openFfoam-case.</param>
        /// <param name="env">Enum that specifies the environment.</param>
        public RunManagerWSL(string casePath, OpenFOAMEnvironment env)
            : base(casePath, env)
        {
            //windows subsystem for linux
            m_CommandBat = casePath + @"\RunWSL.bat";
        }

        /// <summary>
        /// Initial shell commands for running the windows subsystem for linux environment.
        /// </summary>
        /// <returns>List with shell commands as string.</returns>
        public override List<string> InitialEnvRunCommands()
        {
            List<string> shellCommands = new List<string>
            {
                "bash -c -i \""
            };
            return shellCommands;
        }

        /// <summary>
        /// Adjust commands for running in <see cref="T:RunManager:RunCommands"/> method.
        /// </summary>
        /// <param name="commands">Contains commands as string.</param>
        /// <returns>True if suceed and false if not.</returns>
        public override bool RunCommands(List<string> commands)
        {
            commands.Add("\"");
            bool succeed = base.RunCommands(commands);
            return succeed;
        }

        /// <summary>
        /// The method writes command in one line into the streamWriter-object.
        /// </summary>
        /// <param name="sw">StreamWriter object.</param>
        /// <param name="command">Command as string.</param>
        public override void WriteLine(StreamWriter sw, string command)
        {
            if(command.Equals("\"") || command.Contains("bash") || command.Contains("blockMesh"))
            {            
                sw.Write(command);
                return;
            }
            sw.Write(" && "+ command);
        }
    }

    /// <summary>
    /// Runmanager that is used for running OpenFOAM in Docker.
    /// </summary>
    public class RunManagerDocker : RunManager
    {
        public RunManagerDocker(string casePath, OpenFOAMEnvironment env)
            : base(casePath, env)
        {

        }

        public override List<string> InitialEnvRunCommands()
        {
            throw new NotImplementedException();
        }


        public override void WriteLine(StreamWriter sw, string command)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// RunManager that is used for running OpenFOAM via SSH.
    /// </summary>
    public class RunManagerSSH : RunManager
    {
        /// <summary>
        /// Settings-object contains ssh details.
        /// </summary>
        readonly Settings m_Settings;

        /// <summary>
        /// Constructor needs the casePath of the openFoam-case and environment.
        /// </summary>
        /// <param name="casePath">Path to openFfoam-case.</param>
        /// <param name="env">Enum that specifies the environment.</param>
        public RunManagerSSH(string casePath, OpenFOAMEnvironment env, Settings settings)
            :base(casePath, env)
        {
            m_CommandBat = casePath + @"\RunSSH.bat";
            m_Settings = settings;
        }

        /// <summary>
        /// Initial shell commands for running OpenFOAM via ssh remote server.
        /// </summary>
        /// <returns>List with shell commands as string.</returns>
        public override List<string> InitialEnvRunCommands()
        {
            //-t for print out all outputs from remote server to client.
            List<string> shellCommands = new List<string>
            {
                "scp -P "+ m_Settings.SSH.Port + " -r " + m_CasePath + " " + m_Settings.SSH.ConnectionString() + ":" + m_Settings.SSH.ServerCaseFolder,
                "ssh -p " + m_Settings.SSH.Port + " -t " + m_Settings.SSH.ConnectionString(),
                " \"" + m_Settings.SSH.OfAlias,
                "cd " + m_Settings.SSH.ServerCaseFolder
            };
            return shellCommands;
        }

        /// <summary>
        /// Adjust commands for running in <see cref="T:RunManager:RunCommands"/> method.
        /// </summary>
        /// <param name="commands">Contains commands as string.</param>
        /// <returns>True if suceed and false if not.</returns>
        public override bool RunCommands(List<string> commands)
        {
            commands.Add("\"");

            //Download directory from Server: scp -r user@ssh.example.com:/path/to/remote/source /path/to/local/destination
            if (m_Settings.SSH.Download)
            {
                commands.Add("scp -P " + m_Settings.SSH.Port + " -r " + m_Settings.SSH.ConnectionString()+ ":" + m_Settings.SSH.ServerCaseFolder + "/. " + m_CasePath);
            }
            if(m_Settings.SSH.Delete)
            {
                commands.Add("ssh -p " + m_Settings.SSH.Port + " -t " + m_Settings.SSH.ConnectionString() +  " \"rm -rf " + m_Settings.SSH.ServerCaseFolder);
            }
            bool succeed = base.RunCommands(commands);
            return succeed;
        }

        /// <summary>
        /// The method writes the command into the streamWriter-object.
        /// </summary>
        /// <param name="sw">StreamWriter object.</param>
        /// <param name="command">Command as string.</param>
        public override void WriteLine(StreamWriter sw, string command)
        {
            //add \" as command to show the end of the commands for one bash operation
            if(command.Contains("scp")||command.Equals("\""))
            {
                sw.WriteLine(command);
                return;
            }
            if (command.Contains("ssh") || command.Contains("source"))
            {
                sw.Write(command);
                return;
            }
            sw.Write(" && " + command);
        }
    }
}
