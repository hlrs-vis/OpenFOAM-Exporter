using System.Collections.Generic;

namespace BIM.OpenFOAMExport.OpenFOAM
{
    /// <summary>
    /// P_rgh-Parameter for HeatTransfer.
    /// </summary>
    public class P_rgh : P
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="version">Version-Object</param>
        /// <param name="path">Path to this file.</param>
        /// <param name="attributes">Additional attributs.</param>
        /// <param name="format">Format of this file.</param>
        /// <param name="settings">Settings-object</param>
        /// <param name="_wallName">Name of the patch wall.</param>
        /// <param name="_InletNames">Patchnames of the inlets as string-array.</param>
        /// <param name="_OutletNames">Patchnames of the outlets as string-array.</param>
        public P_rgh(Version version, string path, Dictionary<string, object> attributes, SaveFormat format, Settings settings, string _wallName,
            List<string> _InletNames, List<string> _OutletNames)
            : base(version, path, attributes, format, settings, _wallName, _InletNames, _OutletNames)
        {

        }
    }
}
