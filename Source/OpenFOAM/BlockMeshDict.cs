using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Collections;
using System;

namespace BIM.OpenFoamExport.OpenFOAM
{
    /// <summary>
    /// The BlockMeshDict-Class contains all attributes for blockMesh in Openfoam.
    /// </summary>
    public class BlockMeshDict : FoamDict
    {
        /// <summary>
        /// Cellsize for the boundingbox
        /// </summary>
        private Vector3D m_CellSize;

        /// <summary>
        /// Simplegrading vector
        /// </summary>
        private Vector3D m_SimpleGrading;

        /// <summary>
        /// Point in 3d-Space for boundingbox
        /// </summary>
        private Vector3D m_VecLowerEdgeLeft;

        /// <summary>
        /// Point in 3d-Space for boundingbox
        /// </summary>
        private Vector3D m_VecUpperEdgeRight;

        /// <summary>
        /// Vertices for Boundingbox
        /// </summary>
        private ArrayList m_Vertices;

        /// <summary>
        /// Edges-Dict
        /// </summary>
        private ArrayList m_Edges;

        /// <summary>
        /// MergePatchPair-Dict
        /// </summary>        
        private ArrayList m_MergePatchPair;

        /// <summary>
        /// Blocks-Dict
        /// </summary>
        private ArrayList m_Blocks;

        /// <summary>
        /// Boundary
        /// </summary>        
        private ArrayList m_Boundary;

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="version">Version-object.</param>
        /// <param name="path">Path to this File.</param>
        /// <param name="attributes">Additional attributes.</param>
        /// <param name="format">Ascii or Binary.</param>
        /// <param name="settings"></param>
        /// <param name="vecLowerEdgeLeft">3d-point.</param>
        /// <param name="vecUpperEdgeRight">3d-point.</param>
        public BlockMeshDict(Version version, string path, Dictionary<string, object> attributes, SaveFormat format, Settings settings, Vector3D vecLowerEdgeLeft, Vector3D vecUpperEdgeRight)
            : base("blockMeshDict", "dictionary", version, path, attributes, format, settings)
        {
            m_VecLowerEdgeLeft = vecLowerEdgeLeft;
            m_VecUpperEdgeRight = vecUpperEdgeRight;

            m_Vertices = new ArrayList();
            m_Edges = new ArrayList();
            m_MergePatchPair = new ArrayList();
            m_Blocks = new ArrayList();
            m_Boundary = new ArrayList();

            InitAttributes();
        }

        /// <summary>
        /// Initialize attributes.
        /// </summary>
        public override void InitAttributes()
        {
            m_SimpleGrading = (Vector3D)m_DictFile["simpleGrading"];
            EnlargeBoundingboxVector(1);
            m_CellSize = (Vector3D)m_DictFile["cellSize"];
            if (m_CellSize.Length == 0)
            {
                InitDefaultCellSize();
            }

            InitBoundingboxFromPoints();
            InitBlocks();
            InitEdges();
            InitBoundary();
            InitMergePatchPair();

            FoamFile.Attributes.Add("vertices", m_Vertices);
            FoamFile.Attributes.Add("blocks", m_Blocks);
            FoamFile.Attributes.Add("edges", m_Edges);
            FoamFile.Attributes.Add("boundary", m_Boundary);
            FoamFile.Attributes.Add("mergePatchPair", m_MergePatchPair);
        }

        /// <summary>
        /// Initialize vertices with two points in 3D-Space.
        /// </summary>
        private void InitBoundingboxFromPoints()
        {
            m_Vertices.Add(m_VecLowerEdgeLeft);
            m_Vertices.Add(new Vector3D(m_VecUpperEdgeRight.X, m_VecLowerEdgeLeft.Y, m_VecLowerEdgeLeft.Z));
            m_Vertices.Add(new Vector3D(m_VecUpperEdgeRight.X, m_VecUpperEdgeRight.Y, m_VecLowerEdgeLeft.Z));
            m_Vertices.Add(new Vector3D(m_VecLowerEdgeLeft.X, m_VecUpperEdgeRight.Y, m_VecLowerEdgeLeft.Z));
            m_Vertices.Add(new Vector3D(m_VecLowerEdgeLeft.X, m_VecLowerEdgeLeft.Y, m_VecUpperEdgeRight.Z));
            m_Vertices.Add(new Vector3D(m_VecUpperEdgeRight.X, m_VecLowerEdgeLeft.Y, m_VecUpperEdgeRight.Z));
            m_Vertices.Add(m_VecUpperEdgeRight);
            m_Vertices.Add(new Vector3D(m_VecLowerEdgeLeft.X, m_VecUpperEdgeRight.Y, m_VecUpperEdgeRight.Z));

        }

        /// <summary>
        /// Initialize CellSize for BlockMesh.
        /// </summary>
        private void InitDefaultCellSize()
        {
            m_CellSize.X = Math.Round(m_VecUpperEdgeRight.X - m_VecLowerEdgeLeft.X);
            m_CellSize.Y = Math.Round(m_VecUpperEdgeRight.Y - m_VecLowerEdgeLeft.Y);
            m_CellSize.Z = Math.Round(m_VecUpperEdgeRight.Z - m_VecLowerEdgeLeft.Z);
        }

        /// <summary>
        /// Initialize block dictionary.
        /// </summary>
        private void InitBlocks()
        {
            m_Blocks.Add("hex (0 1 2 3 4 5 6 7) (" + m_CellSize.ToString().Replace(';', ' ') + ")");
            m_Blocks.Add("simpleGrading (" + m_SimpleGrading.ToString().Replace(';', ' ') + ")");
        }

        /// <summary>
        /// Initialize edges dictionary.
        /// </summary>
        private void InitEdges()
        {
            //TO-DO: implement later
        }

        /// <summary>
        /// Initialize boundaries for the blockMesh.
        /// </summary>
        private void InitBoundary()
        {
            Dictionary<string, object> boundingBox = new Dictionary<string, object>()
            {
                {"type", "wall"} ,
                {"faces", new ArrayList {
                          {new int[]{ 0, 3, 2, 1 } },
                          {new int[]{ 4, 5, 6, 7 } },
                          {new int[]{ 1, 2, 6, 5 } },
                          {new int[]{ 3, 0, 4, 7 } },
                          {new int[]{ 0, 1, 5, 4 } },
                          {new int[]{ 2, 3, 7, 6 } } }
                }
            };
            m_Boundary.Add(new KeyValuePair<string,object>("boundingBox",boundingBox));
        }

        /// <summary>
        /// Initialize MergePathPair-Dictionary.
        /// </summary>
        private void InitMergePatchPair()
        {
            //TO-DO: implement later.
        }

        /// <summary>
        /// Enlarge vectors which used for creating the Boundingbox in BlockMeshDict.
        /// </summary>
        /// <param name="add">Additional size.</param>
        private void EnlargeBoundingboxVector(float add)
        {
            m_VecLowerEdgeLeft.X -= add;
            m_VecLowerEdgeLeft.Y -= add;
            m_VecLowerEdgeLeft.Z -= add;
            m_VecUpperEdgeRight.X += add;
            m_VecUpperEdgeRight.Y += add;
            m_VecUpperEdgeRight.Z += add;
        }
    }
}
