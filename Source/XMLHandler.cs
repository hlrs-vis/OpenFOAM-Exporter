using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace BIM.OpenFOAMExport
{
    /// <summary>
    /// 
    /// </summary>
    public class XMLHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        private void ReadConfig(string path)
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
        private void CreateConfig(Dictionary<string, object> dict)
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
                CreateXMLTree(defaultElement, dict);
                elements.Add(defaultElement);

                config.Add(elements);

                XElement ssh = config.Root.Element("SSH");
                ssh.Add(
                        new XElement("user", "name"),
                        new XElement("host", "111.122.1.123"),
                        new XElement("serverCasePath", "/home/\"User\"/OpenFOAMRemote/"),
                        new XElement("ofAlias", "source/opt/openfoam6/etc/bashrc")
                );
                config.Save(configPath);
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
