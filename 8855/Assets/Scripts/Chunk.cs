using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Chunk {

    public Voxel[,,] voxelMap = new Voxel[Data.ChunkWidth, Data.ChunkHeight, Data.ChunkWidth];
    public bool threadLocked = false;
    public bool isTerrainMapGenerated = false;
    public bool _isActive;

    public GameObject chunkObject;

    private MeshFilter meshFilter;
    private readonly World world;
    private Vector2Int pos;

    private int vertexIndex = 0;
    private readonly List<Vector3> vertices = new();
    private readonly List<int> triangles = new();
    private readonly List<Vector2> uvs = new();

    public Queue<VoxelMod> modifications = new();

    public Chunk (Vector2Int pos, World world, bool generateOnLoad) {

        world.generatedChunks.Add(pos);
        _isActive = true;

        this.pos = pos;
        this.world = world;

        chunkObject = new();
        chunkObject.transform.parent = world.transform;
        chunkObject.transform.position = new Vector3Int(pos.x, 0, pos.y) * Data.ChunkWidth;
        chunkObject.AddComponent<MeshRenderer>().material = world.material;
        meshFilter = chunkObject.AddComponent<MeshFilter>();

        if (generateOnLoad)
            GenerateTerrainData();

    }

    public void GenerateTerrainData () {

        threadLocked = true;
        new Thread(new ThreadStart(GTD)).Start();

    }

    private void GTD () {

        for (int x = 0; x < Data.ChunkWidth; x++) {
            for (int z = 0; z < Data.ChunkWidth; z++) {
                for (int y = 0; y < Data.ChunkHeight; y++) {

                    voxelMap[x, y, z].id = world.GetVoxel(new Vector3Int(x, y, z) + new Vector3Int(pos.x, 0, pos.y) * Data.ChunkWidth);

                }
            }
        }

        isTerrainMapGenerated = true;
        threadLocked = false;

    }

    public bool IsActive {

        get { return _isActive; }
        set {

            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);

        }
    }

    public void UpdateChunkInThread () {

        threadLocked = true;
        new Thread(new ThreadStart(UpdateChunk)).Start();

    }

    private void UpdateChunk () {

        while (modifications.Count > 0) {

            VoxelMod vmod = modifications.Dequeue();
            Vector3Int pos = vmod.position.v;
            voxelMap[pos.x, pos.y, pos.z].id = vmod.id;

        }

        lock (world.chunksToDraw) {
            world.chunksToDraw.Enqueue(this);
        }

        threadLocked = false;

    }

    public bool IsEditable () {

        return isTerrainMapGenerated && !threadLocked;

    }

    public void GenerateMesh () {

        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        for (int x = 0; x < Data.ChunkWidth; x++) {
            for (int y = 0; y < Data.ChunkHeight; y++) {
                for (int z = 0; z < Data.ChunkWidth; z++) {

                    if (voxelMap[x, y, z].id != 0) {

                        for (int p = 0; p < 6; p++) {

                            Vector3Int pos = Data.faceChecks[p] + new Vector3Int(x, y, z);

                            if (!( Data.IsInChunk(pos) && world.blockTypes[voxelMap[pos.x, pos.y, pos.z].id].isSolid )) {

                                for (int i = 0; i < 4; i++) {

                                    vertices.Add(Data.voxelVerts[Data.voxelTris[p, i]] + new Vector3(x, y, z));
                                    uvs.Add(( Data.voxelUVs[i] + Data.TexturePos(world.blockTypes[voxelMap[x, y, z].id].GetTextureID(p)) ) / Data.TextureSize);

                                }

                                for (int i = 0; i < 6; i++) {

                                    triangles.Add(Data.order[i] + vertexIndex);

                                }

                                vertexIndex += 4;

                            }
                        }
                    }
                }
            }
        }

        Mesh mesh = new() {

            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray()

        };

        mesh.RecalculateNormals();
        Object.Destroy(meshFilter.sharedMesh);
        meshFilter.sharedMesh = mesh;

    }
}
