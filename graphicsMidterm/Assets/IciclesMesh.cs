using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(MeshFilter))]
public class IciclesMesh : MonoBehaviour
{
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



    // Mesh Information
    public Mesh sourceSurfaceMesh;
    Vector3[] originalVertices;

    // Water Supply
    public Vector3[] rayOrigins;
    int[] intersectionPoints;
    Ray[] ray;
    int[] triangles;
    RaycastHit hit;

    void Start()
    {
        sourceSurfaceMesh = GetComponent<MeshFilter>().sharedMesh;
        originalVertices = sourceSurfaceMesh.vertices;
        triangles = sourceSurfaceMesh.triangles;

        ComputeWaterCoefficient();
    }

    // Reference: https://profs.etsmtl.ca/epaquette/Research/Papers/Gagnon.2011/Gagnon-Icicles-2011.pdf
    // The goal is to determine, for each vertex, if the water reaches it and to compute an approximate amount of water.

    Vector3 gravityVector = new Vector3(0.0f, -9.8f, 0.0f); // m/s^2; uses just a downward force
    int numberOfRays = 10;

    //Vector3 c = new Vector3(0,0,0);
    // Determines the water coefficient of each vertex of the object
    float[] vertexWaterCoefficients;
    void ComputeWaterCoefficient()
    {
        // Water Supply Definition
        // Rain comes from a source surface ss provided by the user
        // sourceSurfaceMesh defined above;

        // Rain drops are computed using ray casting from the source surface according to the gravity vector. Ray origins are randomly distributed on the source surface based on the number of rays provided by the user
        for (int i = 0; i < numberOfRays; i++)
        {
            //rayOrigins[i] = GetRandomPointOnMesh(sourceSurfaceMesh); // for each desired ray, calculates a random position on the source surface; stores it in an array in case we need to access the origin point later
            Debug.Log(GetRandomPointOnMesh(sourceSurfaceMesh)); // for each desired ray, calculates a random position on the source surface; stores it in an array in case we need to access the origin point later

            ray[i] = new Ray(GetRandomPointOnMesh(sourceSurfaceMesh), gravityVector); // creates rays with the random origin and gives direction based on the gravity vector

            // At each intersection point, upward facing vertices at a distance lower than an influence radius rii are added to the water supply
            
        }

        // (3) At each intersection point, upward facing vertices at a distance lower than an influence radius rii are added to the water supply
        RaycastHit hit;
        
        for (int i = 0; i < numberOfRays; i++)
        {
            // For each ray, check is ray is hitting the mesh
            //if (Physics.Raycast(ray[i], out hit))
            //{
                // If yes, get the closest vertex as the intersection point and store in an array
            //    intersectionPoints[i] = GetClosestVertex(hit, triangles);

                // For each intersection point, check if it is lower than the influence radius
                // If yes, store to the water supply
            //}
        }
        // To visualize the water supply, give the vertex a color and interpolate between the colors of the vertices






        // This part simulates the water flow; water flow is computed only from vertex to vertex along the edges
        //int i = 0;
        int higherVertexIndex = 0;
        // Upward computation
        // foreach vertex v from the mesh do
        foreach (Vector3 v in originalVertices)
        {
            Debug.Log("Checking Vertices: " + "Local: " + v + " World: " + transform.localToWorldMatrix * v); // All of the vertices on the sphere are between -0.5 and 0.5

            Vector3 c = v; // Current vertex c = v
            float wc = 0.0f; // Water coefficient wc = 0

            // variables for finding neighboring vertices
            float minDistance = float.MaxValue;
            int p = 0;
            int[] closestVertexIndex = new int[10];

            float[] verticesDistances = new float[originalVertices.Length];

            // Find the minimum distance between vertices
            for (int i = 0; i < originalVertices.Length; i++)
            {
                if (v != originalVertices[i])
                {
                    // go through vertices and store the distances
                    verticesDistances[i] = Vector3.Distance(originalVertices[i], v);

                    if (verticesDistances[i] < minDistance)
                    {
                        minDistance = verticesDistances[i];
                    }
                }
            }

            Debug.Log("Mindistance: " + minDistance);

            // Find the closest vertices by comparing the distances found in the last loop; there should be 4 with the same distance and they are the neighboring vertices
            for (int i = 1; i < originalVertices.Length; i++)
            {
                // compare and update minimum distance & closest character if required
                if (verticesDistances[i] == minDistance)
                {
                    closestVertexIndex[p] = i;

                    Debug.Log("Closest Vertex Index: " + i);
                    Debug.Log("Vertex" + p + ": " + originalVertices[closestVertexIndex[p]]);

                    p++;
                }
            }


            //while(c.y < //while there are higher neighbor vertices to c do
            //foreach higher neighbor vertex n do
            //// Higher with respect to gravity g
            //Vector3 cn = Vector3.Normalize(Vector3.Distance(n, c)); //cn = normalized vector from c to n; Vector3.Distance(other.position, transform.position)
            //Vector3 p = Vector3.Dot(cn, g);//p = dot product(cn, g)
            //Select neighbor nmin for which p is minimal
            //// The most upward n with respect to g
            //if c or nmin ∈ water supply then
            //d = distance between c and nmin
            //Multiply d by −p
            //if only c or nmin ∈ water supply then
            ///* There is less water since one of the
            //vertices is not in the water supply */
            //Divide the result by 2
            //wc = wc + result

            //c = nmin

            //vertexWaterCoefficients[i] = wc; //Save wc at vertex v
            //i++;
        }
    }


