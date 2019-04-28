using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(MeshFilter))]
public class IciclesMesh : MonoBehaviour
{
    // Reference: https://profs.etsmtl.ca/epaquette/Research/Papers/Gagnon.2011/index.html
    // Procedural Icicles
    // Water supply
    // Source surface is a plane above the object. This is the water source and contains the origins for the raycasts that will then be sent down and checked to see if it is hitting the mesh of the model
    // Once the raycast hits the model mesh, it grabs the closet upward facing vertex at a distance lower than an influence radius and adds that to the water supply which is an array of vertices from the mesh or maybe their indexes
    // To vizualize, assign red to vertices in the water supply and white to vertices out and interpolate between

    // Water coefficient (provides an approximate quantity of water for each vertex)
    // At each vertex, find if there are vertices higher on the mesh. Move around the mesh by selecting the neighboring vertex that has the smallest calculated p value
    // If the current vertex or the pmin neighbor is part of the water supply found earlier then multiply the distance by -p
    // if only c or only pmin neighbor is in the water supply, then divide by 2
    // calculate the rest of the water coeffient
    // save the water coefficient at that vertex
    // To visualize red and blue vertices correspond respectively to lower and higher values of wc

    // Drip points identification
    // Drip limit dl is set by the user as an angle with respect to the gravity vector. This angle is used to determine the necessary angle of a vertex for water to drip off at
    // Find the drip region using polygons which have at least on vertives with a non-zero water coefficient and for which all normal  vectors of all their vertices satisfy the drip criterion
    // specify the number of icicles
    // using the icicle number, a set of points are randomly distributed on the drip region found earlier. these are the drip points

    // Icicles trajectories definition ( proposes rules and control parameters that allow the creation of several types of icicles)
    // A trajectory is created at each drip point and using the water coefficient, the final length of the icicle can be computed directly
    // The user can adjust the appearance of the icicles with a few parameters: curvature angle c, probability of subdivision d, and angle of dispersion a
    // L-System Rules:
    // ω : FX
    // p1' :X  (1-d)->  + (c)FFF/(a)X                   corresponds to the growth of the icicle
    // p1'' :X  (d)-> [+(c)+(30◦)F-(30◦)FX]A            creates a new branch
    // p2 :A  -> +(c)FFF\(a)X                           used to grow the main trunk after the creation of a branch
    // Breaking down the role of each parameter
    // Parameter c represents the angle of curvature. While growing, the icicle is rotated by this angle at each step; For linear icicles, c should have a low value
    // Parameter a is also a rotation done at each growth step, but is a rotation (roll) around a trajectory.It affects the irregularity of the branch growth direction as well as the spread of the icicle branches. computed from the user specified angle au as a = au × 137.5◦/360◦
    // Parameter d represents the probability of creating a new branch

    // Surface Creation (surface is created around the trajectory from the previous stage by computing the varying radius along the trajectory)
    // Icicle profile
    // three parameters are provided to the user: as and fs which respectively control the amplitude and the frequency of the undulation, and t, which controls the conical shape
    // R(x) = tip+ (L-x) * t + as * sin(x * fs)             For a given position x, R(x) provides the radius of the icicle; tip is the radius of the tip of the icicle adjusted to a value of 2.44millimeters
    // Modeling
    // Metaballs are placed on the points of trajectory and radii are derived from the profile function; poins are random within a distance nc in the plane perpendiculat to the trajectory
    // Base of the Icicle
    // To create the area of the base of the icicle, a set of metaballs is distributed over the surface of the object around the drip point, up to a distance eb specified by the user
    // The number of metaballs distributed on the surface is also specified by the user and is noted nmb
    // radius of each metaball is calculated with
    // rb = (eb-d)^2/(eb)^2 * wc
    // Glaze Ice
    // user-defined lifetime of a water drop lt
    // user-defined number of metaballs ngi
    // rgi = minGI + s * [dUp * lt + dDown * (1 - lt)]      rgi is radius of each metaball; dUp is the distance between the current vertex and the highest vertex; dDown is distance between current vertex and the lowest vertex in the drip region; minGI is minimum thickness of ice glaze; s is scaling of the glaze ice thickness


    // Model Parameters
    Mesh modelMesh;
    Vector3[] originalVertices;
    Vector3[] normals;
    int[] triangles;

