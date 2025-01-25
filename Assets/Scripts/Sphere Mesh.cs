using System.Collections.Generic;
using UnityEngine;

public class SphereMesh : MonoBehaviour
{
    //Value of B within Triple Integral.
    public int resolution = 50;

    //Value of Radius of the Sphere Mesh Render.
    public float radius = 1f;

    //Values used for adjusting Unity's Spring Joint Component.
    public float springStrength = 5000f;
    public float springDamping = 2000f;

    //Material used in order to visualize the SpringJoints [Edges]
    public Material lineMaterial;
    public Material skinMaterial;

    //Value used in order to determine the mass [Vertecies]
    public float vertexMass = 100f;

    //Vertex Deformation on Collision.
    public float deformationFactor = 0.05f;

    //"List" Created to keep track of vertecies. 
    private GameObject[,] vertexGrid;
    private Vector3[,] initialPositions;

    //Toggle for Gravity Testing.
    public bool Gravity = true; 
    //Toggle for Spring Joint Testing.
    public bool springToggle = true;
    //Toggle for Inner Spring Joint Testing.
    public bool internalStructer = true;

    //Used for Creation of the Skin Mesh.
    private Mesh skinMesh;
    private Vector3[] skinVertices;
    private int[] skinTriangles;


    void Start()
    {
        CreateSoftBodySphere();
    }

    void FixedUpdate()
    { 
        StabilizationForce();
        SkinMeshUpdater();
    }



    //***FUNCTION USED TO GENERATE THE SPHERE MESH***
    void CreateSoftBodySphere()
    {
        //Creates a new Game Object in the scene with a name.
        GameObject sphereParent = new GameObject("SoftBodySphere");

        vertexGrid = new GameObject[resolution + 1, resolution + 1];
        initialPositions = new Vector3[resolution + 1, resolution + 1];

        //The poles on the sphere (top and bottom) have a cluster of vertecies which can cause potential issues.
        float poleOffset = 0.05f;

        // List to store all created vertices' positions for distance check
        List<Vector3> vertexPositions = new List<Vector3>();


        //Iterate the Integral from A (0) to B (resolution). [FOR THETA (ROW)]
        for (int i = 0; i <= resolution; i++)
        {
            //theta => [0, 180] -> 180 (PI)
            float theta = Mathf.PI * i / resolution;

            //If Theta is 0 or PI.
            if (i == 0 || i == resolution)
            {
                theta += (i == 0) ? poleOffset : -poleOffset;
            }

            //Iterate the Integral from A (0) to B (resolution).  [FOR PHI (COL)]
            for (int j = 0; j <= resolution; j++)
            {
                //phi => [0, 360] -> 360 (2PI)
                float phi = 2 * Mathf.PI * j / resolution;


                //Parametric Equation for all Axis.
                float x = radius * Mathf.Sin(theta) * Mathf.Cos(phi);
                float y = radius * Mathf.Sin(theta) * Mathf.Sin(phi);
                float z = radius * Mathf.Cos(theta);

                Vector3 position = new Vector3(x, y, z);

                //If the condition is True -> Stops this iteration and CONTINUES to the next
                if (IsTooCloseToExistingVertex(position, vertexPositions))
                {
                    continue;
                }

                //Adds the current vertex position to the Vertex List.
                vertexPositions.Add(position);

                //After Coordinates are collected, insert them into a new vertex (point)
                //**GREATER DETAIL WITHIN ITS OWN FUCNTION**
                GameObject vertex = CreateVertex(position);

                //Sets the newly created vertex as a child to the Original Sphere Object.
                vertex.transform.parent = sphereParent.transform;

                //Stores the vertex in the list.
                vertexGrid[i, j] = vertex;
                initialPositions[i, j] = position;
            }
        }

        //After all the Vertices have been created from A to B (iterated) connects them with Spring Joints.
        //**GREATER DETAIL WITHIN ITS OWN FUCNTION**
        if (springToggle)
        {
            AddSpringJointsAndVisualize();
        }

        CreateSkinMesh();
    }

