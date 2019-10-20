﻿using System.Collections.Generic;
using System.Windows;
using Autodesk.Revit.DB;
using System.Windows.Media.Media3D;
using System.Collections;

namespace BIM.OpenFOAMExport.OpenFOAM
{
    /// <summary>
    /// The class SnappyHexMeshDict heritates from abstract class FoamDict and contains all default attributes for this openFOAM-File.
    /// </summary>
    public class SnappyHexMeshDict : FOAMDict
    {
        ///Constant strings
        private const string nameGeometry = "name";
        private const string level = "level";
        private const string regions = "regions";

        /// <summary>
        /// Name of the STL
        /// </summary>
        private readonly string m_STLName;

        /// <summary>
        /// Point in 3d-Space that is used to seperate between outer and inner mesh in the snappyHexMesh-algorithmn
        /// </summary>
        private Vector3D m_LocationInMesh;

        /// <summary>
        /// Contains inlet and outlet as Faces
        /// </summary>
        private readonly Dictionary<KeyValuePair<string, Document>, KeyValuePair<Face, Transform>> m_Faces;

        /// <summary>
        /// Simulation default.
        /// </summary>
        private Dictionary<string, object> m_SettingsCMC;

        //Default-Dictionaries in SnappyHexMeshDict
        private Dictionary<string, object> m_Geometry;
        private Dictionary<string, object> m_CastellatedMeshControls;
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

            FoamFile.Attributes.Add("castellatedMesh", m_DictFile["castellatedMesh"]);
            FoamFile.Attributes.Add("snap", m_DictFile["snap"]);
            FoamFile.Attributes.Add("addLayers", m_DictFile["addLayers"]);
            FoamFile.Attributes.Add("geometry", m_Geometry);
            FoamFile.Attributes.Add("castellatedMeshControls", m_CastellatedMeshControls);
            FoamFile.Attributes.Add("snapControls", m_DictFile["snapControls"]);
            FoamFile.Attributes.Add("addLayersControls", m_DictFile["addLayersControls"]);
            FoamFile.Attributes.Add("meshQualityControls", m_DictFile["meshQualityControls"]);
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
            List<string> addAttributes = new List<string> { "maxLocalCells", "maxGlobalCells", "minRefinementCells", "maxLoadUnbalance", "nCellsBetweenLevels", "features"};
            foreach(var s in addAttributes)
            {
                m_CastellatedMeshControls.Add(s, m_SettingsCMC[s]);
            }
            m_CastellatedMeshControls.Add("refinementSurfaces", m_RefinementSurfaces);
            m_CastellatedMeshControls.Add("resolveFeatureAngle", m_SettingsCMC["resolveFeatureAngle"]);
            m_CastellatedMeshControls.Add("refinementRegions", m_SettingsCMC["refinementRegions"]);
            m_CastellatedMeshControls.Add("locationInMesh", m_LocationInMesh);
            m_CastellatedMeshControls.Add("allowFreeStandingZoneFaces", m_SettingsCMC["allowFreeStandingZoneFaces"]);
        }

        /// <summary>
        /// Initialize RefinementSurfaces in CastellatedMesh-Dictionary.
        /// </summary>
        private void InitRefinementSurfaces()
        {
            InitRegionsRefinement();
            m_StlRefinement.Add(level, m_SettingsCMC["wallLevel"]);
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
                    vec = (Vector)m_SettingsCMC["inletLevel"];
                }
                else if (name.Contains("Outlet") || name.Contains("Abluft"))
                {
                    vec = (Vector)m_SettingsCMC["outletLevel"];
                }
                m_RegionsRefinementCastellated.Add(name, new Dictionary<string, object>() { { level, vec } });
            }
            foreach (var entry in m_Settings.MeshResolution)
            {
                FamilyInstance instance = entry.Key as FamilyInstance;
                name = instance.Symbol.Family.Name + "_" + instance.Name.Replace(' ', '_') + "_" + entry.Key.Id;
                vec = new Vector(entry.Value, entry.Value);
                m_RegionsRefinementCastellated.Add(name, new Dictionary<string, object>() { { level, vec } });
            }
        }

        /// <summary>
        /// Initialize Vector.
        /// </summary>
        private void InitLocationInMesh()
        {
            m_LocationInMesh = m_Settings.LocationInMesh;
        }
    }
}