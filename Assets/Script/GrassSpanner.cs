//
// created by jiadong chen
// http://www.chenjd.me
//
using UnityEngine;
using System.Collections.Generic;

using Random = System.Random;

public class GrassSpanner : MonoBehaviour {


    #region 字段

    public Texture2D heightMap;
    public float terrainHeight;
    public int terrainSize = 250;
    public Material terrainMat;
    public Material grassMat;
    private List<Vector3> verts = new List<Vector3>();
    private Random random;

    #endregion

    void Start()
    {
        this.random = new Random();
        GenerateTerrain();
        GenerateField(30, 50);
    }


    /// <summary>
    /// 生成地形
    /// </summary>
    private void GenerateTerrain()
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        for (int i = 0; i < this.terrainSize; i++)
        {
            for (int j = 0; j < this.terrainSize; j++)
            {
                verts.Add(new Vector3(i, heightMap.GetPixel(i, j).grayscale * this.terrainHeight , j));
                if (i == 0 || j == 0)
                    continue;
                tris.Add(terrainSize * i + j);
                tris.Add(terrainSize * i + j - 1);
                tris.Add(terrainSize * (i - 1) + j - 1);
                tris.Add(terrainSize * (i - 1) + j - 1);
                tris.Add(terrainSize * (i - 1) + j);
                tris.Add(terrainSize * i + j);
            }
        }

        Vector2[] uvs = new Vector2[verts.Count];

        for (var i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(verts[i].x, verts[i].z);
        }

        GameObject plane = new GameObject("groundPlane");
        plane.AddComponent<MeshFilter>();
        MeshRenderer renderer = plane.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = terrainMat;

        Mesh groundMesh = new Mesh();
        groundMesh.vertices = verts.ToArray(); 
        groundMesh.uv = uvs;
        groundMesh.triangles = tris.ToArray();
        groundMesh.RecalculateNormals(); 
        plane.GetComponent<MeshFilter>().mesh = groundMesh;

        this.verts.Clear();
    }

    /// <summary>
    /// 生成草地
    /// </summary>
    /// <param name="grassPatchRowCount"></param>
    /// <param name="grassCountPerPatch"></param>
    private void GenerateField(int grassPatchRowCount, int grassCountPerPatch)
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < 65000; i++)
        {
            indices.Add(i);
        }

        Vector3 startPosition = new Vector3(0, 0, 0);
        Vector3 patchSize = new Vector3(terrainSize / grassPatchRowCount, 0, terrainSize / grassPatchRowCount);

        for (int x = 0; x < grassPatchRowCount; x++)
        {
            for (int y = 0; y < grassPatchRowCount; y++)
            {
                this.GenerateGrass(startPosition, patchSize, grassPatchRowCount);
                startPosition.x += patchSize.x;
            }

            startPosition.x = 0;
            startPosition.z += patchSize.z;
        }

        GameObject grassLayer;
        MeshFilter mf;
        MeshRenderer renderer;
        Mesh m;

        while (verts.Count > 65000)
        {
            m = new Mesh();
            m.vertices = verts.GetRange(0, 65000).ToArray();
            m.SetIndices(indices.ToArray(), MeshTopology.Points, 0);

            grassLayer = new GameObject("grassLayer");
            mf = grassLayer.AddComponent<MeshFilter>();
            renderer = grassLayer.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = grassMat;
            mf.mesh = m;
            verts.RemoveRange(0, 65000);
        }

        m = new Mesh();
        m.vertices = verts.ToArray();
        m.SetIndices(indices.GetRange(0, verts.Count).ToArray(), MeshTopology.Points, 0);
        grassLayer = new GameObject("grassLayer");
        mf = grassLayer.AddComponent<MeshFilter>();
        renderer = grassLayer.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = grassMat;
        mf.mesh = m;

        return;
    }

    private void GenerateGrass(Vector3 startPosition, Vector3 patchSize, int grassCountPerPatch)
    {
        for (var i = 0; i < grassCountPerPatch; i++)
        {
            var randomizedZDistance = (float)this.random.NextDouble() * patchSize.z;
            var randomizedXDistance = (float)this.random.NextDouble() * patchSize.x;

            int indexX = (int)((startPosition.x + randomizedXDistance));
            int indexZ = (int)((startPosition.z + randomizedZDistance));

            if (indexX >= terrainSize)
            {
                indexX = (int)terrainSize - 1;
            }

            if (indexZ >= terrainSize)
            {
                indexZ = (int)terrainSize - 1;
            }

            var currentPosition = new Vector3(startPosition.x + (randomizedXDistance), heightMap.GetPixel(indexX, indexZ).grayscale * (terrainHeight + 1), startPosition.z + randomizedZDistance);
            this.verts.Add(currentPosition);
        }
    }

}
