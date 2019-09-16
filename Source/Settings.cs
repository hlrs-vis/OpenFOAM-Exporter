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
using BIM.OpenFOAMExport.OpenFOAM;

namespace BIM.OpenFOAMExport
{
    /// <summary>
    /// Properties of a surface like inlet/outlet.
    /// </summary>
    public struct SurfaceProperties
    {
        /// <summary>
        /// Area of the surface.
        /// </summary>
        public double Area { get; set; }

        /// <summary>
        /// Boundary of the surface.
        /// </summary>
        public double Boundary { get; set; }

        /// <summary>
        /// Mean flow velocity through surface.
        /// </summary>
        public double MeanFlowVelocity { get; set; }

        /// <summary>
        /// Face normal of the surface.
        /// </summary>
        public XYZ FaceNormal { get; set; }
    }

    /// <summary>
    /// K-Epsilon turbulence model datatype.
    /// </summary>
    public struct KEpsilon
    {
        /// <summary>
        /// Turbulence energie.
        /// </summary>
        public double K { get; set; }

        /// <summary>
        /// Dissipation rate.
        /// </summary>
        public double Epsilon { get; set; }
    }

    /// <summary>
    /// Represents an initial parameter from the null folder.
    /// </summary>
    public struct InitialParameter
    {
        /// <summary>
        /// Name of Parameter.
        /// </summary>
        string name;

        /// <summary>
        /// Value of internalField.
        /// </summary>
        dynamic internalField;

        /// <summary>
        /// List of inlet-, outlet-, wall-patches.
        /// </summary>
        Dictionary<string, FOAMParameterPatch<dynamic>> patches;

        /// <summary>
        /// Solver for incompressible CFD.
        /// </summary>
        SolverControlDict solver;

        /// <summary>
        /// Turbulence simulationType.
        /// </summary>
        SimulationType simulationType;

        /// <summary>
        /// Turbulence-model.
        /// </summary>
        dynamic turbulenceModel;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="_name">Name for parameter.</param>
        /// <param name="_internalField">Value for internalfield.</param>
        /// <param name="_turbulenceModel">Turbulence-model.</param>
        /// <param name="_solverInc">Incompressible-Solver</param>
        /// <param name="_stype">Turbulence-type.</param>
        public InitialParameter(string _name, dynamic _internalField , dynamic _turbulenceModel,
            SolverControlDict _solverInc = SolverControlDict.simpleFoam, SimulationType _stype = SimulationType.RAS)
        {
            name = _name;
            turbulenceModel = _turbulenceModel;
            simulationType = _stype;
            patches = new Dictionary<string, FOAMParameterPatch<dynamic>>();
            internalField = _internalField;
            solver = _solverInc;
        }

        /// <summary>
        /// Getter-Setter for name of parameter.
        /// </summary>
        public string Name { get => name; set => name = value; }

        /// <summary>
        /// Getter-Setter for internalfield.
        /// </summary>
        public dynamic InternalField { get => internalField; set => internalField = value; }

        /// <summary>
        /// Getter-Setter for Patches that contains inlets, outlets and the wall.
        /// </summary>
        public Dictionary<string, FOAMParameterPatch<dynamic>> Patches { get => patches; }

        /// <summary>
        /// Getter-Setter for incompressible solver.
        /// </summary>
        public SolverControlDict SolverInc { get => solver; set => solver = value; }

        /// <summary>
        /// Getter-Setter for turbulence simulationType.
        /// </summary>
        public SimulationType SimulationType { get => simulationType; set => simulationType = value; }

        /// <summary>
        /// Getter-Setter for turbulence model.
        /// </summary>
        public dynamic TurbulenceModel { get => turbulenceModel; set => turbulenceModel = value; }
    }

    /// <summary>
    /// SSH struct contains all informations about the tunnel-connection.
    /// </summary>
    public struct SSH
    {
        /// <summary>
        /// User-name.
        /// </summary>
        string user;

        /// <summary>
        /// IP of the server (/local computer-name)
        /// </summary>
        string serverIP;

        /// <summary>
        /// Alias to start openFOAM-Environment on the server.
        /// </summary>
        string ofAlias;

        /// <summary>
        /// Folder on server openfoam case will be copied to.
        /// </summary>
        string serverCaseFolder;

        /// <summary>
        /// Port server.
        /// </summary>
        int port;

        /// <summary>
        /// Download after simulation.
        /// </summary>
        bool download;

        /// <summary>
        /// Delete after simulation.
        /// </summary>
        bool delete;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="_user">The user to login.</param>
        /// <param name="_ip">IP of the server.</param>
        /// <param name="_alias">Alias for starting openfoam.</param>
        /// <param name="_caseFolder">Casefolder on server.</param>
        /// <param name="_download">if true, case folder will be downloaded from server after simulation.</param>
        /// <param name="_delete">if true, case folder will be deleted after simulation.</param>
        /// <param name="_port">SSH Port.</param>
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

        /// <summary>
        /// Getter-Setter for user.
        /// </summary>
        public string User { get => user; set => user = value; }
        /// <summary>
        /// Getter-Setter for serverIP.
        /// </summary>
        public string ServerIP { get => serverIP; set => serverIP = value; }
        /// <summary>
        /// Getter-Setter for ofAlias.
        /// </summary>
        public string OfAlias { get => ofAlias; set => ofAlias = value; }
        /// <summary>
        /// Getter-Setter for serverCaseFolder.
        /// </summary>
        public string ServerCaseFolder { get => serverCaseFolder; set => serverCaseFolder = value; }
        /// <summary>
        /// Getter-Setter for download.
        /// </summary>
        public bool Download { get => download; set => download = value; }
        /// <summary>
        /// Getter-Setter for delete.
        /// </summary>
        public bool Delete { get => delete; set => delete = value; }
        /// <summary>
        /// Getter-Setter for port.
        /// </summary>
        public int Port { get => port; set => port = value; }

        /// <summary>
        /// Connection string.
        /// </summary>
        /// <returns>user + @ + serverIP as string.</returns>
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
        /// <summary>
        /// Type of patch.
        /// </summary>
        string type;

        /// <summary>
        /// PatchType-Enum: inlet, outlet or wall.
        /// </summary>
        PatchType patchType;

        /// <summary>
        /// Attributes of the patch.
        /// </summary>
        Dictionary<string, object> attributes;

