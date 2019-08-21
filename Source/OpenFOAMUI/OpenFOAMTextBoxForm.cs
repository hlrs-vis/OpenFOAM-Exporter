using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BIM.OpenFOAMExport.OpenFOAMUI
{
    /// <summary>
    /// Class that uses Form as base class combined with a textBox.
    /// </summary>
    public partial class OpenFOAMTextBoxForm : Form
    {
        /// <summary>
        /// TextBox.
        /// </summary>
        private TextBox m_TxtBox = new TextBox();

        /// <summary>
        /// Regular expression for text in textBox.
        /// </summary>
        private readonly Regex m_RegTxt;

        #region Constructor.

        /// <summary>
        /// Default constructor.
        /// </summary>
        public OpenFOAMTextBoxForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize Regular Expression with this constructor.
        /// </summary>
        /// <param name="reg">Regex object.</param>
        public OpenFOAMTextBoxForm(Regex reg, string text)
        {
            m_RegTxt = reg;
            InitializeComponent();
            InitializeTextBox(text);
        }
        #endregion

        /// <summary>
        /// Initialize Textbox.
        /// </summary>
        /// <param name="text">string for text</param>
        private void InitializeTextBox(string text)
        {
            m_TxtBox = textBox1;
            m_TxtBox.Text = text;
        }

        /// <summary>
        /// Set the text for lblText.
        /// </summary>
        /// <param name="txt">string for text.</param>
        public void SetLBLText(string txt)
        {
            lblTxt.Text = txt;
        }

        /// <summary>
        /// Set the text for lblVaribal
        /// </summary>
        /// <param name="txt">string for text</param>
        public void SetLBLVariable(string txt)
        {
            lblEnvironmentVariable.Text = txt;
        }
        /// <summary>
        /// Getter-Setter for textBox.
        /// </summary>
        public TextBox TxtBox
        {
            get
            {
                return m_TxtBox;
            }
            set
            {
                m_TxtBox = value;
            }
        }
    }
}
