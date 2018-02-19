using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThreadedSimpleMesh
{
    public List<Vector3> verts;
    public List<Vector2> uvs;
    public List<int> tris;


    public void SetVertices(List<Vector3> newverts)
    {
        verts = newverts;
    }

    public void SetTriangles(List<int> newtris, int submesh)
    {
        tris = newtris;
    }

    public void SetUVs(int uvIndex, List<Vector2> newuvs)
    {
        uvs = newuvs;
    }

    public Mesh GenerateMesh()
    {
        Mesh newMesh = new Mesh();
        
        newMesh.SetVertices(verts);
        newMesh.SetTriangles(tris, 0);
        newMesh.SetUVs(0, uvs);

        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();

        return newMesh;
    }
}

public class ThreadedComplexMesh
{
    public List<Vector3> verts = new List<Vector3>();
    public List<List<Vector2>> uvs = new List<List<Vector2>>();
    public List<List<int>> tris = new List<List<int>>();

    public void SetVertices(List<Vector3> newverts)
    {
        verts = newverts;
    }

    public void SetTriangles(List<int> newtris, int submesh)
    {
        while (submesh >= tris.Count)
        {
            tris.Add(new List<int>());
        }
        tris[submesh] = newtris;
    }

    public void SetUVs(int uvIndex, List<Vector2> newuvs)
    {
        while (uvIndex >= uvs.Count)
        {
            uvs.Add(new List<Vector2>());
        }
        uvs[uvIndex] = newuvs;
    }

    public Mesh GenerateMesh()
    {
        Mesh newMesh = new Mesh();
        
        newMesh.SetVertices(verts);

        newMesh.subMeshCount = tris.Count;
        for (int i = 0; i < tris.Count; i++)
        {
            newMesh.SetTriangles(tris[i], i);
        }

        for (int i = 0; i < uvs.Count && i < 2; i++)
        {
            newMesh.SetUVs(i, uvs[i]);
        }

        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();


        return newMesh;
    }

}
