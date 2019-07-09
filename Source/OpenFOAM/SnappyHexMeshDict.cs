using System.Collections.Generic;
using System.Windows;
using Autodesk.Revit.DB;
using System.Windows.Media.Media3D;

namespace BIM.OpenFoamExport.OpenFOAM
{
    /// <summary>
    /// The class SnappyHexMeshDict heritates from abstract class FoamDict and contains all default attributes for this openFOAM-File.
    /// </summary>
    public class SnappyHexMeshDict : FoamDict
    {
        ///Constant strings
        private const string nameGeometry = "name";
        private const string level = "level";
        private const string regions = "regions";

        /// <summary>
        /// Name of the STL
        /// </summary>
        private string m_STLName;

        /// <summary>
        /// Point in 3d-Space that is used to seperate between outer and inner mesh in the snappyHexMesh-algorithmn
        /// </summary>
        private Vector3D m_LocationInMesh;

        /// <summary>
        /// Contains inlet and outlet as Faces
        /// </summary>
        private Dictionary<KeyValuePair<string, Document>, KeyValuePair<Face, Transform>> m_Faces;

        private Dictionary<string, object> m_SettingsCMC;

        //Default-Dictionaries in SnappyHexMeshDict
        private Dictionary<string, object> m_Geometry;
        private Dictionary<string, object> m_CastellatedMeshControls;
        //private Dictionary<string, object> m_SnapControls;
        //private Dictionary<string, object> m_AddLayersControls;
        //private Dictionary<string, object> m_MeshQualityControls;
        private Dictionary<string, object> m_RefinementSurfaces;

        //Geometry-Dictionary
        Dictionary<string, object> m_Regions;
        Dictionary<string, object> m_Stl;

        //Castellated-Dictionary
        Dictionary<string, object> m_StlRefinement;
        Dictionary<string, object> m_RegionsRefinementCastellated;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="version">Version-object.</param>
        /// <param name="path">Path to this file.</param>
        /// <param name="attributes">Additional attributes.</param>
        /// <param name="format">Ascii or Binary</param>
        /// <param name="settings">Settings-object</param>
        /// <param name="stlName">Name of the STL</param>
        /// <param name="faces">Outlet & Inlet as Faces in a Dictionary with name as Key.</param>
        public SnappyHexMeshDict(Version version, string path, Dictionary<string, object> attributes, SaveFormat format, Settings settings, string stlName, string stlWallName,
            Dictionary<KeyValuePair<string, Document>, KeyValuePair<Face, Transform>> faces)
            : base("snappyHexMeshDict", "dictionary", version, path, attributes, format, settings)
        {
            m_STLName = stlName;
            m_Faces = faces;

            m_Geometry = new Dictionary<string, object>();
            m_CastellatedMeshControls = new Dictionary<string, object>();
            //m_SnapControls = new Dictionary<string, object>();
            //m_AddLayersControls = new Dictionary<string, object>();
            //m_MeshQualityControls = new Dictionary<string, object>();
            m_RefinementSurfaces = new Dictionary<string, object>();

            m_Regions = new Dictionary<string, object>();
            m_Stl = new Dictionary<string, object>();

            m_StlRefinement = new Dictionary<string, object>();
            m_RegionsRefinementCastellated = new Dictionary<string, object>();

            m_SettingsCMC = m_DictFile["castellatedMeshControls"] as Dictionary<string, object>;

            InitAttributes();
        }

        /// <summary>
        /// Initialize attributes of this file.
        /// </summary>
        public override void InitAttributes()
        {
            InitGeometry();
            InitCastellatedMeshControls();
            //InitSnapControls();
            //InitAddLayersControls();
            //InitMeshQualityControls();

            FoamFile.Attributes.Add("castellatedMesh", m_DictFile["castellatedMesh"]/*m_Settings.CastellatedMesh*/);
            FoamFile.Attributes.Add("snap", m_DictFile["snap"]/*m_Settings.Snap*/);
            FoamFile.Attributes.Add("addLayers", m_DictFile["addLayers"]/*m_Settings.AddLayers*/);
            FoamFile.Attributes.Add("geometry", m_Geometry);
            FoamFile.Attributes.Add("castellatedMeshControls", m_CastellatedMeshControls);
            FoamFile.Attributes.Add("snapControls", m_DictFile["snapControls"]/*m_SnapControls*/);
            FoamFile.Attributes.Add("addLayersControls", m_DictFile["addLayersControls"]/*m_AddLayersControls*/);
            FoamFile.Attributes.Add("meshQualityControls", m_DictFile["meshQualityControls"]/*m_MeshQualityControls*/);
            FoamFile.Attributes.Add("debug", m_Settings.Debug);
            FoamFile.Attributes.Add("mergeTolerance", m_Settings.MergeTolerance);
        }