    // Water Supply Parameters
    public GameObject ss; // Water source surface (must be a plane)
    Mesh ssMesh;
    Vector3 g = new Vector3(0, -1, 0); // Gravity vector
    int nr = 100; // Number of rays (500)
    float rii = 0.1f; // Radius of influence at the intersection
    Vector3[] waterSupplyVertices; // Stores the vertices that are in the water supply

    // Water Coefficient Parameters
    Vector3[,] higherVertices; // Stores the higher vertices at a certain vertex
    float[] waterCoefficient; // Stores the calculated water coefficient for each vertex

    // Drip Point Parameters
    int ns = 10; // Number of icicles
    float dl = 75; // Drip limit in degrees
    Vector3[] dripCriterionVertices;

    // Icicle Trajectory Parameters
    float c = 2; // Curve angle in degrees
    float d = 0.1f; // Division probability
    float au = 360; // Roll angle of growth and dispersal in degrees

    // Surface Modeling Parameters
    float t = 0.5f; // Growth ratio
    float asin = 0.3f; // Sine wave amplitude
    int fs = 50; // Sine wave frequency
    int eb = 1; // Base extent
    int nmb = 100; // Number of metaballs at the base
    int nc = 1; // Noise coefficient

    // Glaze Ice Parameters
    int minGI = 0; // Minimum radius for the glaze ice
    int ngi = 5000; // Number of metaballs used for the glaze ice
    int s = 1; // Scaling of the glaze ice
    float lt = 0.5f; // Lifetime of a water drop

    void Start()
    {
        // Model Parameters
        modelMesh = GetComponent<MeshFilter>().mesh;
        originalVertices = modelMesh.vertices;
        normals = modelMesh.normals;
        triangles = modelMesh.triangles;

        // Water Supply Parameters
        ssMesh = ss.GetComponent<MeshFilter>().mesh;
        Physics.IgnoreLayerCollision(4, 4, true);
        waterSupplyVertices = new Vector3[originalVertices.Length];

        // Water Coefficient Parameters
        higherVertices = new Vector3[originalVertices.Length, originalVertices.Length];
        waterCoefficient = new float[originalVertices.Length];

        // Function Calls
        CalculateWaterSupply();
        //FindHigherVertices();
        CalculateWaterCoefficient();
    }

    // Gets a random point on the plane to simulate the random positions of raindrops
    public Vector3 GetRandomPos()
    {
        Bounds bounds = ssMesh.bounds;

        float minX = ss.transform.position.x - ss.transform.localScale.x * bounds.size.x * 0.5f;
        float minZ = ss.transform.position.z - ss.transform.localScale.z * bounds.size.z * 0.5f;

        Vector3 newVec = new Vector3(Random.Range(minX, -minX), ss.transform.position.y - 0.01f, Random.Range(minZ, -minZ));
        return newVec;
    }

    // Finds the vertices that are "in" the water supply AKA where precipation would be hitting
    void CalculateWaterSupply()
    {
        int wsIndex = 0;

        for (int i = 0; i < nr; i++)
        {
            // Creates the rays and directs them with the gravity vector
            RaycastHit hit;
            Vector3 rayOrigin = GetRandomPos();

            Ray ray = new Ray(rayOrigin, g);
            Debug.DrawRay(rayOrigin, g, Color.red, 5);

            // At each intersection point, upward facing vertices at a distance lower than an influence radius rii are added to the water supply
            if(Physics.Raycast(ray, out hit))
            {
                // Transform the raycast hit point from world space to object space
                Vector3 hitPointLocal = transform.InverseTransformPoint(hit.point);

                // Only check rays that hit the gameObject
                if (hit.collider != null)
                {
                    Debug.Log(hit.collider.gameObject.name);
                    
                    for (int j = 0; j < originalVertices.Length; j++)
                    {
                        // Checks all vertices to see if they are within the influence radius around the hit point
                        if (Vector3.Distance(originalVertices[j], hitPointLocal) < rii)
                        {
                            // Checks if a vertex is upward facing
                            if(normals[j].y > 0)
                            {
                                //Debug.Log("Water Supply: Index: " + j + " Hit Point: " + hitPointLocal + " Vertex: " + originalVertices[j] + " Normal: " + normals[j]);

                                // Add the vertex to the water supply
                                waterSupplyVertices[wsIndex] = originalVertices[j];
                                wsIndex++;
                            }
                        }
                    }
                }
            }
        }
    }

