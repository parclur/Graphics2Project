using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class IciclesMesh : MonoBehaviour
{
    // Old; taken from the sphere cube mesh deformation
    public float springForce = 20f;
    public float damping = 5f;
    
    Mesh sourceSurfaceMesh;
    Vector3[] originalVertices, displacedVertices;
    Vector3[] vertexVelocities;
    
    float uniformScale = 1f;
    
    void Start()
    {
        sourceSurfaceMesh = GetComponent<MeshFilter>().mesh;
        originalVertices = sourceSurfaceMesh.vertices;
        displacedVertices = new Vector3[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            displacedVertices[i] = originalVertices[i];
        }
        vertexVelocities = new Vector3[originalVertices.Length];

        ComputeWaterCoefficient();
    }
    
    void Update()
    {
        uniformScale = transform.localScale.x;
        for (int i = 0; i < displacedVertices.Length; i++)
        {
            UpdateVertex(i);
        }
        sourceSurfaceMesh.vertices = displacedVertices;
        sourceSurfaceMesh.RecalculateNormals();
    }

    // Old; taken from the sphere cube mesh deformation
    void UpdateVertex(int i)
    {
        Vector3 velocity = vertexVelocities[i];
        Vector3 displacement = displacedVertices[i] - originalVertices[i];
        displacement *= uniformScale;
        velocity -= displacement * springForce * Time.deltaTime;
        velocity *= 1f - damping * Time.deltaTime;
        vertexVelocities[i] = velocity;
        displacedVertices[i] += velocity * (Time.deltaTime / uniformScale);
    }

    // Old; taken from the sphere cube mesh deformation
    public void AddDeformingForce(Vector3 point, float force)
    {
        point = transform.InverseTransformPoint(point);
        for (int i = 0; i < displacedVertices.Length; i++)
        {
            AddForceToVertex(i, point, force);
        }
    }

    // Old; taken from the sphere cube mesh deformation
    void AddForceToVertex(int i, Vector3 point, float force)
    {
        Vector3 pointToVertex = displacedVertices[i] - point;
        pointToVertex *= uniformScale;
        float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
        float velocity = attenuatedForce * Time.deltaTime;
        vertexVelocities[i] += pointToVertex.normalized * velocity;
    }

    // Reference: https://profs.etsmtl.ca/epaquette/Research/Papers/Gagnon.2011/Gagnon-Icicles-2011.pdf
    // The goal is to determine, for each vertex, if the water reaches it and to compute an approximate amount of water.
    Vector3[] rayOrigins;
    int[] intersectionPoints;
    Ray[] ray;
    int[] triangles;


    //Vector3 c = new Vector3(0,0,0);
    // Determines the water coefficient of each vertex of the object
    float[] vertexWaterCoefficients;
    void ComputeWaterCoefficient()
    {
        int i = 0;
        // Upward computation
        // foreach vertex v from the mesh do
        foreach (Vector3 v in originalVertices)
        {
            Debug.Log("Checking Vertices: " + v); // All of the vertices on the sphere are between -0.5 and 0.5

            Vector3 c = v; // Current vertex c = v
            float wc = 0.0f; // Water coefficient wc = 0

            //while(c.y < //while there are higher neighbor vertices to c do
            //foreach higher neighbor vertex n do
            //// Higher with respect to gravity g
            Vector3 cn = Vector3.Normalize(Vector3.Distance(n, c)); //cn = normalized vector from c to n; Vector3.Distance(other.position, transform.position)
            Vector3 p = Vector3.Dot(cn, g);//p = dot product(cn, g)
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

            vertexWaterCoefficients[i] = wc; //Save wc at vertex v
            i++;
        }

        // Downward computation - ineffiecnt
        /*
        // (1) Rain comes from a source surface ss provided by the user
        // sourceSurfaceMesh defined above;

        // (2) Rain drops are computed using ray casting from the source surface according to the gravity vector. Ray origins are randomly distributed on the source surface based on the number of rays provided by the user
        Vector3 gravityVector = new Vector3(0.0f, -9.8f, 0.0f); // m/s^2; uses just a downward force
        int numberOfRays = 10;

        for (int i = 0; i < numberOfRays; i++)
        {
            rayOrigins[i] = GetRandomPointOnMesh(sourceSurfaceMesh); // for each desired ray, calculates a random position on the source surface; stores it in an array in case we need to access the origin point later

            ray[i] = new Ray(rayOrigins[i], gravityVector); // creates rays with the random origin and gives direction based on the gravity vector
        }

        // (3) At each intersection point, upward facing vertices at a distance lower than an influence radius rii are added to the water supply
        RaycastHit hit;
        for (int i = 0; i < numberOfRays; i++)
        {
            // For each ray, check is ray is hitting the mesh
            if (Physics.Raycast(ray[i], out hit))
            {
                // If yes, get the closest vertex as the intersection point and store in an array
                intersectionPoints[i] = GetClosestVertex(hit, );

                // For each intersection point, check if it is lower than the influence radius
                // If yes, store to the water supply
            }
        }
        // To visualize the water supply, give the vertex a color and interpolate between the colors of the vertices
        */
    }

    //int[] GetRaycastHitTriangles(RaycastHit hit)
    //{
    //    MeshCollider meshCollider = hit.collider as MeshCollider;
    //    if (meshCollider == null || meshCollider.sharedMesh == null)
    //        return;
    //
    //    Mesh mesh = meshCollider.sharedMesh;
    //    Vector3[] vertices = mesh.vertices;
    //    int[] triangles = mesh.triangles;
    //    Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
    //    Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
    //    Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
    //    Transform hitTransform = hit.collider.transform;
    //    p0 = hitTransform.TransformPoint(p0);
    //    p1 = hitTransform.TransformPoint(p1);
    //    p2 = hitTransform.TransformPoint(p2);
    //    Debug.DrawLine(p0, p1);
    //    Debug.DrawLine(p1, p2);
    //    Debug.DrawLine(p2, p0);
    //}

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

        /*
         * 
         * more readably:
         * 
for(int ii = 0 ; ii < indices.Length; ii+=3)
{
    Vector3 A = Points[indices[ii]];
    Vector3 B = Points[indices[ii+1]];
    Vector3 C = Points[indices[ii+2]];
    Vector3 V = Vector3.Cross(A-B, A-C);
    Area += V.magnitude * 0.5f;
}
         * 
         * 
         * */
    }
}

/*
The first stage of the proposed method is the computation of the water flow. The goal is to determine, for
each vertex, if the water reaches it and to compute an
approximate amount of water. The water flow is used
to select the locations where to grow the icicles and the
amount of water is used to determine the growth rate
of the icicles.
The distribution of water is governed by two factors:
water reaches the surface providing the water supply,
and then flows on the surface until it accumulates in a
concave area or falls from the surface. When considering icicles, rain is one of the most important source of
water [7]. In our implementation, the rain comes from
a source surface ss provided by the user. Rain drops
are computed using ray casting from the source surface according to the gravity vector. Ray origins are
randomly distributed on the source surface based on
the number of rays provided by the user. At each intersection point, upward facing vertices at a distance
lower than an influence radius rii are added to the water supply. Visualization of the results is provided by
assigning a red color to the vertices in the water supply and a white color to the others. Figure 2 presents
an example of water supply in which the polygons are
rendered by interpolating the colors of the vertices.
 */
