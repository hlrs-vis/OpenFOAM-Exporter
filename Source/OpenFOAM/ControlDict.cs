using System.Collections.Generic;

namespace BIM.OpenFoamExport.OpenFOAM
{
    /// <summary>
    /// The ControlDict-Class represents the controlDict in the OpenFOAM-System folder.
    /// </summary>
    public class ControlDict : FoamDict
    {
        /// <summary>
        /// additional functions
        /// </summary>
        private Dictionary<string, object> m_Functions;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="version">Version-object.</param>
        /// <param name="path">Path to this file.</param>
        /// <param name="attributes">Additional attributes.</param>
        /// <param name="format">Ascii or Binary</param>
        /// <param name="settings">Settings-object</param>
        /// <param name="_functions">Additional functions as string</param>
        public ControlDict(Version version, string path, Dictionary<string, object> attributes, SaveFormat format, Settings settings, string _functions)
            : base("controlDict" , "dictionary", version, path, attributes, format, settings)
        {
            m_Functions = new Dictionary<string, object>();

            InitAttributes();
        }

        /// <summary>
        /// Initialize attributes of this file.
        /// </summary>
        public override void InitAttributes()
        {
            base.InitAttributes();
            FoamFile.Attributes.Add("functions", m_Functions);
        }
        
        /// <summary>
        /// Initializes functions dictionary.
        /// </summary>
        private void InitFunction()
        {
            //TO-DO: Implement later.
        }
    }
}
