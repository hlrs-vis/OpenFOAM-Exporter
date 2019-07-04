using System.Collections.Generic;

namespace BIM.OpenFoamExport.OpenFOAM
{
    /// <summary>
    /// The DecomposParDict-Class contains all attributes for decomposing case for parallel computing in Openfoam.
    /// </summary>
    public class DecomposeParDict : FoamDict
    {
        /// <summary>
        /// Number of cpu cores
        /// </summary>        
        private int m_NumberOfSubdomains;
        
        /// <summary>
        /// Method for decomposing.
        /// </summary>
        private MethodDecompose m_Method;

        /// <summary>
        /// SimpleCoeffs-Dictionary
        /// </summary>
        private Dictionary<string, object> m_SimpleCoeffs;

        /// <summary>
        /// HierarchicalCoeffs-Dictionary
        /// </summary>
        private Dictionary<string, object> m_HierarchicalCoeffs;

        /// <summary>
        /// ManualCoeffs-Dicitonary
        /// </summary>
        private Dictionary<string, object> m_ManualCoeffs;
        
        /// <summary>
        /// Getter for numberOfSubdomains
        /// </summary>
        public int NumberOfSubdomains { get => m_NumberOfSubdomains;}

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="version">Version-object.</param>
        /// <param name="path">Path to this File.</param>
        /// <param name="attributes">Additional attributes.</param>
        /// <param name="format">Ascii or Binary.</param>
        /// <param name="settings">Settings-objects</param>
        public DecomposeParDict(Version version, string path, Dictionary<string, object> attributes, SaveFormat format, Settings settings)
            : base("decomposeParDict", "dictionary", version, path, attributes, format)
        {
            m_Settings = settings;

            m_SimpleCoeffs = new Dictionary<string, object>();
            m_HierarchicalCoeffs = new Dictionary<string, object>();
            m_ManualCoeffs = new Dictionary<string, object>();
            InitAttributes();
        }

        /// <summary>
        /// Initialize attributes.
        /// </summary>
        public override void InitAttributes()
        {
            m_NumberOfSubdomains = m_Settings.NumberOfSubdomains;
            m_Method = m_Settings.MethodDecompose;
            m_SimpleCoeffs.Add("n", m_Settings.SimpleCoeffs.N);
            m_SimpleCoeffs.Add("delta", m_Settings.SimpleCoeffs.Delta);
            m_HierarchicalCoeffs.Add("n", m_Settings.HierarchicalCoeffs.N);
            m_HierarchicalCoeffs.Add("delta", m_Settings.HierarchicalCoeffs.Delta);
            m_HierarchicalCoeffs.Add("order", m_Settings.Order);
            m_ManualCoeffs.Add("dataFile", m_Settings.DataFile);

            FoamFile.Attributes.Add("numberOfSubdomains", m_NumberOfSubdomains);
            FoamFile.Attributes.Add("method", m_Method);
            FoamFile.Attributes.Add("simpleCoeffs", m_SimpleCoeffs);
            FoamFile.Attributes.Add("hierarchicalCoeffs", m_HierarchicalCoeffs);
            FoamFile.Attributes.Add("manualCoeffs", m_ManualCoeffs);
        }

    }
}
