using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class PathRenderer : MonoBehaviour {




    private int vertexIndex = 0;
    private readonly List<Vector3> vertices = new();
    private readonly List<int> triangles = new();
    private readonly List<Vector2> uvs = new();



    public MeshFilter meshFilter;

    public World world;


    public void AAA (Queue<VoxelAndPos> a) {

        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();


        while (a.Count > 0) {


            DDDD(a.Dequeue());



        }

        GenerateMesh();

    }
    void DDDD (VoxelAndPos vp) {

        for (int p = 0; p < 6; p++) {
            for (int i = 0; i < 4; i++) {
                vertices.Add(Data.voxelVerts[Data.blockMesh[p, i]] + new Vector3Int(vp.pos.c.x, 0, vp.pos.c.y) * 16 + vp.pos.v);



                uvs.Add((Data.voxelUVs[i] + Data.TexturePos(world.blockTypes[vp.id].GetTextureID(p))) / Data.TextureSize);



            }
            for (int i = 0; i < 6; i++) {
                triangles.Add(Data.order[i] + vertexIndex);
            }
            vertexIndex += 4;
        }
    }
    public void GenerateMesh () {
        Mesh mesh = new() {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray()
        };
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;
    }
}
