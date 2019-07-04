using System.Collections.Generic;

namespace BIM.OpenFoamExport.OpenFOAM
{
    /// <summary>
    /// Abstract base class for simulation parameter that vary with used simulation-model.
    /// </summary>
    public abstract class FoamParameter : FoamDict
    {
        /// <summary>
        /// Name of the patch wall
        /// </summary>
        protected string m_WallName;

        /// <summary>
        /// ValueType for entries in boundaryField
        /// </summary>
        protected string m_Uniform;

        /// <summary>
        /// String-array with all patch-names of the inlets
        /// </summary>
        protected List<string> m_InletNames;

        /// <summary>
        /// String-array with all patchnames of the outlets
        /// </summary>
        protected List<string> m_OutletNames;

        /// <summary>
        /// Struct for internalField-entry
        /// </summary>
        protected struct InternalField<T>
        {
            T m_Value;

            public T Value
            {
                get
                {
                    return m_Value;
                }
                set
                {
                    m_Value = value;
                }
            }
        }

        /// <summary>
        /// Dimension-entry
        /// </summary>
        protected int[] m_Dimensions;

        /// <summary>
        /// Dictionary which specify the different patch handlings.
        /// </summary>
        protected Dictionary<string, object> m_BoundaryField;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="version">Version-Object</param>
        /// <param name="path">Path to this file.</param>
        /// <param name="attributes">Additional attributs.</param>
        /// <param name="format">Format of this file.</param>
        /// <param name="settings">Settings-object</param>
        /// <param name="_class">Specify class of Parameter.</param>
        /// <param name="_object">Name of the FoamParameter.</param>
        /// <param name="_wallName">Name of the patch wall.</param>
        /// <param name="_InletNames">Patchnames of the inlets as string-array.</param>
        /// <param name="_OutletNames">Patchnames of the outlets as string-array.</param>
        public FoamParameter(Version version, string path, Dictionary<string, object> attributes, SaveFormat format, Settings settings, string _class, string _object, string _wallName,
            List<string> _InletNames, List<string> _OutletNames)
            : base(_class, _object, version, path, attributes, format)
        {
            m_Settings = settings;
            m_WallName = _wallName;
            m_InletNames = _InletNames;
            m_OutletNames = _OutletNames;
            m_Uniform = "uniform";
            m_Dimensions = new int[7];
            m_BoundaryField = new Dictionary<string, object>();
            InitAttributes();
        }
    }
}
