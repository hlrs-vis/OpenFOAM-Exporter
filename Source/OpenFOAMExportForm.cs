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

namespace BIM.OpenFOAMExport
{
    public partial class OpenFOAMExportForm : System.Windows.Forms.Form
    {
        /// <summary>
        /// DataGenerator-object.
        /// </summary>
        private DataGenerator m_Generator = null;

        /// <summary>
        /// Sorted dictionary for the category-TreeView.
        /// </summary>
        private SortedDictionary<string, Category> m_CategoryList = new SortedDictionary<string, Category>();

        /// <summary>
        /// OpenFOAM-TreeView for default simulation parameter
        /// </summary>
        private readonly OpenFOAMTreeView m_OpenFOAMTreeView = new OpenFOAMTreeView();

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
        private readonly Autodesk.Revit.UI.UIApplication m_Revit = null;

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
        public OpenFOAMExportForm(Autodesk.Revit.UI.UIApplication revit)
        {
            InitializeComponent();
            m_Revit = revit;
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

            // comboBoxEnv
            var enumEnv = OpenFOAMEnvironment.blueCFD;
            foreach (var value in Enum.GetValues(enumEnv.GetType()))
            {
                comboBoxEnv.Items.Add(value);
            }
            comboBoxEnv.SelectedItem = enumEnv;

            // comboBoxSolver
            var enumSolver = SolverIncompressible.simpleFoam;
            foreach (var value in Enum.GetValues(enumSolver.GetType()))
            {
                comboBoxSolver.Items.Add(value);
            }
            comboBoxSolver.SelectedItem = enumSolver;
            //To-Do: ADD EVENT FOR CHANGING ENUM AND ALL PARAMETER THAT ARE RELATED TO SOLVER.

            // comboBoxTransportModel
            var enumTransport = TransportModel.Newtonian;
            foreach (var value in Enum.GetValues(enumTransport.GetType()))
            {
                comboBoxTransportModel.Items.Add(value);
            }
            comboBoxTransportModel.SelectedItem = enumTransport;

            // textBoxCPU
            textBoxCPU.Text = m_Settings.NumberOfSubdomains.ToString();
            //To-Do: ADD EVENT FOR CHANGING TEXT

            InitializeSSH();
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
                false, 0, 100, 1, 100, 0, 8, 6, 4, selectedCategories, dup);

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
            if(m_Settings == null)
            {
                return false;
            }

            bool succeed = false;

            //get duct-terminals in active document
            List<Element> ductTerminals = DataGenerator.GetDefaultCategoryListOfClass<FamilyInstance>(m_Revit.ActiveUIDocument.Document, BuiltInCategory.OST_DuctTerminal);

            foreach (FamilyInstance instance in ductTerminals)
            {
                double paramValue = 0;
                foreach (Parameter param in instance.Parameters)
                {
                    paramValue = 0;
                    try
                    {
                        paramValue = GetAirFlowValue(paramValue, param);
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
                        string nameDuctFam = famSym.Family.Name + "_" + instance.Id.ToString();
                        string nameDuct = nameDuctFam.Replace(" ", "_");
                        if (nameDuct.Contains("Abluft") || nameDuct.Contains("Outlet"))
                        {
                            m_Settings.Outlet.Add(nameDuct, new System.Windows.Media.Media3D.Vector3D(0, 0, paramValue));
                            succeed = true;
                        }
                        else if(nameDuct.Contains("Zuluft") || nameDuct.Contains("Inlet"))
                        {
                            m_Settings.Inlet.Add(nameDuct, new System.Windows.Media.Media3D.Vector3D(0, 0, -paramValue));
                            succeed = true;
                        }
                        break;
                    }
                }
            }
            return succeed;
        }

        /// <summary>
        /// Initialize paramValue with the airflow parameter as double.
        /// </summary>
        /// <param name="paramValue">Parameter value empty.</param>
        /// <param name="param">Parameter object.</param>
        /// <returns>Double.</returns>
        private static double GetAirFlowValue(double paramValue, Parameter param)
        {
            switch (param.Definition.ParameterType)
            {
                //volumeflow
                case ParameterType.HVACAirflow:
                    {
                        break;
                    }
                //velocity
                case ParameterType.HVACVelocity:
                    {
                        //convert into dot-comma convetion
                        paramValue = double.Parse(param.AsValueString().Trim(' ', 'm', '/', 's'), System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    }
                //pressure loss
                case ParameterType.HVACPressure:
                    {
                        break;
                    }
                    //****************ADD HER MORE PARAMETERTYPE TO HANDLE THEM****************//
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
            //m_OpenFOAMTreeView = new OpenFOAMTreeView();
            //InitializeOFTreeViewSize();
            //TO-DO: UPDATE OpenFOAMTreeView
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
            string helpfile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            helpfile = System.IO.Path.Combine(helpfile, "OpenFOAM_Export.chm");

            if (System.IO.File.Exists(helpfile) == false)
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
                SolverIncompressible appInc = (SolverIncompressible)comboBoxSolver.SelectedItem;
                m_Settings.AppIncompressible = appInc;

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
                    return;
                }

                // save Revit document's triangular data in a temporary file
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
        /// ValueChanged event for txtBoxUserIP.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void TxtBoxUserIP_ValueChanged(object sender, EventArgs e)
        {
            string txtBox = txtBoxUserIP.Text;

            if(m_RegUserIP.IsMatch(txtBox))
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

        /// <summary>
        /// ValueChanged event for txtBoxAlias.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void TxtBoxAlias_ValueChanged(object sender, EventArgs e)
        {
            string txt = txtBoxAlias.Text;
            SSH ssh = m_Settings.SSH;
            ssh.OfAlias = txt;
            m_Settings.SSH = ssh;

            //TO-DO: IF XML-CONFIG IMPLEMENTED => ADD CHANGES
        }

        /// <summary>
        /// ValueChanged for txtBoxServerCaseFolder.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void TxtBoxServerCaseFolder_ValueChanged(object sender, EventArgs e)
        {
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

        /// <summary>
        /// ValueChanged event for txtBoxPort.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void TxtBoxPort_ValueChanged(object sender, EventArgs e)
        {
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

            //TO-DO: IF XML-CONFIG IMPLEMENTED => ADD CHANGES
        }

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
    }
}
