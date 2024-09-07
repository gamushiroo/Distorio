using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour {

    public MeshFilter meshFilter;
    public World world;
    public Transform handBlock;

    public float switchEase;
    public float placeEase;

    void Start() {

        switchEase = 1;
        placeEase = 1;

    }

    // Update is called once per frame
    void Update() {

        switchEase += Time.deltaTime;
        switchEase = Mathf.Clamp(switchEase, 0, 1);

        placeEase += Time.deltaTime;
        placeEase = Mathf.Clamp(placeEase, 0, 1);

        handBlock.localPosition = new Vector3(0, EaseSqu(switchEase), 0);
        handBlock.localRotation = Quaternion.Euler((1 - EaseSqu(placeEase)) * 45, 0, 0);
    }

    public float EaseSqu(float pos) {

        return pos == 1 ? 1 : 1 - Mathf.Pow(2, -10 * pos);

    }

    public void GenerateMesh (int selectedBlockIndex) {

        switchEase = 0;
        int vertexIndex = 0;
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();
        if (selectedBlockIndex != 0) {
            for (int p = 0; p < 6; p++) {
                for (int i = 0; i < 4; i++) {
                    vertices.Add(Data.voxelVerts[Data.blockMesh[p, i]]);
                    uvs.Add(( Data.voxelUVs[i] + Data.TexturePos(world.blockTypes[selectedBlockIndex].GetTextureID(p)) ) / Data.TextureSize);
                }
                for (int i = 0; i < 6; i++)
                    triangles.Add(Data.order[i] + vertexIndex);
                vertexIndex += 4;
            }
        }
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
