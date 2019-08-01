//
// STL exporter library: this library works with Autodesk(R) Revit(R) to export an STL file containing model geometry.
// Copyright (C) 2013  Autodesk, Inc.
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

using System.Collections.Generic;
using Category = Autodesk.Revit.DB.Category;
using Autodesk.Revit.DB;
using System.Collections;
using System.Windows;
using System.Windows.Media.Media3D;
using System;
using System.Xml.Linq;
using System.IO;

namespace BIM.OpenFoamExport
{
    public struct SSH
    {
        string user;
        string serverIP;
        string ofAlias;
        string serverCaseFolder;
        int port;
        bool download;
        bool delete;

        public SSH(string _user, string _ip, string _alias, string _caseFolder, bool _download, bool _delete, int _port)
        {
            user = _user;
            serverIP = _ip;
            ofAlias = _alias;
            serverCaseFolder = _caseFolder;
            download = _download;
            delete = _delete;
            port = _port;
        }

        public string User { get => user; set => user = value; }
        public string ServerIP { get => serverIP; set => serverIP = value; }
        public string OfAlias { get => ofAlias; set => ofAlias = value; }
        public string ServerCaseFolder { get => serverCaseFolder; set => serverCaseFolder = value; }
        public bool Download { get => download; set => download = value; }
        public bool Delete { get => delete; set => delete = value; }
        public int Port { get => port; set => port = value; }

        public string ConnectionString()
        {
            return user + "@" + serverIP;
        }
    }

    /// <summary>
    /// Patch for boundaryField in Parameter-Dictionaries.
    /// </summary>
    /// <typeparam name="T">Type for value.</typeparam>
    public struct FOAMParameterPatch<T>
    {
        //Attributes for Parameter-Dictionaries in 0 folder.
        string type;
        Dictionary<string, object> attributes;
        T value;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="_type">Type of Patch</param>
        /// <param name="_uniform">uniform or nonuniform.</param>
        /// <param name="_value">Vector3D or double.</param>
        public FOAMParameterPatch(string _type, string _uniform, T _value)
        {
            value = _value;
            type = _type;
            if(_value != null)
            {
                attributes = new Dictionary<string, object>
                {
                    { "type", type },
                    { "value " + _uniform, value}
                };
            }
            else
            {
                attributes = new Dictionary<string, object>
                {
                    { "type", type }
                };
            }

        }

        public FOAMParameterPatch(string _type)
        {
            type = _type;
            value = default(T);
            attributes = new Dictionary<string, object>
            {
                { "type", type }
            };
        }

        //Getter for Attributes.
        public Dictionary<string, object> Attributes { get => attributes; }

        public Dictionary<string, object> ToDictionary()
        {
            return attributes;
        }
    }

    /// <summary>
    /// Coeffs-Parameter for DecomposeParDict.
    /// </summary>
    public struct CoeffsMethod
    {
        //Attributes
        Vector3D n;
        double delta;

        //Getter-Setter
        public Vector3D N { get => n; }
        public double Delta { get => delta; set => delta = value; }

        /// <summary>
        /// Initialize Vector N with the number of cpu's.
        /// </summary>
        /// <param name="numberOfSubdomains">Number of physical CPU's.</param>
        public void SetN(int numberOfSubdomains)
        {
            //Algo for subDomains

        }

        /// <summary>
        /// Initialize Vector N with given Vecotr _n.
        /// </summary>
        /// <param name="_n">Explicit Vector for N.</param>
        public void SetN(Vector3D _n)
        {
            n = _n;
        }

        public Dictionary<string,object> ToDictionary()
        {
            Dictionary<string, object> attributes = new Dictionary<string, object>();
            attributes.Add("n", n);
            attributes.Add("delta", delta);
            return attributes;
        }
    }

    /// <summary>
    /// P-FvSolution.
    /// </summary>
    public struct PFv
    {
        //Parameter for the p-Dictionary in FvSolutionDictionary
        FvSolutionParamter param;
        Agglomerator agglomerator;
        CacheAgglomeration cacheAgglomeration;
        int nCellsInCoarsesLevel;
        int nPostSweeps;
        int nPreSweepsre;
        int mergeLevels;

        //Getter-Setter
        public FvSolutionParamter Param { get => param; set => param = value; }
        public Agglomerator Agglomerator { get => agglomerator; set => agglomerator = value; }
        public CacheAgglomeration CacheAgglomeration { get => cacheAgglomeration; set => cacheAgglomeration = value; }
        public int NCellsInCoarsesLevel { get => nCellsInCoarsesLevel; set => nCellsInCoarsesLevel = value; }
        public int NPostSweeps { get => nPostSweeps; set => nPostSweeps = value; }
        public int NPreSweepsre { get => nPreSweepsre; set => nPreSweepsre = value; }
        public int MergeLevels { get => mergeLevels; set => mergeLevels = value; }

