//GNU License ENTRY FROM STL-EXPORTER
//Source Code: https://github.com/Autodesk/revit-stl-extension
// STL exporter library: this library works with Autodesk(R) Revit(R) to export an STL file containing model geometry.
// Copyright (C) 2013  Autodesk, Inc.
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//
// Modified version
// Author: Marko Djuric

using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using RevitApplication = Autodesk.Revit.ApplicationServices.Application;
using GeometryElement = Autodesk.Revit.DB.GeometryElement;
using GeometryOptions = Autodesk.Revit.DB.Options;
using GeometryInstance = Autodesk.Revit.DB.GeometryInstance;
using RevitView = Autodesk.Revit.DB.View;
using BIM.OpenFOAMExport.OpenFOAM;
using Material = Autodesk.Revit.DB.Material;
using System.Security;

namespace BIM.OpenFOAMExport
{
    /// <summary>
    /// Generate triangular data and save them in a temporary file.
    /// </summary>
    public class DataGenerator
    {
        //STL-Exporter objects
        public enum GeneratorStatus { SUCCESS, FAILURE, CANCEL };
        private SaveData m_Writer;
        private readonly RevitApplication m_RevitApp;
        private readonly Document m_ActiveDocument;
        private readonly RevitView m_ActiveView;
        private int m_TriangularNumber;

        /// <summary>
        /// Name of the STL.
        /// </summary>
        private string m_STLName;

        /// <summary>
        /// Name of the WallInSTL
        /// </summary>
        private string m_STLWallName;

        /// <summary>
        /// RunManager for OpenFOAM-Environment
        /// </summary>
        private RunManager m_RunManager;

        /// <summary>
        /// Vector for boundingbox
        /// </summary>
        private Vector3D m_LowerEdgeVector;

        /// <summary>
        /// Vector for boundingbox
        /// </summary>
        private Vector3D m_UpperEdgeVector;

        /// <summary>
        /// Current View-Options
        /// </summary>
        private readonly GeometryOptions m_ViewOptions;

        /// <summary>
        /// Categories which will be included in STL
        /// </summary>
        private SortedDictionary<string, Category> m_Categories;

        /// <summary>
        /// Settings of the whole project
        /// </summary>
        private Settings m_Settings;

        /// <summary>
        /// Cancel GUI
        /// </summary>
        private readonly OpenFOAMExportCancelForm m_StlExportCancel = new OpenFOAMExportCancelForm();

        /// <summary>
        /// Materials from inlet/outlet
        /// </summary>
        private List<ElementId> m_InletOutletMaterials;

        /// <summary>
        /// Duct-Terminals
        /// </summary>
        private List<Element> m_DuctTerminalsInDoc;

        /// <summary>
        /// OpenFOAM-Dictionaries
        /// </summary>
        private List<FOAMDict> m_OpenFOAMDictionaries;

        /// <summary>
        /// Faces of the inlet/outlet for openFOAM-Simulation
        /// </summary>
        private Dictionary<KeyValuePair<string, Document>, KeyValuePair<Face, Transform>> m_FacesInletOutlet;

