using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using System;
using System.Threading.Tasks;

public class CMesh : ICloneable
{
    public CMesh()
    {
        
    }

    public CMesh(Vector3[] vertices, int[] triangles, Vector2[] uv, Vector3[] normals)
    {
        this.vertices = vertices;
        this.triangles = triangles;
        this.uv = uv;
        this.normals = normals;
    }

    public Vector3[] vertices { get; set; }
    public int[] triangles { get; set; }
    public Vector2[] uv { get; set; }
    public Vector3[] normals { get; set; }
    public UnityEngine.Rendering.IndexFormat indexFormat { get; set; }
    public int quality { get; set; }

    public object Clone()
    {
        return new CMesh(vertices, triangles, uv, normals);
    }

    public Mesh ToUnityMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = this.vertices;
        mesh.normals = this.normals;
        mesh.triangles = this.triangles;
        mesh.uv = this.uv;
        return mesh;
    }
}

public struct TriangleIndices
{
    public int v1;
    public int v2;
    public int v3;

    public TriangleIndices(int v1, int v2, int v3)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
    }
}

public class Face:ICloneable
{
    public TriangleIndices triangleIndices;
    public int levelOfDetail = 0;
    public List<Face> subFaces = new List<Face>();
    public Face(TriangleIndices triangleIndices, int levelOfDetail = 0)
    {
        this.triangleIndices = triangleIndices;
        this.levelOfDetail = levelOfDetail;
    }

    public Face(TriangleIndices triangleIndices, List<Face> subFaces, int levelOfDetail = 0)
    {
        this.triangleIndices = triangleIndices;
        this.levelOfDetail = levelOfDetail;
        this.subFaces = subFaces;
    }

    public object Clone()
    {
        return new Face(this.triangleIndices, this.subFaces, this.levelOfDetail);
    }
}


public class PlanetMeshHandler
{
    #region publicVar
    public float radius = 1;
    public float recursionLevel = 2;
    public List<float> detailDistance = new List<float>();
    public List<int> detailRecursionLevel = new List<int>();
    internal CMesh cMesh = new CMesh();
    private List<Face> faces = new List<Face>();
    private List<int> triList = new List<int>();
    private List<Vector3> vertList = new List<Vector3>();
    private Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();
    public Vector3 gObjPosition;
    #endregion

    public PlanetMeshHandler(float radius, float recursionLevel)
    {
        this.radius = radius;
        this.recursionLevel = recursionLevel;
        this.CreateMesh();
    }

