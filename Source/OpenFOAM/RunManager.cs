using BIM.OpenFoamExport.OpenFOAMUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Windows.Forms;

namespace BIM.OpenFoamExport.OpenFOAM
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
        private OpenFOAMEnvironment m_Env;

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
                        MessageBox.Show(OpenFoamExportResource.ERR_FILE_READONLY, OpenFoamExportResource.MESSAGE_BOX_TITLE,
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
                MessageBox.Show(OpenFoamExportResource.ERR_SECURITY_EXCEPTION, OpenFoamExportResource.MESSAGE_BOX_TITLE,
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                succeed = false;
            }
            catch (IOException)
            {
                MessageBox.Show(OpenFoamExportResource.ERR_IO_EXCEPTION, OpenFoamExportResource.MESSAGE_BOX_TITLE,
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                succeed = false;
            }
            catch (Exception)
            {
                MessageBox.Show(OpenFoamExportResource.ERR_EXCEPTION, OpenFoamExportResource.MESSAGE_BOX_TITLE,
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
                if (command.Equals("\"") || command.Contains("rm -r") || command.Contains("call") || /*DEBUGGING BATCH*/command.Contains("pause"))
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
                            OpenFoamExportResource.MESSAGE_BOX_TITLE); ;
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
        /// Interface for initialize config.
        /// </summary>
        public virtual void CreateEnvConfig()
        {
            string defaultEnvPath = "None";
            string envTag = "<" + m_Env + ">"; ;
            string assemblyDir = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8);
            string assemblyDirCorrect = assemblyDir.Remove(assemblyDir.IndexOf("OpenFoamExport.dll"), 18).Replace("/", "\\");
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

        /// <summary>
        /// Initialize OpenFOAMTextBoxForm.
        /// </summary>
        private void StartOpenFOAMTextBoxForm()
        {
            OpenFOAMTextBoxForm m_OpenFOAMTextBoxForm = new OpenFOAMTextBoxForm();
            m_OpenFOAMTextBoxForm.Show();
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
        /// User on ssh server.
        /// </summary>
        string m_User = string.Empty;

        /// <summary>
        /// Server ip.
        /// </summary>
        string m_ServerIP = string.Empty;

        /// <summary>
        /// Alias to start openfoam-environment.
        /// </summary>
        string m_OpenFoamAlias = string.Empty;

        /// <summary>
        /// Destination path for simulation case folder on server.
        /// </summary>
        string m_ServerCasePath = string.Empty;

        /// <summary>
        /// If true, the server will send caseFolder back to client after simulation.
        /// </summary>
        bool m_SendBack = false;

        /// <summary>
        /// Constructor needs the casePath of the openFoam-case and environment.
        /// </summary>
        /// <param name="casePath">Path to openFfoam-case.</param>
        /// <param name="env">Enum that specifies the environment.</param>
        public RunManagerSSH(string casePath, OpenFOAMEnvironment env)
            :base(casePath, env)
        {
            m_CommandBat = casePath + @"\RunSSH.bat";
        }

        /// <summary>
        /// Initial shell commands for running OpenFOAM via ssh remote server.
        /// </summary>
        /// <returns>List with shell commands as string.</returns>
        public override List<string> InitialEnvRunCommands()
        {
            //string regex in textbox => host@server-ip
            //System.Text.RegularExpressions.Regex m_Vector3DReg = new System.Text.RegularExpressions.Regex("^\\S+@\\S+$");
            m_User = "mdjur";
            m_ServerIP = "192.168.2.102";
            //TO-DO: OpenFOAMTextBoxForm benutzen um user und server vom User zu erfragen.
            //OPENFOAMTEXTBOXFORM
            //upload directory to Server: scp -r /path/to/local/source user@ssh.example.com:/path/to/remote/destination 
            m_ServerCasePath = "/home/" + m_User + "/OpenFoamRemote";
            m_SendBack = true;
            //-t for print out all outputs from remote server to client.
            List<string> shellCommands = new List<string>
            {
                "ssh " + m_User + "@" + m_ServerIP + " -t ",
                "\"scp -r " + m_CasePath + " " + m_User + "@" + m_ServerIP + ":" + m_ServerCasePath + " && ",
                "cd " + m_ServerCasePath,
                //m_OpenFoamAlias
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
            //Download directory from Server: scp -r user@ssh.example.com:/path/to/remote/source /path/to/local/destination
            if(m_SendBack)
            {
                commands.Add("scp -r " + m_User + "@" + m_ServerIP + ":" + m_ServerCasePath + m_CasePath);
            }
            commands.Add("\"");
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
            if (command.Equals("\"") || command.Contains("ssh"))
            {
                sw.Write(command);
                return;
            }
            sw.Write(" && " + command);
        }
    }
}