        /// <summary>
        /// Initialize the Geometry-Dictionary.
        /// </summary>
        private void InitGeometry()
        {
            InitGeometryRegions();
            m_Stl.Add("type", "triSurfaceMesh");
            m_Stl.Add(nameGeometry, m_STLName);
            m_Stl.Add(regions, m_Regions);

            string nameWithExtension = m_STLName + ".stl";
            m_Geometry.Add(nameWithExtension, m_Stl);
        }

        /// <summary>
        /// Initialize the regions in the Geometry-Dictionary
        /// </summary>
        private void InitGeometryRegions()
        {
            string name;
            string wallName = "wallSTL";
            m_Regions.Add(wallName, new Dictionary<string, object> { { nameGeometry, wallName } });
            foreach (var face in m_Faces)
            {
                //face.Key.Key = Name + ID
                name = face.Key.Key;
                name = name.Replace(" ", "_");
                m_Regions.Add(name, new Dictionary<string, object> { { nameGeometry, name } });
            }
        }

        /// <summary>
        /// Initialize the CastellatedMeshControl-Dictionary.
        /// </summary>
        private void InitCastellatedMeshControls()
        {
            InitRefinementSurfaces();
            InitLocationInMesh();

            //m_CastellatedMeshControls.Add("maxLocalCells", m_Settings.MaxLocalCells);
            //m_CastellatedMeshControls.Add("maxGlobalCells", m_Settings.MaxGlobalCells);
            //m_CastellatedMeshControls.Add("minRefinementCells", m_Settings.MinRefinementCalls);
            //m_CastellatedMeshControls.Add("maxLoadUnbalance", m_Settings.MaxLoadUnbalance);
            //m_CastellatedMeshControls.Add("nCellsBetweenLevels", m_Settings.NCellsBetweenLevels);
            //m_CastellatedMeshControls.Add("features", m_Settings.Features);            
            List<string> addAttributes = new List<string> { "maxLocalCells", "maxGlobalCells", "minRefinementCells", "maxLoadUnbalance", "nCellsBetweenLevels", "features" };
            foreach(var s in addAttributes)
            {
                m_CastellatedMeshControls.Add(s, m_SettingsCMC[s]);
            }

            m_CastellatedMeshControls.Add("refinementSurfaces", m_RefinementSurfaces);
            m_CastellatedMeshControls.Add("resolveFeatureAngle", m_SettingsCMC["resolveFeatureAngle"]/*m_Settings.ResolveFeatureAngle*/);
            m_CastellatedMeshControls.Add("refinementRegions", m_SettingsCMC["refinementRegions"]/*m_Settings.RefinementRegions*/);
            m_CastellatedMeshControls.Add("locationInMesh", m_LocationInMesh);
            m_CastellatedMeshControls.Add("allowFreeStandingZoneFaces", m_SettingsCMC["allowFreeStandingZoneFaces"]/*m_Settings.AllowFreeStandingZoneFaces*/);
        }

        /// <summary>
        /// Initialize RefinementSurfaces in CastellatedMesh-Dictionary.
        /// </summary>
        private void InitRefinementSurfaces()
        {
            InitRegionsRefinement();
            m_StlRefinement.Add(level, m_SettingsCMC["wallLevel"]/*m_Settings.WallLevel*/);
            m_StlRefinement.Add(regions, m_RegionsRefinementCastellated);
            m_RefinementSurfaces.Add(m_STLName, m_StlRefinement);
        }

        /// <summary>
        /// Initialize Regions in RefinementSurfaces.
        /// </summary>
        private void InitRegionsRefinement()
        {
            Vector vec = new Vector();
            string name;
            foreach (var face in m_Faces)
            {
                name = face.Key.Key;
                name = name.Replace(" ", "_");
                if (name.Contains("Inlet") || name.Contains("Zuluft"))
                {
                    vec = (Vector)m_SettingsCMC["inletLevel"]/*m_Settings.InletLevel*/;
                }
                else if (name.Contains("Outlet") || name.Contains("Abluft"))
                {
                    vec = (Vector)m_SettingsCMC["outletLevel"] /*m_Settings.OutletLevel*/;
                }
                m_RegionsRefinementCastellated.Add(name, new Dictionary<string, object>() { { level, vec } });
            }
        }

        /// <summary>
        /// Initialize Vector.
        /// </summary>
        private void InitLocationInMesh()
        {
            m_LocationInMesh = new Vector3D(3.9697562561035156, -0.5521240234375, 1.4000000000000001);
        }