    // To vizualize the water supply, assign red to vertices in the water supply and white to vertices out and interpolate between
    void WaterSupplyVisualization()
    {
        
    }

    // Fills the higher vertices array so that the vertices are easily sorted through during the water coefficient calculations
    void FindHigherVertices()
    {
        for (int i = 0; i < originalVertices.Length; i++)
        {
            higherVertices[i, 0] = originalVertices[i]; // sets the first value to be the starting vertex

            for (int j = 1; j < originalVertices.Length; j++)
            {
                if (originalVertices[i].y < originalVertices[j].y) // Gets all vertices with a higher y value instead of ones that are directly adjacent
                {
                    higherVertices[i, j] = originalVertices[j];
                    Debug.Log("Current Vertex: " + higherVertices[i,0] + " Higher Vertices: " + higherVertices[i, j]);
                }
            }
        }
    }

    // Goes through the higher vertices from the current index and caluclates their distance, finding the closest one
    int FindClosestHigherNeighbor(int currentVertexIndex)
    {
        int neighborIndex = 1;

        float neighborDistance;
        float minNeighborDistance = float.MaxValue;

        for (int i = 1; i < originalVertices.Length; i++)
        {
            if (i != currentVertexIndex)
            {
                neighborDistance = Vector3.Distance(originalVertices[currentVertexIndex], originalVertices[0]);

                if (neighborDistance < minNeighborDistance)
                {
                    minNeighborDistance = neighborDistance;

                    neighborIndex = i;
                }
            }
        }

        return neighborIndex;
    }

    // Water coefficient (provides an approximate quantity of water for each vertex)
    void CalculateWaterCoefficient()
    {
        Vector3 c; // Current vertex
        float wc = 0; // Water coefficient

        int neighborIndex = 1;

        Vector3 cn; // cn = normalized vector from c to n
        float p = 0; // p = dot product(cn, g)
        float minp = float.MaxValue; // Used to compare p's of different vertices
        Vector3 nmin; // The neighbor vertex for which p is minimal

        FindHigherVertices();

        // Foreach vertex from the mesh
        for (int i = 0; i < originalVertices.Length; i++)
        {
            c = originalVertices[i];

            // while there are higher neighbor vertices to c do
            //if (higherVertices.GetLength(1) > 1)
            if (higherVertices[i, 1] != null)
            {

                // foreach higher neighbor vertex n do
                // Higher with respect to gravity g
                for (int j = 1; j < higherVertices.GetLength(1); j++)
                {
                    // cn = normalized vector from c to n
                    cn = Vector3.Normalize(higherVertices[i, j] - higherVertices[i, 0]);
                    // p = dot product(cn, g)
                    p = Vector3.Dot(cn, g);

                    // Debug.Log("Water Coefficient: Index: " + i + " Vertex: " + c + " cn: " + cn + " p: " + p);

                    // Select neighbor nmin for which p is minimal
                    if (p < minp)
                    {
                        minp = p;
                        neighborIndex = j;
                    }
                }

                // Select neighbor nmin for which p is minimal
                nmin = higherVertices[i, neighborIndex];
                // The most upward n with respect to g
                bool cIsInWaterSupply = false;
                bool nminIsInWaterSupply = false;

                // if c or nmin ∈ water supply then
                for (int j = 0; j < waterSupplyVertices.Length; j++)
                {
                    if (c == waterSupplyVertices[j])
                    {
                        cIsInWaterSupply = true;
                    }

                    if (nmin == waterSupplyVertices[j])
                    {
                        nminIsInWaterSupply = true;
                    }
                }

                if (cIsInWaterSupply || nminIsInWaterSupply)
                {
                    // d = distance between c and nmin
                    float d = Vector3.Distance(c, nmin);
                    //Debug.Log("d: " + d);
                    // Multiply d by −p
                    d *= -p;
                    Debug.Log("d: " + d + " d * -p: " + d);

                    // if only c or nmin ∈ water supply then
                    if ((cIsInWaterSupply && nminIsInWaterSupply) || (cIsInWaterSupply && nminIsInWaterSupply))
                    {
                        // There is less water since one of the vertices is not in the water supply
                        //Divide the result by 2
                        d = d / 2;
                        Debug.Log("d /2: " + d);
                    }

                    // wc = wc + result
                    wc = 0;
                    wc = wc + d;

                    // c = nmin
                    c = nmin;
                }
            }

            waterCoefficient[i] = wc;
            Debug.Log("Water Coefficient: Index: " + i + " Vertex: " + c + " wc: " + wc);
        }
    }

