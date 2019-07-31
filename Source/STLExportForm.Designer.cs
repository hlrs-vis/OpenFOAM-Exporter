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

namespace BIM.OpenFoamExport
{
    partial class STLExportForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(STLExportForm));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.tpCategories = new System.Windows.Forms.TabPage();
            this.tvCategories = new System.Windows.Forms.TreeView();
            this.btnCheckNone = new System.Windows.Forms.Button();
            this.btnCheckAll = new System.Windows.Forms.Button();
            this.tpGeneral = new System.Windows.Forms.TabPage();
            this.cbOpenFOAM = new System.Windows.Forms.CheckBox();
            this.cbExportSharedCoordinates = new System.Windows.Forms.CheckBox();
            this.cbExportColor = new System.Windows.Forms.CheckBox();
            this.comboBox_DUT = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbIncludeLinked = new System.Windows.Forms.CheckBox();
            this.gbSTLFormat = new System.Windows.Forms.GroupBox();
            this.rbAscii = new System.Windows.Forms.RadioButton();
            this.rbBinary = new System.Windows.Forms.RadioButton();
            this.tabOpenFOAM = new System.Windows.Forms.TabControl();
            this.tbOpenFOAM = new System.Windows.Forms.TabPage();
            this.gbDefault = new System.Windows.Forms.GroupBox();
            this.gbGeneral = new System.Windows.Forms.GroupBox();
            this.lblTransportModel = new System.Windows.Forms.Label();
            this.comboBoxTransportModel = new System.Windows.Forms.ComboBox();
            this.textBoxCPU = new System.Windows.Forms.TextBox();
            this.lblCPU = new System.Windows.Forms.Label();
            this.comboBoxSolver = new System.Windows.Forms.ComboBox();
            this.lblSolver = new System.Windows.Forms.Label();
            this.comboBoxEnv = new System.Windows.Forms.ComboBox();
            this.lblEnv = new System.Windows.Forms.Label();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.tabSSH = new System.Windows.Forms.TabPage();
            this.lblUserHost = new System.Windows.Forms.Label();
            this.lblAlias = new System.Windows.Forms.Label();
            this.lblCaseFolder = new System.Windows.Forms.Label();
            this.txtBoxUserIP = new System.Windows.Forms.TextBox();
            this.txtBoxAlias = new System.Windows.Forms.TextBox();
            this.txtBoxCaseFolder = new System.Windows.Forms.TextBox();
            this.cbDownload = new System.Windows.Forms.CheckBox();
            this.cbDelete = new System.Windows.Forms.CheckBox();
            this.tpCategories.SuspendLayout();
            this.tpGeneral.SuspendLayout();
            this.gbSTLFormat.SuspendLayout();
            this.tabOpenFOAM.SuspendLayout();
            this.tbOpenFOAM.SuspendLayout();
            this.gbGeneral.SuspendLayout();
            this.tabSSH.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnHelp
            // 
            resources.ApplyResources(this.btnHelp, "btnHelp");
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // btnSave
            // 
            resources.ApplyResources(this.btnSave, "btnSave");
            this.btnSave.Name = "btnSave";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // tpCategories
            // 
            this.tpCategories.BackColor = System.Drawing.SystemColors.Control;
            this.tpCategories.Controls.Add(this.tvCategories);
            this.tpCategories.Controls.Add(this.btnCheckNone);
            this.tpCategories.Controls.Add(this.btnCheckAll);
            resources.ApplyResources(this.tpCategories, "tpCategories");
            this.tpCategories.Name = "tpCategories";
            // 
            // tvCategories
            // 
            this.tvCategories.CheckBoxes = true;
            resources.ApplyResources(this.tvCategories, "tvCategories");
            this.tvCategories.Name = "tvCategories";
            // 
            // btnCheckNone
            // 
            resources.ApplyResources(this.btnCheckNone, "btnCheckNone");
            this.btnCheckNone.Name = "btnCheckNone";
            this.btnCheckNone.UseVisualStyleBackColor = true;
            this.btnCheckNone.Click += new System.EventHandler(this.btnCheckNone_Click);
            // 
            // btnCheckAll
            // 
            resources.ApplyResources(this.btnCheckAll, "btnCheckAll");
            this.btnCheckAll.Name = "btnCheckAll";
            this.btnCheckAll.UseVisualStyleBackColor = true;
            this.btnCheckAll.Click += new System.EventHandler(this.btnCheckAll_Click);
            // 
            // tpGeneral
            // 
            this.tpGeneral.BackColor = System.Drawing.SystemColors.Control;
            this.tpGeneral.Controls.Add(this.cbOpenFOAM);
            this.tpGeneral.Controls.Add(this.cbExportSharedCoordinates);
            this.tpGeneral.Controls.Add(this.cbExportColor);
            this.tpGeneral.Controls.Add(this.comboBox_DUT);
            this.tpGeneral.Controls.Add(this.label1);
            this.tpGeneral.Controls.Add(this.cbIncludeLinked);
            this.tpGeneral.Controls.Add(this.gbSTLFormat);
            resources.ApplyResources(this.tpGeneral, "tpGeneral");
            this.tpGeneral.Name = "tpGeneral";
            // 
            // cbOpenFOAM
            // 
            resources.ApplyResources(this.cbOpenFOAM, "cbOpenFOAM");
            this.cbOpenFOAM.Name = "cbOpenFOAM";
            this.cbOpenFOAM.UseVisualStyleBackColor = true;
            this.cbOpenFOAM.CheckedChanged += new System.EventHandler(this.cbOpenFOAM_CheckedChanged);
            // 
            // cbExportSharedCoordinates
            // 
            resources.ApplyResources(this.cbExportSharedCoordinates, "cbExportSharedCoordinates");
            this.cbExportSharedCoordinates.Name = "cbExportSharedCoordinates";
            this.cbExportSharedCoordinates.UseVisualStyleBackColor = true;
            // 
            // cbExportColor
            // 
            resources.ApplyResources(this.cbExportColor, "cbExportColor");
            this.cbExportColor.Name = "cbExportColor";
            this.cbExportColor.UseVisualStyleBackColor = true;
            // 
            // comboBox_DUT
            // 
            this.comboBox_DUT.FormattingEnabled = true;
            resources.ApplyResources(this.comboBox_DUT, "comboBox_DUT");
            this.comboBox_DUT.Name = "comboBox_DUT";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // cbIncludeLinked
            // 
            resources.ApplyResources(this.cbIncludeLinked, "cbIncludeLinked");
            this.cbIncludeLinked.Name = "cbIncludeLinked";
            this.cbIncludeLinked.UseVisualStyleBackColor = true;
            this.cbIncludeLinked.CheckedChanged += new System.EventHandler(this.cbIncludeLinked_CheckedChanged);
            // 
            // gbSTLFormat
            // 
            this.gbSTLFormat.Controls.Add(this.rbAscii);
            this.gbSTLFormat.Controls.Add(this.rbBinary);
            resources.ApplyResources(this.gbSTLFormat, "gbSTLFormat");
            this.gbSTLFormat.Name = "gbSTLFormat";
            this.gbSTLFormat.TabStop = false;
            // 
            // rbAscii
            // 
            resources.ApplyResources(this.rbAscii, "rbAscii");
            this.rbAscii.Name = "rbAscii";
            this.rbAscii.UseVisualStyleBackColor = true;
            // 
            // rbBinary
            // 
            this.rbBinary.Checked = true;
            resources.ApplyResources(this.rbBinary, "rbBinary");
            this.rbBinary.Name = "rbBinary";
            this.rbBinary.TabStop = true;
            this.rbBinary.UseVisualStyleBackColor = true;
            this.rbBinary.CheckedChanged += new System.EventHandler(this.rbExportFormat_CheckedChanged);
            // 
            // tabOpenFOAM
            // 
            this.tabOpenFOAM.Controls.Add(this.tpGeneral);
            this.tabOpenFOAM.Controls.Add(this.tpCategories);
            this.tabOpenFOAM.Controls.Add(this.tbOpenFOAM);
            this.tabOpenFOAM.Controls.Add(this.tabSSH);
            resources.ApplyResources(this.tabOpenFOAM, "tabOpenFOAM");
            this.tabOpenFOAM.Name = "tabOpenFOAM";
            this.tabOpenFOAM.SelectedIndex = 0;
            // 
            // tbOpenFOAM
            // 
            this.tbOpenFOAM.BackColor = System.Drawing.SystemColors.Control;
            this.tbOpenFOAM.Controls.Add(this.gbDefault);
            this.tbOpenFOAM.Controls.Add(this.gbGeneral);
            resources.ApplyResources(this.tbOpenFOAM, "tbOpenFOAM");
            this.tbOpenFOAM.Name = "tbOpenFOAM";
            // 
            // gbDefault
            // 
            resources.ApplyResources(this.gbDefault, "gbDefault");
            this.gbDefault.Name = "gbDefault";
            this.gbDefault.TabStop = false;
            // 
            // gbGeneral
            // 
            this.gbGeneral.Controls.Add(this.lblTransportModel);
            this.gbGeneral.Controls.Add(this.comboBoxTransportModel);
            this.gbGeneral.Controls.Add(this.textBoxCPU);
            this.gbGeneral.Controls.Add(this.lblCPU);
            this.gbGeneral.Controls.Add(this.comboBoxSolver);
            this.gbGeneral.Controls.Add(this.lblSolver);
            this.gbGeneral.Controls.Add(this.comboBoxEnv);
            this.gbGeneral.Controls.Add(this.lblEnv);
            this.gbGeneral.Controls.Add(this.vScrollBar1);
            resources.ApplyResources(this.gbGeneral, "gbGeneral");
            this.gbGeneral.Name = "gbGeneral";
            this.gbGeneral.TabStop = false;
            // 
            // lblTransportModel
            // 
            resources.ApplyResources(this.lblTransportModel, "lblTransportModel");
            this.lblTransportModel.Name = "lblTransportModel";
            // 
            // comboBoxTransportModel
            // 
            this.comboBoxTransportModel.FormattingEnabled = true;
            resources.ApplyResources(this.comboBoxTransportModel, "comboBoxTransportModel");
            this.comboBoxTransportModel.Name = "comboBoxTransportModel";
            // 
            // textBoxCPU
            // 
            resources.ApplyResources(this.textBoxCPU, "textBoxCPU");
            this.textBoxCPU.Name = "textBoxCPU";
            // 
            // lblCPU
            // 
            resources.ApplyResources(this.lblCPU, "lblCPU");
            this.lblCPU.Name = "lblCPU";
            // 
            // comboBoxSolver
            // 
            this.comboBoxSolver.FormattingEnabled = true;
            resources.ApplyResources(this.comboBoxSolver, "comboBoxSolver");
            this.comboBoxSolver.Name = "comboBoxSolver";
            // 
            // lblSolver
            // 
            resources.ApplyResources(this.lblSolver, "lblSolver");
            this.lblSolver.Name = "lblSolver";
            // 
            // comboBoxEnv
            // 
            this.comboBoxEnv.FormattingEnabled = true;
            resources.ApplyResources(this.comboBoxEnv, "comboBoxEnv");
            this.comboBoxEnv.Name = "comboBoxEnv";
            // 
            // lblEnv
            // 
            resources.ApplyResources(this.lblEnv, "lblEnv");
            this.lblEnv.Name = "lblEnv";
            // 
            // vScrollBar1
            // 
            resources.ApplyResources(this.vScrollBar1, "vScrollBar1");
            this.vScrollBar1.Name = "vScrollBar1";
            // 
            // tabSSH
            // 
            this.tabSSH.BackColor = System.Drawing.SystemColors.Control;
            this.tabSSH.Controls.Add(this.cbDelete);
            this.tabSSH.Controls.Add(this.cbDownload);
            this.tabSSH.Controls.Add(this.txtBoxCaseFolder);
            this.tabSSH.Controls.Add(this.txtBoxAlias);
            this.tabSSH.Controls.Add(this.txtBoxUserIP);
            this.tabSSH.Controls.Add(this.lblCaseFolder);
            this.tabSSH.Controls.Add(this.lblAlias);
            this.tabSSH.Controls.Add(this.lblUserHost);
            resources.ApplyResources(this.tabSSH, "tabSSH");
            this.tabSSH.Name = "tabSSH";
            // 
            // lblUserHost
            // 
            resources.ApplyResources(this.lblUserHost, "lblUserHost");
            this.lblUserHost.Name = "lblUserHost";
            // 
            // lblAlias
            // 
            resources.ApplyResources(this.lblAlias, "lblAlias");
            this.lblAlias.Name = "lblAlias";
            // 
            // lblCaseFolder
            // 
            resources.ApplyResources(this.lblCaseFolder, "lblCaseFolder");
            this.lblCaseFolder.Name = "lblCaseFolder";
            // 
            // txtBoxUserIP
            // 
            resources.ApplyResources(this.txtBoxUserIP, "txtBoxUserIP");
            this.txtBoxUserIP.Name = "txtBoxUserIP";
            // 
            // txtBoxAlias
            // 
            resources.ApplyResources(this.txtBoxAlias, "txtBoxAlias");
            this.txtBoxAlias.Name = "txtBoxAlias";
            // 
            // txtBoxCaseFolder
            // 
            resources.ApplyResources(this.txtBoxCaseFolder, "txtBoxCaseFolder");
            this.txtBoxCaseFolder.Name = "txtBoxCaseFolder";
            // 
            // cbDownload
            // 
            resources.ApplyResources(this.cbDownload, "cbDownload");
            this.cbDownload.Checked = true;
            this.cbDownload.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDownload.Name = "cbDownload";
            this.cbDownload.UseVisualStyleBackColor = true;
            // 
            // cbDelete
            // 
            resources.ApplyResources(this.cbDelete, "cbDelete");
            this.cbDelete.Checked = true;
            this.cbDelete.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDelete.Name = "cbDelete";
            this.cbDelete.UseVisualStyleBackColor = true;
            // 
            // STLExportForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tabOpenFOAM);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "STLExportForm";
            this.tpCategories.ResumeLayout(false);
            this.tpGeneral.ResumeLayout(false);
            this.tpGeneral.PerformLayout();
            this.gbSTLFormat.ResumeLayout(false);
            this.tabOpenFOAM.ResumeLayout(false);
            this.tbOpenFOAM.ResumeLayout(false);
            this.gbGeneral.ResumeLayout(false);
            this.gbGeneral.PerformLayout();
            this.tabSSH.ResumeLayout(false);
            this.tabSSH.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TabPage tpCategories;
        private System.Windows.Forms.TreeView tvCategories;
        private System.Windows.Forms.Button btnCheckNone;
        private System.Windows.Forms.Button btnCheckAll;
        private System.Windows.Forms.TabPage tpGeneral;
        private System.Windows.Forms.CheckBox cbExportSharedCoordinates;
        private System.Windows.Forms.CheckBox cbExportColor;
        private System.Windows.Forms.ComboBox comboBox_DUT;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbIncludeLinked;
        private System.Windows.Forms.GroupBox gbSTLFormat;
        private System.Windows.Forms.RadioButton rbAscii;
        private System.Windows.Forms.RadioButton rbBinary;
        private System.Windows.Forms.TabControl tabOpenFOAM;
        private System.Windows.Forms.CheckBox cbOpenFOAM;
        private System.Windows.Forms.TabPage tbOpenFOAM;
        private System.Windows.Forms.GroupBox gbDefault;
        private System.Windows.Forms.GroupBox gbGeneral;
        private System.Windows.Forms.ComboBox comboBoxEnv;
        private System.Windows.Forms.Label lblEnv;
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private System.Windows.Forms.ComboBox comboBoxSolver;
        private System.Windows.Forms.Label lblSolver;
        private System.Windows.Forms.TextBox textBoxCPU;
        private System.Windows.Forms.Label lblCPU;
        private System.Windows.Forms.Label lblTransportModel;
        private System.Windows.Forms.ComboBox comboBoxTransportModel;
        private System.Windows.Forms.TabPage tabSSH;
        private System.Windows.Forms.CheckBox cbDelete;
        private System.Windows.Forms.CheckBox cbDownload;
        private System.Windows.Forms.TextBox txtBoxCaseFolder;
        private System.Windows.Forms.TextBox txtBoxAlias;
        private System.Windows.Forms.TextBox txtBoxUserIP;
        private System.Windows.Forms.Label lblCaseFolder;
        private System.Windows.Forms.Label lblAlias;
        private System.Windows.Forms.Label lblUserHost;
    }
}