    // return index of point in the middle of p1 and p2
    public static int getMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float radius)
    {
        // first check if we have it already
        bool firstIsSmaller = p1 < p2;
        long smallerIndex = firstIsSmaller ? p1 : p2;
        long greaterIndex = firstIsSmaller ? p2 : p1;
        long key = (smallerIndex << 32) + greaterIndex;

        int ret;
        lock (cache)
        {
            if (cache.TryGetValue(key, out ret))
            {
                return ret;
            }

            // not in cache, calculate it
            Vector3 point1 = vertices[p1];
            Vector3 point2 = vertices[p2];
            Vector3 middle = new Vector3
            (
                (point1.x + point2.x) / 2f,
                (point1.y + point2.y) / 2f,
                (point1.z + point2.z) / 2f
            );

            // add vertex makes sure point is on unit sphere
            int i = vertices.Count;
            vertices.Add(middle.normalized * radius);

            // store it, return index
            cache.Add(key, i);
            return i;
        }
    }

    internal void CreateMesh()
    {
        cMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        int index = 0;

        // create 12 vertices of a icosahedron
        float t = (1f + Mathf.Sqrt(5f)) / 2f;

        #region vertList
        vertList.Add(new Vector3(-1f, t, 0f).normalized * radius);
        vertList.Add(new Vector3(1f, t, 0f).normalized * radius);
        vertList.Add(new Vector3(-1f, -t, 0f).normalized * radius);
        vertList.Add(new Vector3(1f, -t, 0f).normalized * radius);

        vertList.Add(new Vector3(0f, -1f, t).normalized * radius);
        vertList.Add(new Vector3(0f, 1f, t).normalized * radius);
        vertList.Add(new Vector3(0f, -1f, -t).normalized * radius);
        vertList.Add(new Vector3(0f, 1f, -t).normalized * radius);

        vertList.Add(new Vector3(t, 0f, -1f).normalized * radius);
        vertList.Add(new Vector3(t, 0f, 1f).normalized * radius);
        vertList.Add(new Vector3(-t, 0f, -1f).normalized * radius);
        vertList.Add(new Vector3(-t, 0f, 1f).normalized * radius);
        #endregion

        #region faces
        // create 20 triangles of the icosahedron
        //List<TriangleIndices> faces = new List<TriangleIndices>();

        // 5 faces around point 0
        faces.Add(new Face(new TriangleIndices(0, 11, 5)));
        faces.Add(new Face(new TriangleIndices(0, 5, 1)));
        faces.Add(new Face(new TriangleIndices(0, 1, 7)));
        faces.Add(new Face(new TriangleIndices(0, 7, 10)));
        faces.Add(new Face(new TriangleIndices(0, 10, 11)));

        // 5 adjacent faces 
        faces.Add(new Face(new TriangleIndices(1, 5, 9)));
        faces.Add(new Face(new TriangleIndices(5, 11, 4)));
        faces.Add(new Face(new TriangleIndices(11, 10, 2)));
        faces.Add(new Face(new TriangleIndices(10, 7, 6)));
        faces.Add(new Face(new TriangleIndices(7, 1, 8)));

        // 5 faces around point 3
        faces.Add(new Face(new TriangleIndices(3, 9, 4)));
        faces.Add(new Face(new TriangleIndices(3, 4, 2)));
        faces.Add(new Face(new TriangleIndices(3, 2, 6)));
        faces.Add(new Face(new TriangleIndices(3, 6, 8)));
        faces.Add(new Face(new TriangleIndices(3, 8, 9)));

        // 5 adjacent faces 
        faces.Add(new Face(new TriangleIndices(4, 9, 5)));
        faces.Add(new Face(new TriangleIndices(2, 4, 11)));
        faces.Add(new Face(new TriangleIndices(6, 2, 10)));
        faces.Add(new Face(new TriangleIndices(8, 6, 7)));
        faces.Add(new Face(new TriangleIndices(9, 8, 1)));
        #endregion

        // refine triangles
        for (int i = 0; i < recursionLevel; i++)
        {
            List<Face> faces2 = new List<Face>();
            foreach (var tri in faces)
            {
                // replace triangle by 4 triangles
                int a = getMiddlePoint(tri.triangleIndices.v1, tri.triangleIndices.v2, ref vertList, ref middlePointIndexCache, radius);
                int b = getMiddlePoint(tri.triangleIndices.v2, tri.triangleIndices.v3, ref vertList, ref middlePointIndexCache, radius);
                int c = getMiddlePoint(tri.triangleIndices.v3, tri.triangleIndices.v1, ref vertList, ref middlePointIndexCache, radius);

                faces2.Add(new Face(new TriangleIndices(tri.triangleIndices.v1, a, c)));
                faces2.Add(new Face(new TriangleIndices(tri.triangleIndices.v2, b, a)));
                faces2.Add(new Face(new TriangleIndices(tri.triangleIndices.v3, c, b)));
                faces2.Add(new Face(new TriangleIndices(a, b, c)));
            }
            faces = faces2;
        }

        cMesh.vertices = vertList.ToArray();

        for (int i = 0; i < faces.Count; i++)
        {
            triList.Add(faces[i].triangleIndices.v1);
            triList.Add(faces[i].triangleIndices.v2);
            triList.Add(faces[i].triangleIndices.v3);
        }

        cMesh.triangles = triList.ToArray();
        cMesh.uv = new Vector2[vertList.Count];

        Vector3[] normales = new Vector3[vertList.Count];
        for (int i = 0; i < normales.Length; i++)
        {
            normales[i] = vertList[i].normalized;
        }

        cMesh.normals = normales;
    }

    private Face DevideFacesRec(Face face, int levelOfDetail, int index = 0)
    {
        if (index != levelOfDetail)
        {
            if (face.subFaces.Count == 0)
            {
                //replace triangle by 4 triangles
                int a = getMiddlePoint(face.triangleIndices.v1, face.triangleIndices.v2, ref vertList, ref middlePointIndexCache, radius);
                int b = getMiddlePoint(face.triangleIndices.v2, face.triangleIndices.v3, ref vertList, ref middlePointIndexCache, radius);
                int c = getMiddlePoint(face.triangleIndices.v3, face.triangleIndices.v1, ref vertList, ref middlePointIndexCache, radius);

                face.subFaces.Add(new Face(new TriangleIndices(face.triangleIndices.v1, a, c), 1));
                face.subFaces.Add(new Face(new TriangleIndices(face.triangleIndices.v2, b, a), 1));
                face.subFaces.Add(new Face(new TriangleIndices(face.triangleIndices.v3, c, b), 1));
                face.subFaces.Add(new Face(new TriangleIndices(a, b, c), 1));
            }

            for (int i = 0; i < face.subFaces.Count; i++)
            {
                face.subFaces[i] = DevideFacesRec(face.subFaces[i], levelOfDetail, index + 1);
            }
        }
        else
        {
            lock (triList)
            {
                triList.Add(face.triangleIndices.v1);
                triList.Add(face.triangleIndices.v2);
                triList.Add(face.triangleIndices.v3);
            }
            return face;
        }

        return face;
    }

    private Face DivideFace(Face face, List<float> detailDistance, List<int> detailQuality)
    {
        Vector3 v3MiddlePointTriangle = (vertList[face.triangleIndices.v1] + vertList[face.triangleIndices.v2] + vertList[face.triangleIndices.v3]) / 3;
        float distanceToTri = Mathf.Abs((v3MiddlePointTriangle - gObjPosition).magnitude);

        if (distanceToTri < detailDistance[3])
        {
            face = DevideFacesRec(face, detailQuality[3]);
        }
        else if (distanceToTri < detailDistance[2])
        {
            face = DevideFacesRec(face, detailQuality[2]);
        }
        else if (distanceToTri < detailDistance[1])
        {
            face = DevideFacesRec(face, detailQuality[1]);
        }
        else if (distanceToTri < detailDistance[0])
        {
            face = DevideFacesRec(face, detailQuality[0]);
        }
        else
        {
            lock (triList)
            {
                triList.Add(face.triangleIndices.v1);
                triList.Add(face.triangleIndices.v2);
                triList.Add(face.triangleIndices.v3);
            }
        }
        return face;
    }

    public void UpdateCMash()
    {
        triList.Clear();
        middlePointIndexCache.Clear();
        Parallel.For(0, faces.Count, i =>
        {
            Face face;
            lock (faces)
            {
                face = (Face)faces[i].Clone();
            }
            face = DivideFace(face, detailDistance, detailRecursionLevel);
            lock (faces)
            {
                faces[i] = face;
            }
        });

        cMesh.vertices = vertList.ToArray();

        cMesh.triangles = triList.ToArray();
        cMesh.uv = new Vector2[vertList.Count];

        Vector3[] normales = new Vector3[vertList.Count];
        for (int i = 0; i < normales.Length; i++)
            normales[i] = vertList[i].normalized;

        cMesh.normals = normales;
    }
}