        /// <summary>
        /// Value of the patch.
        /// </summary>
        T value;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="_type">Type of Patch</param>
        /// <param name="_uniform">uniform or nonuniform.</param>
        /// <param name="_value">Vector3D or double.</param>
        public FOAMParameterPatch(string _type, string _uniform, T _value, PatchType _patchType)
        {
            value = _value;
            type = _type;
            patchType = _patchType;
            if(_value != default && !_uniform.Equals(""))
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

        /// <summary>
        /// Getter-Method for patchType.
        /// </summary>
        public PatchType Type { get => patchType; }

        /// <summary>
        /// Getter for Attributes
        /// </summary>
        public Dictionary<string, object> Attributes { get => attributes; }
    }

    /// <summary>
    /// Coeffs-Parameter for DecomposeParDict.
    /// </summary>
    public struct CoeffsMethod
    {
        //Attributes
        /// <summary>
        /// Distribution n-Vector in DecomposeParDict.
        /// </summary>
        Vector3D n;

        /// <summary>
        /// Delta of DecomposeParDict.
        /// </summary>
        double delta;

        /// <summary>
        /// Getter for n-Vector.
        /// </summary>
        public Vector3D N { get => n; }

        /// <summary>
        /// Getter for Delta.
        /// </summary>
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

        /// <summary>
        /// Creates Dictionary and adds attributes to it.
        /// </summary>
        /// <returns>Dictionary filled with attributes.</returns>
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
        /// <summary>
        /// Parameter for the p-Dicitonionary in FvSolutionDicitonary.
        /// </summary>
        FvSolutionParameter param;

        /// <summary>
        /// Agglomerator-Enum.
        /// </summary>
        Agglomerator agglomerator;

        /// <summary>
        /// CachAgglomeration-Enum.
        /// </summary>
        CacheAgglomeration cacheAgglomeration;

        /// <summary>
        /// Interger for nCellsInCoarsesLevel.
        /// </summary>
        int nCellsInCoarsesLevel;

        /// <summary>
        /// Integer for nPostSweeps.
        /// </summary>
        int nPostSweeps;

        /// <summary>
        /// Integer for nPreSweepsre.
        /// </summary>
        int nPreSweepsre;

        /// <summary>
        /// Integer for mergeLevels.
        /// </summary>
        int mergeLevels;

        //Getter-Setter
        public FvSolutionParameter Param { get => param; set => param = value; }
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
    public struct FvSolutionParameter
    {
        //Paramter that has to be set in FvSolitonDict
        /// <summary>
        /// Smoother-type.
        /// </summary>
        Smoother smoother;

        /// <summary>
        /// Solver for FvSolutionDict.
        /// </summary>
        SolverFV solver;

        /// <summary>
        /// Double for relTol in FvSolutionDict.
        /// </summary>
        double relTol;

        /// <summary>
        /// Double for tolerance in FvSolutionDict.
        /// </summary>
        double tolerance;

        /// <summary>
        /// Double for nSweeps in FvSolutionDict.
        /// </summary>
        int nSweeps;

        /// <summary>
        /// Manipulates the matrix equation (AP^(-1))*Px=b to solve it more readily.
        /// </summary>
        Preconditioner preconditioner;

        //Getter-Setter for Parameter
        public Smoother Smoother { get => smoother; set => smoother = value; }
        public SolverFV Solver { get => solver; set => solver = value; }
        public double RelTol { get => relTol; set => relTol = value; }
        public double Tolerance { get => tolerance; set => tolerance = value; }
        public int NSweeps { get => nSweeps; set => nSweeps = value; }
        public Preconditioner Preconditioner { get => preconditioner; set => preconditioner = value; }

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
                {"solver" , Solver },
                {"preconditioner", Preconditioner }

            };
            return paramList;
        }
    }

    /// <summary>
    /// Turbulence attributes for the openfoam dictionary turbulenceProperties.
    /// </summary>
    public struct TurbulenceParameter
    {
        /// <summary>
        /// Type of simulation.
        /// </summary>
        SimulationType simulationType;

        /// <summary>
        /// Model for simulation
        /// </summary>
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
        /// Getter for simulationType.
        /// </summary>
        public SimulationType SimType { get => simulationType; }

        /// <summary>
        /// Getter for structModel.
        /// </summary>
        public ValueType StructModel { get => _structModel;  }

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

        public RASModel RASModel { get => rasModel; }

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

        public LESModel LESModel { get => lesModel; }
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
    /// Initial boundary parameter for simulation.
    /// </summary>
    public enum InitialFOAMParameter
    {
        U = 0,
        p,
        nut,
        epsilon,
        k,
        T,
        p_rgh,
        alphat
    }

    /// <summary>
    /// Preconditioner for fvSolution-Solver.
    /// </summary>
    public enum Preconditioner
    {
        DIC,
        Diagonal,
        DILU,
        FDIC = 0,
        GAMG
    }

    /// <summary>
    /// Patchtype for initialParameter.
    /// </summary>
    public enum PatchType
    {
        wall = 0,
        inlet,
        outlet
    }

    /// <summary>
    /// Enum-Objects for simulationmodel LES.
    /// </summary>
    public enum LESModel
    {
        //TO-DO: DONT DELETE THIS! EVERY MODEL NEEDS TO BE IMPLEMENTED WITH BOUNDARYCONDITION DEPENDENCY
        //NOT IMPLEMENTED FOAMPARAMETER DEPENDENCY YET!
        //DeardorffDiffStress = 0,
        //Smagorinsky,
        //SpalartAllmarasDDES,
        //SpalartAllmarasDES,
        //SpalartAllmarasIDDES,
        //WALE,
        //dynamicKEqn,
        //dynamicLagrangian,
        //kEqn,
        //kOmegaSSTDES
        //
        //implement in AddLESModelParameterToList
        //
    }

    /// <summary>
    /// Enum-Objects for simulationmodel RAS.
    /// </summary>
    public enum RASModel
    {
        //LRR = 0,
        ////NOT IMPLEMENTED FOAMPARAMETER DEPENDENCY YET!
        //LamBremhorstKE,
        //LaunderSharmaKE,
        //LienCubicKE,
        //LienLeschzine,
        RNGkEpsilon,
        //NOT IMPLEMENTED FOAMPARAMETER DEPENDENCY YET!
        //SSG,
        //ShihQuadraticKE,
        //buoyantKEpsilon,
        //SpalartAllmaras,
        kEpsilon,
        //NOT IMPLEMENTED FOAMPARAMETER DEPENDENCY YET!
        //kOmega,
        //kOmegaSST,
        //kOmegaSSTLM,
        //kOmegaSSTSAS,
        //kkLOmega,
        //qZeta,
        //realizableKE,
        //v2f
        //
        //implement in AddRASModelParameterToList 
        //
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
        //BirdCarreau,
        //CrossPowerLaw,
        //powerLaw,
        //HerschelBulkley,
        //Casson,
        //strainRateFunction
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
        ssh
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

