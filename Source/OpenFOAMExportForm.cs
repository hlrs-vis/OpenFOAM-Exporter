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
using System.Collections.Generic;
using System.Drawing;
using BIM.OpenFOAMExport.OpenFOAMUI;
using System.Windows.Forms;

using Category = Autodesk.Revit.DB.Category;
using Autodesk.Revit.DB;
using System.Text.RegularExpressions;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System.IO;

namespace BIM.OpenFOAMExport
{
    public partial class OpenFOAMExportForm : System.Windows.Forms.Form
    {
        /// <summary>
        /// DataGenerator-object.
        /// </summary>
        private DataGenerator m_Generator = null;

        /// <summary>
        /// Active document.
        /// </summary>
        private Document m_ActiveDocument;

        /// <summary>
        /// Element sphere in scene.
        /// </summary>
        private ElementId m_SphereLocationInMesh;

        /// <summary>
        /// Current duct terminals in scene of the active document.
        /// </summary>
        private List<Element> m_DuctTerminals;
        
        /// <summary>
        /// Current inlet/outlet material that specify the surfaces.
        /// </summary>
        private List<ElementId> m_InletOutletMaterials;

        /// <summary>
        /// For click events.
        /// </summary>
        private bool m_Clicked = false;

        /// <summary>
        /// For changing events.
        /// </summary>
        private bool m_Changed = false;

        /// <summary>
        /// Sorted dictionary for the category-TreeView.
        /// </summary>
        private SortedDictionary<string, Category> m_CategoryList = new SortedDictionary<string, Category>();

        /// <summary>
        /// OpenFOAM-TreeView for default simulation parameter
        /// </summary>
        private OpenFOAMTreeView m_OpenFOAMTreeView;

        /// <summary>
        /// Sorted dictionary for the unity properties that can be set in a drop down menu.
        /// </summary>
        private readonly SortedDictionary<string, DisplayUnitType> m_DisplayUnits = new SortedDictionary<string, DisplayUnitType>();

        /// <summary>
        /// Selected Unittype in comboBox.
        /// </summary>
        private static DisplayUnitType m_SelectedDUT = DisplayUnitType.DUT_UNDEFINED;

        /// <summary>
        /// Project-Settings.
        /// </summary>
        private Settings m_Settings;

        /// <summary>
        /// Revit-App.
        /// </summary>
        private UIApplication m_Revit = null;

        /// <summary>
        /// String regular expression string for a vector entry.
        /// </summary>
        private const string m_Entry = @"(-?\d*\.)?(-?\d+)";

        /// <summary>
        /// Regular expression for 3 dim vector.
        /// </summary>
        private Regex m_LocationReg = new Regex("^" + m_Entry + "\\s+" + m_Entry + "\\s+" + m_Entry + "$");

        /// <summary>
        /// Regular Expression for txtBoxUserIP.
        /// </summary>
        private readonly Regex m_RegUserIP = new Regex("^\\S+@\\S+$");

        /// <summary>
        /// Regular Expression for txtBoxServerCaseFolder.
        /// </summary>
        private readonly Regex m_RegServerCasePath = new Regex("^\\S+$");

        /// <summary>
        /// Regular Expression for txtBoxPort.
        /// </summary>
        private readonly Regex m_RegPort = new Regex("^\\d+$");

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="revit">The revit application.</param>
        public OpenFOAMExportForm(UIApplication revit)
        {
            InitializeComponent();

            m_OpenFOAMTreeView = new OpenFOAMTreeView();
            m_Revit = revit;

            //needs to be implemented cause of non-modal window.
            FormClosed += OpenFOAMExportForm_FormClosed;
            m_Revit.ViewActivating += Revit_ViewActivating;            
            
            m_ActiveDocument = m_Revit.ActiveUIDocument.Document;
            tbSSH.Enabled = false;
            tbOpenFOAM.Enabled = false;

            //set size for OpenFOAMTreeView
            InitializeOFTreeViewSize();

            // get new data generator
            m_Generator = new DataGenerator(m_Revit.Application, m_Revit.ActiveUIDocument.Document, m_Revit.ActiveUIDocument.Document.ActiveView);

            //add OpenFOAMTreeView to container default
            gbDefault.Controls.Add(m_OpenFOAMTreeView);

            // scan for categories to populate category list
            InitializeCategoryList();

            //intialize unit
            InitializeUnitsForSTL();

            // initialize the UI differently for Families
            if (revit.ActiveUIDocument.Document.IsFamilyDocument)
            {
                cbIncludeLinked.Enabled = false;
                tvCategories.Enabled = false;
                btnCheckAll.Enabled = false;
                btnCheckNone.Enabled = false;
            }

            // initialize settings
            InitializeSettings();

            // initialize openfoam treeview openfoam
            InitializeDefaultParameterOpenFOAM();

            InitializeComboBoxes();

            // textBoxCPU
            textBoxCPU.Text = m_Settings.NumberOfSubdomains.ToString();
            //To-Do: ADD EVENT FOR CHANGING TEXT

            // textBoxLocationInMesh
            txtBoxLocationInMesh.Text = m_Settings.LocationInMesh.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")).Replace(",", " ");

            //initialize SSH
            InitializeSSH();
        }

        /// <summary>
        /// Initializes the comboBoxes of the OpenFOAM-Tab.
        /// </summary>
        private void InitializeComboBoxes()
        {
            // comboBoxEnv
            var enumEnv = OpenFOAMEnvironment.blueCFD;
            foreach (var value in Enum.GetValues(enumEnv.GetType()))
            {
                comboBoxEnv.Items.Add(value);
            }
            comboBoxEnv.SelectedItem = enumEnv;

            // comboBoxSolver
            var enumSolver = SolverControlDict.buoyantBoussinesqSimpleFoam;
            comboBoxSolver.Items.Add(enumSolver);
            comboBoxSolver.Items.Add(SolverControlDict.simpleFoam);

            //Not all solver are implemented yet.
            //foreach (var value in Enum.GetValues(enumSolver.GetType()))
            //{
            //    comboBoxSolver.Items.Add(value);
            //}

            comboBoxSolver.SelectedItem = enumSolver;

            // comboBoxTransportModel
            var enumTransport = TransportModel.Newtonian;
            foreach (var value in Enum.GetValues(enumTransport.GetType()))
            {
                comboBoxTransportModel.Items.Add(value);
            }
            comboBoxTransportModel.SelectedItem = enumTransport;
        }

        /// <summary>
        /// Initialize settings.
        /// </summary>
        private void InitializeSettings()
        {
            SaveFormat saveFormat;
            if (rbBinary.Checked)
            {
                saveFormat = SaveFormat.binary;
            }
            else
            {
                saveFormat = SaveFormat.ascii;
            }

            ElementsExportRange exportRange;

            exportRange = ElementsExportRange.OnlyVisibleOnes;

            // get selected categories from the category list
            List<Category> selectedCategories = new List<Category>();

            // only for projects
            if (m_Revit.ActiveUIDocument.Document.IsFamilyDocument == false)
            {
                foreach (TreeNode treeNode in tvCategories.Nodes)
                {
                    AddSelectedTreeNode(treeNode, selectedCategories);
                }
            }

            DisplayUnitType dup = m_DisplayUnits[comboBox_DUT.Text];

            saveFormat = SaveFormat.ascii;

            // create settings object to save setting information
            m_Settings = new Settings(saveFormat, exportRange, cbOpenFOAM.Checked, cbIncludeLinked.Checked, cbExportColor.Checked, cbExportSharedCoordinates.Checked,
                false, 0, 100, 1, 100, 0, 6, 6, 4, selectedCategories, dup);
            if (!InitBIMData())
            {
                return;
            }
            m_Settings.InitOpenFOAMFolderDictionaries();
        }