    //***FUNCTION FOR CREATING OUTTER LAYER (SKIN) FOR SPHERE***
    void CreateSkinMesh()
    {
        //Creates a New Game Object for the Skin (#1).
        GameObject skinObject = new GameObject("SoftBodySkin");
        skinObject.transform.parent = transform;

        //Creates a Mesh Filter and Render for the Skin (#2).
        MeshFilter meshFilter = skinObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = skinObject.AddComponent<MeshRenderer>();

        //Assign a Material to the Mesh Render (#3).
        meshRenderer.material = skinMaterial;

        //Create a new Mesh (#4).
        skinMesh = new Mesh();
        meshFilter.mesh = skinMesh;

        //Get Mapped Out Vertices Information (#5).
        int rows = vertexGrid.GetLength(0);
        int cols = vertexGrid.GetLength(1);


        //With the Vertices Map Out Triangles (#6).
        skinVertices = new Vector3[rows * cols];
        skinTriangles = new int[(rows - 1) * (cols - 1) * 6];

        //Counter for Vertices and Triangles (#7).
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                //Mapping vertexGrid to skin Vertices (#8).
                if (vertexGrid[i, j] != null)
                {
                    skinVertices[vertIndex] = vertexGrid[i, j].transform.position;
                }
                else
                {
                    skinVertices[vertIndex] = Vector3.zero;
                }


                //Including ALL, BUT last Row and Col
                if (i < rows - 1 && j < cols - 1)
                {
                    int a = vertIndex;
                    int b = vertIndex + 1;
                    int c = vertIndex + cols;
                    int d = vertIndex + cols + 1;

                    //FIRST TRIANGLE MAP.
                    skinTriangles[triIndex++] = a;
                    skinTriangles[triIndex++] = b;
                    skinTriangles[triIndex++] = c;

                    //SECOND TRIANGLE MAP.
                    skinTriangles[triIndex++] = b;
                    skinTriangles[triIndex++] = d;
                    skinTriangles[triIndex++] = c;
                }

                vertIndex++;
            }
        }

        //Assign the Values to the Skin Mesh (#9).
        skinMesh.vertices = skinVertices;
        skinMesh.triangles = skinTriangles;

        //Lighting Fix (#10).
        skinMesh.RecalculateNormals();
    }

    void SkinMeshUpdater()
    {
        //Null Check.
        if (skinMesh == null || skinVertices == null) return;

        int vertIndex = 0;

        int rows = vertexGrid.GetLength(0);
        int cols = vertexGrid.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                //If not Null -> Updates the Skin Vertices to match the Vertex Grid.
                if (vertexGrid[i, j] != null)
                {
                    skinVertices[vertIndex] = vertexGrid[i, j].transform.position;
                }

                vertIndex++;

                if (vertIndex >= skinVertices.Length)
                {
                    break;
                }
            }
        }

        if (vertIndex > skinVertices.Length)
        {
            return;
        }

        skinMesh.vertices = skinVertices;
        skinMesh.RecalculateNormals();

    }

    //***FUNCTION USED TO DETERMINE IF A NEWLY CREATED VERTEX IS TOO CLOSE TO AN ALREADY CREATED VERTEX***
    bool IsTooCloseToExistingVertex(Vector3 newPosition, List<Vector3> existingPositions)
    {
        //Minimum Distance Between Verteces.
        float minDistance = 0.1f; 

        foreach (Vector3 existingPosition in existingPositions)
        {
            //If a newly created vertex is within the Min Distance of an already existing position.
            if (Vector3.Distance(newPosition, existingPosition) < minDistance)
            {
                return true;
            }
        }

        return false;
    }



    //***FUNCTION USED TO CREATE THE VERTECIES WITHIN THE MESH***
    GameObject CreateVertex(Vector3 position)
    {
        //Creates a GameObject with a Mesh Render, then give it TYPE Sphere Collider.
        GameObject vertex = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        //Sets is position to the given (x,y,z) Coordinates.
        vertex.transform.position = position;

        //Scales the given vertex for Visualization.
        vertex.transform.localScale = Vector3.one * (radius / resolution * 0.5f);

        //Gives the vertex GameObject a rigidbody.
        Rigidbody rb = vertex.AddComponent<Rigidbody>();
        rb.mass = vertexMass;
        rb.useGravity = Gravity;

        //Gives the vertex GameObject a sphere Collider.
        SphereCollider collider = vertex.GetComponent<SphereCollider>();
        collider.isTrigger = false;

        VertexCollisionHandler collisionHandler = vertex.AddComponent<VertexCollisionHandler>();
        collisionHandler.Initialize(position, radius, deformationFactor);

        return vertex;
    }



    //***FUNCTION USED TO CONNECT VERTECIES TO THE EDGES***
    void AddSpringJointsAndVisualize()
    {
        //Sets Rows by getting [i] from vertexGrid (theta).
        int rows = vertexGrid.GetLength(0); 
        //Sets Cols by getting [j] from vertexGrid (phi).
        int cols = vertexGrid.GetLength(1);


        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                //Creates a Game Object the is the initial vertex as the start [0,0].
                GameObject vertexA = vertexGrid[i, j];

                if (vertexA == null)
                {
                    continue;
                }

                //RIGHT NEIGHBOR.
                if (j < cols - 1)
                {
                    GameObject vertexB = vertexGrid[i, j + 1];

                    if (vertexB != null)
                    {
                        CreateSpring(vertexA, vertexB);
                    }
                }

                //BOTTOM NEIGHBOR.
                if (i < rows - 1)
                {
                    GameObject vertexB = vertexGrid[i + 1, j];

                    if (vertexB != null)
                    {
                        CreateSpring(vertexA, vertexB);
                    }
                }

                //DIAGONAL NEIGHBOR.
                if (i < rows - 1 && j < cols -1)
                {
                    GameObject vertexB = vertexGrid[i + 1, j + 1];

                    if (vertexB != null)
                    {
                        CreateSpring(vertexA, vertexB);
                    }
                }

                //ANTI-DIAGONAL NEIGHBOR.
                if (i < rows - 1 && j > 0)
                {
                    GameObject vertexB = vertexGrid[i + 1, j - 1];

                    if (vertexB != null)
                    {
                        CreateSpring(vertexA, vertexB);
                    }
                }

                if (internalStructer)
                {
                    AddInternalSprings(vertexA, i, j);
                }
            }
        }
    }



    //***ADDITIONAL SPRINT JOINTS INSIDE THE SPHERE TO WORK AS STRUCTURE SUPPORT TO KEEP ITS SHAPE***
    void AddInternalSprings(GameObject vertexA, int i, int j)
    {
        int rows = vertexGrid.GetLength(0);
        int cols = vertexGrid.GetLength(1);

        //Internal Spring Range
        for (int di = 1; di < 3; di++)
        {
            for (int dj = 1; dj < 3; dj++)
            {
                int ni = (i + di) % rows;
                int nj = (j + dj) % cols;

                if (vertexGrid[ni, nj] != null)
                {
                    CreateSpring(vertexA, vertexGrid[ni, nj]);
                }
            }
        }
    }



    //***FUNCTION USED TO CREATE THE EDGES WITHIN THE MESH***
    void CreateSpring(GameObject vertexA, GameObject vertexB)
    {
        //Null Check.
        if (vertexA == null || vertexB == null)
        {
            return;
        }

        //Creates a SpringJoint Component Attached to Vertex A (#1).
        SpringJoint spring = vertexA.AddComponent<SpringJoint>();

        //Connects the SpringJoint Component to the Rigidbody of Vertex B (#2).
        spring.connectedBody = vertexB.GetComponent<Rigidbody>();

        //Using the previously stated variables, apply them to the SpringJoint Component (#3).
        spring.spring = springStrength;
        spring.damper = springDamping;

        //SpringJoint Compression Distance (#4).
        spring.minDistance = Vector3.Distance(vertexA.transform.position, vertexB.transform.position) * 0.99f;

        //SpringJoint Extension Distance (#5).
        spring.maxDistance = Vector3.Distance(vertexA.transform.position, vertexB.transform.position) * 1.01f;

        //Creates a Line Render of the SpringJoint for Visualization (#6).
        CreateSpringLine(vertexA, vertexB);
    }


    //***FUNCTION USED TO CREATE A VISUALIZATION OF THE SPRING JOINTS***
    void CreateSpringLine(GameObject vertexA, GameObject vertexB)
    {
        //New Game Object Created to Portray the Edges (#1).
        GameObject lineObject = new GameObject("SpringLine");

        //Adding a LineRenderer Component to the newly created Game Object (#2).
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        //Predefined variable for the line material (Color) (#3).
        lineRenderer.material = lineMaterial;

        //Set the dimensions of the Spring Line (#4).
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;

        //Setting up the Start and End Points on the Spring Line (#5).
        lineRenderer.SetPosition(0, vertexA.transform.position);
        lineRenderer.SetPosition(1, vertexB.transform.position);

        //Continuously Update the line render of the SpringJoints (#6).
        lineObject.AddComponent<SpringLineUpdater>().Initialize(vertexA.transform, vertexB.transform);
    }


    void StabilizationForce()
    {
        for (int i = 0; i < vertexGrid.GetLength(0); i++)
        {
            for (int j = 0; j < vertexGrid.GetLength(1); j++)
            {
                GameObject vertex = vertexGrid[i, j];
                if (vertex == null) continue;

                Rigidbody rb = vertex.GetComponent<Rigidbody>();

                Vector3 currentPosition = vertex.transform.position;
                Vector3 targetPosition = transform.TransformPoint(initialPositions[i, j]);

                Vector3 restoringForce = (targetPosition - currentPosition) * springStrength;

                rb.AddForce(restoringForce, ForceMode.Force);
            }
        }
    }



    //***UPDATES AND HANDLES COLLISIONS WITH ALL THE VERTECIES***
    public class VertexCollisionHandler : MonoBehaviour
    {
        private Vector3 defaultPosition;

        private float radius;
        private float deformationFactor;

        private Rigidbody rb;

        public void Initialize(Vector3 defaultPosition, float radius, float deformationFactor)
        {
            this.defaultPosition = defaultPosition;

            this.radius = radius;
            this.deformationFactor = deformationFactor;

            rb = GetComponent<Rigidbody>();
        }

        void OnCollisionStay(Collision collision)
        {
            //Deform the vertex when in contact with another object
            Vector3 collisionForce = collision.contacts[0].point - transform.position;
            rb.AddForce(-collisionForce.normalized * deformationFactor, ForceMode.Impulse);
        }

        void OnCollisionExit(Collision collision)
        {
            //Restore to the default position after collision ends
            Vector3 returnForce = (defaultPosition - transform.position) * deformationFactor * 2f;
            rb.AddForce(returnForce, ForceMode.VelocityChange);
        }

        void FixedUpdate()
        {
            Vector3 elasticForce = (defaultPosition - transform.position) * deformationFactor * 0.5f;
            rb.AddForce(elasticForce, ForceMode.Force);
        }
    }



    //***UPDATES THE LINE RENDER EVERY FRAME***
    public class SpringLineUpdater : MonoBehaviour
    {
        private Transform pointA;
        private Transform pointB;

        private LineRenderer lineRenderer;

        public void Initialize(Transform pointA, Transform pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;

            lineRenderer = GetComponent<LineRenderer>();
        }

        void Update()
        {
            if (lineRenderer != null && pointA != null && pointB != null)
            {
                lineRenderer.SetPosition(0, pointA.position);
                lineRenderer.SetPosition(1, pointB.position);
            }
        }
    }

}
