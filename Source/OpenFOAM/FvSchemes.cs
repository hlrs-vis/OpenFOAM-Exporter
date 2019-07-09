using System.Collections.Generic;

namespace BIM.OpenFoamExport.OpenFOAM
{
    /// <summary>
    /// This class is represantive for the fvSchemes-Dictionary in the system folder of the openFOAM-case-folder.
    /// </summary>
    public class FvSchemes : FoamDict
    {
        //Dict-entries for this dictionary
        //private Dictionary<string, object> m_ddtSchemes;
        //private Dictionary<string, object> m_gradSchemes;
        //private Dictionary<string, object> m_divSchemes;
        //private Dictionary<string, object> m_laplacianSchemes;
        //private Dictionary<string, object> m_interpolitionSchemes;
        //private Dictionary<string, object> m_snGradSchemes;
        //private Dictionary<string, object> m_fluxRequired;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="version">Version-object.</param>
        /// <param name="path">Path to this file.</param>
        /// <param name="attributes">Additional attributes.</param>
        /// <param name="format">Ascii or Binary</param>
        /// <param name="settings">Settings-object</param>
        public FvSchemes(Version version, string path, Dictionary<string, object> attributes, SaveFormat format, Settings settings)
            : base("fvSchemes", "dictionary", version,path,attributes,format, settings)
        {

            //m_ddtSchemes = new Dictionary<string, object>();
            //m_gradSchemes = new Dictionary<string, object>();
            //m_divSchemes = new Dictionary<string, object>();
            //m_laplacianSchemes = new Dictionary<string, object>();
            //m_interpolitionSchemes = new Dictionary<string, object>();
            //m_snGradSchemes = new Dictionary<string, object>();
            //m_fluxRequired = new Dictionary<string, object>();

            InitAttributes();
        }

        /// <summary>
        /// Initialize attributes of this file.
        /// </summary>
        public override void InitAttributes()
        {
            base.InitAttributes();
            //Dictionary<string, object> system = m_Settings.SimulationDefault["System"] as Dictionary<string, object>;
            //Dictionary<string, object> fvSchemes = system["FvSchemes"] as Dictionary<string, object>;

            //foreach (var obj in fvSchemes)
            //{
            //    FoamFile.Attributes.Add(obj.Key, obj.Value);
            //}
            //m_ddtSchemes.Add(m_Settings.DdtSchemes.Key, m_Settings.DdtSchemes.Value);
            //m_gradSchemes.Add(m_Settings.GradSchemes.Key, m_Settings.GradSchemes.Value);
            //foreach (var obj in m_Settings.DivSchemes)
            //{
            //    m_divSchemes.Add(obj.Key, obj.Value);
            //}
            //m_laplacianSchemes.Add(m_Settings.LaplacianSchemes.Key, m_Settings.LaplacianSchemes.Value);
            //m_interpolitionSchemes.Add(m_Settings.InterpolationSchemes.Key, m_Settings.InterpolationSchemes.Value);
            //m_snGradSchemes.Add(m_Settings.SnGradSchemes.Key, m_Settings.SnGradSchemes.Value);
            //m_fluxRequired.Add(m_Settings.FluxRequired.Key, m_Settings.FluxRequired.Value);

            //FoamFile.Attributes.Add("ddtSchemes", m_ddtSchemes);
            //FoamFile.Attributes.Add("gradSchemes", m_gradSchemes);
            //FoamFile.Attributes.Add("divSchemes", m_divSchemes);
            //FoamFile.Attributes.Add("laplacianSchemes", m_laplacianSchemes);
            //FoamFile.Attributes.Add("interpolationSchemes", m_interpolitionSchemes);
            //FoamFile.Attributes.Add("snGradSchemes", m_snGradSchemes);
            //FoamFile.Attributes.Add("fluxRequired", m_fluxRequired);
        }
    }
}
