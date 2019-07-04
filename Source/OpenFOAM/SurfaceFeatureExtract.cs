using System.Collections.Generic;

namespace BIM.OpenFoamExport.OpenFOAM
{
    /// <summary>
    /// The class SurfaceFeatureExtract represents the Dictionary for extracting eMesh.
    /// </summary>
    public class SurfaceFeatureExtract : FoamDict
    {
        /// <summary>
        /// Dictionary for SurfaceFeatures
        /// </summary>
        private Dictionary<string, object> m_SurfaceFeature;

        /// <summary>
        /// Name of the STL
        /// </summary>
        private string m_STLName;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="version">Version-object.</param>
        /// <param name="path">Path to this File.</param>
        /// <param name="attributes">Additional attributes.</param>
        /// <param name="format">Ascii or Binary.</param>
        /// <param name="settings">Settings-object</param>
        /// <param name="stlName">Name of the stl</param>
        public SurfaceFeatureExtract(Version version, string path, Dictionary<string, object> attributes, SaveFormat format, Settings settings, string stlName)
            : base("surfaceFeatureExtractDict", "dictionary", version, path, attributes, format)
        {
            m_Settings = settings;
            m_SurfaceFeature = new Dictionary<string, object>();
            m_STLName = stlName;
            InitAttributes();
        }

        /// <summary>
        /// Initialize Attributes.
        /// </summary>
        public override void InitAttributes()
        {
            m_SurfaceFeature.Add("extractionMethod", m_Settings.ExtractionMethod);
            m_SurfaceFeature.Add("extractFromSurfaceCoeffs", m_Settings.ExtractFromSurfaceCoeffs);
            m_SurfaceFeature.Add("writeObj", m_Settings.WriteObj);

            FoamFile.Attributes.Add(m_STLName + ".stl", m_SurfaceFeature);

            //TO-DO: Dont set in this class
            //m_Settings.Features.Add("{file \"" + m_STLName + ".eMesh\"; level 3;}");
        }
    }
}
