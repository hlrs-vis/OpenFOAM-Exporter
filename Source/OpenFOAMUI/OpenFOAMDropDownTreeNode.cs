//*********************************************************************************************************************************//
// Source Code: https://www.codeproject.com/Articles/14544/A-TreeView-Control-with-ComboBox-Dropdown-Nodes
// Additional Code: OpenFOAMDropDownTreeNode(Enum @enum)
// Modified by Marko Djuric
//*********************************************************************************************************************************//

using System;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace BIM.OpenFoamExport.OpenFOAMUI
{
    /// <summary>
    /// This class is in use for list OpenFOAM-Parameter as a dropdownlist.
    /// </summary>
    public class OpenFOAMDropDownTreeNode : TreeNode
    {
        private ComboBox m_ComboBox = new ComboBox();

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        /// </summary>
        public OpenFOAMDropDownTreeNode()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        public OpenFOAMDropDownTreeNode(string text)
            : base(text)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="children">The children.</param>
        public OpenFOAMDropDownTreeNode(string text, TreeNode[] children)
            : base(text, children)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        /// </summary>
        /// <param name="serializationInfo">A <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> containing the data to deserialize the class.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> containing the source and destination of the serialized stream.</param>
        public OpenFOAMDropDownTreeNode(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="imageIndex">Index of the image.</param>
        /// <param name="selectedImageIndex">Index of the selected image.</param>
        public OpenFOAMDropDownTreeNode(string text, int imageIndex, int selectedImageIndex)
            : base(text, imageIndex, selectedImageIndex)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="imageIndex">Index of the image.</param>
        /// <param name="selectedImageIndex">Index of the selected image.</param>
        /// <param name="children">The children.</param>
        public OpenFOAMDropDownTreeNode(string text, int imageIndex, int selectedImageIndex, TreeNode[] children)
            : base(text, imageIndex, selectedImageIndex, children)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        /// </summary>
        /// <param name="enum">Enum-object</param>
        public OpenFOAMDropDownTreeNode(Enum @enum)
            : base(@enum.ToString())
        {
            foreach (var value in Enum.GetValues(@enum.GetType()))
            {
                m_ComboBox.Items.Add(value.ToString());
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