        /// <summary>
        /// Initialize air flow velocity in settings for each duct terminals based on BIM-Data.
        /// </summary>
        /// <returns>true, if BIMData used.</returns>
        private bool InitBIMData()
        {
            if (m_Settings == null)
            {
                return false;
            }

            //get duct-terminals in active document
            m_DuctTerminals = DataGenerator.GetDefaultCategoryListOfClass<FamilyInstance>(m_Revit.ActiveUIDocument.Document, BuiltInCategory.OST_DuctTerminal);

            //get materials
            m_InletOutletMaterials = DataGenerator.GetMaterialList(m_ActiveDocument, m_DuctTerminals);

            return InitAirFlowVelocity();
        }

        /// <summary>
        /// Searches for inlet/outlet to compute airflow velocity based on airflow volume and surface area.
        /// </summary>
        /// <returns>True, if there is no error while computing.</returns>
        private bool InitAirFlowVelocity()
        {
            bool succeed = false;
            foreach (FamilyInstance instance in m_DuctTerminals)
            {
                double paramValue = 0;
                double surfaceArea = /*GetSurfaceArea(instance);*/ Math.Round(GetSurfaceParameter(instance, GetFaceArea), 2);
                foreach (Parameter param in instance.Parameters)
                {
                    paramValue = 0;
                    try
                    {
                        paramValue = ComputeAirFlowValue(paramValue, surfaceArea, param);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(OpenFOAMExportResource.ERR_FORMAT, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }

                    if (paramValue != 0)
                    {
                        FamilySymbol famSym = instance.Symbol;

                        //get FaceNormal
                        XYZ faceNormal = /*GetOrientationInletOutlet(instance);*/ GetSurfaceParameter(instance, GetFaceNormal);
                        double faceBoundary = GetSurfaceParameter(instance, GetFaceBoundary);

                        string nameDuctFam = famSym.Family.Name + "_" + instance.Id.ToString();
                        string nameDuct = nameDuctFam.Replace(" ", "_");
                        if (nameDuct.Contains("Abluft") || nameDuct.Contains("Outlet"))
                        {
                            //negate faceNormal = outlet.
                            SurfaceProperties sProp = new SurfaceProperties
                            {
                                Area = surfaceArea,
                                Boundary = faceBoundary,
                                FaceNormal = faceNormal,
                                MeanFlowVelocity = /*new System.Windows.Media.Media3D.Vector3D(faceNormal.X, faceNormal.Y, faceNormal.Z) * (*/-paramValue/*)*/
                            };
                            m_Settings.Outlet.Add(nameDuct, sProp/*new System.Windows.Media.Media3D.Vector3D(faceNormal.X, faceNormal.Y, faceNormal.Z) * (-paramValue)*/);
                            succeed = true;
                        }
                        else if (nameDuct.Contains("Zuluft") || nameDuct.Contains("Inlet"))
                        {
                            SurfaceProperties sProp = new SurfaceProperties
                            {
                                Area = surfaceArea,
                                Boundary = faceBoundary,
                                FaceNormal = faceNormal,
                                MeanFlowVelocity = /*new System.Windows.Media.Media3D.Vector3D(faceNormal.X, faceNormal.Y, faceNormal.Z) * */paramValue
                            };
                            m_Settings.Inlet.Add(nameDuct, sProp/*new System.Windows.Media.Media3D.Vector3D(faceNormal.X, faceNormal.Y, faceNormal.Z) * paramValue*/);
                            succeed = true;
                        }
                        break;
                    }
                }
            }
            return succeed;
        }

        ///// <summary>
        ///// Search for inlet/outlet face and return the surfaceArea.
        ///// </summary>
        ///// <param name="instance">FamilyInstance for searching inlet/outlet face.</param>
        ///// <returns>Surfacearea as double.</returns>
        //private double GetSurfaceArea(FamilyInstance instance)
        //{
        //    double area = 0;
        //    var m_ViewOptions = m_Revit.Application.Create.NewGeometryOptions();
        //    m_ViewOptions.View = m_Revit.ActiveUIDocument.Document.ActiveView;

        //    var geometry = instance.get_Geometry(m_ViewOptions);
        //    Solid solid = DataGenerator.ExtractSolid(m_ActiveDocument, geometry, null);
        //    if (solid == default)
        //        return area;

        //    Face face = DataGenerator.GetFace(m_InletOutletMaterials, solid);
        //    if(face != null)
        //    {
        //        var edges = face.EdgeLoops;
        //        double boundary = 0;
        //        if (!edges.IsEmpty)
        //        {
        //            foreach (Edge edge in edges.get_Item(0) as EdgeArray)
        //            {
        //                boundary += UnitUtils.ConvertFromInternalUnits(edge.ApproximateLength, DisplayUnitType.DUT_METERS);
        //            }
        //        }

        //        //convert from imperial unit system to metric
        //        area = UnitUtils.ConvertFromInternalUnits(face.Area, DisplayUnitType.DUT_SQUARE_METERS);
        //    }
        //    return area;
        //}

        /// <summary>
        /// Get surface parameter based on given Face-Function. 
        /// </summary>
        /// <typeparam name="T">return type.</typeparam>
        /// <param name="instance">Familyinstance.</param>
        /// <param name="func">Face-function/methode.</param>
        /// <returns>Parameter as type T.</returns>
        private T GetSurfaceParameter<T>(FamilyInstance instance, Func<Face, T> func)
        {
            T value = default(T);
            var m_ViewOptions = m_Revit.Application.Create.NewGeometryOptions();
            m_ViewOptions.View = m_Revit.ActiveUIDocument.Document.ActiveView;

            var geometry = instance.get_Geometry(m_ViewOptions);
            Solid solid = DataGenerator.ExtractSolid(m_ActiveDocument, geometry, null);
            if (solid == default)
                return value;

            Face face = DataGenerator.GetFace(m_InletOutletMaterials, solid);
            if (face != null)
            {
                value = func(face);
            }
            return value;
        }

        /// <summary>
        /// Get face normal from face.
        /// </summary>
        /// <param name="face">Face object.</param>
        /// <returns>Facenormal as xyz object.</returns>
        private XYZ GetFaceNormal(Face face)
        {
            UV point = new UV();
            return face.ComputeNormal(point);
        }

        /// <summary>
        /// Get face are from given face.
        /// </summary>
        /// <param name="face">Face object.</param>
        /// <returns>Area of the face as double.</returns>
        private double GetFaceArea(Face face)
        {
            return UnitUtils.ConvertFromInternalUnits(face.Area, DisplayUnitType.DUT_SQUARE_METERS);
        }

        /// <summary>
        /// Get face boundary from the given face.
        /// </summary>
        /// <param name="face">Face object.</param>
        /// <returns>Boundary of the face as double.</returns>
        private double GetFaceBoundary(Face face)
        {
            var edges = face.EdgeLoops;
            double boundary = 0;
            if (!edges.IsEmpty && edges != null)
            {
                foreach (Edge edge in edges.get_Item(0) as EdgeArray)
                {
                    boundary += Math.Round(UnitUtils.ConvertFromInternalUnits(edge.ApproximateLength, DisplayUnitType.DUT_METERS),2);
                }
            }
            return boundary;
        }


        ///// <summary>
        ///// Search for inlet/outlet face and return the faceNormal.
        ///// </summary>
        ///// <param name="instance">FamilyInstance for searching inlet/outlet face.</param>
        ///// <returns>Face normal.</returns>
        //private XYZ GetOrientationInletOutlet(FamilyInstance instance)
        //{
        //    XYZ faceNormal = new XYZ(0, 0, 0);
        //    var m_ViewOptions = m_Revit.Application.Create.NewGeometryOptions();
        //    m_ViewOptions.View = m_Revit.ActiveUIDocument.Document.ActiveView;

        //    var geometry = instance.get_Geometry(m_ViewOptions);
        //    Solid solid = DataGenerator.ExtractSolid(m_ActiveDocument, geometry, null);

        //    if (solid == default)
        //        return faceNormal;

        //    Face face = DataGenerator.GetFace(m_InletOutletMaterials, solid);
        //    if(face != null)
        //    {
        //        UV point = new UV();
        //        faceNormal = face.ComputeNormal(point);
        //    }
            
        //    return faceNormal;
        //}

        /// <summary>
        /// Initialize paramValue with the airflow parameter as double.
        /// </summary>
        /// <param name="paramValue">Parameter value empty.</param>
        /// <param name="area">Surface area.</param>
        /// <param name="param">Parameter object.</param>
        /// <returns>Double.</returns>
        private static double ComputeAirFlowValue(double paramValue, double area, Parameter param)
        {
            bool volumenFlow = false;
            switch (param.Definition.ParameterType)
            {
                //volumeflow
                case ParameterType.HVACAirflow:
                    {
                        //m³/h => m³/s
                        paramValue = UnitUtils.ConvertFromInternalUnits(param.AsDouble(), DisplayUnitType.DUT_CUBIC_METERS_PER_SECOND);/*double.Parse(param.AsValueString().Trim(" m³/h".ToCharArray()), System.Globalization.CultureInfo.InvariantCulture)*/;
                        volumenFlow = true;
                        break;
                    }
                //pressure loss
                //case ParameterType.HVACPressure:
                //    {
                //        //Not implemented yet.
                //        break;
                //    }
                    //****************ADD HER MORE PARAMETERTYPE TO HANDLE THEM****************//
            }

            if(volumenFlow && area != 0)
            {
                paramValue = paramValue / area;
            }

            return paramValue;
        }

        /// <summary>
        /// Initialize SSH-Tab.
        /// </summary>
        private void InitializeSSH()
        {
            txtBoxUserIP.Text = m_Settings.SSH.ConnectionString();
            txtBoxAlias.Text = m_Settings.SSH.OfAlias;
            txtBoxCaseFolder.Text = m_Settings.SSH.ServerCaseFolder;
            txtBoxPort.Text = m_Settings.SSH.Port.ToString();
            cbDelete.Checked = m_Settings.SSH.Delete;
            cbDownload.Checked = m_Settings.SSH.Download;
        }

        /// <summary>
        /// Initialize OpenFOAM-TreeView with default parameter from settings.
        /// </summary>
        private void InitializeDefaultParameterOpenFOAM()
        {
            List<string> keyPath = new List<string>();
            foreach (var att in m_Settings.SimulationDefault)
            {
                keyPath.Add(att.Key);
                TreeNode treeNodeSimulation = new TreeNode(att.Key);
                if (att.Value is Dictionary<string, object>)
                {
                    treeNodeSimulation = GetChildNode(att.Key, att.Value as Dictionary<string, object>, keyPath);
                }
                keyPath.Remove(att.Key);
                if (treeNodeSimulation != null)
                    m_OpenFOAMTreeView.Nodes.Add(treeNodeSimulation);
            }
        }

        /// <summary>
        /// Initialize comboBox for Units.
        /// </summary>
        private void InitializeUnitsForSTL()
        {
            string unitName = "Use Internal: Feet";
            m_DisplayUnits.Add(unitName, DisplayUnitType.DUT_UNDEFINED);
            int selectedIndex = comboBox_DUT.Items.Add(unitName);
            if (m_SelectedDUT == DisplayUnitType.DUT_UNDEFINED)
                comboBox_DUT.SelectedIndex = selectedIndex;

            Units currentUnits = m_Revit.ActiveUIDocument.Document.GetUnits();
            DisplayUnitType currentDut = currentUnits.GetFormatOptions(UnitType.UT_Length).DisplayUnits;
            unitName = "Use Current: " + LabelUtils.GetLabelFor(currentDut);
            m_DisplayUnits.Add(unitName, currentDut);
            selectedIndex = comboBox_DUT.Items.Add(unitName);
            if (m_SelectedDUT == currentDut)
                comboBox_DUT.SelectedIndex = selectedIndex;

            foreach (DisplayUnitType dut in UnitUtils.GetValidDisplayUnits(UnitType.UT_Length))
            {
                if (currentDut == dut)
                    continue;
                unitName = LabelUtils.GetLabelFor(dut);
                m_DisplayUnits.Add(unitName, dut);
                selectedIndex = comboBox_DUT.Items.Add(unitName);
                if (m_SelectedDUT == dut)
                    comboBox_DUT.SelectedIndex = selectedIndex;
            }
        }

        /// <summary>
        /// Initialize category-tab.
        /// </summary>
        private void InitializeCategoryList()
        {
            m_CategoryList = m_Generator.ScanCategories(true);

            foreach (Category category in m_CategoryList.Values)
            {
                TreeNode treeNode = GetChildNode(category, m_Revit.ActiveUIDocument.Document.ActiveView);
                if (treeNode != null)
                    tvCategories.Nodes.Add(treeNode);
            }
        }

        /// <summary>
        /// Initialize OpenFOAM-TreeView.
        /// </summary>
        private void InitializeOFTreeViewSize()
        {
            Size sizeOpenFoamTreeView = gbDefault.Size;
            sizeOpenFoamTreeView.Height -= 20;
            sizeOpenFoamTreeView.Width -= 20;
            m_OpenFOAMTreeView.Bounds = gbDefault.Bounds;
            m_OpenFOAMTreeView.Size = sizeOpenFoamTreeView;
            m_OpenFOAMTreeView.Top += 10;
        }

        /// <summary>
        /// Get all openFoam parameter from child nodes.
        /// </summary>
        /// <param name="parent">Parent node</param>
        /// <param name="dict">Dictionary that contains attributes.</param>
        /// <returns>TreeNode with all attributes from dictionary as child nodes.</returns>
        private TreeNode GetChildNode(string parent, Dictionary<string, object> dict, List<string> keyPath)
        {
            if (parent == null)
                return null;

            TreeNode treeNode = new TreeNode(parent)
            {
                Tag = parent
            };

            if (dict == null)
                return treeNode;

            if (dict.Count == 0)
                return treeNode;

            foreach(var att in dict)
            {
                //new datatypes needs to be handled here.
                keyPath.Add(att.Key);
                TreeNode child = new TreeNode(att.Key);
                if (att.Value is Dictionary<string,object>)
                {
                    child = GetChildNode(att.Key, att.Value as Dictionary<string, object>, keyPath);
                }
                else if (att.Value is Enum)
                {
                    Enum @enum = att.Value as Enum;
                    OpenFOAMDropDownTreeNode<dynamic> dropDown = new OpenFOAMDropDownTreeNode<dynamic>(@enum, ref m_Settings, keyPath);
                    child.Nodes.Add(dropDown);
                }
                else if (att.Value is bool)
                {
                    bool? _bool = att.Value as bool?;
                    if(_bool != null)
                    {
                        OpenFOAMDropDownTreeNode<dynamic> dropDown = new OpenFOAMDropDownTreeNode<dynamic>((bool)_bool, ref m_Settings, keyPath);
                        child.Nodes.Add(dropDown);
                    }
                }
                else if(att.Value is FOAMParameterPatch<dynamic>)
                {
                    FOAMParameterPatch<dynamic> patch = (FOAMParameterPatch<dynamic>)att.Value;
                    child = GetChildNode(att.Key, patch.Attributes, keyPath);
                }
                else
                {
                    OpenFOAMTextBoxTreeNode<dynamic> txtBoxNode = new OpenFOAMTextBoxTreeNode<dynamic>(att.Value, ref m_Settings, keyPath);
                    child.Nodes.Add(txtBoxNode);
                }

                keyPath.Remove(att.Key);

                if (child != null)
                    treeNode.Nodes.Add(child);
            }
            return treeNode;
        }

        /// <summary>
        /// Get all subcategory.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="view">Current view.</param>
        /// <returns></returns>
        private TreeNode GetChildNode(Category category,Autodesk.Revit.DB.View view)
        {
            if(category == null)
                return null;

            if (!category.get_AllowsVisibilityControl(view))
                return null;

            TreeNode treeNode = new TreeNode(category.Name)
            {
                Tag = category,
                Checked = true
            };

            if (category.SubCategories.Size == 0)
            {                
                return treeNode;
            }

            foreach (Category subCategory in category.SubCategories)
            {
                TreeNode child = GetChildNode(subCategory,view);
                if(child !=null)
                    treeNode.Nodes.Add(child);
            }

            return treeNode;
        }

        /// <summary>
        /// Help button click event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void BtnHelp_Click(object sender, EventArgs e)
        {
            string helpfile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            helpfile = Path.Combine(helpfile, "OpenFOAM_Export.chm");

            if (File.Exists(helpfile) == false)
            {
                MessageBox.Show("Help File " + helpfile + " NOT FOUND!");
                return;
            }

            Help.ShowHelp(this, "file://" + helpfile);

        }

        /// <summary>
        /// Save button click event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            string fileName = OpenFOAMDialogManager.SaveDialog();

            if (!string.IsNullOrEmpty(fileName))
            {
                if (!UpdateNonDefaultSettings())
                    return;

                TopMost = false;

                // save Revit document's triangular data in a temporary file, generate openFOAM-casefolder and start simulation
                m_Generator = new DataGenerator(m_Revit.Application, m_Revit.ActiveUIDocument.Document, m_Revit.ActiveUIDocument.Document.ActiveView);
                DataGenerator.GeneratorStatus succeed = m_Generator.SaveSTLFile(fileName, m_Settings/*aSetting*/);

                if (succeed == DataGenerator.GeneratorStatus.FAILURE)
                {
                    this.DialogResult = DialogResult.Cancel;
                    MessageBox.Show(OpenFOAMExportResource.ERR_SAVE_FILE_FAILED, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
                             MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else if (succeed == DataGenerator.GeneratorStatus.CANCEL)
                {
                    this.DialogResult = DialogResult.Cancel;
                    MessageBox.Show(OpenFOAMExportResource.CANCEL_FILE_NOT_SAVED, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
                             MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(OpenFOAMExportResource.CANCEL_FILE_NOT_SAVED, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Updates settings parameter from general section of the OpenFOAM-Tab.
        /// </summary>
        private bool UpdateNonDefaultSettings()
        {
            if (m_Settings == null)
            {
                return false;
            }

            SaveFormat saveFormat;
            if (rbBinary.Checked)
            {
                saveFormat = SaveFormat.binary;
            }
            else
            {
                saveFormat = SaveFormat.ascii;
            }
            m_Settings.SaveFormat = saveFormat;

            ElementsExportRange exportRange;
            exportRange = ElementsExportRange.OnlyVisibleOnes;
            m_Settings.ExportRange = exportRange;

            // get selected categories from the category list
            List<Category> selectedCategories = new List<Category>();

            // only for projects
            if (m_Revit.ActiveUIDocument.Document.IsFamilyDocument == false)
            {
                foreach (TreeNode treeNode in tvCategories.Nodes)
                {
                    AddSelectedTreeNode(treeNode, selectedCategories);
                }
            }

            //Set current selected unit
            DisplayUnitType dup = m_DisplayUnits[comboBox_DUT.Text];
            m_SelectedDUT = dup;
            m_Settings.Units = dup;

            //Set current selected OpenFoam-Environment as active.
            OpenFOAMEnvironment env = (OpenFOAMEnvironment)comboBoxEnv.SelectedItem;
            m_Settings.OpenFOAMEnvironment = env;

            //Set current selected incompressible solver
            SolverControlDict appInc = (SolverControlDict)comboBoxSolver.SelectedItem;
            m_Settings.AppSolverControlDict = appInc;

            //Set current selected transportModel
            TransportModel transport = (TransportModel)comboBoxTransportModel.SelectedItem;
            m_Settings.TransportModel = transport;

            //set number of cpu
            if (int.TryParse(textBoxCPU.Text, out int cpu))
            {
                m_Settings.NumberOfSubdomains = cpu;
            }
            else
            {
                MessageBox.Show("Please type in the number of subdomains (CPU) for the simulation");
                textBoxCPU.Text = m_Settings.NumberOfSubdomains.ToString();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Add selected category into dictionary.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="selectedCategories"></param>
        private void AddSelectedTreeNode(TreeNode treeNode,List<Category> selectedCategories)
        {
            if (treeNode.Checked)
              selectedCategories.Add((Category)treeNode.Tag);

            if(treeNode.Nodes.Count != 0)
            {
                foreach(TreeNode child in treeNode.Nodes)
                {
                    AddSelectedTreeNode(child, selectedCategories);
                }
            }
        }

        /// <summary>
        /// Cancel button click event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Check all button click event, checks all categories in the category list.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void BtnCheckAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode treeNode in tvCategories.Nodes)
            {
                SetCheckedforTreeNode(treeNode,true);
            }
        }

        /// <summary>
        /// Check none click event, unchecks all categories in the category list.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void BtnCheckNone_Click(object sender, EventArgs e)
        {
            foreach (TreeNode treeNode in tvCategories.Nodes)
            {
                SetCheckedforTreeNode(treeNode,false);
            }
        }

        /// <summary>
        /// Set the checked property of treenode to true or false.
        /// </summary>
        /// <param name="treeNode">The tree node.</param>
        /// <param name="selected">Checked or not.</param>
        private void SetCheckedforTreeNode(TreeNode treeNode,bool selected)
        {
            treeNode.Checked = selected;
            if (treeNode.Nodes.Count != 0)
            {
                foreach (TreeNode child in treeNode.Nodes)
                {
                    SetCheckedforTreeNode(child, selected);
                }
            }
        }

        /// <summary>
        /// Included linked models click event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void CbIncludeLinked_CheckedChanged(object sender, EventArgs e)
        {
            if (cbIncludeLinked.Checked == true)
            {
                MessageBox.Show(OpenFOAMExportResource.WARN_PROJECT_POSITION, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// ExportFormat click event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void RbExportFormat_CheckedChanged(object sender, EventArgs e)
        {
            cbExportColor.Enabled = rbBinary.Checked;
        }

        /// <summary>
        /// OpenFOAM checked event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void CbOpenFOAM_CheckedChanged(object sender, EventArgs e)
        {
            if (cbOpenFOAM.Checked == true)
            {
                rbBinary.Enabled = false;
                comboBox_DUT.SelectedIndex = comboBox_DUT.Items.IndexOf("Meter");
                tbOpenFOAM.Enabled = true;
                m_Settings.OpenFOAM = true;
                cbExportColor.Enabled = false;
                cbExportSharedCoordinates.Enabled = false;
                cbIncludeLinked.Enabled = false;
                rbAscii.Select();
            }
            else
            {
                tbOpenFOAM.Enabled = false;
                cbExportColor.Enabled = true;
                cbExportSharedCoordinates.Enabled = true;
                cbIncludeLinked.Enabled = true;
                m_Settings.OpenFOAM = false;
                rbBinary.Enabled = true;
            }
        }

        /// <summary>
        /// ValueChanged event for comboBoxEnv.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void ComboBoxEnv_SelectedValueChanged(object sender, EventArgs e)
        {
            if((OpenFOAMEnvironment)comboBoxEnv.SelectedItem == OpenFOAMEnvironment.ssh)
            {
                tbSSH.Enabled = true;
            }
            else
            {
                tbSSH.Enabled = false;
            }
        }

        /// <summary>
        /// ValueChanged event for comboBoxSolver.
        /// </summary>
        /// <param name="sender">Sender objet.</param>
        /// <param name="e">event args.</param>
        private void ComboBoxSolver_SelectedValueChanged(object sender, EventArgs e)
        {
            m_Settings.AppSolverControlDict = (SolverControlDict)comboBoxSolver.SelectedItem;
            m_Settings.Update();
            TreeNodeCollection collection = m_OpenFOAMTreeView.Nodes;
            collection.Clear();
            InitializeDefaultParameterOpenFOAM();
        }

        /// <summary>
        /// ValueChanged event for comboBoxTransport.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event args.</param>
        private void ComboBoxTransport_SelectedValueChanged(object sender, EventArgs e)
        {
            //Not implemented yet.
        }

        /// <summary>
        /// ValueChanged event for txtBoxUserIP.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void TxtBoxUserIP_ValueChanged(object sender, EventArgs e)
        {
            //TextBox_ValueChanged();
            string txtBox = txtBoxUserIP.Text;

            if (m_RegUserIP.IsMatch(txtBox))
            {
                SSH ssh = m_Settings.SSH;
                ssh.User = txtBox.Split('@')[0];
                ssh.ServerIP = txtBox.Split('@')[1];
                m_Settings.SSH = ssh;
            }
            else
            {
                MessageBox.Show(OpenFOAMExportResource.ERR_FORMAT + " " + txtBox, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
                             MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return;
            }

            //TO-DO: IF XML-CONFIG IMPLEMENTED => ADD CHANGES
        }

        ///// <summary>
        ///// Changes ssh user and server ip in settings.
        ///// </summary>
        //private void TxtBoxUserIP_ChangeValue()
        //{
        //    if (!m_Changed)
        //    {
        //        return;
        //    }

        //    string txtBox = txtBoxUserIP.Text;

        //    if (m_RegUserIP.IsMatch(txtBox))
        //    {
        //        SSH ssh = m_Settings.SSH;
        //        ssh.User = txtBox.Split('@')[0];
        //        ssh.ServerIP = txtBox.Split('@')[1];
        //        m_Settings.SSH = ssh;
        //    }
        //    else
        //    {
        //        MessageBox.Show(OpenFOAMExportResource.ERR_FORMAT + " " + txtBox, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
        //                     MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        txtBoxUserIP.Text = m_Settings.SSH.ConnectionString();
        //        //return;
        //    }
        //}

        ///// <summary>
        ///// KeyPress-Event txtBoxUserIp.
        ///// </summary>
        ///// <param name="sender">Sender.</param>
        ///// <param name="e">KeyPressEventArgs.</param>
        //private void TxtBoxUserIP_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    TextBox_KeyPress(e, TxtBoxUserIP_ChangeValue);
        //}

        ///// <summary>
        ///// Leave-Event txtBoxUserIP.
        ///// </summary>
        ///// <param name="sender">Sender.</param>
        ///// <param name="e">EventArgs.</param>
        //private void TxtBoxUserIP_Leave(object sender, EventArgs e)
        //{
        //    TextBox_Leave();
        //    //m_Clicked = !m_Clicked;
        //    //m_Changed = !m_Changed;
        //}

        ///// <summary>
        ///// Click-Event txtBoxUserIP.
        ///// </summary>
        ///// <param name="sender">Sender.</param>
        ///// <param name="e">EventArgs.</param>
        //private void TxtBoxUserIP_Click(object sender, EventArgs e)
        //{
        //    TextBox_Clicked();
        //}

        /// <summary>
        /// ValueChanged event for txtBoxAlias.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void TxtBoxAlias_ValueChanged(object sender, EventArgs e)
        {
            //TextBox_ValueChanged();
            string txt = txtBoxAlias.Text;
            SSH ssh = m_Settings.SSH;
            ssh.OfAlias = txt;
            m_Settings.SSH = ssh;

            //TO-DO: IF XML-CONFIG IMPLEMENTED => ADD CHANGES
        }

        ///// <summary>
        ///// Change the corresponding value in settings of the txtBoxAlias.
        ///// </summary>
        //private void TxtBoxAlias_ChangeValue()
        //{
        //    if (!m_Changed)
        //        return;

        //    string txt = txtBoxAlias.Text;
        //    SSH ssh = m_Settings.SSH;
        //    ssh.OfAlias = txt;
        //    m_Settings.SSH = ssh;
        //}

        ///// <summary>
        ///// KeyPress-Event txtBoxAlias.
        ///// </summary>
        ///// <param name="sender">Sender.</param>
        ///// <param name="e">KeyPressEventArgs.</param>
        //private void TxtBoxAlias_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    TextBox_KeyPress(e, TxtBoxAlias_ChangeValue);
        //}

        ///// <summary>
        ///// Leave-Event txtBoxAlias.
        ///// </summary>
        ///// <param name="sender">Sender.</param>
        ///// <param name="e">EventArgs.</param>
        //private void TxtBoxAlias_Leave(object sender, EventArgs e)
        //{
        //    TextBox_Leave();
        //}

        ///// <summary>
        ///// Click-Event txtBoxAlias.
        ///// </summary>
        ///// <param name="sender">Sender.</param>
        ///// <param name="e">EventArgs.</param>
        //private void TxtBoxAlias_Click(object sender, EventArgs e)
        //{
        //    TextBox_Clicked();
        //}

        /// <summary>
        /// ValueChanged for txtBoxServerCaseFolder.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void TxtBoxServerCaseFolder_ValueChanged(object sender, EventArgs e)
        {
            //TextBox_ValueChanged();
            string txtBox = txtBoxCaseFolder.Text;

            if (m_RegServerCasePath.IsMatch(txtBox))
            {
                SSH ssh = m_Settings.SSH;
                ssh.ServerCaseFolder = txtBox;
                m_Settings.SSH = ssh;
            }
            else
            {
                MessageBox.Show(OpenFOAMExportResource.ERR_FORMAT + " " + txtBox, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
                             MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            //TO-DO: IF XML-CONFIG IMPLEMENTED => ADD CHANGES

        }

        ///// <summary>
        ///// Change the corresponding value in settings of the txtServerCaseFolder.
        ///// </summary>
        //private void TxtBoxServerCaseFolder_ChangeValue()
        //{
        //    if (!m_Changed)
        //        return;

        //    string txtBox = txtBoxCaseFolder.Text;

        //    if (m_RegServerCasePath.IsMatch(txtBox))
        //    {
        //        SSH ssh = m_Settings.SSH;
        //        ssh.ServerCaseFolder = txtBox;
        //        m_Settings.SSH = ssh;
        //    }
        //    else
        //    {
        //        MessageBox.Show(OpenFOAMExportResource.ERR_FORMAT + " " + txtBox, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
        //                     MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        txtBoxCaseFolder.Text = m_Settings.SSH.ServerCaseFolder;
        //        //return;
        //    }
        //    //TO-DO: IF XML-CONFIG IMPLEMENTED => ADD CHANGES
        //}

        ///// <summary>
        ///// KeyPress-Event txtBoxServerCaseFolder.
        ///// </summary>
        ///// <param name="sender">Sender.</param>
        ///// <param name="e">KeyPressEventArgs.</param>
        //private void TxtBoxServerCaseFolder_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    TextBox_KeyPress(e, TxtBoxServerCaseFolder_ChangeValue);
        //}

        ///// <summary>
        ///// Leave-Event txtBoxServerCaseFolder.
        ///// </summary>
        ///// <param name="sender">Sender.</param>
        ///// <param name="e">EventArgs.</param>
        //private void TxtBoxServerCaseFolder_Leave(object sender, EventArgs e)
        //{
        //    TextBox_Leave();
        //}

        ///// <summary>
        ///// Click-Event txtBoxServerCaseFolder.
        ///// </summary>
        ///// <param name="sender">Sender.</param>
        ///// <param name="e">EventArgs.</param>
        //private void TxtBoxServerCaseFolder_Click(object sender, EventArgs e)
        //{
        //    TextBox_Clicked();
        //}

        /// <summary>
        /// ValueChanged event for txtBoxPort.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void TxtBoxPort_ValueChanged(object sender, EventArgs e)
        {
            //TextBox_ValueChanged();
            string txtBox = txtBoxPort.Text;

            if (m_RegPort.IsMatch(txtBox))
            {
                SSH ssh = m_Settings.SSH;
                ssh.Port = Convert.ToInt32(txtBox);
                m_Settings.SSH = ssh;
            }
            else
            {
                MessageBox.Show(OpenFOAMExportResource.ERR_FORMAT + " " + txtBox, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
                             MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            //TO - DO: IF XML-CONFIG IMPLEMENTED => ADD CHANGES
        }

        ///// <summary>
        ///// Change the corresponding value in settings of the txtBoxPort.
        ///// </summary>
        //private void TxtBoxPort_ChangeValue()
        //{
        //    if (!m_Changed)
        //        return;

        //    string txtBox = txtBoxPort.Text;

        //    if (m_RegPort.IsMatch(txtBox))
        //    {
        //        SSH ssh = m_Settings.SSH;
        //        ssh.Port = Convert.ToInt32(txtBox);
        //        m_Settings.SSH = ssh;
        //    }
        //    else
        //    {
        //        MessageBox.Show(OpenFOAMExportResource.ERR_FORMAT + " " + txtBox, OpenFOAMExportResource.MESSAGE_BOX_TITLE,
        //                     MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        txtBoxPort.Text = m_Settings.SSH.Port.ToString();
        //        //return;
        //    }
        //}

        ///// <summary>
        ///// KeyPress-Event txtBoxPort.
        ///// </summary>
        ///// <param name="sender">Sender.</param>
        ///// <param name="e">KeyPressEventArgs.</param>
        //private void TxtBoxPort_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    TextBox_KeyPress(e, TxtBoxPort_ChangeValue);
        //}

        ///// <summary>
        ///// Leave-Event txtBoxPort.
        ///// </summary>
        ///// <param name="sender">Sender.</param>
        ///// <param name="e">EventArgs.</param>
        //private void TxtBoxPort_Leave(object sender, EventArgs e)
        //{
        //    TextBox_Leave();
        //}

        ///// <summary>
        ///// Click-Event txtBoxPort.
        ///// </summary>
        ///// <param name="sender">Sender.</param>
        ///// <param name="e">EventArgs.</param>
        //private void TxtBoxPort_Click(object sender, EventArgs e)
        //{
        //    TextBox_Clicked();
        //}

        /// <summary>
        /// Checked event for cbDownload.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void CbDownload_CheckedChanged(object sender, EventArgs e)
        {
            SSH ssh = m_Settings.SSH;
            ssh.Download = cbDownload.Checked;
            m_Settings.SSH = ssh;

            //TO-DO: IF XML-CONFIG IMPLEMENTED => ADD CHANGES
        }

        /// <summary>
        /// Checked event for cbDelete.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void CbDelete_CheckedChanged(object sender, EventArgs e)
        {
            SSH ssh = m_Settings.SSH;
            ssh.Delete = cbDelete.Checked;
            m_Settings.SSH = ssh;

            //TO-DO: IF XML-CONFIG IMPLEMENTED => ADD CHANGES
        }



        /**********************BOUNDING BOX FOR ROOM**********************/
        //TO-DO: ADD BUTTON TO SELECT BOUNDING-BOX


        /////<summary>
        ///// Create and return a solid representing 
        ///// the bounding box of the input solid.
        ///// Source: https://thebuildingcoder.typepad.com/blog/2016/09/solid-from-bounding-box-and-forge-webinar-4.html#2
        ///// Assumption: aligned with Z axis.
        ///// Written, described and tested by Owen Merrick for 
        ///// http://forums.autodesk.com/t5/revit-api-forum/create-solid-from-boundingbox/m-p/6592486
        ///// </summary>
        //public static Solid CreateSolidFromBoundingBox(Solid inputSolid)
        //{
        //    BoundingBoxXYZ bbox = inputSolid.GetBoundingBox();

        //    // Corners in BBox coords

        //    XYZ pt0 = new XYZ(bbox.Min.X, bbox.Min.Y, bbox.Min.Z);
        //    XYZ pt1 = new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Min.Z);
        //    XYZ pt2 = new XYZ(bbox.Max.X, bbox.Max.Y, bbox.Min.Z);
        //    XYZ pt3 = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z);

        //    // Edges in BBox coords

        //    Line edge0 = Line.CreateBound(pt0, pt1);
        //    Line edge1 = Line.CreateBound(pt1, pt2);
        //    Line edge2 = Line.CreateBound(pt2, pt3);
        //    Line edge3 = Line.CreateBound(pt3, pt0);

        //    // Create loop, still in BBox coords

        //    List<Curve> edges = new List<Curve>();
        //    edges.Add(edge0);
        //    edges.Add(edge1);
        //    edges.Add(edge2);
        //    edges.Add(edge3);

        //    double height = bbox.Max.Z - bbox.Min.Z;

        //    CurveLoop baseLoop = CurveLoop.Create(edges);

        //    List<CurveLoop> loopList = new List<CurveLoop>();
        //    loopList.Add(baseLoop);

        //    Solid preTransformBox = GeometryCreationUtilities
        //      .CreateExtrusionGeometry(loopList, XYZ.BasisZ,
        //        height);

        //    Solid transformBox = SolidUtils.CreateTransformed(
        //      preTransformBox, bbox.Transform);

        //    return transformBox;
        //}


        
        /// <summary>
        /// Click-Event for txtBoxLocationInMesh.
        /// </summary>
        /// <param name="sender">Object.</param>
        /// <param name="e">EventArgs</param>
        private void TxtBoxLocationInMesh_Click(object sender, EventArgs e)
        {
            if(!m_Clicked)
            {
                //System.Windows.Media.Media3D.Vector3D location = m_Settings.LocationInMesh;
                TopMost = true;

                //Create sphere
                XYZ xyz = ConvertLocationInMeshToInternalUnit()/*new XYZ(location.X, location.Y, location.Z)*/;
                CreateSphereDirectShape(xyz);
                m_Clicked = true;
            }
        }

        /// <summary>
        /// Create a cylinder at the given point.
        /// Source: 
        /// https://knowledge.autodesk.com/search-result/caas/CloudHelp/cloudhelp/2016/ENU/Revit-API/files/GUID-A1BCB8D4-5628-45AF-B399-AF573CBBB1D0-htm.html
        /// </summary>
        /// <param name="point">Location for ball.</param>
        /// <param name="height">Height of the ball.</param>
        /// <param name="radius">Radius of the ball.</param>
        /// <returns>Ball as solid.</returns>
        private Solid CreateCylindricalVolume(XYZ point, double height, double radius)
        {
            // build cylindrical shape around endpoint
            List<CurveLoop> curveloops = new List<CurveLoop>();
            CurveLoop circle = new CurveLoop();

            // For solid geometry creation, two curves are necessary, even for closed
            // cyclic shapes like circles
            circle.Append(Arc.Create(point, radius, 0, Math.PI, XYZ.BasisX, XYZ.BasisY));
            circle.Append(Arc.Create(point, radius, Math.PI, 2 * Math.PI, XYZ.BasisX, XYZ.BasisY));
            curveloops.Add(circle);
            Solid createdCylinder = GeometryCreationUtilities.CreateExtrusionGeometry(curveloops, XYZ.BasisZ, height);

            return createdCylinder;
        }

        /// <summary>
        /// Create a sphere at the given point.
        /// Source: https://knowledge.autodesk.com/search-result/caas/CloudHelp/cloudhelp/2016/ENU/Revit-API/files/GUID-DF7B9D4A-5A8A-4E39-8721-B7782CBD7730-htm.html
        /// </summary>
        /// <param name="doc">Document sphere will be added to.</param>
        /// <param name="location">Location point.</param>
        public void CreateSphereDirectShape(XYZ location)
        {
            Solid sphere = CreateSolidSphere(location, 1.0);

            using (Transaction t = new Transaction(m_ActiveDocument, "Create sphere direct shape"))
            {
                //start transaction
                t.Start();
                if (m_SphereLocationInMesh != null)
                {
                    m_ActiveDocument.Delete(m_SphereLocationInMesh);
                }

                //create direct shape and assign the sphere shape
                DirectShape ds = DirectShape.CreateElement(m_ActiveDocument, new ElementId(BuiltInCategory.OST_GenericModel));
                ds.SetShape(new GeometryObject[] { sphere });
                m_SphereLocationInMesh = ds.Id;
                t.Commit();
            }

            ICollection<ElementId> ids = new List<ElementId>();
            ids.Add(m_SphereLocationInMesh);
            HighlightElementInScene(m_ActiveDocument, m_SphereLocationInMesh, 98);
            m_Revit.ActiveUIDocument.Selection.SetElementIds(ids);
           
        }

        /// <summary>
        /// Generate a sphere as solid.
        /// </summary>
        /// <param name="location">Location of sphere.</param>
        /// <param name="radius">Radius of sphere.</param>
        /// <returns>Sphere as solid.</returns>
        private static Solid CreateSolidSphere(XYZ location, double radius)
        {
            List<Curve> profile = new List<Curve>();

            // first create sphere with 2' radius
            XYZ center = location;
            XYZ profilePlus = center + new XYZ(0, radius, 0);
            XYZ profileMinus = center - new XYZ(0, radius, 0);

            profile.Add(Line.CreateBound(profilePlus, profileMinus));
            profile.Add(Arc.Create(profileMinus, profilePlus, center + new XYZ(radius, 0, 0)));

            CurveLoop curveLoop = CurveLoop.Create(profile);
            SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

            Frame frame = new Frame(center, XYZ.BasisX, -XYZ.BasisZ, XYZ.BasisY);
            Solid sphere = GeometryCreationUtilities.CreateRevolvedGeometry(frame, new CurveLoop[] { curveLoop }, 0, 2 * Math.PI, options);
            return sphere;
        }

        /// <summary>
        /// Higlight the given element in the document and set the background to the given transparency value.
        /// </summary>
        /// <param name="doc">Document object.</param>
        /// <param name="elementIdToIsolate">ElementId of the Element that will be highlighted.</param>
        /// <param name="transparency">Transparency value from 0-100.</param>
        private void HighlightElementInScene(Document doc, ElementId elementIdToIsolate, int transparency)
        {
            OverrideGraphicSettings ogsFade = OverideGraphicSettingsTransparency(transparency, false, false, true);
            OverrideGraphicSettings ogsIsolate = OverideGraphicSettingsTransparency(0, true, true, false);

            using (Transaction t = new Transaction(doc, "Isolate with Fade"))
            {
                t.Start();
                foreach (Element e in new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType())
                {
                    if (e.Id == elementIdToIsolate || elementIdToIsolate == null)
                        doc.ActiveView.SetElementOverrides(e.Id, ogsIsolate);
                    else
                    {
                        doc.ActiveView.SetElementOverrides(e.Id, ogsFade);
                        e.Pinned = true;
                    }
                }
                t.Commit();
            }
        }

        /// <summary>
        /// Generates a overrideGraphicSettings for transparency.
        /// </summary>
        /// <param name="transparency">Transparency value from 0-100.</param>
        /// <param name="foregroundVisible">Foreground visibility.</param>
        /// <param name="backgroundVisible">Background visibility.</param>
        /// <param name="halfTone">Halftone.</param>
        /// <returns>OverrideGraphicSettings object with given attributes.</returns>
        private static OverrideGraphicSettings OverideGraphicSettingsTransparency(int transparency, bool foregroundVisible, bool backgroundVisible, bool halfTone)
        {
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();
            ogs.SetSurfaceTransparency(transparency);
            ogs.SetSurfaceForegroundPatternVisible(foregroundVisible);
            ogs.SetSurfaceBackgroundPatternVisible(backgroundVisible);
            ogs.SetHalftone(halfTone);
            return ogs;
        }

        /// <summary>
        /// Coloring solid.
        /// </summary>
        /// <param name="doc">Document.</param>
        /// <param name="solidId">Solid elementId.</param>
        /// <param name="color"></param>
        private void ColoringSolid(Document doc, ElementId solidId, Autodesk.Revit.DB.Color color)
        {
            if(doc.GetElement(solidId) == null)
            {
                return;
            }
            using (Transaction t = new Transaction(m_ActiveDocument, "Create sphere direct shape"))
            {
                t.Start();

                OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                ogs.SetProjectionLineColor(color);
                ogs.SetSurfaceForegroundPatternColor(color);
                ogs.SetCutForegroundPatternColor(color);
                ogs.SetCutBackgroundPatternColor(color);
                ogs.SetCutLineColor(color);
                ogs.SetSurfaceBackgroundPatternColor(color);
                ogs.SetHalftone(false);

                doc.ActiveView.SetElementOverrides(solidId, ogs);
                t.Commit();
            }
        }

        /// <summary>
        /// TextBoxLocationInMesh textBox_TextChanged - event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">EventArgs.</param>
        private void TxtBoxLocationInMesh_ValueChanged(object sender, EventArgs e)
        {
            TextBox_ValueChanged();
        }

        /// <summary>
        /// TextBoxLocationInMesh textBox_TextChanged - event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">EventArgs.</param>
        private void LocationInMesh_ChangeValue()
        {
            XYZ previousLocation = ConvertLocationInMeshToInternalUnit();
            if (m_LocationReg.IsMatch(txtBoxLocationInMesh.Text) && m_SphereLocationInMesh != null)
            {
                List<double> entries = OpenFOAMTreeView.GetListFromVector3DString(txtBoxLocationInMesh.Text);
                
                //needs to be converted from internal Unit to meters
                double x = UnitUtils.ConvertFromInternalUnits(entries[0], DisplayUnitType.DUT_METERS);
                double y = UnitUtils.ConvertFromInternalUnits(entries[1], DisplayUnitType.DUT_METERS);
                double z = UnitUtils.ConvertFromInternalUnits(entries[2], DisplayUnitType.DUT_METERS);
                m_Settings.LocationInMesh = new System.Windows.Media.Media3D.Vector3D(x, y, z);

                //internal length unit = feet
                XYZ internalXYZ = new XYZ(entries[0], entries[1], entries[2]);
                internalXYZ = internalXYZ - previousLocation;
                MoveSphere(internalXYZ);
            }
            else
            {
                string previousLoc = previousLocation.ToString();
                MessageBox.Show("Please insert 3 dimensional vector in this format: x y z -> (x,y,z) ∊ ℝ", OpenFOAMExportResource.MESSAGE_BOX_TITLE);
                txtBoxLocationInMesh.Text = previousLoc.Replace(", ", " ").Trim('(', ')');
            }
        }

        /// <summary>
        /// Convert locationInMesh in settings to Internal Unit and return as XYZ object. 
        /// </summary>
        /// <returns>Location as XYZ as internal Unit.</returns>
        private XYZ ConvertLocationInMeshToInternalUnit()
        {
            System.Windows.Media.Media3D.Vector3D previousVec = m_Settings.LocationInMesh;

            double xPre = UnitUtils.ConvertToInternalUnits(previousVec.X, DisplayUnitType.DUT_METERS);
            double yPre = UnitUtils.ConvertToInternalUnits(previousVec.Y, DisplayUnitType.DUT_METERS);
            double zPre = UnitUtils.ConvertToInternalUnits(previousVec.Z, DisplayUnitType.DUT_METERS);

            XYZ previousLocation = new XYZ(xPre, yPre, zPre);
            return previousLocation;
        }

        /// <summary>
        /// Enter event in txtBoxLocationInMesh.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs.</param>
        private void TxtBoxLocationInMesh_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox_KeyPress(e, LocationInMesh_ChangeValue);
        }

        /// <summary>
        /// TextBoxLocationInMesh leave - event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">EventArgs.</param>
        private void TxtBoxLocationInMesh_Leave(object sender, EventArgs e)
        {
            TextBox_Leave();

            TopMost = false;

            var vector = GetLocationOfElementAsVector(m_SphereLocationInMesh);
            if(vector != new System.Windows.Media.Media3D.Vector3D())
            {
                m_Settings.LocationInMesh = vector;
            }
            OverrideGraphicSettings ogs = OverideGraphicSettingsTransparency(0, true, true, false);
            using (Transaction t = new Transaction(m_ActiveDocument, "Delete sphere"))
            {
                //start transaction
                t.Start();
                if (m_SphereLocationInMesh != null)
                {
                    m_ActiveDocument.Delete(m_SphereLocationInMesh);
                }
                foreach (Element elem in new FilteredElementCollector(m_ActiveDocument, m_ActiveDocument.ActiveView.Id).WhereElementIsNotElementType())
                {
                    m_ActiveDocument.ActiveView.SetElementOverrides(elem.Id, ogs);
                    elem.Pinned = false;
                }
                t.Commit();
            }

            XYZ previousLocation = ConvertLocationInMeshToInternalUnit();
            string previousLoc = previousLocation.ToString();
            txtBoxLocationInMesh.Text = previousLoc.Replace(", ", " ").Trim('(', ')');
            m_SphereLocationInMesh = null;
        }

        /// <summary>
        /// Search the elementId in active document and return the origin location of it as Vector3D object.
        /// </summary>
        /// <param name="id">ElementId of element to search for.</param>
        /// <returns>Location of element as vector3D.</returns>
        private System.Windows.Media.Media3D.Vector3D GetLocationOfElementAsVector(ElementId id)
        {
            System.Windows.Media.Media3D.Vector3D location = new System.Windows.Media.Media3D.Vector3D();
            if (id == null)
                return location;

            var locationInMesh = m_ActiveDocument.GetElement(id) as DirectShape;
            if(locationInMesh != null)
            {
                var geometry = locationInMesh.get_Geometry(new Options());
                foreach (Solid solid in geometry)
                {
                    XYZ point = solid.GetBoundingBox().Transform.Origin;
                    location.X = UnitUtils.ConvertFromInternalUnits(point.X, DisplayUnitType.DUT_METERS);
                    location.Y = UnitUtils.ConvertFromInternalUnits(point.Y, DisplayUnitType.DUT_METERS);
                    location.Z = UnitUtils.ConvertFromInternalUnits(point.Z, DisplayUnitType.DUT_METERS);
                }
            }
            return location;
        }

        /// <summary>
        /// Move sphere to point in 3D-Space of active revit document.
        /// </summary>
        /// <param name="xyz">Location sphere will be moved to.</param>
        private void MoveSphere(XYZ xyz)
        {
            using (Transaction t = new Transaction(m_ActiveDocument, "Move sphere"))
            {
                t.Start();
                var elem = m_ActiveDocument.GetElement(m_SphereLocationInMesh);
                elem.Location.Move(xyz);
                t.Commit();
            }
            m_Changed = false;
        }

        /// <summary>
        /// If value bool changed and textBox has been clicked set m_Changed to true.
        /// </summary>
        private void TextBox_ValueChanged()
        {
            if (!m_Clicked)
                return;

            if (!m_Changed)
                m_Changed = true;
        }

        /// <summary>
        /// If pressed key equals enter than call the changeValueFunc-method.
        /// </summary>
        /// <param name="e">KeyPressEventArgs</param>
        /// <param name="changeValueFunc">Action that will be called after enter in TxtBox.</param>
        private void TextBox_KeyPress(KeyPressEventArgs e, Action changeValueFunc)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (m_Changed)
                {
                    changeValueFunc();
                }
            }
        }

        /// <summary>
        /// If click is false change it to true;
        /// </summary>
        private void TextBox_Clicked()
        {
            if (!m_Clicked)
            {
                m_Clicked = true;
            }
        }

        /// <summary>
        /// Negate m_Changed and m_Clicked after leave txtBox.
        /// </summary>
        private void TextBox_Leave()
        {
            m_Changed = false;
            m_Clicked = false;
        }

        /// <summary>
        /// Event that will be called if user changes view while add-in is open.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">ViewActivatedEventArgs.</param>
        private void Revit_ViewActivating(object sender, ViewActivatingEventArgs e)
        {
            if(m_Changed || m_Clicked)
            {
                TxtBoxLocationInMesh_Leave(sender, e);
            }
        }

        /// <summary>
        /// Event that will be called after closing the form.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">FormClosedEventArgs.</param>
        private void OpenFOAMExportForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            TxtBoxLocationInMesh_Leave(sender, e);
        }
    }
}