        /// <summary>
        /// Creates a Dictionary of data.
        /// </summary>
        /// <returns>Dictionary filled with attributes.</returns>
        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> pList = new Dictionary<string, object>
            {
                {"agglomerator" , Agglomerator},
                {"relTol" , Param.RelTol },
                {"tolerance" , Param.Tolerance },
                {"nCellsInCoarsesLevel", NCellsInCoarsesLevel },
                {"smoother" , Param.Smoother },
                {"solver" , Param.Solver },
                {"cacheAgglomeration" , CacheAgglomeration },
                {"nPostSweeps" , NPostSweeps },
                {"nPreSweepsre" , NPreSweepsre },
                {"mergeLevels", MergeLevels }
            };
            return pList;
        }
    }

    /// <summary>
    /// Fv-SolutionParam
    /// </summary>
    public struct FvSolutionParamter
    {
        //Paramter that has to be set in FvSolitonDict
        Smoother smoother;
        SolverFV solver;
        double relTol;
        double tolerance;
        int nSweeps;

        //Getter-Setter for Parameter
        public Smoother Smoother { get => smoother; set => smoother = value; }
        public SolverFV Solver { get => solver; set => solver = value; }
        public double RelTol { get => relTol; set => relTol = value; }
        public double Tolerance { get => tolerance; set => tolerance = value; }
        public int NSweeps { get => nSweeps; set => nSweeps = value; }

        /// <summary>
        /// Create List of attributes and return.
        /// </summary>
        /// <returns>List with special attributes.</returns>
        public List<KeyValuePair<string, object>> ToList()
        {
            List<KeyValuePair<string, object>> paramList = new List<KeyValuePair<string, object>>
            { 
                {new KeyValuePair<string, object> ( "relTol" , RelTol )},
                {new KeyValuePair<string, object> ( "tolerance" , Tolerance )},
                {new KeyValuePair<string, object> ( "nSweeps" , NSweeps) },
                {new KeyValuePair<string, object> ( "smoother" , Smoother) },
                {new KeyValuePair<string, object> ( "solver" , Solver) },

            };
            return paramList;
        }

        /// <summary>
        /// Creates a Dictionary of data.
        /// </summary>
        /// <returns>Dictionary filled with attributes.</returns>
        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>
            {
                {"relTol" , RelTol },
                {"tolerance" , Tolerance },
                {"nSweeps" , NSweeps},
                {"smoother" , Smoother },
                {"solver" , Solver }

            };
            return paramList;
        }
    }

    /// <summary>
    /// Turbulence attributes for the openfoam dictionary turbulenceProperties.
    /// </summary>
    public struct TurbulenceParameter
    {
        //type of simulation
        SimulationType simulationType;
        //model for simulation
        ValueType _structModel;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="simType">Simulationtype enum.</param>
        /// <param name="simModel">Simulation model enum.</param>
        /// <param name="turbulence">true = on, false = off</param>
        /// <param name="printCoeff">true = on, false = off</param>
        public TurbulenceParameter(SimulationType simType, Enum simModel, bool turbulence = true, bool printCoeff = true)
        {
            simulationType = simType;
            _structModel = null;
            switch (simulationType)
            {
                case SimulationType.RAS:
                    RAS ras = new RAS((RASModel)simModel, turbulence, printCoeff);
                    _structModel = ras;
                    break;
                case SimulationType.LES:
                    LES les = new LES((LESModel)simModel);
                    _structModel = les;
                    //TO-DO: Implement.
                    break;
                case SimulationType.laminar:
                    //TO-DO: Implement.
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// This methode creates and returns the attributes as dictionary<string, object>.
        /// </summary>
        /// <returns>Dictionary with attributes.</returns>
        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "simulationType", simulationType }
            };
            switch(simulationType)
            {
                case SimulationType.RAS:
                    dict.Add(simulationType.ToString(), ((RAS)_structModel).ToDictionary());
                    break;
                case SimulationType.LES:
                    //TO-DO: Implement LES.
                    break;
                case SimulationType.laminar:
                    //TO-DO: Implement Laminar.
                    break;

            }
            return dict;
        }
    }

    /// <summary>
    /// RAS-Model attributes in turbulenceProperties.
    /// </summary>
    public struct RAS
    {
        //internal enum for on and off
        enum OnOff
        {
            on = 0,
            off
        }

        //Enum for model name
        RASModel rasModel;
        //turbulence on or off
        OnOff turbulence;
        //print coefficient on or off
        OnOff printCoeffs;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="model">Enum-Object for simulation model.</param>
        /// <param name="turb">turbulence true = on, false = off</param>
        /// <param name="printCoeff">printCoeef true = on, false = off</param>
        public RAS(RASModel model, bool turb, bool printCoeff)
        {
            rasModel = model;
            if (turb)
            {
                turbulence = OnOff.on;
            }
            else
            {
                turbulence = OnOff.off;
            }

            if(printCoeff)
            {
                printCoeffs = OnOff.on;
            }
            else
            {
                printCoeffs = OnOff.off;
            }
        }

        /// <summary>
        /// Returns all attributes as Dictionary<string,object>
        /// </summary>
        /// <returns>Dictionary filled with attributes.</returns>
        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "RASModel", rasModel },
                { "turbulence", turbulence },
                { "printCoeffs", printCoeffs}
            };
            return dict;
        }
    }

    /// <summary>
    /// Simulationmodel LES-Parameter.
    /// </summary>
    public struct LES
    {
        LESModel lesModel;
        public LES(LESModel _lesModel)
        {
            lesModel = _lesModel;
        }
        //TO-DO: implement LES
    }

    /// <summary>
    /// Simulationmodel Laminar-Parameter.
    /// </summary>
    public struct Laminar
    {
        //SimulationType typeL;
        //TO-DO: Implement laminar
    }

    /// <summary>
    /// Enum-Objects for simulationmodel LES.
    /// </summary>
    public enum LESModel
    {
        DeardorffDiffStress = 0,
        Smagorinsky,
        SpalartAllmarasDDES,
        SpalartAllmarasDES,
        SpalartAllmarasIDDES,
        WALE,
        dynamicKEqn,
        dynamicLagrangian,
        kEqn,
        kOmegaSSTDES
    }

    /// <summary>
    /// Enum-Objects for simulationmodel RAS.
    /// </summary>
    public enum RASModel
    {
        LRR = 0,
        LamBremhorstKE,
        LaunderSharmaKE,
        LienCubicKE,
        LienLeschzine,
        RNGkEpsilon,
        SSG,
        ShihQuadraticKE,
        buoyantKEpsilon,
        SpalartAllmaras,
        kEpsilon,
        kOmega,
        kOmegaSST,
        kOmegaSSTLM,
        kOmegaSSTSAS,
        kkLOmega,
        qZeta,
        realizableKE,
        v2f
    }
    
    /// <summary>
    /// Enum for simulationtype.
    /// </summary>
    public enum SimulationType
    {
        laminar = 0,
        RAS,
        LES
    }

    /// <summary>
    /// Enum for TransportModel in TransportProperties.
    /// </summary>
    public enum TransportModel
    {
        Newtonian = 0,
        BirdCarreau,
        CrossPowerLaw,
        powerLaw,
        HerschelBulkley,
        Casson,
        strainRateFunction
    }

    /// <summary>
    /// ExtractionMethode for SurfaceFeatuerExtract.
    /// </summary>
    public enum ExtractionMethod
    {
        none = 0,
        extractFromFile,
        extractFromSurface
    }

    /// <summary>
    /// MethodDecompose in DecomposeParDict.
    /// </summary>
    public enum MethodDecompose
    {
        simple = 0,
        hierarchical,
        scotch,
        manual
    }

    /// <summary>
    /// OpenFOAM simulation environment.
    /// </summary>
    public enum OpenFOAMEnvironment
    {
        blueCFD = 0,
        //docker, //not implemented yet
        wsl, 
        ssh //not implemeneted yet
    }

    /// <summary>
    /// Agglomerator for fvSolution.
    /// </summary>
    public enum Agglomerator
    {
        faceAreaPair = 0
    }


    /// <summary>
    /// Smoother for fvSolution.
    /// </summary>
    public enum Smoother
    {
        GaussSeidel = 0,
        symGaussSeidel,
        DIC,
        DILU,
        DICGaussSeidel
    }

    /// <summary>
    /// Solver for fvSolution.
    /// </summary>
    public enum SolverFV
    {
        PCG = 0,
        PBiCGStab,
        PBiCG,
        smoothSolver,
        GAMG,
        diagonal
    }

    /// <summary>
    /// cacheAgglomeration in fvSolution.
    /// </summary>
    public enum CacheAgglomeration
    {
        on = 0,
        off
    }

    /// <summary>
    /// startFrom in controlDict.
    /// </summary>
    public enum StartFrom
    {
        firstTime = 0,
        startTime,
        latestTime
    }

    /// <summary>
    /// stop in controlDict.
    /// </summary>
    public enum StopAt
    {
        endTime = 0,
        writeNow,
        noWriteNow,
        nextWrite
    }

    /// <summary>
    /// writeControl in controlDict.
    /// </summary>
    public enum WriteControl
    {
        timeStep = 0,
        runTime,
        adjustableRunTime,
        cpuTime,
        clockTime
    }

    /// <summary>
    /// format in controlDict.
    /// </summary>
    public enum WriteFormat
    {
        ascii = 0,
        binary
    }

    /// <summary>
    /// writeCompresion in controlDict.
    /// </summary>
    public enum WriteCompression
    {
        on = 0,
        off
    }

    /// <summary>
    /// Timeformat in controlDict.
    /// </summary>
    public enum TimeFormat
    {
        Fixed = 0,
        scientific,
        general
    }

    public enum SolverIncompressible
    {
        simpleFoam = 0,
        //other than simpleFoam not implemented yet
        adjointShapeOptimizationFoam,
        boundaryFoam,
        icoFoam,
        nonNewtonianIcoFoam,
        pimpleDyMFoam,
        pimpleFoam,
        pisoFoam,
        porousSimpleFoam,
        shallowWaterFoam,
        SRFPimpleFoam,
        SRFSimpleFoam
    }

    /// <summary>
    /// The file format of STL.
    /// </summary>
    public enum SaveFormat
    {
        binary = 0,
        ascii
    }

    /// <summary>
    /// The type of mesh.
    /// </summary>
    public enum MeshType
    {
        Snappy = 0,
        cfMesh
    }

    /// <summary>
    /// The range of elements to be exported.
    /// </summary>
    public enum ElementsExportRange
    {
        All = 0,
        OnlyVisibleOnes
    }

    /// <summary>
    /// Settings made by user to export.
    /// </summary>
    public class Settings
    {
        private Dictionary<string, object> m_SimulationDefaultList;

        private Dictionary<string, object> m_System;
        private Dictionary<string, object> m_Constant;
        private Dictionary<string, object> m_Null;
        
        private SaveFormat m_SaveFormat;
        private ElementsExportRange m_ExportRange;
        private MeshType m_Mesh;

        private OpenFOAMEnvironment m_openFOAMEnvironment;

        //BlockMeshDict
        private Vector3D m_SimpleGrading;
        private Vector3D m_CellSize;

        //ControlDict
        private SolverIncompressible m_AppIncompressible;
        private StartFrom m_StartFrom;
        private StopAt m_StopAt;
        private WriteControl m_WriteControl;
        private WriteFormat m_WriteFormat;
        private WriteCompression m_WriteCompression;
        private TimeFormat m_TimeFormat;
        private double m_StartTime;
        private double m_EndTime;
        private double m_DeltaT;
        private double m_WriteInterval;
        private double m_PurgeWrite;
        private double m_WritePrecision;
        private double m_TimePrecision;
        private bool m_RunTimeModifiable;

        //SurfaceFeatureExtract
        private ExtractionMethod m_ExtractionMethod;
        private Dictionary<string, object> m_ExtractFromSurfaceCoeffs;
        private int m_IncludedAngle;
        private string m_WriteObj;

        //DecomposeParDict
        private int m_NumberOfSubdomains;
        private MethodDecompose m_MethodDecompose;

        //DecomposeParDict-simpleCoeffs
        private CoeffsMethod m_SimpleCoeffs;

        //DecomposParDict-hierarchicalCoeffs
        private CoeffsMethod m_HierarchicalCoeffs;
        private string m_Order;

        //DecomposParDict-manualCoeffs
        private string m_DataFile; 

        //FvSchemes
        private KeyValuePair<string, string> m_ddtSchemes;
        private KeyValuePair<string, string> m_gradSchemes;
        private List<KeyValuePair<string, string>> m_divSchemes;
        private KeyValuePair<string, string> m_laplacianSchemes;
        private KeyValuePair<string, string> m_interpolationSchemes;
        private KeyValuePair<string, string> m_snGradSchemes;
        private KeyValuePair<string, string> m_fluxRequired;


        //FvSolution
        private PFv m_p;
        private FvSolutionParamter m_U;
        private FvSolutionParamter m_k;
        private FvSolutionParamter m_epsilon;
        private int m_nNonOrhtogonalCorrectors;
        private double m_relaxFactor_k;
        private double m_relaxFactor_U;
        private double m_relaxFactor_epsilon;
        private double m_relaxFactor_p;
        private Dictionary<string, object> m_residualControl;


        //SnappyHexMeshDict-General
        private bool m_CastellatedMesh;
        private bool m_Snap;
        private bool m_AddLayers;
        private int m_Debug;
        private double m_MergeTolerance;

        //SnappyHexMeshDict-CastelletedMeshControls
        private int m_MaxLocalCells;
        private int m_MaxGlobalCells;
        private int m_MinRefinementCalls;
        private int m_ResolveFeatureAngle;
        private int m_NCellsBetweenLevels;
        private double m_MaxLoadUnbalance;
        private string m_NameEMesh;
        private ArrayList m_Features;
        private int m_FeatureLevel;
        private Vector m_WallLevel;
        private Vector m_OutletLevel;
        private Vector m_InletLevel;
        private Dictionary<string, object> m_RefinementRegions;
        private bool m_AllowFreeStandingZoneFaces;

        //SnappyHexMeshDict-SnapControls
        private int m_NSmoothPatch;
        private int m_Tolerance;
        private int m_NSolverIter;
        private int m_NRelaxIterSnap;
        private int m_NFeatureSnapIter;
        private bool m_ImplicitFeatureSnap;
        private bool m_MultiRegionFeatureSnap;

        //SnappyHexMeshDict-AddLayersControls
        private bool m_RelativeSizes;
        private double m_ExpansionRatio;
        private double m_FinalLayerThickness;
        private double m_MinThickness;
        private double m_MaxFaceThicknessRatio;
        private double m_MaxThicknessToMeadialRatio;
        private int m_NGrow;
        private int m_FeatureAngle;
        private int m_NRelaxeIterLayer;
        private int m_NRelaxedIterLayer;
        private int m_nSmoothSurfaceNormals;
        private int m_NSmoothThickness;
        private int m_NSmoothNormals;
        private int m_MinMedianAxisAngle;
        private int m_NBufferCellsNoExtrude;
        private int m_NLayerIter;
        private Dictionary<string, object> m_Layers;

        //SnappyHexMeshDict-MeshQualityControls
        private int m_MaxNonOrtho;
        private int m_MaxBoundarySkewness;
        private int m_MaxInternalSkewness;
        private int m_MaxConcave;
        private double m_MinFlatness;
        private double m_MinVol;
        private double m_MinTetQuality;
        private int m_MinArea;
        private double m_MinTwist;
        private double m_MinDeterminant;
        private double m_MinFaceWeight;
        private double m_MinVolRatio;
        private int m_MinTriangleTwist;
        private int m_NSmoothScale;
        private double m_ErrorReduction;
        private Dictionary<string, object> m_Relaxed;
        private int m_MaxNonOrthoMeshQualtiy;


        //CfMesh


        //U
        private Vector3D m_InternalFieldU;
        private FOAMParameterPatch<Vector3D> m_WallU;
        private FOAMParameterPatch<Vector3D> m_InletU;
        private FOAMParameterPatch<Vector3D> m_OutletU;


        //epsilon
        private double m_InternalFieldEpsilon;
        private FOAMParameterPatch<double> m_WallEpsilon;
        private FOAMParameterPatch<double> m_InletEpsilon;
        private FOAMParameterPatch<double> m_OutletEpsilon;


        //p
        private double m_InternalFieldP;
        private FOAMParameterPatch<double> m_WallP;
        private FOAMParameterPatch<double> m_InletP;
        private FOAMParameterPatch<double> m_OutletP;


        //nut
        private double m_InternalFieldNut;
        private FOAMParameterPatch<double> m_WallNut;
        private FOAMParameterPatch<double> m_InletNut;
        private FOAMParameterPatch<double> m_OutletNut;


        //k
        private double m_InternalFieldK;
        private FOAMParameterPatch<double> m_WallK;
        private FOAMParameterPatch<double> m_InletK;
        private FOAMParameterPatch<double> m_OutletK;


        //g
        private double m_GValue;

        //transportProperties
        private TransportModel m_TransportModel;
        private Dictionary<string, object> m_TransportModelParameter;


        //turbulenceProperties
        private TurbulenceParameter m_TurbulenceParameter;


        //SSH
        private SSH m_SSH;

        //General
        private bool m_OpenFOAM;
        private bool m_IncludeLinkedModels;
        private bool m_exportColor;
        private bool m_exportSharedCoordinates;
        private List<Category> m_SelectedCategories;
        private DisplayUnitType m_Units;

        //Getter-Setter Runmanager
        public OpenFOAMEnvironment OpenFOAMEnvironment { get => m_openFOAMEnvironment; set => m_openFOAMEnvironment = value; }


        //Getter-Setter BlockMeshDict
        public Vector3D SimpleGrading { get => m_SimpleGrading; set => m_SimpleGrading = value; }
        public Vector3D CellSize { get => m_CellSize; set => m_CellSize = value; }


        //Getter-Setter ControlDict        
        public SolverIncompressible AppIncompressible { get => m_AppIncompressible; set => m_AppIncompressible = value; }
        public TimeFormat _TimeFormat { get => m_TimeFormat; set => m_TimeFormat = value; }
        public WriteFormat _WriteFormat { get => m_WriteFormat; set => m_WriteFormat = value; }
        public WriteControl _WriteControl { get => m_WriteControl; set => m_WriteControl = value; }
        public StopAt _StopAt { get => m_StopAt; set => m_StopAt = value; }
        public WriteCompression WriteCompression { get => m_WriteCompression; set => m_WriteCompression = value; }
        public StartFrom _StartFrom { get => m_StartFrom; set => m_StartFrom = value; }
        public double StartTime { get => m_StartTime; set => m_StartTime = value; }
        public double EndTime { get => m_EndTime; set => m_EndTime = value; }
        public double DeltaT { get => m_DeltaT; set => m_DeltaT = value; }
        public double WriteInterval { get => m_WriteInterval; set => m_WriteInterval = value; }
        public double PurgeWrite { get => m_PurgeWrite; set => m_PurgeWrite = value; }
        public double WritePrecision { get => m_WritePrecision; set => m_WritePrecision = value; }
        public double TimePrecision { get => m_TimePrecision; set => m_TimePrecision = value; }
        public bool RunTimeModifiable { get => m_RunTimeModifiable; set => m_RunTimeModifiable = value; }


        //Getter-Setter SurfaceFeatureExtract
        public ExtractionMethod ExtractionMethod { get => m_ExtractionMethod; set => m_ExtractionMethod = value; }
        public Dictionary<string, object> ExtractFromSurfaceCoeffs { get => m_ExtractFromSurfaceCoeffs; set => m_ExtractFromSurfaceCoeffs = value; }
        public int IncludedAngle { get => m_IncludedAngle; set => m_IncludedAngle = value; }
        public string WriteObj { get => m_WriteObj; set => m_WriteObj = value; }


        //Getter-Setter DecomposeParDict
        public int NumberOfSubdomains { get => m_NumberOfSubdomains; set => m_NumberOfSubdomains = value; }
        public MethodDecompose MethodDecompose { get => m_MethodDecompose; set => m_MethodDecompose = value; }
        public CoeffsMethod SimpleCoeffs { get => m_SimpleCoeffs; set => m_SimpleCoeffs = value; }
        public CoeffsMethod HierarchicalCoeffs { get => m_HierarchicalCoeffs; set => m_HierarchicalCoeffs = value; }
        public string Order { get => m_Order; set => m_Order = value; }
        public string DataFile { get => m_DataFile; set => m_DataFile = value; }


        //Getter-Setter FvSchemes
        public KeyValuePair<string, string> DdtSchemes { get => m_ddtSchemes; set => m_ddtSchemes = value; }
        public KeyValuePair<string, string> GradSchemes { get => m_gradSchemes; set => m_gradSchemes = value; }
        public List<KeyValuePair<string, string>> DivSchemes { get => m_divSchemes; set => m_divSchemes = value; }
        public KeyValuePair<string, string> LaplacianSchemes { get => m_laplacianSchemes; set => m_laplacianSchemes = value; }
        public KeyValuePair<string, string> InterpolationSchemes { get => m_interpolationSchemes; set => m_interpolationSchemes = value; }
        public KeyValuePair<string, string> SnGradSchemes { get => m_snGradSchemes; set => m_snGradSchemes = value; }
        public KeyValuePair<string, string> FluxRequired { get => m_fluxRequired; set => m_fluxRequired = value; }


        //Getter-Setter FvSolution
        public PFv P1 { get => m_p; set => m_p = value; }
        public FvSolutionParamter U1 { get => m_U; set => m_U = value; }
        public FvSolutionParamter K { get => m_k; set => m_k = value; }
        public FvSolutionParamter Epsilon { get => m_epsilon; set => m_epsilon = value; }
        public int NNonOrhtogonalCorrectors { get => m_nNonOrhtogonalCorrectors; set => m_nNonOrhtogonalCorrectors = value; }
        public double RelaxFactor_k { get => m_relaxFactor_k; set => m_relaxFactor_k = value; }
        public double RelaxFactor_U { get => m_relaxFactor_U; set => m_relaxFactor_U = value; }
        public double RelaxFactor_epsilon { get => m_relaxFactor_epsilon; set => m_relaxFactor_epsilon = value; }
        public double RelaxFactor_p { get => m_relaxFactor_p; set => m_relaxFactor_p = value; }
        public Dictionary<string, object> ResidualControl { get => m_residualControl; set => m_residualControl = value; }


        //Getter-Setter SnappyHexMesh
        public bool CastellatedMesh { get => m_CastellatedMesh; set => m_CastellatedMesh = value; }
        public bool Snap { get => m_Snap; set => m_Snap = value; }
        public bool AddLayers { get => m_AddLayers; set => m_AddLayers = value; }
        public int Debug { get => m_Debug; set => m_Debug = value; }
        public double MergeTolerance { get => m_MergeTolerance; set => m_MergeTolerance = value; }
        public int MaxLocalCells { get => m_MaxLocalCells; set => m_MaxLocalCells = value; }
        public int MaxGlobalCells { get => m_MaxGlobalCells; set => m_MaxGlobalCells = value; }
        public int MinRefinementCalls { get => m_MinRefinementCalls; set => m_MinRefinementCalls = value; }
        public int ResolveFeatureAngle { get => m_ResolveFeatureAngle; set => m_ResolveFeatureAngle = value; }
        public int NCellsBetweenLevels { get => m_NCellsBetweenLevels; set => m_NCellsBetweenLevels = value; }
        public double MaxLoadUnbalance { get => m_MaxLoadUnbalance; set => m_MaxLoadUnbalance = value; }
        public ArrayList Features { get => m_Features; set => m_Features = value; }
        public string NameEMesh { get => m_NameEMesh; set => m_NameEMesh = value; }
        public int FeatureLevel { get => m_FeatureLevel; set => m_FeatureLevel = value; }
        public Vector WallLevel { get => m_WallLevel; set => m_WallLevel = value; }
        public Vector OutletLevel { get => m_OutletLevel; set => m_OutletLevel = value; }
        public Vector InletLevel { get => m_InletLevel; set => m_InletLevel = value; }
        public Dictionary<string, object> RefinementRegions { get => m_RefinementRegions; set => m_RefinementRegions = value; }
        public bool AllowFreeStandingZoneFaces { get => m_AllowFreeStandingZoneFaces; set => m_AllowFreeStandingZoneFaces = value; }
        public int NSmoothPatch { get => m_NSmoothPatch; set => m_NSmoothPatch = value; }
        public int Tolerance { get => m_Tolerance; set => m_Tolerance = value; }
        public int NSolverIter { get => m_NSolverIter; set => m_NSolverIter = value; }
        public int NRelaxIterSnap { get => m_NRelaxIterSnap; set => m_NRelaxIterSnap = value; }
        public int NFeatureSnapIter { get => m_NFeatureSnapIter; set => m_NFeatureSnapIter = value; }
        public bool ImplicitFeatureSnap { get => m_ImplicitFeatureSnap; set => m_ImplicitFeatureSnap = value; }
        public bool MultiRegionFeatureSnap { get => m_MultiRegionFeatureSnap; set => m_MultiRegionFeatureSnap = value; }
        public bool RelativeSizes { get => m_RelativeSizes; set => m_RelativeSizes = value; }
        public double ExpansionRatio { get => m_ExpansionRatio; set => m_ExpansionRatio = value; }
        public double FinalLayerThickness { get => m_FinalLayerThickness; set => m_FinalLayerThickness = value; }
        public double MinThickness { get => m_MinThickness; set => m_MinThickness = value; }
        public double MaxFaceThicknessRatio { get => m_MaxFaceThicknessRatio; set => m_MaxFaceThicknessRatio = value; }
        public double MaxThicknessToMeadialRatio { get => m_MaxThicknessToMeadialRatio; set => m_MaxThicknessToMeadialRatio = value; }
        public int NGrow { get => m_NGrow; set => m_NGrow = value; }
        public int FeatureAngle { get => m_FeatureAngle; set => m_FeatureAngle = value; }
        public int NRelaxedIterLayer { get => m_NRelaxedIterLayer; set => m_NRelaxedIterLayer = value; }
        public int NSmoothSurfaceNormals { get => m_nSmoothSurfaceNormals; set => m_nSmoothSurfaceNormals = value; }
        public int NSmoothThickness { get => m_NSmoothThickness; set => m_NSmoothThickness = value; }
        public int NSmoothNormals { get => m_NSmoothNormals; set => m_NSmoothNormals = value; }
        public int MinMedianAxisAngle { get => m_MinMedianAxisAngle; set => m_MinMedianAxisAngle = value; }
        public int NBufferCellsNoExtrude { get => m_NBufferCellsNoExtrude; set => m_NBufferCellsNoExtrude = value; }
        public int NLayerIter { get => m_NLayerIter; set => m_NLayerIter = value; }
        public Dictionary<string, object> Layers { get => m_Layers; set => m_Layers = value; }
        public int MaxNonOrtho { get => m_MaxNonOrtho; set => m_MaxNonOrtho = value; }
        public int MaxBoundarySkewness { get => m_MaxBoundarySkewness; set => m_MaxBoundarySkewness = value; }
        public int MaxInternalSkewness { get => m_MaxInternalSkewness; set => m_MaxInternalSkewness = value; }
        public int MaxConcave { get => m_MaxConcave; set => m_MaxConcave = value; }
        public double MinFlatness { get => m_MinFlatness; set => m_MinFlatness = value; }
        public double MinVol { get => m_MinVol; set => m_MinVol = value; }
        public double MinTetQuality { get => m_MinTetQuality; set => m_MinTetQuality = value; }
        public int MinArea { get => m_MinArea; set => m_MinArea = value; }
        public double MinTwist { get => m_MinTwist; set => m_MinTwist = value; }
        public double MinDeterminant { get => m_MinDeterminant; set => m_MinDeterminant = value; }
        public double MinFaceWeight { get => m_MinFaceWeight; set => m_MinFaceWeight = value; }
        public double MinVolRatio { get => m_MinVolRatio; set => m_MinVolRatio = value; }
        public int MinTriangleTwist { get => m_MinTriangleTwist; set => m_MinTriangleTwist = value; }
        public int NSmoothScale { get => m_NSmoothScale; set => m_NSmoothScale = value; }
        public double ErrorReduction { get => m_ErrorReduction; set => m_ErrorReduction = value; }
        public Dictionary<string, object> Relaxed { get => m_Relaxed; set => m_Relaxed = value; }
        public int MaxNonOrthoMeshQualtiy { get => m_MaxNonOrthoMeshQualtiy; set => m_MaxNonOrthoMeshQualtiy = value; }
        public int NRelaxeIterLayer { get => m_NRelaxeIterLayer; set => m_NRelaxeIterLayer = value; }

        //Getter-Setter-U
        public Vector3D InternalFieldU { get => m_InternalFieldU; set => m_InternalFieldU = value; }
        public FOAMParameterPatch<Vector3D> WallU { get => m_WallU; set => m_WallU = value; }
        public FOAMParameterPatch<Vector3D> InletU { get => m_InletU; set => m_InletU = value; }
        public FOAMParameterPatch<Vector3D> OutletU { get => m_OutletU; set => m_OutletU = value; }

        //Getter-Setter-Epsilon
        public double InternalFieldEpsilon { get => m_InternalFieldEpsilon; set => m_InternalFieldEpsilon = value; }
        public FOAMParameterPatch<double> WallEpsilon { get => m_WallEpsilon; set => m_WallEpsilon = value; }
        public FOAMParameterPatch<double> InletEpsilon { get => m_InletEpsilon; set => m_InletEpsilon = value; }
        public FOAMParameterPatch<double> OutletEpsilon { get => m_OutletEpsilon; set => m_OutletEpsilon = value; }

        //Getter-Setter-P
        public double InternalFieldP { get => m_InternalFieldP; set => m_InternalFieldP = value; }
        public FOAMParameterPatch<double> WallP { get => m_WallP; set => m_WallP = value; }
        public FOAMParameterPatch<double> InletP { get => m_InletP; set => m_InletP = value; }
        public FOAMParameterPatch<double> OutletP { get => m_OutletP; set => m_OutletP = value; }

        //Getter-Setter-Nut
        public double InternalFieldNut { get => m_InternalFieldNut; set => m_InternalFieldNut = value; }
        public FOAMParameterPatch<double> WallNut { get => m_WallNut; set => m_WallNut = value; }
        public FOAMParameterPatch<double> InletNut { get => m_InletNut; set => m_InletNut = value; }
        public FOAMParameterPatch<double> OutletNut { get => m_OutletNut; set => m_OutletNut = value; }

        //Getter-Setter-K
        public double InternalFieldK { get => m_InternalFieldK; set => m_InternalFieldK = value; }
        public FOAMParameterPatch<double> WallK { get => m_WallK; set => m_WallK = value; }
        public FOAMParameterPatch<double> InletK { get => m_InletK; set => m_InletK = value; }
        public FOAMParameterPatch<double> OutletK { get => m_OutletK; set => m_OutletK = value; }

        //Getter-Setter-G
        public double GValue { get => m_GValue; set => m_GValue = value; }

        //Getter-Setter-TransportProperties
        public TransportModel TransportModel { get => m_TransportModel; set => m_TransportModel = value; }
        public Dictionary<string,object> TransportModelParameter { get => m_TransportModelParameter; }

        //Getter-Setter-TurbulenceProperties
        public TurbulenceParameter TurbulenceParameter { get => m_TurbulenceParameter; set => m_TurbulenceParameter = value; }

        //Getter-Setter-SSH
        public SSH SSH { get => m_SSH; set => m_SSH = value; }

        /// <summary>
        /// Binary or ASCII STL file.
        /// </summary>
        public SaveFormat SaveFormat
        {
            get
            {
                return m_SaveFormat;
            }
            set
            {
                m_SaveFormat = value;
            }
        }

        /// <summary>
        /// The range of elements to be exported.
        /// </summary>
        public ElementsExportRange ExportRange
        {
            get
            {
                return m_ExportRange;
            }
            set
            {
                m_ExportRange = value;
            }
        }

        /// <summary>
        /// SnappyHexMesh or cfMesh.
        /// </summary>
        public MeshType Mesh
        {
            get
            {
                return m_Mesh;
            }
        }

        /// <summary>
        /// Start simulation.
        /// </summary>
        public bool OpenFOAM
        {
            get
            {
                return m_OpenFOAM;
            }
            set
            {
                m_OpenFOAM = value;
            }
        }

        /// <summary>
        /// Include linked models.
        /// </summary>
        public bool IncludeLinkedModels
        {
            get
            {
                return m_IncludeLinkedModels;
            }
            set
            {
                m_IncludeLinkedModels = value;
            }
        }

        /// <summary>
        /// Export Color.
        /// </summary>
        public bool ExportColor
        {
            get
            {
                return m_exportColor;
            }
            set
            {
                m_exportColor = value;
            }
        }

        /// <summary>
        /// Export point in shared coordinates.
        /// </summary>
        public bool ExportSharedCoordinates
        {
            get
            {
                return m_exportSharedCoordinates;
            }
            set
            {
                m_exportSharedCoordinates = value;
            }
        }

        /// <summary>
        /// Include selected categories.
        /// </summary>
        public List<Category> SelectedCategories
        {
            get
            {
                return m_SelectedCategories;
            }
            set
            {
                m_SelectedCategories = value;
            }
        }

        /// <summary>
        /// Units for STL.
        /// </summary>
        public DisplayUnitType Units
        {
            get
            {
                return m_Units;
            }
            set
            {
                m_Units = value;
            }
        }

        public Dictionary<string, object> SimulationDefault
        {
            get
            {
                return m_SimulationDefaultList;
            }
            set
            {
                this.SimulationDefault = value;
            }
        }



        /// <summary>
        /// General Constructor for Test.
        /// </summary>
        /// <param name="saveFormat">Save format for stl.</param>
        /// <param name="exportRange"></param>
        /// <param name="mesh"></param>
        /// <param name="startFrom"></param>
        /// <param name="stopAt"></param>
        /// <param name="writeControl"></param>
        /// <param name="writeFormat"></param>
        /// <param name="timeFormat"></param>
        public Settings(SaveFormat saveFormat = SaveFormat.ascii, ElementsExportRange exportRange = ElementsExportRange.OnlyVisibleOnes, MeshType mesh = MeshType.Snappy, OpenFOAMEnvironment openFOAMEnv = OpenFOAMEnvironment.blueCFD,
            StartFrom startFrom = StartFrom.latestTime, SolverIncompressible appInc = SolverIncompressible.simpleFoam,
            StopAt stopAt = StopAt.endTime, WriteControl writeControl = WriteControl.timeStep, WriteFormat writeFormat = WriteFormat.ascii, WriteCompression writeCompression = WriteCompression.off,
            TimeFormat timeFormat = TimeFormat.general, ExtractionMethod extractionMethod = ExtractionMethod.extractFromSurface, MethodDecompose methodDecompose = MethodDecompose.simple, Agglomerator agglomerator = Agglomerator.faceAreaPair, CacheAgglomeration cacheAgglomeration = CacheAgglomeration.on, SolverFV solverP = SolverFV.GAMG,
            SolverFV solverU = SolverFV.smoothSolver, SolverFV solverK = SolverFV.smoothSolver, SolverFV solverEpsilon = SolverFV.smoothSolver, Smoother smootherU = Smoother.GaussSeidel, Smoother smootherK = Smoother.GaussSeidel,
            Smoother smootherEpsilon = Smoother.GaussSeidel, TransportModel transportModel = TransportModel.Newtonian, SimulationType simulationType = SimulationType.RAS)
        {
            //Dictionary for setting default values in OpenFOAM-Tab
            m_SimulationDefaultList = new Dictionary<string, object>();

            m_System = new Dictionary<string, object>();
            m_Constant = new Dictionary<string, object>();
            m_Null = new Dictionary<string, object>();

            m_SaveFormat = saveFormat;
            m_ExportRange = exportRange;
            m_Mesh = mesh;
            m_openFOAMEnvironment = openFOAMEnv;

            //blockMeshDict

            m_CellSize = new Vector3D(0, 0, 0);
            m_SimpleGrading = new Vector3D(1.0, 1.0, 1.0);

            //controlDict

            m_AppIncompressible = appInc;
            m_StartFrom = startFrom;
            m_StartTime = 0;
            m_StopAt = stopAt;
            m_EndTime = 100;
            m_DeltaT = 1;
            m_WriteControl = writeControl;
            m_WriteInterval = 100;
            m_PurgeWrite = 0;
            m_WriteFormat = writeFormat;
            m_WritePrecision = 8;
            m_WriteCompression = writeCompression;
            m_TimeFormat = timeFormat;
            m_TimePrecision = 6;
            m_RunTimeModifiable = true;


            //surfaceFeatureExtract

            m_ExtractionMethod = extractionMethod;
            m_IncludedAngle = 150;
            m_WriteObj = "yes";


            //decomposeParDict

            m_NumberOfSubdomains = 4;
            m_MethodDecompose = methodDecompose;

            m_SimpleCoeffs = new CoeffsMethod
            {
                Delta = 0.001
            };
            m_SimpleCoeffs.SetN(new Vector3D(2, 2, 1));

            m_HierarchicalCoeffs = new CoeffsMethod
            {
                Delta = 0.001
            };
            m_HierarchicalCoeffs.SetN(new Vector3D(2, 2, 1));
            m_Order = "xyz";

            m_DataFile = "cellDecomposition";

            //FvSchemes

            //To-DO: Implement schemes depending on used Simulation model
            m_ddtSchemes = new KeyValuePair<string, string>("default", "steadyState");
            m_gradSchemes = new KeyValuePair<string, string>("default", "cellLimited leastSquares 1");
            m_divSchemes = new List<KeyValuePair<string, string>>
            {
                {new KeyValuePair<string, string>("default", "none") },
                {new KeyValuePair<string, string>("div(phi,epsilon)", "bounded Gauss linearUpwind grad(epsilon)") },
                {new KeyValuePair<string, string>("div(phi,U)", "bounded Gauss linearUpwindV grad(U)")},
                {new KeyValuePair<string, string>("div((nuEff*dev2(T(grad(U)))))", "Gauss linear") },
                {new KeyValuePair<string, string>("div(phi,k)", "bounded Gauss linearUpwind grad(k)")}
            };
            m_laplacianSchemes = new KeyValuePair<string, string>("default", "Gauss linear limited corrected 0.333");
            m_interpolationSchemes = new KeyValuePair<string, string>("default", "linear");
            m_snGradSchemes = new KeyValuePair<string, string>("default", "limited corrected 0.333");
            m_fluxRequired = new KeyValuePair<string, string>("default", "no");


            //FvSolution

            FvSolutionParamter _p = new FvSolutionParamter();
            _p.Solver = solverP;
            _p.RelTol = 0.1;
            _p.Tolerance = 1e-7;
            _p.NSweeps = 0;

            //p-FvSolution-Solvers
            m_p = new PFv
            {
                Param = _p,
                MergeLevels = 1,
                NPreSweepsre = 0,
                NPostSweeps = 2,
                NCellsInCoarsesLevel = 10,
                Agglomerator = agglomerator,
                CacheAgglomeration = cacheAgglomeration
            };

            //U-FvSolution-Solver
            m_U = new FvSolutionParamter
            {
                RelTol = 0.1,
                Tolerance = 1e-8,
                NSweeps = 1,
                Solver = solverU,
                Smoother = smootherU
            };

            //k-FvSolution-Solver
            m_k = new FvSolutionParamter
            {
                RelTol = 0.1,
                Tolerance = 1e-8,
                NSweeps = 1,
                Solver = solverK,
                Smoother = smootherK
            };

            //epsilon-FvSolution-Solver
            m_epsilon = new FvSolutionParamter
            {
                RelTol = 0.1,
                Tolerance = 1e-8,
                NSweeps = 1,
                Solver = solverEpsilon,
                Smoother = smootherEpsilon
            };


            //FvSolution-SIMPLE
            m_nNonOrhtogonalCorrectors = 2;
            m_residualControl = new Dictionary<string, object>();

            //FvSolution-relaxationFactors
            m_relaxFactor_k = 0.7;
            m_relaxFactor_U = 0.7;
            m_relaxFactor_epsilon = 0.7;
            m_relaxFactor_p = 0.3;

            //SnappyHexMesh-General
            m_CastellatedMesh = true;
            m_Snap = true;
            m_AddLayers = false;
            m_Debug = 0;
            m_MergeTolerance = 1e-6;

            //SnappyHexMesh-CastellatedMeshControls

            m_MaxLocalCells = 100000;
            m_MaxGlobalCells = 2000000;
            m_MinRefinementCalls = 10;
            m_MaxLoadUnbalance = 0.10;
            m_NCellsBetweenLevels = 3;
            m_Features = new ArrayList();
            m_FeatureLevel = 4;
            m_WallLevel = new Vector(3, 3);
            m_OutletLevel = new Vector(4, 4);
            m_InletLevel = new Vector(4, 4);
            m_ResolveFeatureAngle = 180;
            m_RefinementRegions = new Dictionary<string, object>();
            m_AllowFreeStandingZoneFaces = true;

            //SnappyHexMesh-SnapControls

            m_NSmoothPatch = 5;
            m_Tolerance = 5;
            m_NSolverIter = 100;
            m_NRelaxIterSnap = 8;
            m_NFeatureSnapIter = 10;
            m_ImplicitFeatureSnap = true;
            m_MultiRegionFeatureSnap = true;


            //SnappyHexMesh-AddLayersControl
            m_RelativeSizes = true;
            m_Layers = new Dictionary<string, object>();
            m_ExpansionRatio = 1.1;
            m_FinalLayerThickness = 0.7;
            m_MinThickness = 0.1;
            m_NGrow = 0;
            m_FeatureAngle = 110;
            m_NRelaxeIterLayer = 3;
            m_nSmoothSurfaceNormals = 1;
            m_NSmoothThickness = 10;
            m_NSmoothNormals = 3;
            m_MaxFaceThicknessRatio = 0.5;
            m_MaxThicknessToMeadialRatio = 0.3;
            m_MinMedianAxisAngle = 130;
            m_NBufferCellsNoExtrude = 0;
            m_NLayerIter = 50;
            m_NRelaxedIterLayer = 20;

            //SnappyHexMesh-MeshQualityControls
            m_MaxNonOrthoMeshQualtiy = 60;
            m_MaxBoundarySkewness = 20;
            m_MaxInternalSkewness = 4;
            m_MaxConcave = 80;
            m_MinFlatness = 0.5;
            m_MinVol = 1e-13;
            m_MinTetQuality = 1e-15;
            m_MinArea = -1;
            m_MinTwist = 0.02;
            m_MinDeterminant = 0.001;
            m_MinFaceWeight = 0.02;
            m_MinVolRatio = 0.01;
            m_MinTriangleTwist = -1;
            m_NSmoothScale = 4;
            m_ErrorReduction = 0.75;
            m_MaxNonOrtho = 75;


            //U

            m_InternalFieldU = new Vector3D(0, 0, 0);
            m_WallU = new FOAMParameterPatch<Vector3D>("fixedValue", "uniform", new Vector3D(0, 0, 0));
            m_InletU = new FOAMParameterPatch<Vector3D>("fixedValue", "uniform", new Vector3D(0.0, 0.0, -5.0));
            m_OutletU = new FOAMParameterPatch<Vector3D>("inletOutlet", "uniform", new Vector3D(0, 0, 0));
            m_OutletU.Attributes.Add("inletValue uniform", new Vector3D(0, 0, 0));


            //Epsilon

            m_InternalFieldEpsilon = 0.01;
            m_WallEpsilon = new FOAMParameterPatch<double>("epsilonWallFunction", "uniform", 0.01);
            m_InletEpsilon = new FOAMParameterPatch<double>("fixedValue", "uniform", 0.01);
            m_OutletEpsilon = new FOAMParameterPatch<double>("inletOutlet", "uniform", 0.1);
            m_OutletEpsilon.Attributes.Add("inletValue uniform", 0.1);


            //Nut

            m_InternalFieldNut = 0;
            m_WallNut = new FOAMParameterPatch<double>("fixedValue", "uniform", 0.01);
            m_InletNut = new FOAMParameterPatch<double>("calculated", "uniform", 0);
            m_OutletNut = new FOAMParameterPatch<double>("calculated", "uniform", 0);


            //P

            m_InternalFieldP = 0;
            m_WallP = new FOAMParameterPatch<double>("zeroGradient");
            m_InletP = new FOAMParameterPatch<double>("zeroGradient");
            m_OutletP = new FOAMParameterPatch<double>("fixedValue", "uniform", 126.7);


            //K

            m_InternalFieldK = 0.1;
            m_WallK = new FOAMParameterPatch<double>("kqRWallFunction", "uniform", 0.1);
            m_InletK = new FOAMParameterPatch<double>("fixedValue", "uniform", 0.1);
            m_OutletK = new FOAMParameterPatch<double>("inletOutlet", "uniform", 0.1);
            m_OutletK.Attributes.Add("inletValue uniform", 0.1);


            //g
            m_GValue = -9.81;


            //TransportProperties

            m_TransportModel = transportModel;
            m_TransportModelParameter = new Dictionary<string, object>();
            m_TransportModelParameter.Add("nu", 1e-05);
            m_TransportModelParameter.Add("beta", 3e-03);
            m_TransportModelParameter.Add("TRef", 300);
            m_TransportModelParameter.Add("Pr", 0.9);
            m_TransportModelParameter.Add("Prt", 0.7);
            m_TransportModelParameter.Add("Cp0", 1000);


            //TurbulenceProperties
            RASModel rasModel = RASModel.RNGkEpsilon;
            m_TurbulenceParameter = new TurbulenceParameter(simulationType, rasModel, true, true);

            InitSystemDictionary();
            InitConstantDictionary();
            InitNullDictionary();

            //SSH
            m_SSH = new SSH("mdjur", "192.168.2.102", "source /opt/openfoam6/etc/bashrc", "/home/mdjur/OpenFOAMRemote/", true, true, 22);

            //General
            m_OpenFOAM = false;
            m_IncludeLinkedModels = false;
            m_exportColor = false;
            m_exportSharedCoordinates = false;
            m_SelectedCategories = new List<Category>();
            m_Units = DisplayUnitType.DUT_UNDEFINED;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="saveFormat"></param>
        /// <param name="exportRange"></param>
        /// <param name="openFoam"></param>
        /// <param name="includeLinkedModels"></param>
        /// <param name="exportColor"></param>
        /// <param name="exportSharedCoordinates"></param>
        /// <param name="runTimeModifiable"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="deltaT"></param>
        /// <param name="writeInterval"></param>
        /// <param name="purgeWrite"></param>
        /// <param name="writePrecision"></param>
        /// <param name="timePrecision"></param>
        /// <param name="numberOfSubdomains"></param>
        /// <param name="selectedCategories"></param>
        /// <param name="units"></param>
        /// <param name="mesh"></param>
        /// <param name="windowsFOAMEnv"></param>
        /// <param name="startFrom"></param>
        /// <param name="appInc"></param>
        /// <param name="stopAt"></param>
        /// <param name="writeControl"></param>
        /// <param name="writeFormat"></param>
        /// <param name="writeCompression"></param>
        /// <param name="timeFormat"></param>
        /// <param name="extractionMethod"></param>
        /// <param name="methodDecompose"></param>
        /// <param name="agglomerator"></param>
        /// <param name="cacheAgglomeration"></param>
        /// <param name="solverP"></param>
        /// <param name="solverU"></param>
        /// <param name="solverK"></param>
        /// <param name="solverEpsilon"></param>
        /// <param name="smootherU"></param>
        /// <param name="smootherK"></param>
        /// <param name="smootherEpsilon"></param>
        /// <param name="transportModel"></param>
        /// <param name="simulationType"></param>
        public Settings(SaveFormat saveFormat, ElementsExportRange exportRange, bool openFoam, bool includeLinkedModels, bool exportColor, bool exportSharedCoordinates, bool runTimeModifiable,
            double startTime, double endTime, double deltaT, double writeInterval, double purgeWrite, double writePrecision, double timePrecision, int numberOfSubdomains,
            List<Category> selectedCategories, DisplayUnitType units, MeshType mesh = MeshType.Snappy, OpenFOAMEnvironment windowsFOAMEnv = OpenFOAMEnvironment.blueCFD,
            StartFrom startFrom = StartFrom.latestTime, SolverIncompressible appInc = SolverIncompressible.simpleFoam,
            StopAt stopAt = StopAt.endTime, WriteControl writeControl = WriteControl.timeStep, WriteFormat writeFormat = WriteFormat.ascii, WriteCompression writeCompression = WriteCompression.off,
            TimeFormat timeFormat = TimeFormat.general, ExtractionMethod extractionMethod = ExtractionMethod.extractFromSurface, MethodDecompose methodDecompose = MethodDecompose.simple, Agglomerator agglomerator = Agglomerator.faceAreaPair, CacheAgglomeration cacheAgglomeration = CacheAgglomeration.on, SolverFV solverP = SolverFV.GAMG,
            SolverFV solverU = SolverFV.smoothSolver, SolverFV solverK = SolverFV.smoothSolver, SolverFV solverEpsilon = SolverFV.smoothSolver, Smoother smootherU = Smoother.GaussSeidel, Smoother smootherK = Smoother.GaussSeidel,
            Smoother smootherEpsilon = Smoother.GaussSeidel, TransportModel transportModel = TransportModel.Newtonian, SimulationType simulationType = SimulationType.RAS)
        {
            //Dictionary for setting default values in OpenFOAM-Tab
            m_SimulationDefaultList = new Dictionary<string, object>();

            m_System = new Dictionary<string, object>();
            m_Constant = new Dictionary<string, object>();
            m_Null = new Dictionary<string, object>();

            m_SaveFormat = saveFormat;
            m_ExportRange = exportRange;
            m_Mesh = mesh;
            m_openFOAMEnvironment = windowsFOAMEnv;


            //blockMeshDict

            m_CellSize = new Vector3D(0,0,0);
            m_SimpleGrading = new Vector3D(1.0, 1.0, 1.0);


            //ControlDict

            m_AppIncompressible = appInc;
            m_StartFrom = startFrom;
            m_StartTime = startTime;
            m_StopAt = stopAt;
            m_EndTime = endTime;
            m_DeltaT = deltaT;
            m_WriteControl = writeControl;
            m_WriteInterval = writeInterval;
            m_PurgeWrite = purgeWrite;
            m_WriteFormat = writeFormat;
            m_WritePrecision = writePrecision;
            m_WriteCompression = writeCompression;
            m_TimeFormat = timeFormat;
            m_TimePrecision = timePrecision;
            m_RunTimeModifiable = runTimeModifiable;


            //surfaceFeatureExtract

            m_ExtractionMethod = extractionMethod;
            m_IncludedAngle = 150;
            m_WriteObj = "yes";


            //DecomposeParDict

            m_NumberOfSubdomains = numberOfSubdomains;
            m_MethodDecompose = methodDecompose;

            m_SimpleCoeffs = new CoeffsMethod
            {
                Delta = 0.001
            };
            m_SimpleCoeffs.SetN(new Vector3D(2, 2, 1));

            m_HierarchicalCoeffs = new CoeffsMethod
            {
                Delta = 0.001
            };
            m_HierarchicalCoeffs.SetN(new Vector3D(2, 2, 1));
            m_Order = "xyz";
            m_DataFile = "cellDecomposition";


            //FvSchemes

            m_ddtSchemes = new KeyValuePair<string, string>("default", "steadyState");
            m_gradSchemes = new KeyValuePair<string, string>("default", "cellLimited leastSquares 1");
            m_divSchemes = new List<KeyValuePair<string, string>>
            {
                {new KeyValuePair<string, string>("default", "none") },
                {new KeyValuePair<string, string>("div(phi,epsilon)", "bounded Gauss linearUpwind grad(epsilon)") },
                {new KeyValuePair<string, string>("div(phi,U)", "bounded Gauss linearUpwindV grad(U)")},
                {new KeyValuePair<string, string>("div((nuEff*dev2(T(grad(U)))))", "Gauss linear") },
                {new KeyValuePair<string, string>("div(phi,k)", "bounded Gauss linearUpwind grad(k)")}
            };
            m_laplacianSchemes = new KeyValuePair<string, string>("default", "Gauss linear limited corrected 0.333");
            m_interpolationSchemes = new KeyValuePair<string, string>("default", "linear");
            m_snGradSchemes = new KeyValuePair<string, string>("default", "limited corrected 0.333");
            m_fluxRequired = new KeyValuePair<string, string>("default", "no");


            //FvSolution

            FvSolutionParamter _p = new FvSolutionParamter
            {
                Solver = solverP,
                RelTol = 0.1,
                Tolerance = 1e-7,
                NSweeps = 0
            };

            //p-FvSolution-Solvers
            m_p = new PFv
            {
                Param = _p,
                MergeLevels = 1,
                NPreSweepsre = 0,
                NPostSweeps = 2,
                NCellsInCoarsesLevel = 10,
                Agglomerator = agglomerator,
                CacheAgglomeration = cacheAgglomeration
            };

            //U-FvSolution-Solver
            m_U = new FvSolutionParamter
            {
                RelTol = 0.1,
                Tolerance = 1e-8,
                NSweeps = 1,
                Solver = solverU,
                Smoother = smootherU
            };

            //k-FvSolution-Solver
            m_k = new FvSolutionParamter
            {
                RelTol = 0.1,
                Tolerance = 1e-8,
                NSweeps = 1,
                Solver = solverK,
                Smoother = smootherK
            };

            //epsilon-FvSolution-Solver
            m_epsilon = new FvSolutionParamter
            {
                RelTol = 0.1,
                Tolerance = 1e-8,
                NSweeps = 1,
                Solver = solverEpsilon,
                Smoother = smootherEpsilon
            };

            //FvSolution-SIMPLE
            m_nNonOrhtogonalCorrectors = 2;
            m_residualControl = new Dictionary<string, object>();

            //FvSolution-relaxationFactors
            m_relaxFactor_k = 0.7;
            m_relaxFactor_U = 0.7;
            m_relaxFactor_epsilon = 0.7;
            m_relaxFactor_p = 0.3;


            //SnappyHexMesh-General
            m_CastellatedMesh = true;
            m_Snap = true;
            m_AddLayers = false;
            m_Debug = 0;
            m_MergeTolerance = 1e-6;

            //SnappyHexMesh-CastellatedMeshControls
            m_MaxLocalCells = 100000;
            m_MaxGlobalCells = 2000000;
            m_MinRefinementCalls = 10;
            m_MaxLoadUnbalance = 0.10;
            m_NCellsBetweenLevels = 3;
            m_Features = new ArrayList();
            m_FeatureLevel = 3;
            m_WallLevel = new Vector(3, 3);
            m_OutletLevel = new Vector(4, 4);
            m_InletLevel = new Vector(4, 4);
            m_ResolveFeatureAngle = 180;
            m_RefinementRegions = new Dictionary<string, object>();
            m_AllowFreeStandingZoneFaces = true;

            //SnappyHexMesh-SnapControls
            m_NSmoothPatch = 5;
            m_Tolerance = 5;
            m_NSolverIter = 100;
            m_NRelaxIterSnap = 8;
            m_NFeatureSnapIter = 10;
            m_ImplicitFeatureSnap = true;
            m_MultiRegionFeatureSnap = true;

            //SnappyHexMesh-AddLayersControl
            m_RelativeSizes = true;
            m_Layers = new Dictionary<string, object>();
            m_ExpansionRatio = 1.1;
            m_FinalLayerThickness = 0.7;
            m_MinThickness = 0.1;
            m_NGrow = 0;
            m_FeatureAngle = 110;
            m_NRelaxeIterLayer = 3;
            m_nSmoothSurfaceNormals = 1;
            m_NSmoothThickness = 10;
            m_NSmoothNormals = 3;
            m_MaxFaceThicknessRatio = 0.5;
            m_MaxThicknessToMeadialRatio = 0.3;
            m_MinMedianAxisAngle = 130;
            m_NBufferCellsNoExtrude = 0;
            m_NLayerIter = 50;
            m_NRelaxedIterLayer = 20;

            //SnappyHexMesh-MeshQualityControls
            m_MaxNonOrthoMeshQualtiy = 60;
            m_MaxBoundarySkewness = 20;
            m_MaxInternalSkewness = 4;
            m_MaxConcave = 80;
            m_MinFlatness = 0.5;
            m_MinVol = 1e-13;
            m_MinTetQuality = 1e-15;
            m_MinArea = -1;
            m_MinTwist = 0.02;
            m_MinDeterminant = 0.001;
            m_MinFaceWeight = 0.02;
            m_MinVolRatio = 0.01;
            m_MinTriangleTwist = -1;
            m_NSmoothScale = 4;
            m_ErrorReduction = 0.75;
            m_MaxNonOrtho = 75;
            m_Relaxed = new Dictionary<string, object>
            {
                {"maxNonOrtho" ,m_MaxNonOrtho}
            };


            //U
            m_InternalFieldU = new Vector3D(0, 0, 0);
            m_WallU = new FOAMParameterPatch<Vector3D>("fixedValue", "uniform", new Vector3D(0, 0, 0));
            m_InletU = new FOAMParameterPatch<Vector3D>("fixedValue", "uniform", new Vector3D(0.0, 0.0, -5.0));
            m_OutletU = new FOAMParameterPatch<Vector3D>("inletOutlet", "uniform", new Vector3D(0, 0, 0));
            m_OutletU.Attributes.Add("inletValue uniform", new Vector3D(0, 0, 0));

            //Epsilon
            m_InternalFieldEpsilon = 0.01;
            m_WallEpsilon = new FOAMParameterPatch<double>("epsilonWallFunction", "uniform", 0.01);
            m_InletEpsilon = new FOAMParameterPatch<double>("fixedValue", "uniform", 0.01);
            m_OutletEpsilon = new FOAMParameterPatch<double>("inletOutlet", "uniform", 0.1);
            m_OutletEpsilon.Attributes.Add("inletValue uniform", 0.1);


            //Nut
            m_InternalFieldNut = 0;
            m_WallNut = new FOAMParameterPatch<double>("fixedValue", "uniform", 0.01);
            m_InletNut = new FOAMParameterPatch<double>("calculated", "uniform", 0);
            m_OutletNut = new FOAMParameterPatch<double>("calculated", "uniform", 0);


            //P
            m_InternalFieldP = 0;
            m_WallP = new FOAMParameterPatch<double>("zeroGradient");
            m_InletP = new FOAMParameterPatch<double>("zeroGradient");
            m_OutletP = new FOAMParameterPatch<double>("fixedValue", "uniform", 126.7);


            //K
            m_InternalFieldK = 0.1;
            m_WallK = new FOAMParameterPatch<double>("kqRWallFunction", "uniform", 0.1);
            m_InletK = new FOAMParameterPatch<double>("fixedValue", "uniform", 0.1);
            m_OutletK = new FOAMParameterPatch<double>("inletOutlet", "uniform", 0.1);
            m_OutletK.Attributes.Add("inletValue uniform", 0.1);

            //g
            m_GValue = -9.81;

            //TransportProperties
            m_TransportModel = transportModel;
            m_TransportModelParameter = new Dictionary<string,object>();
            m_TransportModelParameter.Add("nu", 1e-05);
            m_TransportModelParameter.Add("beta", 3e-03);
            m_TransportModelParameter.Add("TRef", 300);
            m_TransportModelParameter.Add("Pr", 0.9);
            m_TransportModelParameter.Add("Prt", 0.7);
            m_TransportModelParameter.Add("Cp0", 1000);

            //TurbulenceProperties
            RASModel rasModel = RASModel.RNGkEpsilon;
            m_TurbulenceParameter = new TurbulenceParameter(simulationType, rasModel, true, true);

            InitSystemDictionary();
            InitNullDictionary();
            InitConstantDictionary();

            //SSH
            m_SSH = new SSH("mdjur", "192.168.2.102", "source /opt/openfoam6/etc/bashrc", "/home/mdjur/OpenFOAMRemote/", true, true, 22);

            //General
            m_OpenFOAM = openFoam;
            m_IncludeLinkedModels = includeLinkedModels;
            m_exportColor = exportColor;
            m_exportSharedCoordinates = exportSharedCoordinates;
            m_SelectedCategories = selectedCategories;
            m_Units = units;

            CreateConfig();
        }

        /// <summary>
        /// Initialize system dicitonary and add it to simulationDefaultList.
        /// </summary>
        private void InitSystemDictionary()
        {
            CreateBlockMeshDictionary();
            CreateControlDictionary();
            CreateSurfaceFeatureExtractDictionary();
            CreateDecomposeParDictionary();
            CreateFvSchemesDictionary();
            CreateFvSolutionDictionary();
            CreateSnappyDictionary();

            m_SimulationDefaultList.Add("system", m_System);
        }

        /// <summary>
        /// Initialize null dicitonary and add it to simulationDefaultList.
        /// </summary>
        private void InitNullDictionary()
        {
            switch(m_AppIncompressible)
            {
                case SolverIncompressible.simpleFoam:
                {
                        CreateFoamParameterDictionary("U", m_InternalFieldU, m_WallU, m_InletU, m_OutletU);
                        CreateFoamParameterDictionary("epsilon", m_InternalFieldEpsilon, m_WallEpsilon, m_InletEpsilon, m_OutletEpsilon);
                        CreateFoamParameterDictionary("nut", m_InternalFieldNut, m_WallNut, m_InletNut, m_OutletNut);
                        CreateFoamParameterDictionary("p", m_InternalFieldP, m_WallP, m_InletP, m_OutletP);
                        CreateFoamParameterDictionary("k", m_InternalFieldK, m_WallK, m_InletK, m_OutletK);
                        break;
                }
                case SolverIncompressible.adjointShapeOptimizationFoam:
                case SolverIncompressible.boundaryFoam:
                case SolverIncompressible.icoFoam:
                case SolverIncompressible.nonNewtonianIcoFoam:
                case SolverIncompressible.pimpleDyMFoam:
                case SolverIncompressible.pimpleFoam:
                case SolverIncompressible.pisoFoam:
                case SolverIncompressible.porousSimpleFoam:
                case SolverIncompressible.shallowWaterFoam:
                case SolverIncompressible.SRFPimpleFoam:
                case SolverIncompressible.SRFSimpleFoam:
                    break;
            }

            m_SimulationDefaultList.Add("0", m_Null);
        }

        /// <summary>
        /// Initialize constant dicitonary and add it to simulationDefaultList.
        /// </summary>
        private void InitConstantDictionary()
        {
            CreateGDicitionary();
            CreateTransportPropertiesDictionary();
            CreateTurbulencePropertiesDictionary();

            m_SimulationDefaultList.Add("constant", m_Constant);
        }

        /// <summary>
        /// Creates a dictionary for blockMeshDict and adds it to the system Dictionary.
        /// </summary>
        private void CreateBlockMeshDictionary()
        {
            Dictionary<string, object> m_BlockMeshDict = new Dictionary<string, object>();

            m_BlockMeshDict.Add("cellSize", m_CellSize);
            m_BlockMeshDict.Add("simpleGrading", m_SimpleGrading);

            m_System.Add("blockMeshDict", m_BlockMeshDict);
        }

        /// <summary>
        /// Creates a dictionary for controlDict and adds it to the system Dictionary.
        /// </summary>
        private void CreateControlDictionary()
        {
            Dictionary<string, object> m_ControlDict = new Dictionary<string, object>();

            //solver in general-container of simulation-tab
            //m_ControlDict.Add("application", m_AppIncompressible);
            m_ControlDict.Add("startFrom", m_StartFrom);
            m_ControlDict.Add("startTime", m_StartTime);
            m_ControlDict.Add("stopAt", m_StopAt);
            m_ControlDict.Add("endTime", m_EndTime);
            m_ControlDict.Add("deltaT", m_DeltaT);
            m_ControlDict.Add("writeControl", m_WriteControl);
            m_ControlDict.Add("writeInterval", m_WriteInterval);
            m_ControlDict.Add("purgeWrite", m_PurgeWrite);
            m_ControlDict.Add("writeFormat", m_WriteFormat);
            m_ControlDict.Add("writePrecision", m_WritePrecision);
            m_ControlDict.Add("writeCompression", m_WriteCompression);
            m_ControlDict.Add("timeFormat", m_TimeFormat);
            m_ControlDict.Add("timePrecision", m_TimePrecision);
            m_ControlDict.Add("runTimeModifiable", m_RunTimeModifiable);

            m_System.Add("controlDict", m_ControlDict);
        }

        /// <summary>
        /// Creates a dictionary for surfaceFeatureExtract and adds it to the system Dictionary.
        /// </summary>
        private void CreateSurfaceFeatureExtractDictionary()
        {
            Dictionary<string, object> m_SurfaceFeatureExtractDict = new Dictionary<string, object>();

            m_ExtractFromSurfaceCoeffs = new Dictionary<string, object>()
            {
                {"includedAngle", m_IncludedAngle}
            };

            m_SurfaceFeatureExtractDict.Add("extractionMethod", m_ExtractionMethod);
            m_SurfaceFeatureExtractDict.Add("extractFromSurfaceCoeffs", m_ExtractFromSurfaceCoeffs);
            m_SurfaceFeatureExtractDict.Add("writeObj", m_WriteObj);

            m_System.Add("surfaceFeatureExtractDict", m_SurfaceFeatureExtractDict);
        }

        /// <summary>
        /// Creates a dictionary for decomposeParDict and adds it to the system Dictionary.
        /// </summary>
        private void CreateDecomposeParDictionary()
        {
            Dictionary<string, object> m_DecomposeParDict = new Dictionary<string, object>();

            m_DecomposeParDict.Add("method", m_MethodDecompose);
            m_DecomposeParDict.Add("simpleCoeffs", m_SimpleCoeffs.ToDictionary());
            Dictionary<string, object> hierarchical = m_HierarchicalCoeffs.ToDictionary();
            hierarchical.Add("order", m_Order);
            m_DecomposeParDict.Add("hierarchicalCoeefs", hierarchical);
            m_DecomposeParDict.Add("manualCoeffs", new Dictionary<string, object> { { "dataFile", m_DataFile } });

            m_System.Add("decomposeParDict", m_DecomposeParDict);

        }

        /// <summary>
        /// Creates a dictionary for fvSchemes and adds it to the system Dictionary.
        /// </summary>
        private void CreateFvSchemesDictionary()
        {
            Dictionary<string, object> m_FvSchemes = new Dictionary<string, object>();

            m_FvSchemes.Add("ddtSchemes", new Dictionary<string, object> { { m_ddtSchemes.Key, m_ddtSchemes.Value } });
            m_FvSchemes.Add("gradSchemes", new Dictionary<string, object> { { m_gradSchemes.Key, m_gradSchemes.Value } });
            Dictionary<string, object> divSchemes = new Dictionary<string, object>();
            foreach (var obj in m_divSchemes)
            {
                divSchemes.Add(obj.Key, obj.Value);
            }

            m_FvSchemes.Add("divSchemes", divSchemes);
            m_FvSchemes.Add("laplacianSchemes", new Dictionary<string, object> { { m_laplacianSchemes.Key, m_laplacianSchemes.Value } });
            m_FvSchemes.Add("interpolationSchemes", new Dictionary<string, object> { { m_interpolationSchemes.Key, m_interpolationSchemes.Value } });
            m_FvSchemes.Add("snGradSchemes", new Dictionary<string, object> { { m_snGradSchemes.Key, m_snGradSchemes.Value } });
            m_FvSchemes.Add("fluxRequired", new Dictionary<string, object> { { m_fluxRequired.Key, m_fluxRequired.Value } });

            m_System.Add("fvSchemes", m_FvSchemes);
        }

        /// <summary>
        /// Creates a dictionary for fvSolution ands add it to the system Dictionary.
        /// </summary>
        private void CreateFvSolutionDictionary()
        {
            Dictionary<string, object> m_FvSolution = new Dictionary<string, object>();
            Dictionary<string, object> m_Solvers = new Dictionary<string, object>
            {
                {"p", P1.ToDictionary() },
                {"U" , U1.ToDictionary() },
                {"k" , K.ToDictionary() },
                {"epsilon", Epsilon.ToDictionary() }
            };

            Dictionary<string, object> m_SIMPLE = new Dictionary<string, object>
            {
                {"nNonOrthogonalCorrectors" , NNonOrhtogonalCorrectors },
                {"residualControl", ResidualControl }
            };

            Dictionary<string, object> m_RelaxationFactors = new Dictionary<string, object>
            {
                {"k", RelaxFactor_k},
                {"U", RelaxFactor_U },
                {"epsilon", RelaxFactor_epsilon },
                {"p", RelaxFactor_p }
            };

            m_FvSolution.Add("solvers", m_Solvers);
            m_FvSolution.Add("SIMPLE", m_SIMPLE);
            m_FvSolution.Add("relaxationFactors", m_RelaxationFactors);

            m_System.Add("fvSolution", m_FvSolution);
        }

        /// <summary>
        /// Creates a dictionary for snappyHexMeshDict and adds it to the system Dictionary.
        /// </summary>
        private void CreateSnappyDictionary()
        {
            //SnappyHexMesh-General
            Dictionary<string, object> m_SnappyHexMeshDict = new Dictionary<string, object>();

            m_SnappyHexMeshDict.Add("castellatedMesh", m_CastellatedMesh);
            m_SnappyHexMeshDict.Add("snap", m_Snap);
            m_SnappyHexMeshDict.Add("addLayers", m_AddLayers);

            //SnappyHexMesh-CastellatedMeshControls
            Dictionary<string, object> m_CastellatedMeshControls = new Dictionary<string, object>();

            m_CastellatedMeshControls.Add("maxLocalCells", m_MaxLocalCells);
            m_CastellatedMeshControls.Add("maxGlobalCells", m_MaxGlobalCells);
            m_CastellatedMeshControls.Add("minRefinementCells", m_MinRefinementCalls);
            m_CastellatedMeshControls.Add("maxLoadUnbalance", m_MaxLoadUnbalance);
            m_CastellatedMeshControls.Add("nCellsBetweenLevels", m_NCellsBetweenLevels);
            m_CastellatedMeshControls.Add("features", m_Features);
            m_CastellatedMeshControls.Add("wallLevel", m_WallLevel);
            m_CastellatedMeshControls.Add("outletLevel", m_OutletLevel);
            m_CastellatedMeshControls.Add("inletLevel", m_InletLevel);
            m_CastellatedMeshControls.Add("resolveFeatureAngle", m_ResolveFeatureAngle);
            m_CastellatedMeshControls.Add("refinementRegions", m_RefinementRegions);
            m_CastellatedMeshControls.Add("allowFreeStandingZoneFaces", m_AllowFreeStandingZoneFaces);

            m_SnappyHexMeshDict.Add("castellatedMeshControls", m_CastellatedMeshControls);

            //SnappyHexMesh-SnapControls
            Dictionary<string, object> m_SnapControls = new Dictionary<string, object>();

            m_SnapControls.Add("nSmoothPatch", m_NSmoothPatch);
            m_SnapControls.Add("tolerance", m_Tolerance);
            m_SnapControls.Add("nSolveIter", m_NSolverIter);
            m_SnapControls.Add("nRelaxIter", m_NRelaxIterSnap);
            m_SnapControls.Add("nFeatureSnapIter", m_NFeatureSnapIter);
            m_SnapControls.Add("implicitFeatureSnap", m_ImplicitFeatureSnap);
            m_SnapControls.Add("multiRegionFeatureSnap", m_MultiRegionFeatureSnap);

            m_SnappyHexMeshDict.Add("snapControls", m_SnapControls);

            //SnappyHexMesh-AddLayersControl

            Dictionary<string, object> m_AddLayersControl = new Dictionary<string, object>();

            m_AddLayersControl.Add("relativeSizes", m_RelativeSizes);
            m_AddLayersControl.Add("layers", m_Layers);
            m_AddLayersControl.Add("expansionRatio", m_ExpansionRatio);
            m_AddLayersControl.Add("finalLayerThickness", m_FinalLayerThickness);
            m_AddLayersControl.Add("minThickness", m_MinThickness);
            m_AddLayersControl.Add("nGrow", m_NGrow);
            m_AddLayersControl.Add("featureAngle", m_FeatureAngle);
            m_AddLayersControl.Add("nRelaxIter", m_NRelaxeIterLayer);
            m_AddLayersControl.Add("nSmoothSurfaceNormals", m_nSmoothSurfaceNormals);
            m_AddLayersControl.Add("nSmoothThickness", m_NSmoothThickness);
            m_AddLayersControl.Add("nSmoothNormals", m_NSmoothNormals);
            m_AddLayersControl.Add("maxFaceThicknessRatio", m_MaxFaceThicknessRatio);
            m_AddLayersControl.Add("maxThicknessToMedialRatio", m_MaxThicknessToMeadialRatio);
            m_AddLayersControl.Add("minMedianAxisAngle", m_MinMedianAxisAngle);
            m_AddLayersControl.Add("nBufferCellsNoExtrude", m_NBufferCellsNoExtrude);
            m_AddLayersControl.Add("nLayerIter", m_NLayerIter);
            m_AddLayersControl.Add("nRelaxedIter", m_NRelaxedIterLayer);

            m_SnappyHexMeshDict.Add("addLayersControls", m_AddLayersControl);

            //SnappyHexMesh-MeshQualityControls
            m_Relaxed = new Dictionary<string, object>
            {
                {"maxNonOrtho" ,m_MaxNonOrtho}
            };

            Dictionary<string, object> m_MeshQualityControls = new Dictionary<string, object>();

            m_MeshQualityControls.Add("maxNonOrtho", m_MaxNonOrthoMeshQualtiy);
            m_MeshQualityControls.Add("maxBoundarySkewness", m_MaxBoundarySkewness);
            m_MeshQualityControls.Add("maxInternalSkewness", m_MaxInternalSkewness);
            m_MeshQualityControls.Add("maxConcave", m_MaxConcave);
            m_MeshQualityControls.Add("minFlatness", m_MinFlatness);
            m_MeshQualityControls.Add("minVol", m_MinVol);
            m_MeshQualityControls.Add("minTetQuality", m_MinTetQuality);
            m_MeshQualityControls.Add("minArea", m_MinArea);
            m_MeshQualityControls.Add("minTwist", m_MinTwist);
            m_MeshQualityControls.Add("minDeterminant", m_MinDeterminant);
            m_MeshQualityControls.Add("minFaceWeight", m_MinFaceWeight);
            m_MeshQualityControls.Add("minVolRatio", m_MinVolRatio);
            m_MeshQualityControls.Add("minTriangleTwist", m_MinTriangleTwist);
            m_MeshQualityControls.Add("nSmoothScale", m_NSmoothScale);
            m_MeshQualityControls.Add("errorReduction", m_ErrorReduction);
            m_MeshQualityControls.Add("relaxed", m_Relaxed);

            m_SnappyHexMeshDict.Add("meshQualityControls", m_MeshQualityControls);

            m_System.Add("snappyHexMeshDict", m_SnappyHexMeshDict);
        }

        /// <summary>
        /// Creates a FoamParameter Dictionary and adds it to the "0" folder.
        /// </summary>
        /// <typeparam name="T">Type of internalField.</typeparam>
        /// <param name="name">Name of the FoamParameterPatch.</param>
        /// <param name="m_InternalField">Internalfield value.</param>
        /// <param name="m_Wall">FoamParameterPatch for wall.</param>
        /// <param name="m_Inlet">FoamParameterPatch for inlet.</param>
        /// <param name="m_Outlet">FoamParameterPatch for outlet.</param>
        private void CreateFoamParameterDictionary<T>(string name, T m_InternalField, FOAMParameterPatch<T> m_Wall, FOAMParameterPatch<T> m_Inlet, FOAMParameterPatch<T> m_Outlet)
        {
            Dictionary<string, object> m_Dict = new Dictionary<string, object>();

            m_Dict.Add("internalField", m_InternalField);
            m_Dict.Add("wall", m_Wall.ToDictionary());
            m_Dict.Add("inlet", m_Inlet.ToDictionary());
            m_Dict.Add("outlet", m_Outlet.ToDictionary());

            m_Null.Add(name, m_Dict);
        }

        /// <summary>
        /// Creates a Dicitionary for g and adds it to constant.
        /// </summary>
        private void CreateGDicitionary()
        {
            Dictionary<string, object> m_G = new Dictionary<string, object>
            {
                { "g", m_GValue }
            };

            m_Constant.Add("g", m_G);
        }

        /// <summary>
        /// Creates a Dictionary for transportProperties and adds it to constant.
        /// </summary>
        private void CreateTransportPropertiesDictionary()
        {
            Dictionary<string, object> m_TransportProperties = new Dictionary<string, object>();

            m_TransportProperties.Add("transportModel", m_TransportModel);
            m_TransportProperties.Add("transportModelParameter", m_TransportModelParameter);

            m_Constant.Add("transportProperties", m_TransportProperties);
        }

        /// <summary>
        /// Creates a Dictionary for turbulenceProperties and adds it to constant.
        /// </summary>
        private void CreateTurbulencePropertiesDictionary()
        {
            m_Constant.Add("turbulenceProperties", m_TurbulenceParameter.ToDictionary());
        }

        /// <summary>
        /// Create config file if it doesn't exist.
        /// </summary>
        private void CreateConfig()
        {
            string assemblyDir = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8);
            string assemblyDirCorrect = assemblyDir.Remove(assemblyDir.IndexOf("OpenFoamExport.dll"), 18).Replace("/", "\\");
            string configPath = assemblyDirCorrect + "openfoamExporter.config";
            if (!File.Exists(configPath))
            {
                var config = new XDocument();
                var elements = new XElement("OpenFoamConfig",
                    new XElement("OpenFoamEnv"),
                    new XElement("SSH")
                );
                var defaultElement = new XElement("DefaultParameter");
                CreateXMLTree(defaultElement, m_SimulationDefaultList);
                elements.Add(defaultElement);

                config.Add(elements);

                XElement ssh = config.Root.Element("SSH");
                ssh.Add(
                        new XElement("user", "mdjur"),
                        new XElement("host", "192.168.2.102"),
                        new XElement("serverCasePath", "/home/mdjur/OpenFOAMRemote/"),
                        new XElement("ofAlias", "source/opt/openfoam6/etc/bashrc")
                );
                config.Save(configPath);
            }
        }

        /// <summary>
        /// Creates a XML-tree from given dict.
        /// </summary>
        /// <param name="e">XElement xml will be attached to.</param>
        /// <param name="dict">Source for XML-tree.</param>
        private void CreateXMLTree(XElement e, Dictionary<string,object> dict)
        { 
            foreach (var element in dict)
            {
                string nameNode = element.Key;
                nameNode = PrepareXMLString(nameNode);
                var elem = new XElement(nameNode);
                if (element.Value is Dictionary<string, object>)
                {
                    CreateXMLTree(elem, element.Value as Dictionary<string, object>);
                }
                else
                {
                    elem.Value = element.Value.ToString();
                }
                e.Add(elem);
            }
        }

        /// <summary>
        /// Removes critical strings for xml.
        /// </summary>
        /// <param name="nameNode">String which will be prepared.</param>
        /// <returns>Prepared string.</returns>
        private static string PrepareXMLString(string nameNode)
        {
            if (nameNode.Equals("0"))
            {
                nameNode = "null";
                return nameNode;
            }

            var criticalXMLCharacters = new Dictionary<string, string>()
            {
                { "(", "lpar" },
                { ")", "rpar" },
                { ",", "comma" },
                { "*", "ast" },
                { " ", "nbsp" }
            };

            foreach(var critical in criticalXMLCharacters)
            {
                nameNode = nameNode.Replace(critical.Key, critical.Value);
            }

            return nameNode;
        }
    }
}