    // Refer to for random points for next two functions: https://gist.github.com/v21/5378391
    Vector3 GetRandomPointOnMesh(Mesh mesh)
    {
        //if you're repeatedly doing this on a single mesh, you'll likely want to cache cumulativeSizes and total
        float[] sizes = GetTriSizes(mesh.triangles, mesh.vertices);
        float[] cumulativeSizes = new float[sizes.Length];
        float total = 0;

        for (int i = 0; i < sizes.Length; i++)
        {
            total += sizes[i];
            cumulativeSizes[i] = total;
        }

        //so everything above this point wants to be factored out

        float randomsample = Random.value * total;

        int triIndex = -1;

        for (int i = 0; i < sizes.Length; i++)
        {
            if (randomsample <= cumulativeSizes[i])
            {
                triIndex = i;
                break;
            }
        }

        if (triIndex == -1) Debug.LogError("triIndex should never be -1");

        Vector3 a = mesh.vertices[mesh.triangles[triIndex * 3]];
        Vector3 b = mesh.vertices[mesh.triangles[triIndex * 3 + 1]];
        Vector3 c = mesh.vertices[mesh.triangles[triIndex * 3 + 2]];

        //generate random barycentric coordinates

        float r = Random.value;
        float s = Random.value;

        if (r + s >= 1)
        {
            r = 1 - r;
            s = 1 - s;
        }

        //and then turn them back to a Vector3
        Vector3 pointOnMesh = a + r * (b - a) + s * (c - a);
        return pointOnMesh;
    }

    float[] GetTriSizes(int[] tris, Vector3[] verts)
    {
        int triCount = tris.Length / 3;
        float[] sizes = new float[triCount];
        for (int i = 0; i < triCount; i++)
        {
            sizes[i] = .5f * Vector3.Cross(verts[tris[i * 3 + 1]] - verts[tris[i * 3]], verts[tris[i * 3 + 2]] - verts[tris[i * 3]]).magnitude;
        }
        return sizes;
    }

void DripPointsIdentification()
    {

    }

    // Reference: https://answers.unity.com/questions/1305031/pinpointing-one-vertice-with-raycasthit.html
    public static int GetClosestVertex(RaycastHit aHit, int[] aTriangles)
    {
        var b = aHit.barycentricCoordinate;
        int index = aHit.triangleIndex * 3;
        if (aTriangles == null || index < 0 || index + 2 >= aTriangles.Length)
            return -1;
        if (b.x > b.y)
        {
            if (b.x > b.z)
                return aTriangles[index]; // x
            else
                return aTriangles[index + 2]; // z
        }
        else if (b.y > b.z)
            return aTriangles[index + 1]; // y
        else
            return aTriangles[index + 2]; // z
    }
}