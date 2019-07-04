using System.Collections.Generic;
using System.Windows;

namespace BIM.OpenFoamExport.OpenFOAM
{
    /// <summary>
    /// This class is represantive for the fvSolution-Dictionary in the system folder of the openFOAM-case-folder.
    /// </summary>
    public class FvSolution : FoamDict
    {
        //Dict-entries for this dictionary
        private Dictionary<string, object> m_Solvers;
        private Dictionary<string, object> m_SIMPLE;
        private Dictionary<string, object> m_RelaxationFactors;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="version">Version-object.</param>
        /// <param name="path">Path to this file.</param>
        /// <param name="attributes">Additional attributes.</param>
        /// <param name="format">Ascii or Binary</param>
        /// <param name="settings">Settings-object</param>
        public FvSolution(Version version, string path, Dictionary<string, object> attributes, SaveFormat format, Settings settings)
            : base("fvSolution", "dictionary", version, path, attributes, format)
        {
            m_Settings = settings;

            m_Solvers = new Dictionary<string, object>();
            m_SIMPLE = new Dictionary<string, object>();
            m_RelaxationFactors = new Dictionary<string, object>();

            InitAttributes();
        }

        /// <summary>
        /// Initialize attributes of this file.
        /// </summary>
        public override void InitAttributes()
        {
            m_Solvers = new Dictionary<string, object>
            {
                {"p", m_Settings.P1.ToDictionary() },
                {"U" , m_Settings.U1.ToDictionary() },
                {"k" , m_Settings.K.ToDictionary() },
                {"epsilon", m_Settings.Epsilon.ToDictionary() }
            };
            m_SIMPLE = new Dictionary<string, object>
            {
                {"nNonOrthogonalCorrectors" , m_Settings.NNonOrhtogonalCorrectors },
                {"residualControl", m_Settings.ResidualControl }
            };
            m_RelaxationFactors = new Dictionary<string, object>
            {
                {"k", m_Settings.RelaxFactor_k},
                {"U", m_Settings.RelaxFactor_U },
                {"epsilon", m_Settings.RelaxFactor_epsilon },
                {"p", m_Settings.RelaxFactor_p }
            };

            //Add to attributes of the FoamFile
            FoamFile.Attributes.Add("solvers", m_Solvers);
            FoamFile.Attributes.Add("SIMPLE", m_SIMPLE);
            FoamFile.Attributes.Add("relaxationFactors", m_RelaxationFactors);
        }
    }
}
