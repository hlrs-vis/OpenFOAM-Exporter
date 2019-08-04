using System.Collections.Generic;

namespace BIM.OpenFOAMExport.OpenFOAM
{
    /// <summary>
    /// This class is represantive for the fvSolution-Dictionary in the system folder of the openFOAM-case-folder.
    /// </summary>
    public class FvSolution : FOAMDict
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="version">Version-object.</param>
        /// <param name="path">Path to this file.</param>
        /// <param name="attributes">Additional attributes.</param>
        /// <param name="format">Ascii or Binary</param>
        /// <param name="settings">Settings-object</param>
        public FvSolution(Version version, string path, Dictionary<string, object> attributes, SaveFormat format, Settings settings)
            : base("fvSolution", "dictionary", version, path, attributes, format, settings)
        {
            InitAttributes();
        }

        /// <summary>
        /// Initialize attributes of this file.
        /// </summary>
        public override void InitAttributes()
        {
            base.InitAttributes();
        }
    }
}