        /// <summary>
        /// Number of triangles in exported Revit document.
        /// </summary>
        public int TriangularNumber
        {
            get
            {
                return m_TriangularNumber;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="revit">
        /// The application object for the active instance of Autodesk Revit.
        /// </param>
        public DataGenerator(RevitApplication revitApp, Document doc, RevitView view)
        {
            //initialize the member variable
            if (revitApp != null)
            {
                m_RevitApp = revitApp;
                m_ActiveDocument = doc;
                m_ActiveView = view;

                m_ViewOptions = m_RevitApp.Create.NewGeometryOptions();
                m_ViewOptions.View = m_ActiveView;
            }
        }

        /// <summary>
        /// Initialize Runmanager as RunManagerBlueCFD or RunManagerDocker depending on the WindowsFOAMVersion that is set in Settings.
        /// </summary>
        /// <param name="casePath">Path to openFOAM-Case.</param>
        private GeneratorStatus InitRunManager(string casePath)
        {
            switch(m_Settings.OpenFOAMEnvironment)
            {
                case OpenFOAMEnvironment.blueCFD:
                    m_RunManager = new RunManagerBlueCFD(casePath, m_Settings.OpenFOAMEnvironment);
                    break;
                //case OpenFOAMEnvironment.docker:
                //    m_RunManager = new RunManagerDocker(casePath, m_Settings.OpenFOAMEnvironment);
                //    break;
                case OpenFOAMEnvironment.ssh:
                    m_RunManager = new RunManagerSSH(casePath, m_Settings.OpenFOAMEnvironment, m_Settings);
                    break;
                case OpenFOAMEnvironment.wsl:
                    m_RunManager = new RunManagerWSL(casePath, m_Settings.OpenFOAMEnvironment);
                    break;
            }
            return m_RunManager.Status;
        }

    /// <summary>
    /// Create OpenFOAM-Folder at given path.
    /// </summary>
    /// <param name="path">location for the OpenFOAM-folder</param>
    private GeneratorStatus CreateOpenFOAMCase(string path)
        {
            List<string> minCaseFolders = new List<string>();

            //first level folders
            string constant = Path.Combine(path, "constant");
            string nullFolder = Path.Combine(path, "0");
            string system = Path.Combine(path, "system");
            string log = Path.Combine(path, "log");

            //second level folders
            string polyMesh = Path.Combine(constant, "polyMesh");
            string triSurface = Path.Combine(constant, "triSurface");

            //paths to folders
            minCaseFolders.Add(nullFolder);
            minCaseFolders.Add(log);
            minCaseFolders.Add(system);
            minCaseFolders.Add(polyMesh);
            minCaseFolders.Add(triSurface);

            //create folders
            foreach(string folder in minCaseFolders)
            {
                Directory.CreateDirectory(folder);
            }

            GeneratorStatus status = InitRunManager(path);
            if(status != GeneratorStatus.SUCCESS)
            {
                return status;
            }

            //.foam-File
            File.Create(path + "\\" + m_STLName + ".foam");

            //move .stl from given location into triSurface folder TO-DO: Not Moving => Generate
            File.Move(m_Writer.FileName, triSurface + "\\" + m_STLName + ".stl");
            File.Delete(m_Writer.FileName);

            //generate files
            OpenFOAM.Version version = new OpenFOAM.Version();
            m_OpenFOAMDictionaries = new List<FOAMDict>();

            //init folders
            InitSystemFolder(version, system);
            InitNullFolder(version, nullFolder);
            InitConstantFolder(version, constant);

            foreach (FOAMDict openFOAMDictionary in m_OpenFOAMDictionaries)
            {
                openFOAMDictionary.Init();
            }

            List<string> commands = new List<string>();

            //commands
            if (m_Settings.OpenFOAMEnvironment == OpenFOAMEnvironment.ssh)
            {
                SetupLinux(path, commands);
                
            }
            else
            {
                commands.Add("blockMesh");
                commands.Add("surfaceFeatureExtract");
                commands.Add("snappyHexMesh");
                commands.Add("rm -r processor*");
                commands.Add(m_Settings.AppSolverControlDict.ToString());
                commands.Add("rm -r processor*");
                
            }

            //List<string> commands = new List<string> { "blockMesh", "surfaceFeatureExtract", "snappyHexMesh", "rm -r processor*" };
            ////, "simpleFoam", "rm -r processor*"};
            //commands.Add(m_Settings.AppSolverControlDict.ToString());
            //commands.Add("rm -r processor*");
            //commands.Add("rm - rf constant/extendedFeatureEdgeMesh > / dev / null 2 > &1");
            //commands.Add("rm -f constant/triSurface/buildings.eMesh > /dev/null 2>&1");
            //commands.Add("rm -f constant/polyMesh/boundary > /dev/null 2>&1");

            //run commands in windows-openfoam-environment
            if (!m_RunManager.RunCommands(commands))
            {
                return GeneratorStatus.FAILURE;
            }
            return GeneratorStatus.SUCCESS;
        }

        /// <summary>
        /// Add allrun and allclean to the case folder and add corresponding command to the list.
        /// </summary>
        /// <param name="path">Path to case folder.</param>
        /// <param name="commands">List of commands.</param>
        private void SetupLinux(string path, List<string> commands)
        {
            string allrun;
            if (m_Settings.NumberOfSubdomains != 1)
            {
                allrun = "#!/bin/sh" +
                "\ncd ${0%/*} || exit 1    # run from this directory" +
                "\n" +
                "\n# Source tutorial run functions" +
                "\n. $WM_PROJECT_DIR/bin/tools/RunFunctions" +
                "\n" +
                "\nrunApplication surfaceFeatureExtract" +
                "\n" +
                "\nrunApplication blockMesh" +
                "\n" +
                "\nrunApplication decomposePar -copyZero" +
                "\nrunParallel snappyHexMesh -overwrite" +
                "\n" +
                //Problem with regular allrun => bypass through recontstructParMesh and decompose the case again
                "\nrunApplication reconstructParMesh -constant" +
                "\nrm -r processor*" +
                "\nrm -rf log.decomposePar" +
                "\nrunApplication decomposePar" +
                //"\nrunParallel renumberMesh -overwrite" +
                "\nrunParallel $(getApplication)" +
                "\n" +
                "\nrunApplication reconstructPar -latestTime" +
                "\n#------------------------------------------------------------------------------";
            }
            else
            {
                allrun = "#!/bin/sh" +
                "\ncd ${0%/*} || exit 1    # run from this directory" +
                "\n" +
                "\n# Source tutorial run functions" +
                "\n. $WM_PROJECT_DIR/bin/tools/RunFunctions" +
                "\n" +
                "\nrunApplication surfaceFeatureExtract" +
                "\n" +
                "\nrunApplication blockMesh" +
                "\n" +
                "\nrunApplication snappyHexMesh -overwrite" +
                "\n" +
                "\nrunApplication $(getApplication)" +
                "\n#------------------------------------------------------------------------------";
            }

            if (CreateGeneralFile(path, "Allrun.", allrun))
            {
                commands.Add("./Allrun");

                string allclean = "#!/bin/sh" +
                    "\ncd ${0%/*} || exit 1    # run from this directory" +
                    "\n" +
                    "\n# Source tutorial clean functions" +
                    "\n. $WM_PROJECT_DIR/bin/tools/CleanFunctions" +
                    "\n" +
                    "\ncleanCase" +
                    "\n" +
                    "\nrm -rf constant/extendedFeatureEdgeMesh > /dev/null 2>&1" +
                    "\nrm -f constant/triSurface/buildings.eMesh > /dev/null 2>&1" +
                    "\nrm -f constant/polyMesh/boundary > /dev/null 2>&1" +
                    "\n" +
                    "\n#------------------------------------------------------------------------------";

                CreateGeneralFile(path, "Allclean.", allclean);
            }
        }

        /// <summary>
        /// Creates general file in openfoam case folder.
        /// For example: Allrun, Allclean
        /// <paramref name="path"/>Path<param>ref name="path"/>
        /// <paramref name="name"/>Name of the file<paramref name="name"/>
        /// <paramref name="text"/>Text for file.<paramref name="text"/>
        /// </summary>
        private bool CreateGeneralFile(string path, string name, string text)
        {
            bool succeed = true;
            string m_Path = Path.Combine(path, name);
            try
            {
                FileAttributes fileAttribute = FileAttributes.Normal;

                if (File.Exists(m_Path))
                {
                    fileAttribute = File.GetAttributes(m_Path);
                    FileAttributes tempAtt = fileAttribute & FileAttributes.ReadOnly;
                    if (FileAttributes.ReadOnly == tempAtt)
                    {
                        MessageBox.Show(OpenFOAMExportResource.ERR_FILE_READONLY, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
                              MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                    File.Delete(m_Path);
                }

                //Create File.
                using (StreamWriter sw = new StreamWriter(m_Path))
                {
                    fileAttribute = File.GetAttributes(m_Path) | fileAttribute;
                    File.SetAttributes(m_Path, fileAttribute);
                    
                    // Add information to the file.
                    sw.Write(text);
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
        /// Creates all dictionaries in systemFolder.
        /// </summary>
        /// <param name="version">Current version-object.</param>
        /// <param name="system">Path to system folder.</param>
        private void InitSystemFolder(OpenFOAM.Version version, string system)
        {
            //files in the system folder
            string blockMeshDict = Path.Combine(system, "blockMeshDict.");
            string surfaceFeatureExtractDict = Path.Combine(system, "surfaceFeatureExtractDict.");
            string decomposeParDict = Path.Combine(system, "decomposeParDict.");
            string controlDict = Path.Combine(system, "controlDict.");
            string fvSchemes = Path.Combine(system, "fvSchemes.");
            string fvSolution = Path.Combine(system, "fvSolution.");
            string meshDict = "";
            switch (m_Settings.Mesh)
            {
                case MeshType.Snappy:
                    {
                        meshDict = Path.Combine(system, "snappyHexMeshDict.");
                        break;
                    }
                case MeshType.cfMesh:
                    {
                        meshDict = Path.Combine(system, "meshDict.");
                        break;
                    }
            }

            //generate Files
            BlockMeshDict blockMeshDictionary = new BlockMeshDict(version, blockMeshDict, null, SaveFormat.ascii, m_Settings, m_LowerEdgeVector, m_UpperEdgeVector);
            ControlDict controlDictionary = new ControlDict(version, controlDict, null, SaveFormat.ascii, m_Settings, null);
            SurfaceFeatureExtract surfaceFeatureExtractDictionary = new SurfaceFeatureExtract(version, surfaceFeatureExtractDict, null, SaveFormat.ascii, m_Settings, m_STLName);
            DecomposeParDict decomposeParDictionary = new DecomposeParDict(version, decomposeParDict, null, SaveFormat.ascii, m_Settings);
            FvSchemes fvSchemesDictionary = new FvSchemes(version, fvSchemes, null, SaveFormat.ascii, m_Settings);
            FvSolution fvSolutionDictionary = new FvSolution(version, fvSolution, null, SaveFormat.ascii, m_Settings);
            SnappyHexMeshDict snappyHexMeshDictionary = new SnappyHexMeshDict(version, meshDict, null, SaveFormat.ascii, m_Settings, m_STLName, m_STLWallName, m_FacesInletOutlet);

            //runmanager have to know how much cpu's should be used
            m_RunManager.DecomposeParDict = decomposeParDictionary;

            m_OpenFOAMDictionaries.Add(blockMeshDictionary);
            m_OpenFOAMDictionaries.Add(controlDictionary);
            m_OpenFOAMDictionaries.Add(surfaceFeatureExtractDictionary);
            m_OpenFOAMDictionaries.Add(decomposeParDictionary);
            m_OpenFOAMDictionaries.Add(fvSchemesDictionary);
            m_OpenFOAMDictionaries.Add(fvSolutionDictionary);
            m_OpenFOAMDictionaries.Add(snappyHexMeshDictionary);
        }

        /// <summary>
        /// Creates all dictionaries in the nullfolder.
        /// </summary>
        /// <param name="version">Current version-object</param>
        /// <param name="nullFolder">Path to nullfolder.</param>
        private void InitNullFolder(OpenFOAM.Version version, string nullFolder)
        {
            List<string> paramList = new List<string>();

            //Files in nullfolder
            foreach (var param in m_Settings.SimulationDefault["0"] as Dictionary<string, object>)
            {
                paramList.Add(Path.Combine(nullFolder, param.Key + "."));
            }

            //Extract inlet/outlet-names
            List<string> inletNames = new List<string>();
            List<string> outletNames = new List<string>();
            foreach (var face in m_FacesInletOutlet)
            {
                string name = face.Key.Key.Replace(" ", "_");
                if (name.Contains("Zuluft") || name.Contains("Inlet"))
                {
                    inletNames.Add(name);
                }
                else
                {
                    outletNames.Add(name);
                }
            }

            //generate Files
            GenerateFOAMFiles(version, paramList, inletNames, outletNames);
        }

        /// <summary>
        /// Generate FOAM files.
        /// </summary>
        /// <param name="version">Version.</param>
        /// <param name="param">List of FOAMParameter as string.</param>
        /// <param name="inletNames">List of inlet names as string.</param>
        /// <param name="outletNames">List of outlet names as string.</param>
        private void GenerateFOAMFiles(OpenFOAM.Version version, List<string> param, List<string> inletNames, List<string> outletNames)
        {
            FOAMDict parameter;
            foreach(string nameParam in param)
            {
                if(nameParam.Contains("U."))
                {
                    parameter = new U(version, nameParam, null, SaveFormat.ascii, m_Settings, "wall", inletNames, outletNames);
                }
                else if(nameParam.Contains("p."))
                {
                    parameter = new P("p", version, nameParam, null, SaveFormat.ascii, m_Settings, "wall", inletNames, outletNames);
                }
                else if(nameParam.Contains("epsilon."))
                {
                    parameter = new Epsilon(version, nameParam, null, SaveFormat.ascii, m_Settings, "wall", inletNames, outletNames);
                }
                else if(nameParam.Contains("nut."))
                {
                    parameter = new Nut(version, nameParam, null, SaveFormat.ascii, m_Settings, "wall", inletNames, outletNames);
                }
                else if(nameParam.Contains("k."))
                {
                    parameter = new K(version, nameParam, null, SaveFormat.ascii, m_Settings, "wall", inletNames, outletNames);
                }
                else if(nameParam.Contains("alphat"))
                {
                    parameter = new Alphat(version, nameParam, null, SaveFormat.ascii, m_Settings, "wall", inletNames, outletNames);
                }
                else if(nameParam.Contains("p_rgh"))
                {
                    parameter = new P_rgh(version, nameParam, null, SaveFormat.ascii, m_Settings, "wall", inletNames, outletNames);
                }
                else if(nameParam.Contains("T"))
                {
                    parameter = new T(version, nameParam, null, SaveFormat.ascii, m_Settings, "wall", inletNames, outletNames);
                }
                else
                {
                    parameter = new U(version, nameParam, null, SaveFormat.ascii, m_Settings, "wall", inletNames, outletNames);
                }
                m_OpenFOAMDictionaries.Add(parameter);
            }
        }

        /// <summary>
        /// Initialize Dictionaries in constant folder.
        /// </summary>
        /// <param name="version">Version-object.</param>
        /// <param name="constantFolder">Path to constant folder.</param>
        private void InitConstantFolder(OpenFOAM.Version version, string constantFolder)
        {
            string transportProperties = Path.Combine(constantFolder, "transportProperties.");
            string g = Path.Combine(constantFolder, "g.");
            string turbulenceProperties = Path.Combine(constantFolder, "turbulenceProperties.");

            TransportProperties transportPropertiesDictionary = new TransportProperties(version, transportProperties, null, SaveFormat.ascii, m_Settings);
            G gDictionary = new G(version, g, null, SaveFormat.ascii, m_Settings);
            TurbulenceProperties turbulencePropertiesDictionary = new TurbulenceProperties(version, turbulenceProperties, null, SaveFormat.ascii, m_Settings);

            m_OpenFOAMDictionaries.Add(transportPropertiesDictionary);
            m_OpenFOAMDictionaries.Add(gDictionary);
            m_OpenFOAMDictionaries.Add(turbulencePropertiesDictionary);
        }

        /// <summary>
        /// Build a list that contains all elements of the specified category of the given document.
        /// </summary>
        /// <typeparam name="T">Specifies in which class instance should be seperated.</typeparam>
        /// <param name="doc">Active document.</param>
        /// <param name="category">BuiltInCategory from the Autodesk database.</param>
        /// <returns>List of elements with specified category instances.</returns>
        public static List<Element> GetDefaultCategoryListOfClass<T>(Document document, BuiltInCategory category)
        {
            FilteredElementCollector collector = new FilteredElementCollector(document);
            FilteredElementCollector catCollector = collector.OfCategory(category).OfClass(typeof(T));//.OfCategory(category);;
            return catCollector.ToList();
        }


        /// <summary>
        /// Get a list of ElemendId`s that represents the materials in the given element-list
        /// from the Document-Object document.
        /// </summary>
        /// <param name="doc">Document-object which will used for searching in.</param>
        /// <param name="elements">Element-List which will used for searching in.</param>
        /// <returns>List of ElementId's from the materials.</returns>
        public static List<ElementId> GetMaterialList(Document doc, List<Element> elements, List<string> materialNames)
        {
            List<ElementId> materialIds = new List<ElementId>();
            foreach (Element elem in elements)
            {
                ICollection<ElementId> materials = elem.GetMaterialIds(false);
                foreach (ElementId id in materials)
                {
                    Material material = doc.GetElement(id) as Material;
                    if (materialIds.Contains(id))
                    {
                        continue;
                    }

                    //coloring with materials differentiate from the the original materials which 
                    //will be listed in a component list (colored surface with other material)
                    foreach(string matName in materialNames)
                    {
                        if(material.Name.Equals(matName))
                        {
                            materialIds.Add(id);
                        }
                    }

                    //if(material.Name.Equals("Inlet") || material.Name.Equals("Outlet"))
                    //{
                    //    materialIds.Add(id);
                    //}
                }
            }

            return materialIds;
        }

        /// <summary>
        /// Save active Revit document as STL file according to customer's settings.
        /// </summary>
        /// <param name="fileName">The name of the STL file to be saved.</param>
        /// <param name="settings">Settings for save operation.</param>
        /// <returns>Successful or failed.</returns>      
        public GeneratorStatus SaveSTLFile(string fileName, Settings settings)
        {
            m_Settings = settings;
            ///***********************************************Easier to Debug without try and catch********************************************************************///
            ///*********************************************************************************************************************************///
            ///********************************************************************************************************************************///
            ///*******************************************************************************************************************************///
            try
            {

                m_StlExportCancel.Show();

                // save data in certain STL file
                if (SaveFormat.binary == settings.SaveFormat)
                {
                    m_Writer = new SaveDataAsBinary(fileName, settings.SaveFormat);
                }
                else
                {
                    m_Writer = new SaveDataAsAscII(fileName, settings.SaveFormat);
                }

                //TO-DO: create direct into constant/triSurface if simulation is selected.
                m_Writer.CreateFile();

                GeneratorStatus status = ScanElement(settings.ExportRange);

                Application.DoEvents();

                if(status != GeneratorStatus.SUCCESS)
                {
                    m_StlExportCancel.Close();
                    return status;
                }

                if (m_StlExportCancel.CancelProcess == true)
                {
                    m_StlExportCancel.Close();
                    return GeneratorStatus.CANCEL;
                }

                if (0 == m_TriangularNumber)
                {
                    MessageBox.Show(OpenFOAMExportResource.ERR_NOSOLID, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
                             MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    m_StlExportCancel.Close();
                    return GeneratorStatus.FAILURE;
                }

                if (SaveFormat.binary == settings.SaveFormat)
                {
                    // add triangular number to STL file
                    m_Writer.TriangularNumber = m_TriangularNumber;
                    m_Writer.AddTriangularNumberSection();
                }

                m_Writer.CloseFile();
            }
            catch (SecurityException)
            {
                MessageBox.Show(OpenFOAMExportResource.ERR_SECURITY_EXCEPTION, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                m_StlExportCancel.Close();
                return GeneratorStatus.FAILURE;
            }
            catch (Exception)
            {
                MessageBox.Show(OpenFOAMExportResource.ERR_EXCEPTION, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                m_StlExportCancel.Close();
                return GeneratorStatus.FAILURE;
            }

            m_StlExportCancel.Close();
            return GeneratorStatus.SUCCESS;
        }

        /// <summary>
        /// Scans all elements in the active document and creates a list of
        /// the categories of those elements.
        /// </summary>
        /// <returns>Sorted dictionary of categories.</returns>
        public SortedDictionary<string, Category> ScanCategories()
        {
            m_Categories = new SortedDictionary<string, Category>();

            // get all elements in the active document
            FilteredElementCollector filterCollector = new FilteredElementCollector(m_ActiveDocument);

            filterCollector.WhereElementIsNotElementType();

            FilteredElementIterator iterator = filterCollector.GetElementIterator();

            // create sorted dictionary of the categories of the elements
            while (iterator.MoveNext())
            {
                Element element = iterator.Current;
                if (element.Category != null)
                {
                    if (!m_Categories.ContainsKey(element.Category.Name))
                    {
                        m_Categories.Add(element.Category.Name, element.Category);
                    }
                }
            }

            return m_Categories;
        }

        /// <summary>
        /// Gets all categores in all open documents if allCategories is true
        /// or the categories of the elements in the active document if allCategories
        /// is set to false. 
        /// </summary>
        /// <param name="allCategories">True to get all categores in all open documents, 
        /// false to get the categories of the elements in the active document.</param>
        /// <returns>Sorted dictionary of categories.</returns>
        public SortedDictionary<string, Category> ScanCategories(bool allCategories)
        {
            if (!allCategories)
            {
                return ScanCategories();
            }
            else
            {
                // get and return all categories
                SortedDictionary<string, Category> sortedCategories = new SortedDictionary<string, Category>();

                // scan the active document for categories
                foreach (Category category in m_ActiveDocument.Settings.Categories)
                {
                    
                    if (!sortedCategories.ContainsKey(category.Name))
                        sortedCategories.Add(category.Name, category);
                }

                // if linked models exist scan for categories
                List<Document> linkedDocs = GetLinkedModels();

                foreach (Document linkedDoc in linkedDocs)
                {
                    foreach (Category category in linkedDoc.Settings.Categories)
                    {
                        if (!sortedCategories.ContainsKey(category.Name))
                            sortedCategories.Add(category.Name, category);
                    }
                }
                return sortedCategories;
            }
        }

        /// <summary>
        /// Get every Element in all open documents.
        /// </summary>
        /// <param name="exportRange">
        /// The range of elements to be exported.
        /// </param>
        private GeneratorStatus ScanElement(ElementsExportRange exportRange)
        {
            List<Document> documents = new List<Document>();
            GeneratorStatus status = GeneratorStatus.FAILURE;

            string pathSTL = m_Writer.FileName;
            string stlName = pathSTL.Substring(pathSTL.LastIndexOf("\\") + 1).Split('.')[0];
            m_STLName = stlName;

            string pathFolder = pathSTL.Remove(pathSTL.LastIndexOf("."));

            //contains all duct terminals lists of each document
            Dictionary<Document,List<Element>> terminalListOfAllDocuments = new Dictionary<Document, List<Element>>();

            // active document should be the first docuemnt in the list
            documents.Add(m_ActiveDocument);

            // figure out if we need to get linked models
            if (m_Settings.IncludeLinkedModels)
            {
                List<Document> linkedDocList = GetLinkedModels();
                documents.AddRange(linkedDocList);
            }

            if (m_Settings.OpenFOAM)
            {
                stlName = "wallSTL";
                m_STLWallName = stlName;
                m_LowerEdgeVector = new Vector3D(0, 0, 0);
                m_UpperEdgeVector = new Vector3D(0, 0, 0);
            }

            m_Writer.WriteSolidName(stlName, true);

            foreach (Document doc in documents)
            {
                //m_Writer.WriteSolidName("wall", true);
                FilteredElementCollector collector = null;

                if (ElementsExportRange.OnlyVisibleOnes == exportRange)
                {
                    // find the view having the same name of ActiveView.Name in active and linked model documents.
                    ElementId viewId = FindView(doc, m_ActiveView.Name);

                    if (viewId != ElementId.InvalidElementId)
                        collector = new FilteredElementCollector(doc, viewId);
                    else
                        collector = new FilteredElementCollector(doc);
                }
                else
                {
                    collector = new FilteredElementCollector(doc);
                }

                if(m_Settings.OpenFOAM)
                {
                    //get the category list seperated via FamilyInstance in the current document
                    m_DuctTerminalsInDoc = GetDefaultCategoryListOfClass<FamilyInstance>(doc, BuiltInCategory.OST_DuctTerminal);
                    m_InletOutletMaterials = GetMaterialList(doc, m_DuctTerminalsInDoc, new List<string> { "Inlet", "Outlet" });
                }

                collector.WhereElementIsNotElementType();

                //PrintOutElementNames(collector, doc);

                FilteredElementIterator iterator = collector.GetElementIterator();

                while (iterator.MoveNext())
                {
                    Application.DoEvents();

                    if (m_StlExportCancel.CancelProcess == true)
                        return GeneratorStatus.FAILURE;

                    //Element element = iterator.Current;
                    Element currentElement = iterator.Current;

                    // check if element's category is in the list, if it is continue.
                    // if there are no selected categories, take anything.
                    if (m_Settings.SelectedCategories.Count > 0)
                    {
                        if (currentElement.Category == null)
                        {
                            continue;
                        }
                        else
                        {
                            IEnumerable<Category> cats = from cat in m_Settings.SelectedCategories
                                                         where cat.Id == currentElement.Category.Id
                                                         select cat;

                            if (cats.Count() == 0)
                            {
                                continue;
                            }
                        }
                    }

                    // get the GeometryElement of the element
                    GeometryElement geometry = null;
                    geometry = currentElement.get_Geometry(m_ViewOptions);

                    if (null == geometry)
                    {
                        continue;
                    }

                    if(m_Settings.OpenFOAM)
                    {
                        if(IsGeometryInList(m_DuctTerminalsInDoc, geometry))
                        {
                            continue;
                        }
                        else if(IsGeometryInList(m_Settings.MeshResolution.Keys.ToList(), geometry))
                        {
                            continue;
                        }
                    }

                    // get the solids in GeometryElement
                    ScanGeomElement(doc, geometry, null);
                }
                terminalListOfAllDocuments.Add(doc, m_DuctTerminalsInDoc);
            }

            if (m_Settings.OpenFOAM && terminalListOfAllDocuments.Count != 0)
            {
                if(terminalListOfAllDocuments.Count != 0)
                {
                    WriteAirTerminalsToSTL(terminalListOfAllDocuments, stlName);
                    //m_Writer.CloseFile();
                    if (m_Settings.MeshResolution.Count != 0)
                    {
                        WriteMeshResolutionObjectsToSTL(stlName);
                        //m_Writer.CloseFile();
                    }
                    m_Writer.CloseFile();
                    status = CreateOpenFOAMCase(pathFolder);
                }
            }
            else
            {
                m_Writer.WriteSolidName(stlName, false);
                m_Writer.CloseFile();
                status = GeneratorStatus.SUCCESS;
            }
            return status;
        }

        /// <summary>
        /// Checks if the list contains the given geometry.
        /// </summary>
        /// <param name="list">List of Elements in which the method search for geometry.</param>
        /// <param name="geometry">GeomtryElement that the method is searching for.</param>
        /// <return>If true, geometry is in the list.</return></returns>
        private bool IsGeometryInList(List<Element> list, GeometryElement geometry)
        {
            foreach (Element elem in list)
            {
                GeometryElement geometryDuct = elem.get_Geometry(m_ViewOptions);
                if (geometryDuct.GraphicsStyleId == geometry.GraphicsStyleId)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Write the air terminals to STL and extract the inlet/outlet faces.
        /// </summary>
        /// <param name="dict">Contains the document as a key and the corresponding list of air terminals as value.</param>
        /// <param name="stlName">String that represents the name of the STL.</param>
        private void WriteAirTerminalsToSTL(Dictionary<Document, List<Element>> terminals, string stlName)
        {
            m_FacesInletOutlet = new Dictionary<KeyValuePair<string, Document>, KeyValuePair<Face,Transform>>();
            foreach (var elements in terminals)
            {
                foreach (Element elem in elements.Value)
                {
                    GeometryElement geometry = null;
                    geometry = elem.get_Geometry(m_ViewOptions);
                    if (null == geometry)
                    {
                        continue;
                    }
                    //FamilySymbol famSym = elements.Key.GetElement(elem.GetTypeId()) as FamilySymbol;
                    //KeyValuePair<string, Document> inletOutletID = new KeyValuePair<string, Document>(famSym.Family.Name + "_" + elem.Id.ToString(), elements.Key);
                    KeyValuePair<Face,Transform> inletOutlet = ExtractMaterialFaces/*ExtractInletOutlet*/(elements.Key, geometry, null, m_InletOutletMaterials);
                    if (inletOutlet.Key != null)
                    {
                        FamilySymbol famSym = elements.Key.GetElement(elem.GetTypeId()) as FamilySymbol;
                        KeyValuePair<string, Document> inletOutletID = new KeyValuePair<string, Document>(famSym.Family.Name + "_" + elem.Id.ToString(), elements.Key);
                        m_FacesInletOutlet.Add(inletOutletID, inletOutlet);
                    }
                }
            }

            m_Writer.WriteSolidName(stlName, false);

            if(m_FacesInletOutlet.Count == 0)
            {
                return;
            }
            foreach (var face in m_FacesInletOutlet)
            {
                Face currentFace = face.Value.Key;

                //face.Key.Key = Name + ID
                m_Writer.WriteSolidName(face.Key.Key,true);
                Mesh mesh = currentFace.Triangulate();
                if(mesh == null)
                {
                    continue;
                }

                //face.Key.Value = Document ; face.Value.Value = transform
                WriteFaceToSTL(face.Key.Value, mesh, currentFace, face.Value.Value);
                m_Writer.WriteSolidName(face.Key.Key, false);
            }
        }

        /// <summary>
        /// Write objects with parameter "Mesh Resolution" to stl.
        /// </summary>
        /// <param name="stlName">String that represents the name of the STL.</param>
        private void WriteMeshResolutionObjectsToSTL(string stlName)
        {
            foreach (var element in m_Settings.MeshResolution.Keys)
            {
                
                FamilyInstance instance = element as FamilyInstance;
                string name = instance.Symbol.Family.Name + "_" + instance.Name.Replace(' ','_') + "_" + element.Id;
                m_Writer.WriteSolidName(name, true);

                GeometryElement geometry = null;
                geometry = element.get_Geometry(m_ViewOptions);
                if (null == geometry)
                {
                    continue;
                }

                Solid solid = ExtractSolid(m_ActiveDocument, geometry, null);
                if(solid != null)
                {
                    FaceArray faces = solid.Faces;
                    foreach(Face face in faces)
                    {
                        //m_Writer.WriteSolidName(name, true);
                        Mesh mesh = face.Triangulate();
                        if (mesh == null)
                        {
                            continue;
                        }

                        WriteFaceToSTL(m_ActiveDocument, mesh, face, null);
                        //m_Writer.WriteSolidName(name, false);
                    }
                }
                m_Writer.WriteSolidName(name, false);
            }
        }

        ///// <summary>
        ///// Extract the inlet/outlet of the given geometry. Therefore the GeometryObject needs to be converted into Solid. 
        ///// </summary>
        ///// <param name="document">Current document in which the geometry is included.</param>
        ///// <param name="geometry">The geometry that contains the inlet/outlet.</param>
        ///// <param name="transform">Specifies the transformation of the geometry.</param>
        ///// <returns>KeyValuePair that contains the inlet/outlet as Face-object and the corresponding Transform.</returns>
        //private KeyValuePair<Face,Transform> ExtractInletOutlet(Document document, GeometryElement geometry, Transform transform)
        //{
        //    KeyValuePair<Face,Transform> face = new KeyValuePair<Face, Transform>();
        //    foreach (GeometryObject gObject in geometry)
        //    {
        //        Solid solid = gObject as Solid;
        //        if (null != solid)
        //        {
        //            KeyValuePair<Face, Transform> keyValuePair = new KeyValuePair<Face, Transform>(ScanForInletOutlet(document, solid, transform), transform);
        //            if (keyValuePair.Key != null)
        //            {
        //                face = keyValuePair;
        //                //remove break if duct terminals should be visible
        //                break;
        //            }
        //            continue;
        //        }

        //        // if the type of the geometric primitive is instance
        //        GeometryInstance instance = gObject as GeometryInstance;
        //        if (null != instance)
        //        {
        //            Transform newTransform;
        //            if (null == transform)
        //            {
        //                newTransform = instance.Transform;
        //            }
        //            else
        //            {
        //                newTransform = transform.Multiply(instance.Transform);  // get a transformation of the affine 3-space
        //            }
        //            face = ExtractInletOutlet(document, instance.SymbolGeometry, newTransform);
        //            break;
        //        }

        //        GeometryElement geomElement = gObject as GeometryElement;
        //        if (null != geomElement)
        //        {
        //            face = ExtractInletOutlet(document, geomElement, transform);
        //            break;
        //        }
        //    }
        //    return face;
        //}

        private KeyValuePair<Face, Transform> ExtractMaterialFaces(Document document, GeometryElement geometry,
            Transform transform, List<ElementId> materialList)
        {
            KeyValuePair<Face, Transform> face = new KeyValuePair<Face, Transform>();
            foreach (GeometryObject gObject in geometry)
            {
                Solid solid = gObject as Solid;
                if (null != solid)
                {
                    KeyValuePair<Face, Transform> keyValuePair = new KeyValuePair<Face, Transform>(ScanForMaterialFace(document, solid, transform, materialList), transform);
                    if (keyValuePair.Key != null)
                    {
                        face = keyValuePair;
                        break;
                    }
                    continue;
                }

                // if the type of the geometric primitive is instance
                GeometryInstance instance = gObject as GeometryInstance;
                if (null != instance)
                {
                    Transform newTransform;
                    if (null == transform)
                    {
                        newTransform = instance.Transform;
                    }
                    else
                    {
                        newTransform = transform.Multiply(instance.Transform);  // get a transformation of the affine 3-space
                    }
                    face = ExtractMaterialFaces(document, instance.SymbolGeometry, newTransform, materialList);
                    break;
                }

                GeometryElement geomElement = gObject as GeometryElement;
                if (null != geomElement)
                {
                    face = ExtractMaterialFaces(document, geomElement, transform, materialList);
                    break;
                }
            }
            return face;
        }

        /// <summary>
        /// Scans for Inlet/Outlet-face and returns it.
        /// </summary>
        /// <param name="document">Current cocument in which the geometry is included.</param>
        /// <param name="solid">Solid object that includes the inlet/outlet.</param>
        /// <param name="transform">The transformation.</param>
        /// <returns>Inlet/Outlet as a Face-object.</returns>
        private Face ScanForMaterialFace(Document document, Solid solid, Transform transform, List<ElementId> materialList)
        {
            Face faceItem = null;

            // a solid has many faces
            FaceArray faces = solid.Faces;
            if (0 == faces.Size)
            {
                return faceItem;
            }
            foreach (Face face in faces)
            {
                if (face == null)
                {
                    continue;
                }
                if (face.Visibility != Visibility.Visible)
                {
                    continue;
                }

                Mesh mesh = face.Triangulate();
                if (null == mesh)
                {
                    continue;
                }

                m_TriangularNumber += mesh.NumTriangles;
                if (materialList.Contains(face.MaterialElementId))
                {
                    faceItem = face;
                    continue;
                }

                //if face is not a inlet/outlet write it to the wall section of the stl.
                WriteFaceToSTL(document, mesh, face, transform);
            }
            return faceItem;//valuePair;
        }

        ///// <summary>
        ///// Scans for Inlet/Outlet-face and returns it.
        ///// </summary>
        ///// <param name="document">Current cocument in which the geometry is included.</param>
        ///// <param name="solid">Solid object that includes the inlet/outlet.</param>
        ///// <param name="transform">The transformation.</param>
        ///// <returns>Inlet/Outlet as a Face-object.</returns>
        //private Face ScanForInletOutlet(Document document, Solid solid, Transform transform)
        //{
        //    Face faceItem = null;

        //    // a solid has many faces
        //    FaceArray faces = solid.Faces;
        //    if (0 == faces.Size)
        //    {
        //        return faceItem;
        //    }
        //    foreach (Face face in faces)
        //    {
        //        if (face == null)
        //        {
        //            continue;
        //        }
        //        if (face.Visibility != Visibility.Visible)
        //        {
        //            continue;
        //        }

        //        Mesh mesh = face.Triangulate();
        //        if (null == mesh)
        //        {
        //            continue;
        //        }

        //        m_TriangularNumber += mesh.NumTriangles;
        //        if(m_InletOutletMaterials.Contains(face.MaterialElementId))
        //        {
        //            faceItem = face;
        //            continue;
        //        }

        //        //if face is not a inlet/outlet write it to the wall section of the stl.
        //        WriteFaceToSTL(document, mesh, face, transform);
        //    }
        //    return faceItem;//valuePair;
        //}


        /// <summary>
        /// Extract solid of the given geometry. Therefore the GeometryObject needs to be converted into Solid. 
        /// </summary>
        /// <param name="document">Current document in which the geometry is included.</param>
        /// <param name="geometry">The geometry that contains the inlet/outlet.</param>
        /// <param name="transform">Specifies the transformation of the geometry.</param>
        /// <returns>Solid.</returns>
        public static Solid ExtractSolid(Document document, GeometryElement geometry, Transform transform)
        {
            Solid value = default;
            foreach (GeometryObject gObject in geometry)
            {
                Solid solid = gObject as Solid;
                if (null != solid)
                {
                    return solid;
                }

                // if the type of the geometric primitive is instance
                GeometryInstance instance = gObject as GeometryInstance;
                if (null != instance)
                {
                    Transform newTransform;
                    if (null == transform)
                    {
                        newTransform = instance.Transform;
                    }
                    else
                    {
                        newTransform = transform.Multiply(instance.Transform);  // get a transformation of the affine 3-space
                    }
                    value = ExtractSolid(document, instance.SymbolGeometry, newTransform);
                    break;
                }

                GeometryElement geomElement = gObject as GeometryElement;
                if (null != geomElement)
                {
                    value = ExtractSolid(document, instance.SymbolGeometry, transform);
                    break;
                }
            }
            return value;
        }

        /// <summary>
        /// Get the faceNormal of the face from the solid that has a material from the list.
        /// </summary>
        /// <param name="materialIds">MaterialList that will be checked for.</param>
        /// <param name="faceNormal">Reference of the face normal.</param>
        /// <param name="solid">Solid that will be checked.</param>
        /// <returns>Face normal as XYZ object.</returns>
        public static Face GetFace(List<ElementId> materialIds, /*ref Face refFace*//*XYZ faceNormal*/ Solid solid)
        {
            // a solid has many faces
            FaceArray faces = solid.Faces;
            if (0 == faces.Size)
            {
                return null;
            }

            foreach (Face face in faces)
            {
                if (face == null)
                {
                    continue;
                }
                if (face.Visibility != Visibility.Visible)
                {
                    continue;
                }
                if (materialIds.Contains(face.MaterialElementId))
                {
                    return face;
                    //UV point = new UV();
                    //faceNormal = face.ComputeNormal(point);
                    //break;
                }
            }
            return null;
        }

        /// <summary>
        /// Get view by view name.
        /// </summary>
        /// <param name="doc">The document to find the view.</param>
        /// <param name="activeViewName">The view name.</param>
        /// <returns>The element id of the view found.</returns>
        private ElementId FindView(Document doc, string activeViewName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(RevitView));

            IEnumerable<Element> selectedView = from view in collector.ToList<Element>()
                                                where view.Name == activeViewName
                                                select view;

            if (selectedView.Count() > 0)
            {
                return (selectedView.First() as RevitView).Id;
            }

            return ElementId.InvalidElementId;
        }

        /// <summary>
        /// Scan GeometryElement to collect triangles.
        /// </summary>
        /// <param name="geometry">The geometry element.</param>
        /// <param name="trf">The transformation.</param>
        private void ScanGeomElement(Document document, GeometryElement geometry, Transform transform)
        {
            //get all geometric primitives contained in the GeometryElement
            foreach (GeometryObject gObject in geometry)
            {
                // if the type of the geometric primitive is Solid
                Solid solid = gObject as Solid;
                if (null != solid)
                {
                    ScanSolid(document, solid, transform);
                    continue;
                }

                // if the type of the geometric primitive is instance
                GeometryInstance instance = gObject as GeometryInstance;
                if (null != instance)
                {
                    ScanGeometryInstance(document, instance, transform);
                    //ScanGeomElement
                    continue;
                }

                GeometryElement geomElement = gObject as GeometryElement;
                if (null != geomElement)
                {
                    ScanGeomElement(document, geomElement, transform);
                }
            }
        }

        /// <summary>
        /// Scan GeometryInstance to collect triangles.
        /// </summary>
        /// <param name="instance">The geometry instance.</param>
        /// <param name="trf">The transformation.</param>
        private void ScanGeometryInstance(Document document, GeometryInstance instance, Transform transform)
        {
            GeometryElement instanceGeometry = instance.SymbolGeometry;
            if (null == instanceGeometry)
            {
                return;
            }
            Transform newTransform;
            if (null == transform)
            {
                newTransform = instance.Transform;
            }
            else
            {
                newTransform = transform.Multiply(instance.Transform);	// get a transformation of the affine 3-space
            }
            // get all geometric primitives contained in the GeometryElement
            ScanGeomElement(document, instanceGeometry, newTransform);
        }

        /// <summary>
        /// Scan Solid to collect triangles.
        /// </summary>
        /// <param name="solid">The solid.</param>
        /// <param name="trf">The transformation.</param>
        private void ScanSolid(Document document, Solid solid, Transform transform)
        {
            GetTriangular(document, solid, transform);	// get triangles in the solid
        }

        /// <summary>
        /// Get triangles in a solid with transform.
        /// </summary>
        /// <param name="solid">The solid contains triangulars</param>
        /// <param name="transform">The transformation.</param>
        private void GetTriangular(Document document, Solid solid, Transform transform)
        {
            // a solid has many faces
            FaceArray faces = solid.Faces;
            //bool hasTransform = (null != transform);
            if (0 == faces.Size)
            {
                return;
            }

            foreach (Face face in faces)
            {
                if (face.Visibility != Visibility.Visible)
                {
                    continue;
                }
                Mesh mesh = face.Triangulate();
                if (null == mesh)
                {
                    continue;
                }
                m_TriangularNumber += mesh.NumTriangles;
                // write face to stl file
                // a face has a mesh, all meshes are made of triangles
                WriteFaceToSTL(document, mesh, face, transform);
            }
        }

        /// <summary>
        /// Write face to stl file, a face has a mesh, all meshes are made of triangles.
        /// </summary>
        /// <param name="document">Document in which the mesh and face is included.</param>
        /// <param name="mesh">Mesh of the face.</param>
        /// <param name="face">Face which the method writes to stl.</param>
        /// <param name="transform">Specifies transformation of the face.</param>
        private void WriteFaceToSTL(Document document, Mesh mesh, Face face, Transform transform)
        {
            bool hasTransform = (null != transform);
            for (int ii = 0; ii < mesh.NumTriangles; ii++)
            {
                MeshTriangle triangular = mesh.get_Triangle(ii);
                double[] xyz = new double[9];
                XYZ normal = new XYZ();
                try
                {
                    XYZ[] triPnts = new XYZ[3];
                    for (int n = 0; n < 3; ++n)
                    {
                        double x, y, z;
                        XYZ point = triangular.get_Vertex(n);
                        if (hasTransform)
                        {
                            point = transform.OfPoint(point);
                        }
                        if (m_Settings.ExportSharedCoordinates)
                        {
                            ProjectPosition ps = document.ActiveProjectLocation.GetProjectPosition(point);
                            x = ps.EastWest;
                            y = ps.NorthSouth;
                            z = ps.Elevation;
                        }
                        else
                        {
                            x = point.X;
                            y = point.Y;
                            z = point.Z;
                        }
                        if (m_Settings.Units != DisplayUnitType.DUT_UNDEFINED)
                        {
                            xyz[3 * n] = UnitUtils.ConvertFromInternalUnits(x, m_Settings.Units);
                            xyz[3 * n + 1] = UnitUtils.ConvertFromInternalUnits(y, m_Settings.Units);
                            xyz[3 * n + 2] = UnitUtils.ConvertFromInternalUnits(z, m_Settings.Units);
                        }
                        else
                        {
                            xyz[3 * n] = x;
                            xyz[3 * n + 1] = y;
                            xyz[3 * n + 2] = z;
                        }

                        var mypoint = new XYZ(xyz[3 * n], xyz[3 * n + 1], xyz[3 * n + 2]);
                        IsEdgeVectorForBoundary(mypoint);
                        triPnts[n] = mypoint;
                    }

                    XYZ pnt1 = triPnts[1] - triPnts[0];
                    normal = pnt1.CrossProduct(triPnts[2] - triPnts[1]);
                }
                catch (Exception ex)
                {
                    m_TriangularNumber--;
                    OpenFOAMDialogManager.ShowDebug(ex.Message);
                    continue;
                }

                if (m_Writer is SaveDataAsBinary && m_Settings.ExportColor)
                {
                    Material material = document.GetElement(face.MaterialElementId) as Material;
                    if (material != null)
                        ((SaveDataAsBinary)m_Writer).Color = material.Color;
                }
                m_Writer.WriteSection(normal, xyz);

            }
        }

        /// <summary>
        /// Checks if the given vector is bigger or smaller than m_LowerEdgeVector and m_UpperEdgeVector.
        /// </summary>
        /// <param name="xyz">Point in 3d-Space.</param>
        private void IsEdgeVectorForBoundary(XYZ xyz)
        {
            if(xyz.X < m_LowerEdgeVector.X)
            {
                m_LowerEdgeVector.X = xyz.X;
            }
            else if (xyz.Y < m_LowerEdgeVector.Y)
            {
                m_LowerEdgeVector.Y = xyz.Y;
            }
            else if (xyz.Z < m_LowerEdgeVector.Z)
            {
                m_LowerEdgeVector.Z = xyz.Z;
            }

            if(xyz.X > m_UpperEdgeVector.X)
            {
                m_UpperEdgeVector.X = xyz.X;
            }
            else if(xyz.Y > m_UpperEdgeVector.Y)
            {
                m_UpperEdgeVector.Y = xyz.Y;
            }
            else if(xyz.Z > m_UpperEdgeVector.Z)
            {
                m_UpperEdgeVector.Z = xyz.Z;
            }
        }

        /// <summary>
        /// Scans and returns the documents linked to the current model.
        /// </summary>
        /// <returns>List of linked documents.</returns>
        private List<Document> GetLinkedModels()
        {
            List<Document> linkedDocs = new List<Document>();

            try
            {
                // scan the current model looking for Revit links
                List<Element> linkedElements = FindLinkedModelElements();

                foreach (Element linkedElem in linkedElements)
                {
                    RevitLinkType linkType = linkedElem as RevitLinkType;

                    if (linkType != null)
                    {
                        // now look that up in the open documents
                        foreach (Document openedDoc in m_RevitApp.Documents)
                        {
                            if (Path.GetFileNameWithoutExtension(openedDoc.Title).ToUpper() == Path.GetFileNameWithoutExtension(linkType.Name).ToUpper())
                                linkedDocs.Add(openedDoc);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OpenFOAMDialogManager.ShowDebug(ex.Message);
            }

            return linkedDocs;

        }

        /// <summary>
        /// Scan model and return linked model elements.
        /// </summary>
        /// <returns>List of linked model elements.</returns>
        private List<Element> FindLinkedModelElements()
        {
            Document doc = m_ActiveDocument;

            FilteredElementCollector linksCollector = new FilteredElementCollector(doc);
            List<Element> linkElements = linksCollector.WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_RvtLinks)).ToList<Element>();

            FilteredElementCollector familySymbolCollector = new FilteredElementCollector(doc);
            linkElements.AddRange(familySymbolCollector.OfClass(typeof(Autodesk.Revit.DB.FamilySymbol)).ToList<Element>());

            return linkElements;
        }

        /// <summary>
        /// Convert-Assist-Function.
        /// </summary>
        /// <typeparam name="T">Type value will be converted in.</typeparam>
        /// <typeparam name="U">Initial type</typeparam>
        /// <param name="value">Value to convert.</param>
        /// <returns>Converted value.</returns>
        public static T ConvertValue<T,U>(U value)/* where U : IConvertible*/
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
