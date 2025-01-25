using UnityEngine;

public class SphereMesh : MonoBehaviour
{
    //Value of B within Triple Integral.
    public int resolution = 10;

    //Value of Radius of the Sphere Mesh Render.
    public float radius = 1f;

    //Values used for adjusting Unity's Spring Joint Component.
    public float springStrength = 50f;
    public float springDamping = 5f;

    //Material used in order to visualize the SpringJoints.
    public Material lineMaterial;

    public bool Gravity = false;


    void Start()
    {
        CreateSoftBodySphere();
    }


    //***FUNCTION USED TO GENERATE THE SPHERE MESH***
    void CreateSoftBodySphere()
    {
        //Creates a new Game Object in the scene with a name.
        GameObject sphereParent = new GameObject("SoftBodySphere");
        //"List" Created to keep track of vertecies. 
        GameObject[,] vertexGrid = new GameObject[resolution + 1, resolution + 1];


        //Iterate the Integral from A (0) to B (resolution). [FOR THETA (ROW)]
        for (int i = 0; i <= resolution; i++)
        {
            //theta => [0, 180] -> 180 (PI)
            float theta = Mathf.PI * i / resolution;


            //Iterate the Integral from A (0) to B (resolution).  [FOR PHI (COL)]
            for (int j = 0; j <= resolution; j++)
            {
                //phi => [0, 360] -> 360 (2PI)
                float phi = 2 * Mathf.PI * j / resolution;

                //Parametric Equation for all Axis.
                float x = radius * Mathf.Sin(theta) * Mathf.Cos(phi);
                float y = radius * Mathf.Sin(theta) * Mathf.Sin(phi);
                float z = radius * Mathf.Cos(theta);

                //After Coordinates are collected, insert them into a new vertex (point)
                //**GREATER DETAIL WITHIN ITS OWN FUCNTION**
                GameObject vertex = CreateVertex(new Vector3(x, y, z));

                //Sets the newly created vertex as a child to the Original Sphere Object.
                vertex.transform.parent = sphereParent.transform;

                //Stores the vertex in the list.
                vertexGrid[i, j] = vertex;

                //Disables the vertecies gravity and rotation.
                Rigidbody rb = vertex.GetComponent<Rigidbody>();
                rb.useGravity = Gravity;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }

        //After all the Vertices have been created from A to B (iterated) connects them with Spring Joints.
        //**GREATER DETAIL WITHIN ITS OWN FUCNTION**
        AddSpringJointsAndVisualize(vertexGrid);
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
        rb.mass = 0.1f;

        return vertex;
    }



    //***FUNCTION USED TO CONNECT VERTECIES TO THE EDGES***
    void AddSpringJointsAndVisualize(GameObject[,] vertexGrid)
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


                //RIGHT NEIGHBOR.
                if (j < cols - 1)
                {
                    CreateSpring(vertexA, vertexGrid[i, j + 1]);
                }

                //BOTTOM NEIGHBOR.
                if (i < rows - 1)
                {
                    CreateSpring(vertexA, vertexGrid[i + 1, j]);
                }

                //DIAGONAL NEIGHBOR.
                if (i < rows - 1 && j < cols -1)
                {
                    CreateSpring(vertexA, vertexGrid[i + 1, j + 1]);
                }

                //ANTI-DIAGONAL NEIGHBOR.
                if (i < rows - 1 && j > 0)
                {
                    CreateSpring(vertexA, vertexGrid[i + 1, j - 1]);
                }
            }
        }
    }


    //***FUNCTION USED TO CREATE THE EDGES WITHIN THE MESH***
    void CreateSpring(GameObject vertexA, GameObject vertexB)
    {
        //Creates a SpringJoint Component Attached to Vertex A (#1).
        SpringJoint spring = vertexA.AddComponent<SpringJoint>();

        //Connects the SpringJoint Component to the Rigidbody of Vertex B (#2).
        spring.connectedBody = vertexB.GetComponent<Rigidbody>();

        //Using the previously stated variables, apply them to the SpringJoint Component.
        spring.spring = springStrength;
        spring.damper = springDamping;

        //SpringJoint Compression Distance.
        spring.minDistance = Vector3.Distance(vertexA.transform.position, vertexB.transform.position) * 0.8f;

        //SpringJoint Extension Distance.
        spring.maxDistance = Vector3.Distance(vertexA.transform.position, vertexB.transform.position) * 1.2f;

        //Creates a Line Render of the SpringJoint.
        CreateSpringLine(vertexA, vertexB);
    }


    void CreateSpringLine(GameObject vertexA, GameObject vertexB)
    {
        //New Game Object Created to Portray the Edges.
        GameObject lineObject = new GameObject("SpringLine");

        //Adding a LineRenderer Component to the newly created Game Object.
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        //Predefined variable for the line material (Color).
        lineRenderer.material = lineMaterial;

        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;


        lineRenderer.SetPosition(0, vertexA.transform.position);
        lineRenderer.SetPosition(1, vertexB.transform.position);

        lineObject.transform.parent = vertexA.transform;
    }
       
}
