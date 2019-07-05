//*********************************************************************************************************************************//
// Source Code: https://www.codeproject.com/Articles/14544/A-TreeView-Control-with-ComboBox-Dropdown-Nodes
// Additional Code : m_CurrentOFTreeNode, TextBod_TextChanged, TextBox_Leave, TextBox_Click, HideComboBox, TextBox_ChangeValue
// Modified Code: OnNodeMousClick, OnMouseWheel
// Modified by Marko Djuric
//*********************************************************************************************************************************//

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

namespace BIM.OpenFoamExport.OpenFOAMUI
{
    /// <summary>
    /// Use this class for list <see cref="T:OpenFOAMTreeNode"/> and <see cref="T:OpenFOAMDropDownTreeNode"/> in a <see cref="T:TreeView"/>.
    /// </summary>
    public class OpenFOAMTreeView : TreeView
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenFOAMTreeView"/> class.
        /// </summary>
        public OpenFOAMTreeView()
            : base()
        {

        }
        #endregion

        // We'll use this variable to keep track of the current node that is being edited.
        // This is set to something (non-null) only if the node's ComboBox is being displayed.
        private OpenFOAMDropDownTreeNode m_CurrentNode = null;

        private OpenFOAMTextBoxTreeNode<dynamic> m_CurrentOFTreeNode = null;

        private bool m_ChangeValue=false;

        private Regex m_Vector3DReg = new Regex(@"^\d+\s+\d+\s+\d+$");
        private Regex m_VectorReg = new Regex(@"^\d+\s+\d+$");
        private Regex m_SingleReg = new Regex(@"^\d+");

        /// <summary>
        /// Occurs when the <see cref="E:System.Windows.Forms.TreeView.NodeMouseClick"></see> event is fired
        /// -- that is, when a node in the tree view is clicked.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.TreeNodeMouseClickEventArgs"></see> that contains the event data.</param>
        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {
            // Are we dealing with a dropdown node?
            if (e.Node is OpenFOAMDropDownTreeNode)
            {
                m_CurrentNode = (OpenFOAMDropDownTreeNode)e.Node;

                // Need to add the node's ComboBox to the TreeView's list of controls for it to work
                Controls.Add(m_CurrentNode.ComboBox);

                // Set the bounds of the ComboBox, with a little adjustment to make it look right
                m_CurrentNode.ComboBox.SetBounds(
                    m_CurrentNode.Bounds.X - 1,
                    m_CurrentNode.Bounds.Y - 2,
                    m_CurrentNode.Bounds.Width + 25,
                    m_CurrentNode.Bounds.Height);

                // Listen to the SelectedValueChanged event of the node's ComboBox
                m_CurrentNode.ComboBox.SelectedValueChanged += new EventHandler(ComboBox_SelectedValueChanged);
                m_CurrentNode.ComboBox.DropDownClosed += new EventHandler(ComboBox_DropDownClosed);

                // Now show the ComboBox
                m_CurrentNode.ComboBox.Show();
                m_CurrentNode.ComboBox.DroppedDown = true;
            }
            else if (e.Node is OpenFOAMTextBoxTreeNode<dynamic>)
            {
                if (m_CurrentOFTreeNode != (OpenFOAMTextBoxTreeNode<dynamic>)e.Node)
                {
                    HideTextBox();
                }

                m_CurrentOFTreeNode = (OpenFOAMTextBoxTreeNode<dynamic>)e.Node;

                Controls.Add(m_CurrentOFTreeNode.TxtBox);

                m_CurrentOFTreeNode.TxtBox.SetBounds(
                    m_CurrentOFTreeNode.Bounds.X - 1,
                    m_CurrentOFTreeNode.Bounds.Y - 2,
                    m_CurrentOFTreeNode.Bounds.Width + 25,
                    m_CurrentOFTreeNode.Bounds.Height);

                m_CurrentOFTreeNode.TxtBox.TextChanged += new EventHandler(TextBox_TextChanged);
                m_CurrentOFTreeNode.TxtBox.Click += new EventHandler(TextBox_Click);
                m_CurrentOFTreeNode.TxtBox.MouseLeave += new EventHandler(TextBox_Leave);

                m_CurrentOFTreeNode.TxtBox.Show();
            }
            else
            {
                HideTextBox();
                HideComboBox();
            }
            base.OnNodeMouseClick(e);
        }

        /// <summary>
        /// Handles the SelectedValueChanged event of the ComboBox control.
        /// Hides the ComboBox if an item has been selected in it.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        void ComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            HideComboBox();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sende"></param>
        /// <param name="e"></param>
        void TextBox_TextChanged(object sender, EventArgs e)
        {
            m_CurrentOFTreeNode.TxtBox.MouseLeave += new EventHandler(TextBox_Leave);
            m_ChangeValue = true;
        }

