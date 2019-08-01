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
using BIM.OpenFoamExport.OpenFOAMUI;
using System.Windows.Forms;

using Category = Autodesk.Revit.DB.Category;
using Autodesk.Revit.DB;
using System.Text.RegularExpressions;

namespace BIM.OpenFoamExport
{
    public partial class STLExportForm : System.Windows.Forms.Form
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
        private OpenFOAMTreeView m_OpenFOAMTreeView = new OpenFOAMTreeView();

        /// <summary>
        /// Sorted dictionary for the unity properties that can be set in a drop down menu.
        /// </summary>
        private SortedDictionary<string, DisplayUnitType> m_DisplayUnits = new SortedDictionary<string, DisplayUnitType>();

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
        readonly Autodesk.Revit.UI.UIApplication m_Revit = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="revit">The revit application.</param>
        public STLExportForm(Autodesk.Revit.UI.UIApplication revit)
        {
            InitializeComponent();
            m_Revit = revit;
            tbSSH.Enabled = false;
            tbOpenFOAM.Enabled = false;

            //set size for OpenFOAMTreeView
            Size sizeOpenFoamTreeView = gbDefault.Size;
            sizeOpenFoamTreeView.Height -= 20;
            sizeOpenFoamTreeView.Width -= 20;
            m_OpenFOAMTreeView.Bounds = gbDefault.Bounds;
            m_OpenFOAMTreeView.Size = sizeOpenFoamTreeView;
            m_OpenFOAMTreeView.Top += 10;

            //add OpenFOAMTreeView to container default
            gbDefault.Controls.Add(m_OpenFOAMTreeView);

            // get new data generator
            m_Generator = new DataGenerator(m_Revit.Application, m_Revit.ActiveUIDocument.Document, m_Revit.ActiveUIDocument.Document.ActiveView);

            // scan for categories to populate category list
            m_CategoryList = m_Generator.ScanCategories(true);

            foreach (Category category in m_CategoryList.Values)
            {
                TreeNode treeNode = GetChildNode(category,m_Revit.ActiveUIDocument.Document.ActiveView);
                if (treeNode != null)
                    tvCategories.Nodes.Add(treeNode);                               
            }

            //intialize unit
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

            // initialize the UI differently for Families
            if (revit.ActiveUIDocument.Document.IsFamilyDocument)
            {
                cbIncludeLinked.Enabled = false;
                tvCategories.Enabled = false;
                btnCheckAll.Enabled = false;
                btnCheckNone.Enabled = false;

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
            m_SelectedDUT = dup;

            saveFormat = SaveFormat.ascii;

            // create settings object to save setting information
            m_Settings = new Settings(saveFormat, exportRange, cbOpenFOAM.Checked, cbIncludeLinked.Checked, cbExportColor.Checked, cbExportSharedCoordinates.Checked,
                false, 0, 100, 1, 100, 0, 8, 6, 4, selectedCategories, dup);

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
            foreach(var value in Enum.GetValues(enumTransport.GetType()))
            {
                comboBoxTransportModel.Items.Add(value);
            }
            comboBoxTransportModel.SelectedItem = enumTransport;

            // textBoxCPU
            textBoxCPU.Text = m_Settings.NumberOfSubdomains.ToString();
            //To-Do: ADD EVENT FOR CHANGING TEXT

            txtBoxUserIP.Text = m_Settings.SSH.ConnectionString();
            txtBoxAlias.Text = m_Settings.SSH.OfAlias;
            txtBoxCaseFolder.Text = m_Settings.SSH.ServerCaseFolder;
            txtBoxPort.Text = m_Settings.SSH.Port.ToString();
            cbDelete.Checked = m_Settings.SSH.Delete;
            cbDownload.Checked = m_Settings.SSH.Download;
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

            TreeNode treeNode = new TreeNode(parent);
            treeNode.Tag = parent;

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

            TreeNode treeNode = new TreeNode(category.Name);
            treeNode.Tag = category;
            treeNode.Checked = true;

            if(category.SubCategories.Size == 0)
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
        private void btnHelp_Click(object sender, EventArgs e)
        {
            string helpfile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            helpfile = System.IO.Path.Combine(helpfile, "OpenFoam_Export.chm");

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
        private void btnSave_Click(object sender, EventArgs e)
        {
            string fileName = OpenFoamDialogManager.SaveDialog();

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
                int cpu = 0;
                if(int.TryParse(textBoxCPU.Text, out cpu))
                {
                    m_Settings.NumberOfSubdomains = cpu;
                }
                else
                {
                    MessageBox.Show("Please type in the number of subdomains (CPU) for the simulation");
                    return;
                }

                // create settings object to save setting information
                //Settings aSetting = new Settings(saveFormat, exportRange, cbOpenFOAM.Checked, cbIncludeLinked.Checked, cbExportColor.Checked, cbExportSharedCoordinates.Checked,
                //    false, 0, 100, 1, 100, 0, 8, 6, 4, selectedCategories, dup);

                // save Revit document's triangular data in a temporary file
                m_Generator = new DataGenerator(m_Revit.Application, m_Revit.ActiveUIDocument.Document, m_Revit.ActiveUIDocument.Document.ActiveView);
                DataGenerator.GeneratorStatus succeed = m_Generator.SaveSTLFile(fileName, m_Settings/*aSetting*/);

                if (succeed == DataGenerator.GeneratorStatus.FAILURE)
                {
                    this.DialogResult = DialogResult.Cancel;
                    MessageBox.Show(OpenFoamExportResource.ERR_SAVE_FILE_FAILED, OpenFoamExportResource.MESSAGE_BOX_TITLE,
                             MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else if (succeed == DataGenerator.GeneratorStatus.CANCEL)
                {
                    this.DialogResult = DialogResult.Cancel;
                    MessageBox.Show(OpenFoamExportResource.CANCEL_FILE_NOT_SAVED, OpenFoamExportResource.MESSAGE_BOX_TITLE,
                             MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                this.DialogResult = DialogResult.OK;

                this.Close();
            }
            else
            {
                MessageBox.Show(OpenFoamExportResource.CANCEL_FILE_NOT_SAVED, OpenFoamExportResource.MESSAGE_BOX_TITLE,
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
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Check all button click event, checks all categories in the category list.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void btnCheckAll_Click(object sender, EventArgs e)
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
        private void btnCheckNone_Click(object sender, EventArgs e)
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
        private void cbIncludeLinked_CheckedChanged(object sender, EventArgs e)
        {
            if (cbIncludeLinked.Checked == true)
            {
                MessageBox.Show(OpenFoamExportResource.WARN_PROJECT_POSITION, OpenFoamExportResource.MESSAGE_BOX_TITLE,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbExportFormat_CheckedChanged(object sender, EventArgs e)
        {
            cbExportColor.Enabled = rbBinary.Checked;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbOpenFOAM_CheckedChanged(object sender, EventArgs e)
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
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxEnv_SelectedValueChanged(object sender, EventArgs e)
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
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBoxUserIP_ValueChanged(object sender, EventArgs e)
        {
            string txtUser = string.Empty;
            string txtIP = string.Empty;
            string txtBox = txtBoxUserIP.Text;

            Regex reg = new Regex("^\\S+@\\S+$");

            if(reg.IsMatch(txtBox))
            {
                txtUser = txtBox.Split('@')[0];
                txtIP = txtBox.Split('@')[1];
                SSH ssh = m_Settings.SSH;
                ssh.User = txtUser;
                ssh.ServerIP = txtIP;
                m_Settings.SSH = ssh;
            }
            else
            {
                MessageBox.Show(OpenFoamExportResource.ERR_FORMAT + " " + txtBox, OpenFoamExportResource.MESSAGE_BOX_TITLE,
                             MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            //TO-DO: IF XML-CONFIG IMPLEMENTED => ADD CHANGES
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBoxAlias_ValueChanged(object sender, EventArgs e)
        {
            string txt = txtBoxAlias.Text;
            SSH ssh = m_Settings.SSH;
            ssh.OfAlias = txt;
            m_Settings.SSH = ssh;
            //TO-DO: IF XML-CONFIG IMPLEMENTED => ADD CHANGES
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBoxServerCaseFolder_ValueChanged(object sender, EventArgs e)
        {
            string txt = string.Empty;
            string txtBox = txtBoxCaseFolder.Text;
            Regex reg = new Regex("^\\S+$");

            if (reg.IsMatch(txtBox))
            {
                txt = txtBox;
                SSH ssh = m_Settings.SSH;
                ssh.ServerCaseFolder = txt;
                m_Settings.SSH = ssh;
            }
            else
            {
                MessageBox.Show(OpenFoamExportResource.ERR_FORMAT + " " + txtBox, OpenFoamExportResource.MESSAGE_BOX_TITLE,
                             MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            //TO-DO: IF XML-CONFIG IMPLEMENTED => ADD CHANGES

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBoxPort_ValueChanged(object sender, EventArgs e)
        {
            string txt = string.Empty;
            string txtBox = txtBoxPort.Text;
            Regex reg = new Regex("^\\d+$");

            if (reg.IsMatch(txtBox))
            {
                txt = txtBox;
                SSH ssh = m_Settings.SSH;
                ssh.Port = Convert.ToInt32(txt);
                m_Settings.SSH = ssh;
            }
            else
            {
                MessageBox.Show(OpenFoamExportResource.ERR_FORMAT + " " + txtBox, OpenFoamExportResource.MESSAGE_BOX_TITLE,
                             MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            //TO-DO: IF XML-CONFIG IMPLEMENTED => ADD CHANGES
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDownload_ValueChanged(object sender, EventArgs e)
        {
            SSH ssh = m_Settings.SSH;
            ssh.Download = cbDownload.Checked;
            m_Settings.SSH = ssh;
            //TO-DO: IF XML-CONFIG IMPLEMENTED => ADD CHANGES
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDelete_ValueChanged(object sender, EventArgs e)
        {
            SSH ssh = m_Settings.SSH;
            ssh.Delete = cbDelete.Checked;
            m_Settings.SSH = ssh;
            //TO-DO: IF XML-CONFIG IMPLEMENTED => ADD CHANGES
        }
    }
}
