using System.Collections.Generic;

namespace BIM.OpenFOAMExport.OpenFOAM
{
    /// <summary>
    /// This class is used for initialize the turbulence properties for the OpenFOAM simulation.
    /// </summary>
    public class TurbulenceProperties : FOAMDict
    {
        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="version">Version-object.</param>
        /// <param name="path">Path to this File.</param>
        /// <param name="attributes">Additional attributes.</param>
        /// <param name="format">Ascii or Binary.</param>
        /// <param name="settings">Settings-objects</param>
        public TurbulenceProperties(Version version, string path, Dictionary<string, object> attributes, SaveFormat format, Settings settings)
            : base("turbulenceProperties", "dictionary", version, path, attributes, format, settings)
        {
            InitAttributes();
        }

        /// <summary>
        /// Initialize all attributes.
        /// </summary>
        public override void InitAttributes()
        {
            foreach(var obj in m_DictFile)
            {
                FoamFile.Attributes.Add(obj.Key, obj.Value);
            }
        }
    }
}