        /// <summary>
        /// Handles Leave event from Textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBox_Leave(object sender, EventArgs e)
        {
            HideTextBox();
        }

        /// <summary>
        /// Handles Click event of Textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBox_Click(object sender, EventArgs e)
        {
            m_CurrentOFTreeNode.TxtBox.MouseLeave -= TextBox_Leave;
        }

        /// <summary>
        /// Handles the DropDownClosed event of the ComboBox control.
        /// Hides the ComboBox if the user clicks anywhere else on the TreeView or adjusts the scrollbars, or scrolls the mouse wheel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            HideComboBox();
        }


        /// <summary>
        /// Handles the <see cref="E:System.Windows.Forms.Control.MouseWheel"></see> event.
        /// Hides the ComboBox if the user scrolls the mouse wheel.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            HideComboBox();
            HideTextBox();
            base.OnMouseWheel(e);
        }


        /// <summary>
        /// Method to hide the currently-selected node's ComboBox
        /// </summary>
        private void HideComboBox()
        {
            if (m_CurrentNode != null)
            {
                // Unregister the event listener
                m_CurrentNode.ComboBox.SelectedValueChanged -= ComboBox_SelectedValueChanged;
                m_CurrentNode.ComboBox.DropDownClosed -= ComboBox_DropDownClosed;

                // Copy the selected text from the ComboBox to the TreeNode
                m_CurrentNode.Text = m_CurrentNode.ComboBox.Text;

                // Hide the ComboBox
                m_CurrentNode.ComboBox.Hide();
                m_CurrentNode.ComboBox.DroppedDown = false;

                // Remove the control from the TreeView's list of currently-displayed controls
                Controls.Remove(m_CurrentNode.ComboBox);

                // And return to the default state (no ComboBox displayed)
                m_CurrentNode = null;
            }

        }

        /// <summary>
        /// Method to hide the currently-selected node TextBox
        /// </summary>
        private void HideTextBox()
        {
            if(m_CurrentOFTreeNode != null)
            {
                TextBox_ChangeValue();
                m_ChangeValue = false;
                m_CurrentOFTreeNode.TxtBox.TextChanged -= TextBox_TextChanged;
                m_CurrentOFTreeNode.Text = m_CurrentOFTreeNode.TxtBox.Text;
                m_CurrentOFTreeNode.TxtBox.Hide();
                Controls.Remove(m_CurrentOFTreeNode.TxtBox);
                m_CurrentOFTreeNode = null;
            }
        }

        /// <summary>
        /// Changes value of current OpenFOAMTextBoxTreeNode.
        /// </summary>
        private void TextBox_ChangeValue()
        {
            if(!m_ChangeValue)
            {
                return;
            }
            string valueString = m_CurrentOFTreeNode.TxtBox.Text;
            try
            {
                //Vector3D ( d d d )
                if (m_Vector3DReg.IsMatch(valueString) && m_CurrentOFTreeNode.Value is Vector3D)
                {
                    List<double> entries = GetListFromVectorString(valueString);
                    m_CurrentOFTreeNode.Value = new Vector3D(entries[0], entries[1], entries[2]);
                }
                //Vector ( d d )
                else if (m_VectorReg.IsMatch(valueString) && m_CurrentOFTreeNode.Value is Vector)
                {
                    List<double> entries = GetListFromVectorString(valueString);
                    m_CurrentOFTreeNode.Value = new Vector(entries[0], entries[1]);
                }
                //double / integer ( d )
                else if (m_SingleReg.IsMatch(valueString))
                {
                    string value = valueString.Trim();
                    if(m_CurrentOFTreeNode.Value is int)
                    {
                        int j = Convert.ToInt32(value);
                        m_CurrentOFTreeNode.Value = j;
                    }
                    else
                    {
                        double j = Convert.ToDouble(value);
                        m_CurrentOFTreeNode.Value = j;
                    }
                }
                else
                {
                    FormatException format = new FormatException();
                    throw format;
                }
            }
            catch (FormatException)
            {
                System.Windows.Forms.MessageBox.Show(OpenFoamExportResource.ERR_FORMAT
                    + " " + valueString + "\nFormat: " + m_CurrentOFTreeNode.Format
                    , OpenFoamExportResource.MESSAGE_BOX_TITLE,
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vecString"></param>
        /// <returns></returns>
        private List<double> GetListFromVectorString(string vecString)
        {
            if(vecString.Equals("")|| vecString == string.Empty)
            {
                return null;
            }

            List<double> entries = new List<double>();
            double j = 0;
            foreach (string s in vecString.Split(' '))
            {
                s.Trim();
                if (s.Equals(""))
                {
                    continue;
                }
                j = Convert.ToDouble(s);
                entries.Add(j);
            }
            return entries;
        }
    }
}