    // To visualize red and blue vertices correspond respectively to lower and higher values of wc
    void WaterCoefficientVizualization()
    {

    }

    // Drip Point Parameters
    //int ns = 10; // Number of icicles
    //float dl = 75; // Drip limit in degrees

    // Drip point identification
    void DripPointIdentification()
    {
        // At vertex wc != 0 && normal angle is between 0 and dl
        for (int i = 0; i < originalVertices.Length; i++)
        {
            if (waterCoefficient[i] != 0)
            {
                //if ()
            }
        }
        //dripCriterionVertices
        // find the tris that contain these vertices

        // using the icicle number, a set of points are randomly distributed on the drip region found earlier. these are the drip points
    }
    // Drip limit dl is set by the user as an angle with respect to the gravity vector. This angle is used to determine the necessary angle of a vertex for water to drip off at
    // Find the drip region using polygons which have at least on vertices with a non-zero water coefficient and for which all normal  vectors of all their vertices satisfy the drip criterion
    // specify the number of icicles
    // using the icicle number, a set of points are randomly distributed on the drip region found earlier. these are the drip points

    // Icicles trajectories definition ( proposes rules and control parameters that allow the creation of several types of icicles)
    void IcicleTrajectoryDefinition()
    {

    }
    // A trajectory is created at each drip point and using the water coefficient, the final length of the icicle can be computed directly
    // The user can adjust the appearance of the icicles with a few parameters: curvature angle c, probability of subdivision d, and angle of dispersion a
    // L-System Rules:
    // ω : FX
    // p1' :X  (1-d)->  + (c)FFF/(a)X                   corresponds to the growth of the icicle
    // p1'' :X  (d)-> [+(c)+(30◦)F-(30◦)FX]A            creates a new branch
    // p2 :A  -> +(c)FFF\(a)X                           used to grow the main trunk after the creation of a branch
    // Breaking down the role of each parameter
    // Parameter c represents the angle of curvature. While growing, the icicle is rotated by this angle at each step; For linear icicles, c should have a low value
    // Parameter a is also a rotation done at each growth step, but is a rotation (roll) around a trajectory.It affects the irregularity of the branch growth direction as well as the spread of the icicle branches. computed from the user specified angle au as a = au × 137.5◦/360◦
    // Parameter d represents the probability of creating a new branch

    // Surface Creation (surface is created around the trajectory from the previous stage by computing the varying radius along the trajectory)
    // Icicle profile
    // three parameters are provided to the user: as and fs which respectively control the amplitude and the frequency of the undulation, and t, which controls the conical shape
    // R(x) = tip+ (L-x) * t + as * sin(x * fs)             For a given position x, R(x) provides the radius of the icicle; tip is the radius of the tip of the icicle adjusted to a value of 2.44millimeters
    // Modeling
    // Metaballs are placed on the points of trajectory and radii are derived from the profile function; poins are random within a distance nc in the plane perpendiculat to the trajectory
    // Base of the Icicle
    // To create the area of the base of the icicle, a set of metaballs is distributed over the surface of the object around the drip point, up to a distance eb specified by the user
    // The number of metaballs distributed on the surface is also specified by the user and is noted nmb
    // radius of each metaball is calculated with
    // rb = (eb-d)^2/(eb)^2 * wc
    // Glaze Ice
    // user-defined lifetime of a water drop lt
    // user-defined number of metaballs ngi
    // rgi = minGI + s * [dUp * lt + dDown * (1 - lt)]      rgi is radius of each metaball; dUp is the distance between the current vertex and the highest vertex; dDown is distance between current vertex and the lowest vertex in the drip region; minGI is minimum thickness of ice glaze; s is scaling of the glaze ice thickness
}