        ///// <summary>
        ///// Initialize SnapControl-Dictionary.
        ///// </summary>
        //private void InitSnapControls()
        //{
        //    m_SnapControls.Add("nSmoothPatch", m_Settings.NSmoothPatch);
        //    m_SnapControls.Add("tolerance", m_Settings.Tolerance);
        //    m_SnapControls.Add("nSolveIter", m_Settings.NSolverIter);
        //    m_SnapControls.Add("nRelaxIter", m_Settings.NRelaxIterSnap);
        //    m_SnapControls.Add("nFeatureSnapIter", m_Settings.NFeatureSnapIter);
        //    m_SnapControls.Add("implicitFeature", m_Settings.ImplicitFeatureSnap);
        //    m_SnapControls.Add("multiRegionFeatureSnap", m_Settings.MultiRegionFeatureSnap);
        //}

        ///// <summary>
        ///// Initialize AddLayersControl-Dictionary.
        ///// </summary>
        //private void InitAddLayersControls()
        //{
        //    m_AddLayersControls.Add("relativeSizes", m_Settings.RelativeSizes);
        //    m_AddLayersControls.Add("layers", m_Settings.Layers);
        //    m_AddLayersControls.Add("expansionRatio", m_Settings.ExpansionRatio);
        //    m_AddLayersControls.Add("finalLayerThickness", m_Settings.FinalLayerThickness);
        //    m_AddLayersControls.Add("minThickness", m_Settings.MinThickness);
        //    m_AddLayersControls.Add("nGrow", m_Settings.NGrow);
        //    m_AddLayersControls.Add("featureAngle", m_Settings.FeatureAngle);
        //    m_AddLayersControls.Add("nRelaxIter", m_Settings.NRelaxeIterLayer);
        //    m_AddLayersControls.Add("nSmoothSurfaceNormals", m_Settings.NSmoothSurfaceNormals);
        //    m_AddLayersControls.Add("nSmoothThickness", m_Settings.NSmoothThickness);
        //    m_AddLayersControls.Add("nSmoothNormals", m_Settings.NSmoothNormals);
        //    m_AddLayersControls.Add("maxFaceThicknessRatio", m_Settings.MaxFaceThicknessRatio);
        //    m_AddLayersControls.Add("maxThicknessToMedialRatio", m_Settings.MaxThicknessToMeadialRatio);
        //    m_AddLayersControls.Add("minMedianAxisAngle", m_Settings.MinMedianAxisAngle);
        //    m_AddLayersControls.Add("nBufferCellsNoExtrude", m_Settings.NBufferCellsNoExtrude);
        //    m_AddLayersControls.Add("nLayerIter", m_Settings.NLayerIter);
        //    m_AddLayersControls.Add("nRelaxedIter", m_Settings.NRelaxedIterLayer);
        //}

        ///// <summary>
        ///// Initialize MeshQualityControl-Dictionary.
        ///// </summary>
        //private void InitMeshQualityControls()
        //{
        //    m_MeshQualityControls.Add("maxNonOrtho", m_Settings.MaxNonOrthoMeshQualtiy);
        //    m_MeshQualityControls.Add("maxBoundarySkewness", m_Settings.MaxBoundarySkewness);
        //    m_MeshQualityControls.Add("maxInternalSkewness", m_Settings.MaxInternalSkewness);
        //    m_MeshQualityControls.Add("maxConcave", m_Settings.MaxConcave);
        //    m_MeshQualityControls.Add("minFlatness", m_Settings.MinFlatness);
        //    m_MeshQualityControls.Add("minVol", m_Settings.MinVol);
        //    m_MeshQualityControls.Add("minTetQuality", m_Settings.MinTetQuality);
        //    m_MeshQualityControls.Add("minArea", m_Settings.MinArea);
        //    m_MeshQualityControls.Add("minTwist", m_Settings.MinTwist);
        //    m_MeshQualityControls.Add("minDeterminant", m_Settings.MinDeterminant);
        //    m_MeshQualityControls.Add("minFaceWeight", m_Settings.MinFaceWeight);
        //    m_MeshQualityControls.Add("minVolRatio", m_Settings.MinVolRatio);
        //    m_MeshQualityControls.Add("minTriangleTwist", m_Settings.MinTriangleTwist);
        //    m_MeshQualityControls.Add("nSmoothScale", m_Settings.NSmoothScale);
        //    m_MeshQualityControls.Add("errorReduction", m_Settings.ErrorReduction);
        //    m_MeshQualityControls.Add("relaxed", m_Settings.Relaxed);
        //}
    }
}