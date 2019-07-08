//
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
using BIM.OpenFoamExport.OpenFOAM;
using Material = Autodesk.Revit.DB.Material;

namespace BIM.OpenFoamExport
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
        /// Name of the STL
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
        private GeometryOptions m_ViewOptions;

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
        private STLExportCancelForm m_StlExportCancel = new STLExportCancelForm();

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
        private List<FoamDict> openFOAMDictionaries;

        /// <summary>
        /// Faces of the inlet/outlet for openFOAM-Simulation
        /// </summary>
        private Dictionary<KeyValuePair<string, Document>, KeyValuePair<Face, Transform>> faces;

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
        private void InitRunManager(string casePath)
        {
            if (m_Settings.OpenFOAMEnvironment == OpenFOAMEnvironment.blueCFD)
            {
                m_RunManager = new RunManagerBlueCFD(casePath/*, @"C:\Program FIles\blueCFD-Core-2017\setvars.bat"*/);
            }
            else if(m_Settings.OpenFOAMEnvironment == OpenFOAMEnvironment.docker)
            {
                m_RunManager = new RunManagerDocker(casePath);
            }
            else if(m_Settings.OpenFOAMEnvironment == OpenFOAMEnvironment.linux)
            {
                m_RunManager = new RunManagerLinux(casePath);
            }
        }

    /// <summary>
    /// Create OpenFOAM-Folder at given path.
    /// </summary>
    /// <param name="path">location for the OpenFOAM-folder</param>
    private void CreateOpenFOAMCase(string path)
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

            InitRunManager(path);

            //.foam-File
            File.Create(path + "\\" + m_STLName + ".foam");

            //move .stl from given location into triSurface folder TO-DO: Not Moving => Generate
            File.Move(m_Writer.FileName, triSurface + "\\" + m_STLName + ".stl");
            File.Delete(m_Writer.FileName);

            //generate files
            OpenFOAM.Version version = new OpenFOAM.Version();
            openFOAMDictionaries = new List<FoamDict>();

            //Init folders
            InitSystemFolder(version, system);
            InitNullFolder(version, nullFolder);
            InitConstantFolder(version, constant);

            foreach (FoamDict openFOAMDictionary in openFOAMDictionaries)
            {
                openFOAMDictionary.Init();
            }

            //commands as string
            List<string> commands = new List<string> { "blockMesh", "surfaceFeatureExtract",  "snappyHexMesh" , "rm -r processor*", "simpleFoam"};

            //run commands in windows-openfoam-environment
            m_RunManager.RunCommands(commands);
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
            SnappyHexMeshDict snappyHexMeshDictionary = new SnappyHexMeshDict(version, meshDict, null, SaveFormat.ascii, m_Settings, m_STLName, m_STLWallName, faces);

            //runmanager have to know how much cpu's should be used
            m_RunManager.DecomposeParDict = decomposeParDictionary;

            openFOAMDictionaries.Add(blockMeshDictionary);
            openFOAMDictionaries.Add(controlDictionary);
            openFOAMDictionaries.Add(surfaceFeatureExtractDictionary);
            openFOAMDictionaries.Add(decomposeParDictionary);
            openFOAMDictionaries.Add(fvSchemesDictionary);
            openFOAMDictionaries.Add(fvSolutionDictionary);
            openFOAMDictionaries.Add(snappyHexMeshDictionary);
        }

        /// <summary>
        /// Creates all dictionaries in the nullfolder.
        /// </summary>
        /// <param name="version">Current version-object</param>
        /// <param name="nullFolder">Path to nullfolder.</param>
        private void InitNullFolder(OpenFOAM.Version version, string nullFolder)
        {
            //Files in nullfolder
            string u = Path.Combine(nullFolder, "U.");
            string epsilon = Path.Combine(nullFolder, "epsilon.");
            string k = Path.Combine(nullFolder, "k.");
            string nut = Path.Combine(nullFolder, "nut.");
            string p = Path.Combine(nullFolder, "p.");

            //Extract inlet/outlet-names
            List<string> inletNames = new List<string>();
            List<string> outletNames = new List<string>();
            string name = string.Empty;
            foreach (var face in faces)
            {
                name = face.Key.Key.Replace(" ", "_");
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
            U uDict = new U(version, u, null, SaveFormat.ascii, m_Settings, "wall", inletNames, outletNames);
            P pDict = new P(version, p, null, SaveFormat.ascii, m_Settings, "wall", inletNames, outletNames);
            Epsilon epsilonDict = new Epsilon(version, epsilon, null, SaveFormat.ascii, m_Settings, "wall", inletNames, outletNames);
            Nut nutDict = new Nut(version, nut, null, SaveFormat.ascii, m_Settings, "wall", inletNames, outletNames);
            K kDict = new K(version, k, null, SaveFormat.ascii, m_Settings, "wall", inletNames, outletNames);

            openFOAMDictionaries.Add(uDict);
            openFOAMDictionaries.Add(pDict);
            openFOAMDictionaries.Add(epsilonDict);
            openFOAMDictionaries.Add(nutDict);
            openFOAMDictionaries.Add(kDict);
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

            openFOAMDictionaries.Add(transportPropertiesDictionary);
            openFOAMDictionaries.Add(gDictionary);
            openFOAMDictionaries.Add(turbulencePropertiesDictionary);
        }

        /// <summary>
        /// Build a list that contains all elements of the specified category of the given document.
        /// </summary>
        /// <typeparam name="T">Specifies in which class instance should be seperated.</typeparam>
        /// <param name="doc">Active document.</param>
        /// <param name="category">BuiltInCategory from the Autodesk database.</param>
        /// <returns>List of elements with specified category instances.</returns>
        private List<Element> GetDefaultCategoryListOfClass<T>(Document document, BuiltInCategory category)
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
        private List<ElementId> GetMaterialList(Document doc, List<Element> elements)
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
                    //materials differentiate from the the original materials which 
                    //will be listed in a component list (colored surface with other material)
                    if(material.Name.Equals("Inlet") || material.Name.Equals("Outlet"))
                    {
                        materialIds.Add(id);
                    }
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
            ///***********************************************PLS INSERT LATER********************************************************************///
            ///*********************************************************************************************************************************///
            ///********************************************************************************************************************************///
            ///*******************************************************************************************************************************///
            //try
            //{

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

                m_Writer.CreateFile();
                ScanElement(settings.ExportRange);

                Application.DoEvents();

                if (m_StlExportCancel.CancelProcess == true)
                {
                    m_StlExportCancel.Close();
                    return GeneratorStatus.CANCEL;
                }

                if (0 == m_TriangularNumber)
                {
                    MessageBox.Show(OpenFoamExportResource.ERR_NOSOLID, OpenFoamExportResource.MESSAGE_BOX_TITLE,
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
                //m_Writer.CloseFile();
            //}
            //catch (SecurityException)
            //{
            //    MessageBox.Show(OpenFoamExportResource.ERR_SECURITY_EXCEPTION, OpenFoamExportResource.MESSAGE_BOX_TITLE,
            //                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            //    m_StlExportCancel.Close();
            //    return GeneratorStatus.FAILURE;
            //}
            //catch (Exception)
            //{
            //    MessageBox.Show(OpenFoamExportResource.ERR_EXCEPTION, OpenFoamExportResource.MESSAGE_BOX_TITLE,
            //                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            //    m_StlExportCancel.Close();
            //    return GeneratorStatus.FAILURE;
            //}

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
        private void ScanElement(ElementsExportRange exportRange)
        {
            List<Document> documents = new List<Document>();

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
                    m_InletOutletMaterials = GetMaterialList(doc, m_DuctTerminalsInDoc);
                }

                collector.WhereElementIsNotElementType();

                //PrintOutElementNames(collector, doc);

                FilteredElementIterator iterator = collector.GetElementIterator();

                while (iterator.MoveNext())
                {
                    System.Windows.Forms.Application.DoEvents();

                    if (m_StlExportCancel.CancelProcess == true)
                        return;

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
                        if(IsGeometryInList(m_DuctTerminalsInDoc,geometry))
                        {
                            continue;
                        }
                    }

                    // get the solids in GeometryElement
                    ScanGeomElement(doc,geometry, null);
                }
                terminalListOfAllDocuments.Add(doc,m_DuctTerminalsInDoc);
            }

            if (m_Settings.OpenFOAM && terminalListOfAllDocuments.Count != 0)
            {
                WriteAirTerminalsToSTL(terminalListOfAllDocuments, stlName);
                m_Writer.CloseFile();
                CreateOpenFOAMCase(pathFolder);
            }
            else
            {
                m_Writer.WriteSolidName(stlName, false);
                m_Writer.CloseFile();
            }
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
            faces = new Dictionary<KeyValuePair<string, Document>, KeyValuePair<Face,Transform>>();
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
                    KeyValuePair<Face,Transform> inletOutlet = ExtractInletOutlet(elements.Key, geometry, null);
                    if (inletOutlet.Key != null)
                    {
                        FamilySymbol famSym = elements.Key.GetElement(elem.GetTypeId()) as FamilySymbol;
                        KeyValuePair<string, Document> inletOutletID = new KeyValuePair<string, Document>(famSym.Family.Name + "_" + elem.Id.ToString(), elements.Key);
                        faces.Add(inletOutletID, inletOutlet);
                    }
                }
            }

            m_Writer.WriteSolidName(stlName, false);

            if(faces.Count == 0)
            {
                return;
            }
            foreach (var face in faces)
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
        /// Extract the inlet/outlet of the given geometry. Therefore the GeometryObject needs to be converted into Solid. 
        /// </summary>
        /// <param name="document">Current document in which the geometry is included.</param>
        /// <param name="geometry">The geometry that contains the inlet/outlet.</param>
        /// <param name="transform">Specifies the transformation of the geometry.</param>
        /// <returns>KeyValuePair that contains the inlet/outlet as Face-object and the corresponding Transform.</returns>
        private KeyValuePair<Face,Transform> ExtractInletOutlet(Document document, GeometryElement geometry, Transform transform)
        {
            KeyValuePair<Face,Transform> face = new KeyValuePair<Face, Transform>();
            foreach (GeometryObject gObject in geometry)
            {
                Solid solid = gObject as Solid;
                if (null != solid)
                {
                    KeyValuePair<Face, Transform> keyValuePair = new KeyValuePair<Face, Transform>(ScanForInletOutlet(document, solid, transform), transform);
                    if(keyValuePair.Key != null)
                    {
                        face = keyValuePair;
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
                    face = ExtractInletOutlet(document, instance.SymbolGeometry, newTransform);
                    break;
                }

                GeometryElement geomElement = gObject as GeometryElement;
                if (null != geomElement)
                {
                    face = ExtractInletOutlet(document, geomElement, transform);
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
        private Face ScanForInletOutlet(Document document, Solid solid, Transform transform)
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
                if(m_InletOutletMaterials.Contains(face.MaterialElementId))
                {
                    faceItem = face;
                    continue;
                }

                //if face is not a inlet/outlet write it to the wall section of the stl.
                WriteFaceToSTL(document, mesh, face, transform);
            }
            return faceItem;//valuePair;
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
            //return true;
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
            //func(document, instanceGeometry, newTransform);
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
                PlanarFace planarFace = face as PlanarFace;
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
                Autodesk.Revit.DB.XYZ normal = new Autodesk.Revit.DB.XYZ();
                try
                {
                    Autodesk.Revit.DB.XYZ[] triPnts = new Autodesk.Revit.DB.XYZ[3];
                    for (int n = 0; n < 3; ++n)
                    {
                        double x, y, z;
                        Autodesk.Revit.DB.XYZ point = triangular.get_Vertex(n);
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

                    Autodesk.Revit.DB.XYZ pnt1 = triPnts[1] - triPnts[0];
                    normal = pnt1.CrossProduct(triPnts[2] - triPnts[1]);
                }
                catch (Exception ex)
                {
                    m_TriangularNumber--;
                    OpenFoamDialogManager.ShowDebug(ex.Message);
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
                OpenFoamDialogManager.ShowDebug(ex.Message);
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
        /// Initializes the Cancel form.
        /// </summary>
        private void StartCancelForm()
        {
            STLExportCancelForm stlCancel = new STLExportCancelForm();
            stlCancel.Show();
        }
    }
}