﻿//*********************************************************************************************************************************//
// Source Code: https://www.codeproject.com/Articles/14544/A-TreeView-Control-with-ComboBox-Dropdown-Nodes
// Additional Code: OpenFOAMDropDownTreeNode(Enum @enum)
// Modified by Marko Djuric
//*********************************************************************************************************************************//

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BIM.OpenFoamExport.OpenFOAMUI
{
    /// <summary>
    /// This class is in use for list OpenFOAM-Parameter as a dropdownlist.
    /// </summary>
    public class OpenFOAMDropDownTreeNode<T> : OpenFOAMTreeNode<T>
    {
        /// <summary>
        /// ComboBox-Object
        /// </summary>
        private ComboBox m_ComboBox = new ComboBox();

        #region Constructors
        ///// <summary>
        ///// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        ///// </summary>
        ///// <param name="enum">Stored value in node.</param>
        ///// <param name="_settings">Settings-object.</param>
        ///// <param name="_keyPath">Path to value in dictionary in settings.</param>
        //public OpenFOAMDropDownTreeNode(T @enum, ref Settings _settings, List<string> _keyPath)
        //    : base(@enum.ToString(), ref _settings, _keyPath, @enum)
        //{
        //    foreach (var value in Enum.GetValues(@enum.GetType()))
        //    {
        //        m_ComboBox.Items.Add(value);
        //    }
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        /// </summary>
        /// <param name="enum">Stored value in node.</param>
        /// <param name="_settings">Settings-object.</param>
        /// <param name="_keyPath">Path to value in dictionary in settings.</param>
        public OpenFOAMDropDownTreeNode(T _value, ref Settings _settings, List<string> _keyPath)
            : base(_value.ToString(), ref _settings, _keyPath, _value)
        {
            if(_value is Enum)
            {
                var @enum = _value as Enum;
                foreach (var value in Enum.GetValues(@enum.GetType()))
                {
                    m_ComboBox.Items.Add(value);
                }
            }
            else if(_value is bool)
            {
                bool? _bool = _value as bool?;
                if(_bool != null)
                {
                    m_ComboBox.Items.Add(_bool);
                    m_ComboBox.Items.Add(!_bool);
                }
            }
            else
            {
                m_ComboBox.Items.Add("Not initialized in OpenFOAMDropDownTreeNode");
            }

        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="T:OpenFOAMDropDownTreeNode"/> class.
        ///// </summary>
        ///// <param name="_bool">Stored bool value in node.</param>
        ///// <param name="_settings">Settings-object.</param>
        ///// <param name="_keyPath">Path to value in dictionary in settings.</param>
        //public OpenFOAMDropDownTreeNode(bool _bool, ref Settings _settings, List<string> _keyPath)
        //    : base(_bool.ToString(), ref _settings, _keyPath, _bool)
        //{
        //    m_ComboBox.Items.Add(_bool);
        //    m_ComboBox.Items.Add(!_bool);
        //}
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
