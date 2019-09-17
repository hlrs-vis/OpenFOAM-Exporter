using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace BIM.OpenFOAMExport
{
    /// <summary>
    /// This class is in use for handling the xml-config file.
    /// </summary>
    public class XMLHandler
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">Settings-object for current project.</param>
        public XMLHandler(Settings settings)
        {
            CreateConfig(settings);
        }

        /// <summary>
        /// Read the config file and add entries to settings.
        /// </summary>
        /// <param name="path"></param>
        private void ReadConfig(string path, Settings settings)
        {
            if (File.Exists(path))
            {
                XmlTextReader reader = new XmlTextReader(path);
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            {
                                // The node is an element.
                                Console.Write("<" + reader.Name);

                                while (reader.MoveToNextAttribute())
                                {
                                    // Read the attributes.
                                    Console.Write(" " + reader.Name + "='" + reader.Value + "'");
                                }

                                Console.WriteLine(">");
                                break;
                            }
                        case XmlNodeType.Text:
                            {
                                //Display the text in each element.
                                Console.WriteLine(reader.Value);
                                break;
                            }
                        case XmlNodeType.EndElement:
                            {
                                //Display the end of the element.
                                Console.Write("</" + reader.Name);
                                Console.WriteLine(">");
                                break;
                            }
                    }

                }
            }
        }

        /**********************TO-DO: IMPLEMENT READ FOR XML-CONFIG BEFORE INSERT THIS**********************/
        /// <summary>
        /// Create config file if it doesn't exist.
        /// </summary>
        private void CreateConfig(Settings settings)
        {
            //remove file:///
            string assemblyDir = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8);

            //remove name of dll from string
            string assemblyDirCorrect = assemblyDir.Remove(assemblyDir.IndexOf("OpenFOAMExport.dll"), 18).Replace("/", "\\");

            //configname
            string configPath = assemblyDirCorrect + "openFOAMExporter.config";
            if (!File.Exists(configPath))
            {

                var config = new XDocument();
                var elements = new XElement("OpenFOAMConfig",
                    new XElement("OpenFOAMEnv"),
                    new XElement("SSH")
                );

                var defaultElement = new XElement("DefaultParameter");
                Dictionary<string, object> dict = settings.SimulationDefault;
                CreateXMLTree(defaultElement, dict);
                elements.Add(defaultElement);

                config.Add(elements);

                XElement ssh = config.Root.Element("SSH");
                ssh.Add(
                        new XElement("user", settings.SSH.User),
                        new XElement("host", settings.SSH.ServerIP),
                        new XElement("serverCasePath", settings.SSH.ServerCaseFolder),
                        new XElement("ofAlias", settings.SSH.OfAlias),
                        new XElement("port", settings.SSH.Port.ToString()),
                        new XElement("tasks", settings.SSH.Tasks.ToString()),
                        new XElement("download",settings.SSH.Download),
                        new XElement("delete", settings.SSH.Delete),
                        new XElement("slurm", settings.SSH.Slurm)
                );
                config.Save(configPath);
            }
            else
            {
                ReadConfig(configPath, settings);
            }
        }

        /// <summary>
        /// Creates a XML-tree from given dict.
        /// </summary>
        /// <param name="e">XElement xml will be attached to.</param>
        /// <param name="dict">Source for XML-tree.</param>
        private void CreateXMLTree(XElement e, Dictionary<string, object> dict)
        {
            foreach (var element in dict)
            {
                string nameNode = element.Key;
                nameNode = PrepareXMLString(nameNode);
                var elem = new XElement(nameNode);
                if (element.Value is Dictionary<string, object>)
                {
                    CreateXMLTree(elem, element.Value as Dictionary<string, object>);
                }
                else
                {
                    elem.Value = element.Value.ToString();
                }
                e.Add(elem);
            }
        }

        /// <summary>
        /// Removes critical strings for xml.
        /// </summary>
        /// <param name="nameNode">String which will be prepared.</param>
        /// <returns>Prepared string.</returns>
        private static string PrepareXMLString(string nameNode)
        {
            if (nameNode.Equals("0"))
            {
                nameNode = "null";
                return nameNode;
            }

            var criticalXMLCharacters = new Dictionary<string, string>()
            {
                { "(", "lpar" },
                { ")", "rpar" },
                { ",", "comma" },
                { "*", "ast" },
                { " ", "nbsp" }
            };

            foreach (var critical in criticalXMLCharacters)
            {
                nameNode = nameNode.Replace(critical.Key, critical.Value);
            }

            return nameNode;
        }
    }
}