    /// <summary>
    /// Solver for controlDict.
    /// </summary>
    public enum SolverControlDict
    {
        //Incompressible
        simpleFoam = 0,
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
        SRFSimpleFoam,

        //HeatTransfer
        buoyantBoussinesqSimpleFoam,
        buoyantBoussinesqPimpleFoam,
        buoyantPimpleFoam,
        buoyantSimpleFoam,
        chtMultiRegionFoam,
        chtMultiRegionSimpleFoam
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

        private double m_TempWall;
        private double m_TempOutlet;
        private double m_TempInlet;

        //BlockMeshDict
        private Vector3D m_SimpleGrading;
        private Vector3D m_CellSize;

        //ControlDict
        private SolverControlDict m_AppControlDictSolver;
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
        Dictionary<string, object> m_FvParameter;
        Dictionary<string, object> m_RelaxationFactors;
        private int m_nNonOrhtogonalCorrectors;
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
        private Vector3D m_LocationInMesh;
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

        private Dictionary<string, object> m_Outlets;
        private Dictionary<string, object> m_Inlets;

        //Getter-Setter Runmanager
        public OpenFOAMEnvironment OpenFOAMEnvironment { get => m_openFOAMEnvironment; set => m_openFOAMEnvironment = value; }

        //Getter-Setter ControlDict        
        public SolverControlDict AppSolverControlDict { get => m_AppControlDictSolver; set => m_AppControlDictSolver = value; }

        //Getter-Setter DecomposeParDict
        public int NumberOfSubdomains { get => m_NumberOfSubdomains; set => m_NumberOfSubdomains = value; }

        //Getter-Setter FvSolution
        public int NNonOrhtogonalCorrectors { get => m_nNonOrhtogonalCorrectors; set => m_nNonOrhtogonalCorrectors = value; }
        public Dictionary<string, object> ResidualControl { get => m_residualControl; set => m_residualControl = value; }

        //Getter-Setter SnappyHexMesh
        public int Debug { get => m_Debug; set => m_Debug = value; }
        public double MergeTolerance { get => m_MergeTolerance; set => m_MergeTolerance = value; }
        public Vector3D LocationInMesh { get => m_LocationInMesh; set => m_LocationInMesh = value; }

        //Getter-Setter-TransportProperties
        public TransportModel TransportModel { get => m_TransportModel; set => m_TransportModel = value; }

        //Getter-Setter-SSH
        public SSH SSH { get => m_SSH; set => m_SSH = value; }

        //Getter-Setter for Outlets.
        public Dictionary<string, object> Outlet { get => m_Outlets; }
        //Getter-Setter for Inlets.
        public Dictionary<string, object> Inlet { get => m_Inlets; }

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

