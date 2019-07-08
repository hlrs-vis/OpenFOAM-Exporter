//*********************************************************************************************************************************//
// Source Code: https://www.codeproject.com/Articles/14544/A-TreeView-Control-with-ComboBox-Dropdown-Nodes
// Additional Code: OpenFOAMDropDownTreeNode(Enum @enum)
// Modified by Marko Djuric
//*********************************************************************************************************************************//

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace BIM.OpenFoamExport.OpenFOAMUI
{
    /// <summary>
    /// This class is in use for list OpenFOAM-Parameter as a dropdownlist.
    /// </summary>
    public class OpenFOAMDropDownTreeNode : OpenFOAMTreeNode<Enum>
    {
        /// <summary>
        /// ComboBox-Object
        /// </summary>
        private ComboBox m_ComboBox = new ComboBox();

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        /// </summary>
        /// <param name="enum">Stored value in node.</param>
        /// <param name="_settings">Settings-object.</param>
        /// <param name="_keyPath">Path to value in dictionary in settings.</param>
        public OpenFOAMDropDownTreeNode(Enum @enum, ref Settings _settings, List<string> _keyPath)
            : base(@enum.ToString(), ref _settings, _keyPath, @enum)
        {
            foreach (var value in Enum.GetValues(@enum.GetType()))
            {
                m_ComboBox.Items.Add(value);
            }
        }
        #endregion

        /// <summary>
        /// Getter-Setter for ComboBox.
        /// </summary>
        public ComboBox ComboBox
        {
            get
            {
                m_ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                return m_ComboBox;
            }
            set
            {
                m_ComboBox = value;
                m_ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            }
        }
    }
}
