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

        private DecomposeParDict decomposeParDict;

        public DecomposeParDict DecomposeParDict { set => decomposeParDict = value; get => decomposeParDict; }

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="casePath">Path to case.</param>
        public RunManager(string casePath)
        {
            m_CasePath = casePath;
        }

        /// <summary>
        /// Interface for running given commands in OpenFOAM-Environment.
        /// </summary>
        /// <param name="commands">Contains commands as string.</param>
        public abstract bool RunCommands(List<string> commands);

        /// <summary>
        /// Interface for initialize config.
        /// </summary>
        public abstract void CreateEnvConfig();

    }

    /// <summary>
    /// Runmanager that is used for running OpenFOAM in BlueCFD.
    /// </summary>
    public class RunManagerBlueCFD : RunManager
    {
        /// <summary>
        /// Path to setvars.bat file in install folder.
        /// </summary>
        private string m_FOAMEnvPath;

        /// <summary>
        /// Path to command.bat.
        /// </summary>
        private string m_CommandBat;

        /// <summary>
        /// Constructor creates RunManagerBlueCFD-Object.
        /// </summary>
        /// <param name="casePath">Path to case.</param>
        /// <param name="foamPath">Path to DOS_Mode.bat file.</param>
        public RunManagerBlueCFD(string casePath)
            : base(casePath)
        {
            m_CommandBat = casePath + @"\Run.bat";
            CreateEnvConfig();
        }

        /// <summary>
        /// Runs given commands in blueCFD-Environment.
        /// </summary>
        /// <param name="commands">Contains commands as string.</param>
        public override bool RunCommands(List<string> commands)
        {
            bool succeed = true;
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

                //create bat
                using (StreamWriter commandBat = new StreamWriter(m_CommandBat))
                {
                    fileAttribute = File.GetAttributes(m_CommandBat) | fileAttribute;
                    File.SetAttributes(m_CommandBat, fileAttribute);
                    commandBat.WriteLine("call " + "\"" + m_FOAMEnvPath + "\"");
                    commandBat.WriteLine("set " + @"PATH=%HOME%\msys64\usr\bin;%PATH%");
                    commandBat.WriteLine("cd " + m_CasePath);
                    string log = " | tee " + @"log\";
                    foreach (string command in commands)
                    {
                        if(DecomposeParDict != null)
                        {
                            if(command.Equals("snappyHexMesh"))
                            {
                                commandBat.WriteLine("decomposePar" + log + "decomposepar.log");
                                commandBat.WriteLine("mpirun -np " + DecomposeParDict.NumberOfSubdomains + " " + command + " -overwrite -parallel " + log + command + ".log");
                                commandBat.WriteLine("reconstructParMesh -constant" + log + "reconstructParMesh.log");
                                continue;
                            }
                            else if (command.Equals("simpleFoam"))
                            {
                                commandBat.WriteLine("decomposePar");
                                commandBat.WriteLine("mpirun -n " + DecomposeParDict.NumberOfSubdomains + " renumberMesh -overwrite -parallel");
                                commandBat.WriteLine("mpirun -np " + DecomposeParDict.NumberOfSubdomains + " " + command + " -parallel " + log + command + ".log");
                                commandBat.WriteLine("reconstructPar -latestTime");
                                continue;
                            }
                        }
                        commandBat.WriteLine(command + log + command + ".log");
                    }
                }

                //start batch
                using (Process process = Process.Start(m_CommandBat))
                {
                    process.WaitForExit();
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
        /// If config exists, check for default-blueCFD-path otherwise create config through user-input.
        /// </summary>
        public override void CreateEnvConfig()
        {
            string defaultBatPath = @"C:\Program Files\blueCFD-Core-2017\setvars.bat";
            string assemblyDir = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8);
            string assemblyDirCorrect = assemblyDir.Remove(assemblyDir.IndexOf("OpenFoamExport.dll"), 18).Replace("/","\\");
            string configPath = assemblyDirCorrect + "openfoam_env_config.config";
            string blueCFDTag = "<blueCFD>";
            if(!File.Exists(configPath))
            {
                StreamWriter sw = new StreamWriter(configPath);
                sw.WriteLine("**********************Config for OpenFOAM-Environment**********************");
                if(File.Exists(defaultBatPath))
                {
                    sw.WriteLine(blueCFDTag + " " + defaultBatPath);
                    m_FOAMEnvPath = defaultBatPath;
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
                        if (s.Contains(value: blueCFDTag))
                        {
                            configDone = true;
                            if (!s.Contains(defaultBatPath))
                            {
                                m_FOAMEnvPath = s.Substring(s.IndexOf(" "));
                            }
                            else
                            {
                                m_FOAMEnvPath = defaultBatPath;
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
                        if (File.Exists(defaultBatPath))
                        {
                            sw.WriteLine(blueCFDTag + defaultBatPath);
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
    /// Runmanager that is used for running OpenFOAM in Docker.
    /// </summary>
    public class RunManagerDocker : RunManager
    {
        public RunManagerDocker(string casePath)
            : base(casePath)
        {

        }

        public override bool RunCommands(List<string> commands)
        {
            throw new System.NotImplementedException();
        }

        public override void CreateEnvConfig()
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// RunManager that is used for running OpenFOAM in linux.
    /// </summary>
    public class RunManagerLinux : RunManager
    {

        public RunManagerLinux(string casePath)
            :base(casePath)
        {

        }

        public override void CreateEnvConfig()
        {
            throw new NotImplementedException();
        }

        public override bool RunCommands(List<string> commands)
        {
            throw new NotImplementedException();
        }
    }
}