[RequireComponent(typeof(MeshFilter))]
public class PlanetGenerator : MonoBehaviour
{
    public float radius = 1f;
    public int recursionLevel = 3;
    public float updateThreshold = 1f;
    public GameObject gObj;
    private Vector3 gObjPosition;
    private Vector3 gObjPositionOld;
    public float distanceDetail1 = 10f;
    public float distanceDetail2 = 5f;
    public float distanceDetail3 = 3f;
    public float distanceDetail4 = 1f;
    public int qualityDetail1 = 1;
    public int qualityDetail2 = 1;
    public int qualityDetail3 = 1;
    public int qualityDetail4 = 1;
    bool updateMesh = false;
    internal CMesh cMesh;
    private PlanetMeshHandler planetMeshHandler;

    // Start is called before the first frame update
    void Start()
    {
        gObjPositionOld=gObj.transform.position - Vector3.one - new Vector3(updateThreshold, updateThreshold, updateThreshold);
        gObjPosition = gObj.transform.position;
        planetMeshHandler = new PlanetMeshHandler(radius, recursionLevel);
        planetMeshHandler.detailDistance.Add(distanceDetail1);
        planetMeshHandler.detailDistance.Add(distanceDetail2);
        planetMeshHandler.detailDistance.Add(distanceDetail3);
        planetMeshHandler.detailDistance.Add(distanceDetail4);
        planetMeshHandler.detailRecursionLevel.Add(qualityDetail1);
        planetMeshHandler.detailRecursionLevel.Add(qualityDetail2);
        planetMeshHandler.detailRecursionLevel.Add(qualityDetail3);
        planetMeshHandler.detailRecursionLevel.Add(qualityDetail4);

        cMesh = planetMeshHandler.cMesh;

        Mesh mesh = planetMeshHandler.cMesh.ToUnityMesh();
        mesh.indexFormat = planetMeshHandler.cMesh.indexFormat;
        mesh.RecalculateBounds();
        mesh.Optimize();
        transform.gameObject.GetComponent<MeshFilter>().mesh = mesh;

        Thread threadMeshUpdate = new Thread(() => UpdateMesh(UpdateMeshCallback, planetMeshHandler));
        threadMeshUpdate.Start();
    }

    private void FixedUpdate()
    {
        gObjPosition = gObj.transform.position;
        planetMeshHandler.gObjPosition = gObjPosition;
        if (updateMesh == true && cMesh != null)
        {
            updateMesh = false;
            Mesh mesh = transform.gameObject.GetComponent<MeshFilter>().mesh;
            mesh.vertices = cMesh.vertices;
            mesh.triangles = cMesh.triangles;
            mesh.uv = cMesh.uv;
            mesh.normals = cMesh.normals;
            //mesh.RecalculateBounds();
            //mesh.Optimize();
            transform.gameObject.GetComponent<MeshFilter>().mesh = mesh;
        }
    }

    public void UpdateMeshCallback(CMesh mesh)
    {
        cMesh = mesh;
        updateMesh = true;
    }

    public void UpdateMesh(Action<CMesh> callback, PlanetMeshHandler planetMeshHandler)
    {
        while (true)
        {
            if (Math.Abs((gObjPosition - gObjPositionOld).magnitude) >= updateThreshold)
            {
                gObjPositionOld = gObjPosition;
                planetMeshHandler.UpdateCMash();
                callback(planetMeshHandler.cMesh);
            }
        }
    }
}