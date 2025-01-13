using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class Chunks {
    private static readonly Dictionary<Vector2Int, Chunk> chunks = new();
    private static readonly Queue<Queue<VoxelAndPos>> modifications = new();
    private static readonly List<Vector2Int> ChunksToDraw = new();
    private static readonly List<Vector2Int> chunksToUpdate = new();
    public static void Update () {
        ModifyChunks();
        UpdateChunks();
        DrawChunks();
    }
    public static void LoadChunksAround (int _x, int _y) {
        foreach (Chunk c in chunks.Values) {
            c.SetActiveState(false);
        }
        for (int x = _x - MyResources.CRange; x < _x + MyResources.CRange; x++) {
            for (int y = _y - MyResources.CRange; y < _y + MyResources.CRange; y++) {
                Vector2Int pos = new(x, y);
                if (!chunks.ContainsKey(pos)) {
                    chunks.Add(pos, new(new(x, 0, y)));
                }
                chunks[pos].SetActiveState(true);
                if (!chunks[pos].IsTerrainMapGenerated) {
                    chunks[pos].GenerateTerrainData();
                    chunksToUpdate.Add(pos);
                }
            }
        }
    }
    public static void DestroyBlock (Vector3 position) {
        ChunkVoxel pos = MyResources.Vector3ToChunkVoxel(position);
        Queue<VoxelAndPos> queue = new();
        queue.Enqueue(new(pos, 0));
        modifications.Enqueue(queue);
    }
    public static void PlantTrees (Vector3Int pos) {
        Vector2 ddd = new(pos.x, pos.z);
        lock (modifications) {
            if (Noise.Get2DPerlin(ddd, 0.0158f) > 0) {
                if (new System.Random().Next(0, Mathf.FloorToInt(Mathf.Max(0, Noise.Get2DPerlin(ddd, 0.052f) + 0.5f) * 32 + 2)) == 0) {
                    Queue<VoxelAndPos> a = new();
                    a.Enqueue(new(MyResources.Vector3ToChunkVoxel(pos + Vector3Int.up), 4));
                    modifications.Enqueue(a);
                }
            }
            if (Noise.Get2DPerlin(ddd, MyResources.treeZoneScale) > MyResources.treeZoneThreshold) {
                if (Noise.Get2DPerlin(ddd, MyResources.treePlacementScale) > MyResources.treePlacementThreshold) {
                    modifications.Enqueue(Structure.MakeTree(pos + Vector3Int.up));
                }
            }
        }
    }
    public static List<int> GetCollidingBlockIDs (AABB aabb) {
        List<int> a = new();
        for (int x = (int)Math.Floor(aabb.minX); x < (int)Math.Ceiling(aabb.maxX); x++) {
            for (int y = (int)Math.Floor(aabb.minY); y < (int)Math.Ceiling(aabb.maxY); y++) {
                for (int z = (int)Math.Floor(aabb.minZ); z < (int)Math.Ceiling(aabb.maxZ); z++) {
                    a.Add(GetVoxelID(new(x, y, z)));
                }
            }
        }
        return a;
    }
    public static List<AABB> GetCollidingBlocks (AABB aabb) {
        List<AABB> a = new();
        for (int x = (int)Math.Floor(aabb.minX); x < (int)Math.Ceiling(aabb.maxX); x++) {
            for (int y = (int)Math.Floor(aabb.minY); y < (int)Math.Ceiling(aabb.maxY); y++) {
                for (int z = (int)Math.Floor(aabb.minZ); z < (int)Math.Ceiling(aabb.maxZ); z++) {
                    if (MyResources.blockTypes[GetVoxelID(new(x, y, z))].hasCollision) {
                        a.Add(new(x, y, z, x + 1, y + 1, z + 1));
                    }
                }
            }
        }
        return a;
    }
    public static int GetVoxelID (Vector3 position) {
        ChunkVoxel pos = MyResources.Vector3ToChunkVoxel(position);
        return chunks.ContainsKey(pos.c) ? chunks[pos.c].GetVoxelIDChunk(pos.v) : 0;
    }
    public static bool SetBlock (Vector3 position, Vector3 selectingPos, byte itemID) {
        if (itemID != 0 && !MyResources.blockTypes[GetVoxelID(position)].hasCollision && MyResources.blockTypes[GetVoxelID(selectingPos)].hasCollision) {
            Queue<VoxelAndPos> queue = new();
            queue.Enqueue(new(MyResources.Vector3ToChunkVoxel(position), itemID));
            modifications.Enqueue(queue);
            return true;
        }
        return false;
    }
    private static void ModifyChunks () {
        lock (modifications) {
            while (modifications.Any()) {
                Queue<VoxelAndPos> queue = modifications.Dequeue();
                while (queue.Any()) {
                    VoxelAndPos vmod = queue.Dequeue();
                    if (!chunks.ContainsKey(vmod.pos.c)) {
                        chunks.Add(vmod.pos.c, new(new(vmod.pos.c.x, 0, vmod.pos.c.y)));
                    }
                    chunks[vmod.pos.c].EnqueueVoxelMod(vmod);
                    if (!chunksToUpdate.Contains(vmod.pos.c)) {
                        chunksToUpdate.Add(vmod.pos.c);
                    }
                }
            }
        }
    }
    private static void UpdateChunks () {
        for (int i = chunksToUpdate.Count - 1; i >= 0; i--) {
            if (chunks[chunksToUpdate[i]].IsEditable) {
                chunks[chunksToUpdate[i]].UpdateChunk();
                ChunksToDraw.Add(chunksToUpdate[i]);
                for (int p = 0; p < 4; p++) {
                    Vector2Int fffpos = chunksToUpdate[i] + MyResources.chunkCheck[p];
                    if (chunks.ContainsKey(fffpos)) {
                        ChunksToDraw.Add(fffpos);
                    }
                }
                chunksToUpdate.RemoveAt(i);
            }
        }
    }
    private static void DrawChunks () {
        for (int i = ChunksToDraw.Count - 1; i >= 0; i--) {
            if (chunks[ChunksToDraw[i]].IsEditable) {
                chunks[ChunksToDraw[i]].GenerateMesh();
                ChunksToDraw.RemoveAt(i);
            }
        }
    }
}