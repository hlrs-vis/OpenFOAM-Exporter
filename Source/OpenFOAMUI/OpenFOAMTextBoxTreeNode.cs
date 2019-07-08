using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

namespace BIM.OpenFoamExport.OpenFOAMUI
{
    /// <summary>
    /// The class OpenFOAMTextBoxTreeNode is an child class from TreeNode and offers a additional Textbox.
    /// </summary>
    /// <typeparam name="T">Generic Value inside Textbox.</typeparam>
    public class OpenFOAMTextBoxTreeNode<T> : TreeNode
    {
        private T txtBoxValue;
        private string format;
        private TextBox txtBox = new TextBox();
        private List<string> keyPath;
        private Settings settings;

        #region
        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        /// </summary>
        public OpenFOAMTextBoxTreeNode()
            : base()
        {
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        ///// </summary>
        ///// <param name="text">The text.</param>
        //public OpenFOAMTreeNode(string text)
        //    : base(text)
        //{
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="children">The children.</param>
        public OpenFOAMTextBoxTreeNode(string text, TreeNode[] children)
            : base(text, children)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        /// </summary>
        /// <param name="serializationInfo">A <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> containing the data to deserialize the class.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> containing the source and destination of the serialized stream.</param>
        public OpenFOAMTextBoxTreeNode(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="imageIndex">Index of the image.</param>
        /// <param name="selectedImageIndex">Index of the selected image.</param>
        public OpenFOAMTextBoxTreeNode(string text, int imageIndex, int selectedImageIndex)
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
        public OpenFOAMTextBoxTreeNode(string text, int imageIndex, int selectedImageIndex, TreeNode[] children)
            : base(text, imageIndex, selectedImageIndex, children)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        /// </summary>
        /// <param name="_txtBoxValue">Type of value.</param>
        public OpenFOAMTextBoxTreeNode(T _txtBoxValue, ref Settings _settings, List<string> _keyPath)
            : base(_txtBoxValue.ToString().Replace(';', ' '))
        {
            txtBoxValue = _txtBoxValue;
            txtBox.Text = Text;


            settings = _settings;

            keyPath = new List<string>();
            foreach(string s in _keyPath)
            {
                keyPath.Add(s);
            }


            if (txtBoxValue is Vector3D)
            {
                format = "x y z -> x,y,z∊ℝ";
            }
            else if (txtBoxValue is Vector)
            {
                format = "x y -> x,y∊ℝ";
            }
            else if (txtBoxValue is int || txtBoxValue is double)
            {
                format = "int/double";
            }
            else
            {
                format = "pls initialize format for this valueType in OpenFOAMTextBoxTreeNode";
            }
        }
        #endregion

        /// <summary>
        /// Getter-Setter for value.
        /// </summary>
        public T TxtBoxValue
        {
            get
            {
                return txtBoxValue;
            }
            set
            {
                this.txtBoxValue = value;
                Dictionary<string, object> att = settings.SimulationDefault;
                foreach (string s in keyPath)
                {
                    if(att[s] is Dictionary<string,object>)
                    {
                        Dictionary<string, object> newLevel = att[s] as Dictionary<string, object>;
                        att = newLevel;
                    }
                    else
                    {
                        var refValue = att[s] as RefVar<double>;
                        refValue.RefV = Convert.ToDouble(txtBoxValue);
                    }
                    
                }

            }
        }

        /// <summary>
        /// Getter-Setter for TxtBox.
        /// </summary>
        public TextBox TxtBox
        {
            get
            {
                return txtBox;
            }
            set
            {
                txtBox = value;
            }
        }

        /// <summary>
        /// Getter for format-string.
        /// </summary>
        public string Format { get => format;}
    }
}
