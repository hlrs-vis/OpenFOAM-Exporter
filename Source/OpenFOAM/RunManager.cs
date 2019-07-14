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
            m_CommandBat = casePath + @"\Run.bat";
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
        /// Interface for running given commands in OpenFOAM-Environment.
        /// </summary>
        /// <param name="commands">Contains commands as string.</param>
        public virtual bool RunCommands(List<string> commands)
        {
            //create initial Environment commands.
            List<string> envCommands = new List<string>();
            List<string> runCommands = InitialEnvRunCommands();
            foreach(string s in runCommands)
            {
                envCommands.Add(s);
            }
            string log = " | tee " + @"log\";
            if (m_Env == OpenFOAMEnvironment.linux || m_Env == OpenFOAMEnvironment.linuxSubsystem)
            {
                log = "";
            }

            foreach (string command in commands)
            {
                if (DecomposeParDict != null)
                {
                    if (command.Equals("snappyHexMesh"))
                    {
                        envCommands.Add("decomposePar" + log + "decomposepar.log");
                        envCommands.Add("mpirun -np " + DecomposeParDict.NumberOfSubdomains + " " + command + " -overwrite -parallel " + log + command + ".log");
                        envCommands.Add("reconstructParMesh -constant" + log + "reconstructParMesh.log");
                        continue;
                    }
                    else if (command.Equals("simpleFoam"))
                    {
                        envCommands.Add("decomposePar");
                        envCommands.Add("mpirun -n " + DecomposeParDict.NumberOfSubdomains + " renumberMesh -overwrite -parallel");
                        envCommands.Add("mpirun -np " + DecomposeParDict.NumberOfSubdomains + " " + command + " -parallel " + log + command + ".log");
                        envCommands.Add("reconstructPar -latestTime");
                        continue;
                    }
                }
                envCommands.Add(command + log + command + ".log");
            }

            bool succeed = WriteToCommandBat(envCommands);

            if(succeed)
            {
                //start batch
                using (Process process = Process.Start(m_CommandBat))
                {
                    process.WaitForExit();
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
        //public abstract void CreateEnvConfig();

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
                case OpenFOAMEnvironment.linuxSubsystem:
                    {
                        defaultEnvPath = @"C:\Windows\System32\bash.exe";
                        break;
                    }
                case OpenFOAMEnvironment.docker:
                    {
                        //implement docker runmanger.
                        break;
                    }
                case OpenFOAMEnvironment.linux:
                    {
                        //implement linux runmanager.
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
    /// Runmanager that is used for running OpenFOAM in BlueCFD.
    /// </summary>
    public class RunManagerBlueCFD : RunManager
    {
        /// <summary>
        /// Path to setvars.bat file in install folder.
        /// </summary>
        //private string m_FOAMEnvPath;

        /// <summary>
        /// Path to command.bat.
        /// </summary>
        //private string m_CommandBat;

        /// <summary>
        /// Constructor creates RunManagerBlueCFD-Object.
        /// </summary>
        /// <param name="casePath">Path to case.</param>
        /// <param name="foamPath">Path to DOS_Mode.bat file.</param>
        public RunManagerBlueCFD(string casePath, OpenFOAMEnvironment env)
            : base(casePath, env)
        {
            //m_CommandBat = casePath + @"\Run.bat";
            //CreateEnvConfig();
        }

        /// <summary>
        /// Initial shell commands for running the blueCFD environment.
        /// </summary>
        /// <returns>List with shell commands as string.</returns>
        public override List<string> InitialEnvRunCommands()
        {
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

        ///// <summary>
        ///// Write commands to .bat file.
        ///// </summary>
        ///// <param name="command">List of commands</param>
        ///// <returns>If succeed = true, else false.</returns>
        //public override bool WriteToCommandBat(List<string> command)
        //{
        //    bool succeed = true;
        //    try
        //    {
        //        FileAttributes fileAttribute = FileAttributes.Normal;
        //        if (File.Exists(m_CommandBat))
        //        {
        //            fileAttribute = File.GetAttributes(m_CommandBat);
        //            FileAttributes tempAtt = fileAttribute & FileAttributes.ReadOnly;
        //            if (FileAttributes.ReadOnly == tempAtt)
        //            {
        //                MessageBox.Show(OpenFoamExportResource.ERR_FILE_READONLY, OpenFoamExportResource.MESSAGE_BOX_TITLE,
        //                      MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //                return false;
        //            }
        //            File.Delete(m_CommandBat);
        //        }

        //        using (StreamWriter commandBat = new StreamWriter(m_CommandBat))
        //        {
        //            fileAttribute = File.GetAttributes(m_CommandBat) | fileAttribute;
        //            File.SetAttributes(m_CommandBat, fileAttribute);
        //            foreach(string com in command)
        //            {
        //                commandBat.WriteLine(com);
        //            }
        //        }
        //    }
        //    catch (SecurityException)
        //    {
        //        MessageBox.Show(OpenFoamExportResource.ERR_SECURITY_EXCEPTION, OpenFoamExportResource.MESSAGE_BOX_TITLE,
        //                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        succeed = false;
        //    }
        //    catch (IOException)
        //    {
        //        MessageBox.Show(OpenFoamExportResource.ERR_IO_EXCEPTION, OpenFoamExportResource.MESSAGE_BOX_TITLE,
        //                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        succeed = false;
        //    }
        //    catch (Exception)
        //    {
        //        MessageBox.Show(OpenFoamExportResource.ERR_EXCEPTION, OpenFoamExportResource.MESSAGE_BOX_TITLE,
        //                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        succeed = false;
        //    }
        //    return succeed;
        //}



        ///// <summary>
        ///// Runs given commands in blueCFD-Environment.
        ///// </summary>
        ///// <param name="commands">Contains commands as string.</param>
        //public override bool RunCommands(List<string> commands)
        //{
        //    bool succeed = true;
        //    try
        //    {
        //        FileAttributes fileAttribute = FileAttributes.Normal;
        //        if (File.Exists(m_CommandBat))
        //        {
        //            fileAttribute = File.GetAttributes(m_CommandBat);
        //            FileAttributes tempAtt = fileAttribute & FileAttributes.ReadOnly;
        //            if (FileAttributes.ReadOnly == tempAtt)
        //            {
        //                MessageBox.Show(OpenFoamExportResource.ERR_FILE_READONLY, OpenFoamExportResource.MESSAGE_BOX_TITLE,
        //                      MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //                return false;
        //            }
        //            File.Delete(m_CommandBat);
        //        }

        //        //create bat
        //        using (StreamWriter commandBat = new StreamWriter(m_CommandBat))
        //        {
        //            fileAttribute = File.GetAttributes(m_CommandBat) | fileAttribute;
        //            File.SetAttributes(m_CommandBat, fileAttribute);
        //            commandBat.WriteLine("call " + "\"" + m_FOAMEnvPath + "\"");
        //            commandBat.WriteLine("set " + @"PATH=%HOME%\msys64\usr\bin;%PATH%");
        //            commandBat.WriteLine("cd " + m_CasePath);
        //            string log = " | tee " + @"log\";
        //            foreach (string command in commands)
        //            {
        //                if(DecomposeParDict != null)
        //                {
        //                    if(command.Equals("snappyHexMesh"))
        //                    {
        //                        commandBat.WriteLine("decomposePar" + log + "decomposepar.log");
        //                        commandBat.WriteLine("mpirun -np " + DecomposeParDict.NumberOfSubdomains + " " + command + " -overwrite -parallel " + log + command + ".log");
        //                        commandBat.WriteLine("reconstructParMesh -constant" + log + "reconstructParMesh.log");
        //                        continue;
        //                    }
        //                    else if (command.Equals("simpleFoam"))
        //                    {
        //                        commandBat.WriteLine("decomposePar");
        //                        commandBat.WriteLine("mpirun -n " + DecomposeParDict.NumberOfSubdomains + " renumberMesh -overwrite -parallel");
        //                        commandBat.WriteLine("mpirun -np " + DecomposeParDict.NumberOfSubdomains + " " + command + " -parallel " + log + command + ".log");
        //                        commandBat.WriteLine("reconstructPar -latestTime");
        //                        continue;
        //                    }
        //                }
        //                commandBat.WriteLine(command + log + command + ".log");
        //            }
        //        }

        //        //start batch
        //        using (Process process = Process.Start(m_CommandBat))
        //        {
        //            process.WaitForExit();
        //        }
        //    }
        //    catch (SecurityException)
        //    {
        //        MessageBox.Show(OpenFoamExportResource.ERR_SECURITY_EXCEPTION, OpenFoamExportResource.MESSAGE_BOX_TITLE,
        //                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        succeed = false;
        //    }
        //    catch (IOException)
        //    {
        //        MessageBox.Show(OpenFoamExportResource.ERR_IO_EXCEPTION, OpenFoamExportResource.MESSAGE_BOX_TITLE,
        //                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        succeed = false;
        //    }
        //    catch (Exception)
        //    {
        //        MessageBox.Show(OpenFoamExportResource.ERR_EXCEPTION, OpenFoamExportResource.MESSAGE_BOX_TITLE,
        //                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        succeed = false;
        //    }
        //    return succeed;
        //}

        ///// <summary>
        ///// If config exists, check for default-blueCFD-path otherwise create config through user-input.
        ///// </summary>
        //public override void CreateEnvConfig()
        //{
        //    string defaultBatPath = @"C:\Program Files\blueCFD-Core-2017\setvars.bat";
        //    string assemblyDir = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8);
        //    string assemblyDirCorrect = assemblyDir.Remove(assemblyDir.IndexOf("OpenFoamExport.dll"), 18).Replace("/","\\");
        //    string configPath = assemblyDirCorrect + "openfoam_env_config.config";
        //    string blueCFDTag = "<blueCFD>";
        //    if(!File.Exists(configPath))
        //    {
        //        StreamWriter sw = new StreamWriter(configPath);
        //        sw.WriteLine("**********************Config for OpenFOAM-Environment**********************");
        //        if(File.Exists(defaultBatPath))
        //        {
        //            sw.WriteLine(blueCFDTag + " " + defaultBatPath);
        //            m_FOAMEnvPath = defaultBatPath;
        //        }
        //        else
        //        {
        //            ///TO-DO: NEW GUI FOR OPENFOAM-ENVIRONMENT USER INPUT
        //            m_FOAMEnvPath = string.Empty;
        //        }
        //        sw.Close();
        //    }
        //    else
        //    {
        //        using (var sr = File.OpenText(configPath))
        //        {
        //            var configDone = false;
        //            string s;
        //            while ((s = sr.ReadLine()) != null)
        //            {
        //                if (s.Contains(value: blueCFDTag))
        //                {
        //                    configDone = true;
        //                    if (!s.Contains(defaultBatPath))
        //                    {
        //                        m_FOAMEnvPath = s.Substring(s.IndexOf(" "));
        //                    }
        //                    else
        //                    {
        //                        m_FOAMEnvPath = defaultBatPath;
        //                    }
        //                    break;
        //                }
        //            }
        //            sr.Close();
        //            if (configDone)
        //            {
        //                return;
        //            }
        //            using (var sw = File.AppendText(configPath))
        //            {
        //                if (File.Exists(defaultBatPath))
        //                {
        //                    sw.WriteLine(blueCFDTag + defaultBatPath);
        //                }
        //                else
        //                {
        //                    ///TO-DO: NEW GUI FOR OPENFOAM-ENVIRONMENT USER INPUT
        //                    m_FOAMEnvPath = string.Empty;
        //                }
        //            }
        //        }
        //    }
        //}
    }

    /// <summary>
    /// Runmanager that is used for running OpenFOAM native in subsystem.
    /// </summary>
    public class RunManagerLinuxSubsystem : RunManager
    {
        /// <summary>
        /// Constructor needs the casePath of the openFoam-case and environment.
        /// </summary>
        /// <param name="casePath">Path to openFfoam-case.</param>
        /// <param name="env">Enum that specifies the environment.</param>
        public RunManagerLinuxSubsystem(string casePath, OpenFOAMEnvironment env)
            : base(casePath, env)
        {

        }

        //public override void CreateEnvConfig()
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override List<string> InitialEnvRunCommands()
        {
            List<string> shellCommands = new List<string>
            {
                "bash -c -i "
            };
            return shellCommands;
        }

        //public override bool RunCommands(List<string> commands)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// The method writes command in one line into the streamWriter-object.
        /// </summary>
        /// <param name="sw">StreamWriter object.</param>
        /// <param name="command">Command as string.</param>
        public override void WriteLine(StreamWriter sw, string command)
        {
            sw.Write(command + "&& ");
        }

        //public override bool WriteToCommandBat(List<string> command)
        //{
        //    throw new NotImplementedException();
        //}
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

        //public override bool RunCommands(List<string> commands)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public override void CreateEnvConfig()
        //{
        //    throw new System.NotImplementedException();
        //}

        public override List<string> InitialEnvRunCommands()
        {
            throw new NotImplementedException();
        }

        //public override bool WriteToCommandBat(List<string> command)
        //{
        //    throw new NotImplementedException();
        //}

        public override void WriteLine(StreamWriter sw, string command)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// RunManager that is used for running OpenFOAM in linux.
    /// </summary>
    public class RunManagerLinux : RunManager
    {
        public RunManagerLinux(string casePath, OpenFOAMEnvironment env)
            :base(casePath, env)
        {

        }

        //public override void CreateEnvConfig()
        //{
        //    throw new NotImplementedException();
        //}

        public override List<string> InitialEnvRunCommands()
        {
            throw new NotImplementedException();
        }

        //public override bool RunCommands(List<string> commands)
        //{
        //    throw new NotImplementedException();
        //}

        public override void WriteLine(StreamWriter sw, string command)
        {
            throw new NotImplementedException();
        }

        //public override bool WriteToCommandBat(List<string> command)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