        /// <summary>
        /// Get dicitionionary with default values.
        /// </summary>
        public Dictionary<string, object> SimulationDefault
        {
            get
            {
                return m_SimulationDefaultList;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="saveFormat">SaveFormat enum</param>
        /// <param name="exportRange">ExportRange enum.</param>
        /// <param name="openFoam">Bool for switch OpenFOAM-simulation on/off.</param>
        /// <param name="includeLinkedModels">IncludedLinkedModels.</param>
        /// <param name="exportColor">ExportColor enum.</param>
        /// <param name="exportSharedCoordinates">ExportSharedCoordinater for STL.</param>
        /// <param name="runTimeModifiable">Bool for ControlDict.</param>
        /// <param name="startTime">Start time for ControlDict.</param>
        /// <param name="endTime">End time for ControlDict.</param>
        /// <param name="deltaT">DeltaT for ControlDict.</param>
        /// <param name="writeInterval">WriterInterval for ControlDict.</param>
        /// <param name="purgeWrite">PurgeWrite for ControlDict.</param>
        /// <param name="writePrecision">WritePrecision for ControlDict.</param>
        /// <param name="timePrecision">TimePrecision for ControlDict.</param>
        /// <param name="numberOfSubdomains">Number of CPU's</param>
        /// <param name="selectedCategories">SelectedCategories.</param>
        /// <param name="units">Unit.</param>
        public Settings(SaveFormat saveFormat, ElementsExportRange exportRange, bool openFoam, 
            bool includeLinkedModels, bool exportColor, bool exportSharedCoordinates, bool runTimeModifiable,
            double startTime, double endTime, double deltaT, double writeInterval, double purgeWrite,
            double writePrecision, double timePrecision, int numberOfSubdomains, List<Category> selectedCategories,
            DisplayUnitType units)
        {
            Init(saveFormat, exportRange, openFoam,
                includeLinkedModels, exportColor, exportSharedCoordinates, runTimeModifiable,
                startTime, endTime, deltaT, writeInterval, purgeWrite,
                writePrecision, timePrecision, numberOfSubdomains, selectedCategories,
                units);
        }

        /// <summary>
        /// Initialize Settings.
        /// </summary>
        /// <param name="saveFormat">SaveFormat enum</param>
        /// <param name="exportRange">ExportRange enum.</param>
        /// <param name="openFoam">Bool for switch OpenFOAM-simulation on/off.</param>
        /// <param name="includeLinkedModels">IncludedLinkedModels.</param>
        /// <param name="exportColor">ExportColor enum.</param>
        /// <param name="exportSharedCoordinates">ExportSharedCoordinater for STL.</param>
        /// <param name="runTimeModifiable">Bool for ControlDict.</param>
        /// <param name="startTime">Start time for ControlDict.</param>
        /// <param name="endTime">End time for ControlDict.</param>
        /// <param name="deltaT">DeltaT for ControlDict.</param>
        /// <param name="writeInterval">WriterInterval for ControlDict.</param>
        /// <param name="purgeWrite">PurgeWrite for ControlDict.</param>
        /// <param name="writePrecision">WritePrecision for ControlDict.</param>
        /// <param name="timePrecision">TimePrecision for ControlDict.</param>
        /// <param name="numberOfSubdomains">Number of CPU's</param>
        /// <param name="selectedCategories">SelectedCategories.</param>
        /// <param name="units">Unit.</param>
        private void Init(SaveFormat saveFormat, ElementsExportRange exportRange, bool openFoam,
            bool includeLinkedModels, bool exportColor, bool exportSharedCoordinates, bool runTimeModifiable,
            double startTime, double endTime, double deltaT, double writeInterval, double purgeWrite,
            double writePrecision, double timePrecision, int numberOfSubdomains, List<Category> selectedCategories,
            DisplayUnitType units)
        {
            //TO-DO: CONFIG CREATE IS IMPLEMENTED BELOW AND COMMENTED OUT
            //READ FUNCTION NEEDS TO BE IMPLEMENTED
            m_Outlets = new Dictionary<string, object>();
            m_Inlets = new Dictionary<string, object>();

            //Dictionary for setting default values in OpenFOAM-Tab
            m_SimulationDefaultList = new Dictionary<string, object>();

            m_System = new Dictionary<string, object>();
            m_Constant = new Dictionary<string, object>();
            m_Null = new Dictionary<string, object>();

            m_SaveFormat = saveFormat;
            m_ExportRange = exportRange;
            m_Mesh = MeshType.Snappy;
            m_openFOAMEnvironment = OpenFOAMEnvironment.blueCFD;
            ExtractionMethod extractionMethod = ExtractionMethod.extractFromSurface;
            SimulationType simulationType = SimulationType.RAS;

            //blockMeshDict

            m_CellSize = new Vector3D(0, 0, 0);
            m_SimpleGrading = new Vector3D(1.0, 1.0, 1.0);

            //ControlDict

            InitControlDict(runTimeModifiable, startTime, endTime, deltaT, writeInterval, purgeWrite, writePrecision, timePrecision);

            //surfaceFeatureExtract

            m_ExtractionMethod = extractionMethod;
            m_IncludedAngle = 150;
            m_WriteObj = "yes";


            //DecomposeParDict

            InitDecomposeParDict(numberOfSubdomains);

            //FvSchemes

            //TO-DO: fvSchemes and fvSolution depending on FOAMParameter in null folder.
            InitFvSchemes();

            //FvSolution-Solver
            InitFvSolutionSolver();

            //FvSolution-SIMPLE
            InitFvSolutionSIMPLE();

            //FvSolution-relaxationFactors
            InitFvSolutionRelaxationFactors();

            //SnappyHexMesh-General
            InitSnappyHexMesh();

            //g
            m_GValue = -9.81;

            //TransportProperties
            InitTransportProtperties();

            //TurbulenceProperties
            RASModel rasModel = RASModel.RNGkEpsilon;
            m_TurbulenceParameter = new TurbulenceParameter(simulationType, rasModel, true, true);

            //SSH
            m_SSH = new SSH("name", "111.111.1.111", "source /opt/openfoam6/etc/bashrc", "/home/\"User\"/OpenFOAMRemote/", true, true, 22);

            //General
            m_OpenFOAM = openFoam;
            m_IncludeLinkedModels = includeLinkedModels;
            m_exportColor = exportColor;
            m_exportSharedCoordinates = exportSharedCoordinates;
            m_SelectedCategories = selectedCategories;
            m_Units = units;
        }

        /// <summary>
        /// Initialize transportProperties default attributes.
        /// </summary>
        private void InitTransportProtperties()
        {
            TransportModel transportModel = TransportModel.Newtonian;

            m_TransportModel = transportModel;
            m_TransportModelParameter = new Dictionary<string, object>();
            m_TransportModelParameter.Add("nu", 1e-05);
            m_TransportModelParameter.Add("beta", 3e-03);
            m_TransportModelParameter.Add("TRef", 300.0);
            m_TransportModelParameter.Add("Pr", 0.9);
            m_TransportModelParameter.Add("Prt", 0.7);
            m_TransportModelParameter.Add("Cp0", 1000.0);
        }

        /// <summary>
        /// Initialize SnappyHexMesh default attributes.
        /// </summary>
        private void InitSnappyHexMesh()
        {
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
            m_LocationInMesh = new Vector3D(0, 0, 0);

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

            double kelvin = 273.15;
            m_TempWall = kelvin + 25;
            m_TempOutlet = kelvin + 25;
            m_TempInlet = kelvin + 29;
        }

        /// <summary>
        /// Initialize FvSchemes default attributes.
        /// </summary>
        private void InitFvSchemes()
        {
            //To-Do: Make it generic and responsive.
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

            if (m_AppControlDictSolver == SolverControlDict.buoyantBoussinesqSimpleFoam)
            {
                m_divSchemes.Add(new KeyValuePair<string, string>("div(phi,T)", "bounded Gauss linearUpwind default;"));
            }

            m_laplacianSchemes = new KeyValuePair<string, string>("default", "Gauss linear limited corrected 0.333");
            m_interpolationSchemes = new KeyValuePair<string, string>("default", "linear");
            m_snGradSchemes = new KeyValuePair<string, string>("default", "limited corrected 0.333");
            m_fluxRequired = new KeyValuePair<string, string>("default", "no");
        }

        /// <summary>
        /// Initialize DecomposeParDict with default attributes.
        /// </summary>
        /// <param name="numberOfSubdomains">Number of CPU's</param>
        private void InitDecomposeParDict(int numberOfSubdomains)
        {
            MethodDecompose methodDecompose = MethodDecompose.simple;
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
        }

        /// <summary>
        /// Initialze ControlDict with default attributes.
        /// </summary>
        /// <param name="runTimeModifiable">Bool for ControlDict.</param>
        /// <param name="startTime">Start time for ControlDict.</param>
        /// <param name="endTime">End time for ControlDict.</param>
        /// <param name="deltaT">DeltaT for ControlDict.</param>
        /// <param name="writeInterval">WriterInterval for ControlDict.</param>
        /// <param name="purgeWrite">PurgeWrite for ControlDict.</param>
        /// <param name="writePrecision">WritePrecision for ControlDict.</param>
        /// <param name="timePrecision">TimePrecision for ControlDict.</param>
        private void InitControlDict(bool runTimeModifiable, double startTime, double endTime, double deltaT, double writeInterval, double purgeWrite, double writePrecision, double timePrecision)
        {
            StartFrom startFrom = StartFrom.latestTime;
            SolverControlDict appDictSolver = SolverControlDict.buoyantBoussinesqSimpleFoam;
            StopAt stopAt = StopAt.endTime;
            WriteControl writeControl = WriteControl.timeStep;
            WriteFormat writeFormat = WriteFormat.ascii;
            WriteCompression writeCompression = WriteCompression.off;
            TimeFormat timeFormat = TimeFormat.general;

            m_AppControlDictSolver = appDictSolver;
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
        }

        /// <summary>
        /// Initialize relaxationFactors for FvSolution with default attributes.
        /// </summary>
        private void InitFvSolutionRelaxationFactors()
        {
            if (m_AppControlDictSolver == SolverControlDict.buoyantBoussinesqSimpleFoam)
            {
                m_RelaxationFactors = new Dictionary<string, object>
                {
                    { "k", 0.7 },
                    { "U", 0.7 },
                    { "epsilon", 0.7 },
                    { "p_rgh", 0.3 },
                    { "T", 0.5 }
                };
            }
            else
            {
                m_RelaxationFactors = new Dictionary<string, object>
                {
                    { "k", 0.7 },
                    { "U", 0.7 },
                    { "epsilon", 0.7 },
                    { "p", 0.3 }
                };
            }
        }

        /// <summary>
        /// Initialize SIMPLE attributes for FvSolution with default attributes.
        /// </summary>
        private void InitFvSolutionSIMPLE()
        {
            m_nNonOrhtogonalCorrectors = 2;
            m_residualControl = new Dictionary<string, object>();

            if (m_AppControlDictSolver == SolverControlDict.buoyantBoussinesqSimpleFoam)
            {
                m_residualControl.Add("nut", 0.0001);
                m_residualControl.Add("p_rgh", 0.0001);
                m_residualControl.Add("k", 0.0001);
                m_residualControl.Add("U", 0.0001);
                m_residualControl.Add("T", 0.0001);
                m_residualControl.Add("epsilon", 0.0001);
                m_residualControl.Add("alphat", 0.0001);
            }
        }

        /// <summary>
        /// Initialize Solver attributes from FvSolution with default attributes.
        /// </summary>
        private void InitFvSolutionSolver()
        {
            m_FvParameter = new Dictionary<string, object>();

            Agglomerator agglomerator = Agglomerator.faceAreaPair;
            CacheAgglomeration cacheAgglomeration = CacheAgglomeration.on;
            SolverFV solverP = SolverFV.GAMG;
            SolverFV solverU = SolverFV.smoothSolver;
            SolverFV solverK = SolverFV.smoothSolver;
            SolverFV solverEpsilon = SolverFV.smoothSolver;
            Smoother smootherU = Smoother.GaussSeidel;
            Smoother smootherK = Smoother.GaussSeidel;
            Smoother smootherEpsilon = Smoother.GaussSeidel;

            if (m_AppControlDictSolver == SolverControlDict.buoyantBoussinesqSimpleFoam)
            {
                //p_rgh-FvSolution-Solver
                FvSolutionParameter p_rgh = new FvSolutionParameter
                {
                    RelTol = 0.01,
                    Solver = SolverFV.PCG,
                    Tolerance = 1e-8
                };

                m_FvParameter.Add("p_rgh", /*_p*/p_rgh);

                //T-FvSolution-Solver
                FvSolutionParameter T = new FvSolutionParameter
                {
                    RelTol = 0.1,
                    Tolerance = 1e-8,
                    NSweeps = 1,
                    Smoother = Smoother.GaussSeidel,
                    Solver = SolverFV.smoothSolver
                };

                m_FvParameter.Add("T", T);
            }
            else
            {
                //p-FvSolution-Solver
                FvSolutionParameter p = new FvSolutionParameter
                {
                    Solver = solverP,
                    RelTol = 0.1,
                    Tolerance = 1e-7,
                    NSweeps = 0
                };

                PFv m_p = new PFv
                {
                    Param = p,
                    MergeLevels = 1,
                    NPreSweepsre = 0,
                    NPostSweeps = 2,
                    NCellsInCoarsesLevel = 10,
                    Agglomerator = agglomerator,
                    CacheAgglomeration = cacheAgglomeration
                };

                m_FvParameter.Add("p", /*_p*/m_p);
            }

            //U-FvSolution-Solver
            FvSolutionParameter m_U = new FvSolutionParameter
            {
                RelTol = 0.1,
                Tolerance = 1e-8,
                NSweeps = 1,
                Solver = solverU,
                Smoother = smootherU
            };

            m_FvParameter.Add("U", m_U);

            //k-FvSolution-Solver
            FvSolutionParameter m_k = new FvSolutionParameter
            {
                RelTol = 0.1,
                Tolerance = 1e-8,
                NSweeps = 1,
                Solver = solverK,
                Smoother = smootherK
            };

            m_FvParameter.Add("k", m_k);

            //epsilon-FvSolution-Solver
            FvSolutionParameter m_epsilon = new FvSolutionParameter
            {
                RelTol = 0.1,
                Tolerance = 1e-8,
                NSweeps = 1,
                Solver = solverEpsilon,
                Smoother = smootherEpsilon
            };

            m_FvParameter.Add("epsilon", m_epsilon);
        }

        /// <summary>
        /// Create dictionaries and initialize with all default values.
        /// </summary>
        public void InitOpenFOAMFolderDictionaries()
        {
            InitSystemDictionary();
            InitConstantDictionary();
            InitNullDictionary();            

            //CREATES XML BASED ON DEFAULT SETTINGS => NO READ FUNCTION FOR SETTINGS AND OPENFOAMEXPORTFORM IMPLEMENTED YET
            //CreateConfig();
        }

        /// <summary>
        /// Update settings.
        /// </summary>
        public void Update()
        {
            m_SimulationDefaultList = new Dictionary<string, object>();

            m_System = new Dictionary<string, object>();
            m_Constant = new Dictionary<string, object>();
            m_Null = new Dictionary<string, object>();

            InitFvSchemes();
            InitFvSolutionRelaxationFactors();
            InitFvSolutionSIMPLE();
            InitFvSolutionSolver();

            InitOpenFOAMFolderDictionaries();
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
            CreateFoamParametersDictionaries();
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
            Dictionary<string, object> m_Solvers = new Dictionary<string, object>();

            foreach(var solverParam in m_FvParameter)
            {
                if (solverParam.Value is PFv p)
                {
                    m_Solvers.Add(solverParam.Key, p.ToDictionary());
                }
                else if (solverParam.Value is FvSolutionParameter fv)
                {
                    m_Solvers.Add(solverParam.Key, fv.ToDictionary());
                }
            }

            Dictionary<string, object> m_SIMPLE = new Dictionary<string, object>
            {
                {"nNonOrthogonalCorrectors" , NNonOrhtogonalCorrectors },
                {"residualControl", ResidualControl }
            };

            if(m_AppControlDictSolver == SolverControlDict.buoyantBoussinesqSimpleFoam)
            {
                m_SIMPLE.Add("pRefValue", 0);
                //m_SIMPLE.Add("pRefPoint", "(" + m_LocationInMesh.ToString().Replace(';', ' ') + ")");
            }

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
        /// Creates FoamParameters Dictionary and adds it to the "0" folder.
        /// </summary>
        private void CreateFoamParametersDictionaries()
        {
            Dictionary<string, object> m_Dict = new Dictionary<string, object>();
            List<InitialParameter> initialParameters = new List<InitialParameter>();
            CreateFOAMParamterList(initialParameters);
            foreach (InitialParameter initParam in initialParameters)
            {
                m_Dict = new Dictionary<string, object>();
                m_Dict.Add("internalField", (object)initParam.InternalField);
                //string patchName = string.Empty;

                foreach(var patch in initParam.Patches)
                {
                    m_Dict.Add(patch.Key, patch.Value);
                }
                m_Null.Add(initParam.Name, m_Dict);
            }
        }

        /// <summary>
        /// Adds InitialParameter to the given list.
        /// </summary>
        /// <param name="initialParameters">List of initialParameters</param>
        private void CreateFOAMParamterList(List<InitialParameter> initialParameters)
        {
            AddParametersBasedOnSimulationType(initialParameters, AddParametersBasedOnSolverControlDict);
            AddParametersBasedOnSimulationType(initialParameters, ParametersBasedOnTurbulenceModel);
        }

        /// <summary>
        /// Add initialparameter to initialParameterList based on solver in controlDict.
        /// </summary>
        /// <param name="initialParameters">List of initialParameters</param>
        /// <param name="model">TurbulenceModel enum.</param>
        private void AddParametersBasedOnSolverControlDict(List<InitialParameter> initialParameters, Enum model)
        {
            //model for Solver based initialParameter actually not necessary => fix implementation in polishing phase

            if(model is RASModel || model is LESModel /*|| model is Laminar*/)
            {
                switch (m_AppControlDictSolver)
                {
                    case SolverControlDict.simpleFoam:
                        {
                            //U
                            InitialParameter U = CreateInitialParameter(model, InitialFOAMParameter.U);

                            //p
                            InitialParameter p = CreateInitialParameter(model, InitialFOAMParameter.p);

                            initialParameters.Add(U);
                            initialParameters.Add(p);
                            break;
                        }
                    case SolverControlDict.buoyantBoussinesqSimpleFoam:
                        {
                            //U
                            InitialParameter U = CreateInitialParameter(model, InitialFOAMParameter.U);

                            //alphat
                            InitialParameter alphat = CreateInitialParameter(model, InitialFOAMParameter.alphat);

                            //T
                            InitialParameter T = CreateInitialParameter(model, InitialFOAMParameter.T);

                            //p_rgh
                            InitialParameter p_rgh = CreateInitialParameter(model, InitialFOAMParameter.p_rgh);

                            initialParameters.Add(U);
                            initialParameters.Add(p_rgh);
                            initialParameters.Add(alphat);
                            initialParameters.Add(T);

                            break;
                        }
                        //not implemented solver yet
                        //case SolverControlDict.adjointShapeOptimizationFoam:
                        //case SolverControlDict.boundaryFoam:
                        //case SolverControlDict.icoFoam:
                        //case SolverControlDict.nonNewtonianIcoFoam:
                        //case SolverControlDict.pimpleDyMFoam:
                        //case SolverControlDict.pimpleFoam:
                        //case SolverControlDict.pisoFoam:
                        //case SolverControlDict.porousSimpleFoam:
                        //case SolverControlDict.shallowWaterFoam:
                        //case SolverControlDict.SRFPimpleFoam:
                        //case SolverControlDict.SRFSimpleFoam:
                        //case SolverControlDict.buoyantBoussinesqPimpleFoam:
                        //case SolverControlDict.buoyantPimpleFoam:
                        //case SolverControlDict.buoyantSimpleFoam:
                        //case SolverControlDict.chtMultiRegionFoam:
                        //case SolverControlDict.chtMultiRegionSimpleFoam:
                        //    break;
                }
            }
        }

        /// <summary>
        /// Add InitialParameter based on the simulationType by calling Action-Delegate.
        /// </summary>
        /// <param name="initialParameters">List of initialParameter.</param>
        /// <param name="callFunc">Function that will be called.</param>
        private void AddParametersBasedOnSimulationType(List<InitialParameter> initialParameters,
            Action<List<InitialParameter>, Enum> callFunc)
        {
            switch(m_TurbulenceParameter.SimType)
            {
                case SimulationType.laminar:
                {
                    //not implemented yet!
                    break;
                }
                case SimulationType.RAS:
                {
                    RAS ras = (RAS)m_TurbulenceParameter.StructModel;
                    RASModel rasM = ras.RASModel;
                    callFunc(initialParameters, rasM);
                    break;
                }
                case SimulationType.LES:
                {
                    LES les = (LES)m_TurbulenceParameter.StructModel;
                    LESModel lesM = les.LESModel;
                    callFunc(initialParameters, lesM);
                    break;
                }
            }
        }

        /// <summary>
        /// Add InitialParameter based on turbulenceModel that is set in TurbulenceModel.
        /// </summary>
        /// <param name="initialParameters">List of initialParameter.</param>
        /// <param name="model">turbulenceModel.</param>
        private void ParametersBasedOnTurbulenceModel(List<InitialParameter> initialParameters, Enum model)
        {
            if (model is RASModel)
            {
                AddRASModelParameterToList(initialParameters, (RASModel)model);
            }
            else if (model is LESModel)
            {
                AddLESModelParameterToList(initialParameters, (LESModel)model);
            }
        }



        /// <summary>
        /// Add InitialParameter depending on given RASModel.
        /// </summary>
        /// <param name="initialParameters">List of InitialParameter.</param>
        /// <param name="model">Enum RASModel.</param>
        private void AddRASModelParameterToList(List<InitialParameter> initialParameters, RASModel model)
        {
            switch(model)
            {
                case RASModel.kEpsilon:
                case RASModel.RNGkEpsilon:
                    {
                        // k
                        InitialParameter k = CreateInitialParameter(model, InitialFOAMParameter.k);

                        //epsilon
                        InitialParameter epsilon = CreateInitialParameter(model, InitialFOAMParameter.epsilon);

                        //nut
                        InitialParameter nut = CreateInitialParameter(model, InitialFOAMParameter.nut);

                        initialParameters.Add(k);
                        initialParameters.Add(epsilon);
                        initialParameters.Add(nut);
                        break;
                    }
                    //not implemented rasmodels
                //case RASModel.buoyantKEpsilon:
                //case RASModel.kkLOmega:
                //case RASModel.kOmega:
                //case RASModel.kOmegaSST:
                //case RASModel.kOmegaSSTLM:
                //case RASModel.kOmegaSSTSAS:
                //case RASModel.LamBremhorstKE:
                //case RASModel.LaunderSharmaKE:
                //case RASModel.LienCubicKE:
                //case RASModel.LienLeschzine:
                //case RASModel.LRR:
                //case RASModel.qZeta:
                //case RASModel.realizableKE:
                //case RASModel.ShihQuadraticKE:
                //case RASModel.SpalartAllmaras:
                //case RASModel.SSG:
                //case RASModel.v2f:
                //    break;
            }
        }

        /// <summary>
        /// Add InitialParameter depending on given LESModel.
        /// </summary>
        /// <param name="initialParameters">List of InitialParameter.</param>
        /// <param name="model">Enum LESModel.</param>
        private void AddLESModelParameterToList(List<InitialParameter> initialParameters, LESModel model)
        {
            switch(model)
            {
                //not implemented les models
                //case LESModel.DeardorffDiffStress:
                //case LESModel.dynamicKEqn:
                //case LESModel.dynamicLagrangian:
                //case LESModel.kEqn:
                //case LESModel.kOmegaSSTDES:
                //case LESModel.Smagorinsky:
                //case LESModel.SpalartAllmarasDDES:
                //case LESModel.SpalartAllmarasDES:
                //case LESModel.SpalartAllmarasIDDES:
                //case LESModel.WALE:
                //    break;
            }
        }

        /// <summary>
        /// Initialize initialParameter with default values depending on InitialFOAMParameter-Enum.
        /// </summary>
        /// <param name="model">TurbulenceModel enum.</param>
        /// <param name="param">InitialFOAMParameter enum</param>
        /// <returns>InitialParameter for null folder.</returns>
        private InitialParameter CreateInitialParameter(Enum model, InitialFOAMParameter param)
        {
            InitialParameter parameter;
            switch(param)
            {
                case InitialFOAMParameter.p:
                    {
                        parameter = new InitialParameter(param.ToString(), 0.0, model);
                        CreateFOAMParameterPatches<int>(parameter, "zeroGradient", "", default, PatchType.wall, false);
                        CreateFOAMParameterPatches<int>(parameter, "zeroGradient","", default, PatchType.inlet, false);
                        CreateFOAMParameterPatches(parameter, "fixedValue", "uniform", 0.0, PatchType.outlet, false);
                        break;
                    }
                case InitialFOAMParameter.U:
                    {
                        parameter = new InitialParameter(param.ToString(), new Vector3D(0.0, 0.0, 0.0), model);
                        CreateFOAMParameterPatches(parameter, "fixedValue", "uniform", new Vector3D(0.0, 0.0, 0.0), PatchType.wall, true);
                        CreateFOAMParameterPatches(parameter, "fixedValue", "uniform", new Vector3D(0.0, 0.0, -0.25), PatchType.inlet, true);
                        CreateFOAMParameterPatches(parameter, "inletOutlet", "uniform", new Vector3D(0.0, 0.0, 0.0), PatchType.outlet, true);
                        foreach (var outlet in parameter.Patches)
                        {
                            if (outlet.Value.Type == PatchType.outlet)
                            {
                                outlet.Value.Attributes.Add("inletValue uniform", new Vector3D(0.0, 0.0, 0.0));
                            }
                        }
                        break;
                    }
                case InitialFOAMParameter.nut:
                    {
                        parameter = new InitialParameter(param.ToString(), 0.0, model);
                        CreateFOAMParameterPatches(parameter, "fixedValue", "uniform", 0.01, PatchType.wall, false);
                        CreateFOAMParameterPatches(parameter, "calculated", "uniform", 0.0, PatchType.inlet, false);
                        CreateFOAMParameterPatches(parameter, "calculated", "uniform", 0.0, PatchType.outlet, false);
                        break;
                    }
                case InitialFOAMParameter.k:
                    {
                        parameter = new InitialParameter(param.ToString(), 0.1, model);
                        CreateFOAMParameterPatches(parameter, "kqRWallFunction", "uniform", 0.1, PatchType.wall, false/*true*/);
                        CreateFOAMParameterPatches(parameter, "fixedValue", "uniform", 0.1, PatchType.inlet, false/*true*/);
                        CreateFOAMParameterPatches(parameter, "inletOutlet", "uniform", 0.1, PatchType.outlet, false/*true*/);
                        foreach (var outlet in parameter.Patches)
                        {
                            if (outlet.Value.Type == PatchType.outlet)
                            {
                                outlet.Value.Attributes.Add("inletValue uniform", 0.1);
                            }
                        }
                        break;
                    }
                case InitialFOAMParameter.epsilon:
                    {
                        parameter = new InitialParameter(param.ToString(), 0.01, model);
                        CreateFOAMParameterPatches(parameter, "epsilonWallFunction", "uniform", 0.01, PatchType.wall, /*true*/false);
                        CreateFOAMParameterPatches(parameter, "fixedValue", "uniform", 0.01, PatchType.inlet, false/* true*/);
                        CreateFOAMParameterPatches(parameter, "inletOutlet", "uniform", 0.1, PatchType.outlet, false /*true*/);
                        foreach(var outlet in parameter.Patches)
                        {
                            if(outlet.Value.Type == PatchType.outlet)
                            {
                                outlet.Value.Attributes.Add("inletValue uniform", 0.1);
                            }
                        }
                        break;
                    }

                case InitialFOAMParameter.alphat:
                    {
                        parameter = new InitialParameter(param.ToString(), 0.0, model);
                        CreateFOAMParameterPatches(parameter, "alphatJayatillekeWallFunction", "uniform", 0.0, PatchType.wall, false);
                        CreateFOAMParameterPatches<int>(parameter, "zeroGradient", "", default, PatchType.inlet, false);
                        CreateFOAMParameterPatches<int>(parameter, "zeroGradient", "", default, PatchType.outlet, false);
                        foreach(var wall in parameter.Patches)
                        {
                            if(wall.Value.Type == PatchType.wall)
                            {
                                wall.Value.Attributes.Add("Prt", "0.85");
                            }
                        }
                        break;
                    }

                case InitialFOAMParameter.T:
                    {
                        //double kelvin = 273.15;
                        ////double tempWall = kelvin + 25;
                        ////double tempOutlet = kelvin + 25;
                        ////double tempInlet = kelvin + 29;
                        //m_TempWall = kelvin + 25;
                        //m_TempOutlet = kelvin + 25;
                        //m_TempInlet = kelvin + 29;

                        parameter = new InitialParameter(param.ToString(), m_TransportModelParameter["TRef"], model);
                        CreateFOAMParameterPatches(parameter, "fixedValue", "uniform", m_TempWall, PatchType.wall, false);
                        CreateFOAMParameterPatches(parameter, "fixedValue", "uniform", m_TempInlet, PatchType.inlet, false);
                        CreateFOAMParameterPatches(parameter, "fixedValue", "uniform", m_TempOutlet, PatchType.outlet, false);
                        break;
                    }

                case InitialFOAMParameter.p_rgh:
                    {
                        parameter = new InitialParameter(param.ToString(), 0.0, model);
                        CreateFOAMParameterPatches(parameter, "fixedFluxPressure", "uniform", 0.0, PatchType.wall, false);
                        CreateFOAMParameterPatches<int>(parameter, "zeroGradient", "", default, PatchType.inlet, false);
                        CreateFOAMParameterPatches(parameter, "fixedValue", "uniform", 0.0, PatchType.outlet, false);
                        foreach(var wall in parameter.Patches)
                        {
                            if(wall.Value.Type == PatchType.wall)
                            {
                                wall.Value.Attributes.Add("rho", "rhok");
                            }
                        }
                        break;
                    }
                default:
                    {
                        parameter = new InitialParameter(param.ToString(), 0.0, model);
                        CreateFOAMParameterPatches(parameter,"", "", 0.0, PatchType.wall, false);
                        CreateFOAMParameterPatches(parameter, "", "", 0.0, PatchType.inlet, false);
                        CreateFOAMParameterPatches(parameter,"", "", 0.0, PatchType.outlet, false);
                        break;
                    }
            }

            return parameter;
        }

        /// <summary>
        /// Create FOAMParamaterPatches and add them to given InitialParameter.
        /// </summary>
        /// <typeparam name="T">Type of value stored in patch.</typeparam>
        /// <param name="param">InitialParameter object.</param>
        /// <param name="type">Type of patch.</param>
        /// <param name="uniform">Uniform / Nonuniform.</param>
        /// <param name="value">Value that will be stored in patch.</param>
        /// <param name="pType">PatchType: Inlet, Outlet, Wall</param>
        /// <param name="useBIM">Use BIM Data-Dictionaries Outlet/Inlet</param>
        private void CreateFOAMParameterPatches<T>(InitialParameter param, string type, string uniform, T value, PatchType pType, bool useBIM)
        {
            switch(pType)
            {
                case PatchType.inlet:
                    {
                        FOAMParameterPatch<dynamic> _inlet;
                        if(Inlet.Count == 0 || !useBIM)
                        {
                            _inlet = new FOAMParameterPatch<dynamic>(type, uniform, value, pType);
                            param.Patches.Add(pType.ToString(), _inlet);
                        }
                        else
                        {
                            foreach(var inlet in Inlet)
                            {
                                var properties = (SurfaceProperties)inlet.Value;
                                object v = default;
                                if(param.Name.Equals(InitialFOAMParameter.k.ToString()))
                                {
                                    KEpsilon k = CalculateKEpsilon(properties.Area, properties.Boundary, properties.MeanFlowVelocity);
                                    v = k.K;
                                }
                                else if (param.Name.Equals(InitialFOAMParameter.epsilon.ToString()))
                                {
                                    KEpsilon kEpsilon = CalculateKEpsilon(properties.Area, properties.Boundary, properties.MeanFlowVelocity);
                                    v = kEpsilon.Epsilon;
                                }
                                else if(param.Name.Equals(InitialFOAMParameter.U.ToString()))
                                {
                                    v = new Vector3D(properties.FaceNormal.X, properties.FaceNormal.Y, properties.FaceNormal.Z)*properties.MeanFlowVelocity;
                                }
                                //v = properties.MeanFlowVelocity;
                                _inlet = new FOAMParameterPatch<dynamic>(type, uniform, v/*inlet.Value*/, pType);
                                param.Patches.Add(inlet.Key, _inlet);
                            }
                        }
                        break;
                    }

                case PatchType.outlet:
                    {
                        FOAMParameterPatch<dynamic> _outlet;
                        if (Outlet.Count == 0 || !useBIM)
                        {
                            _outlet = new FOAMParameterPatch<dynamic>(type, uniform, value, pType);
                            param.Patches.Add(pType.ToString(), _outlet);
                        }
                        else
                        {
                            foreach(var outlet in Outlet)
                            {
                                var properties = (SurfaceProperties)outlet.Value;
                                object v = default;
                                if (param.Name.Equals(InitialFOAMParameter.k.ToString()))
                                {
                                    KEpsilon k = CalculateKEpsilon(properties.Area, properties.Boundary, properties.MeanFlowVelocity);
                                    v = k.K;
                                }
                                else if (param.Name.Equals(InitialFOAMParameter.epsilon.ToString()))
                                {
                                    KEpsilon epsilon = CalculateKEpsilon(properties.Area, properties.Boundary, properties.MeanFlowVelocity);
                                    v = epsilon.Epsilon;
                                }
                                else if (param.Name.Equals(InitialFOAMParameter.U.ToString()))
                                {
                                    v = new Vector3D(properties.FaceNormal.X, properties.FaceNormal.Y, properties.FaceNormal.Z) * properties.MeanFlowVelocity;
                                }
                                _outlet = new FOAMParameterPatch<dynamic>(type, uniform, v/*outlet.Value*/, pType);
                                param.Patches.Add(outlet.Key, _outlet);
                            }
                        }
                        break;
                    }

                case PatchType.wall:
                    {
                        FOAMParameterPatch<dynamic> wall = new FOAMParameterPatch<dynamic>(type, uniform, value, PatchType.wall);
                        param.Patches.Add(pType.ToString(), wall);
                        break;
                    }
            }
        }

        /// <summary>
        /// Calculate k and epsilon with OpenFOAMCalculator-class.
        /// </summary>
        /// <param name="area">Area of inlet surface.</param>
        /// <param name="boundary">Boundary of inlet surface.</param>
        /// <param name="meanFlowVelocity">Mean flow velocity through inlet.</param>
        private KEpsilon CalculateKEpsilon(double area, double boundary, double meanFlowVelocity)
        {
            OpenFOAMCalculator calculator = new OpenFOAMCalculator();

            double kinematicViscosity = calculator.InterpolateKinematicViscosity(m_TempInlet - 273.15);

            double characteristicLength = calculator.CalculateHydraulicDiameter(area, boundary);
            double reynoldsNumber = calculator.CalculateReynoldsnumber(Math.Abs(meanFlowVelocity), kinematicViscosity, characteristicLength);

            double turbulenceLengthScale = calculator.EstimateTurbulencLengthScalePipe(characteristicLength);
            double turbulenceIntensity = calculator.EstimateTurbulenceIntensityPipe(reynoldsNumber);

            double k = calculator.CalculateK(Math.Abs(meanFlowVelocity), turbulenceIntensity);
            double epsilon = calculator.CalculateEpsilon(turbulenceLengthScale, k);

            KEpsilon kepsilon = new KEpsilon()
            {
                Epsilon = epsilon,
                K = k
            };

            return kepsilon;
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
    }
}
