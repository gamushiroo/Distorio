using System.Collections.Generic;
using UnityEngine;

public static class PathFinder {

    static readonly float[] sqrtValues = new float[3] { 1, Mathf.Sqrt(2), Mathf.Sqrt(3) };
    static readonly KeyValuePair<Vector3Int, Node> init = new(Vector3Int.zero, new(Mathf.Infinity, 0, Mathf.Infinity, Vector3Int.zero));

    public static Queue<VoxelAndPos> FindPath (Vector3Int _start, Vector3Int _goal, World world, bool isGrounded) {
        Queue<VoxelAndPos> values = new();
        Dictionary<Vector3Int, Node> open = new();
        Dictionary<Vector3Int, Node> closed = new();
        Vector3Int localGoal = _goal - _start;
        open.Add(Vector3Int.zero, new(0, 0, 0, Vector3Int.zero));
        for (int q = 0; q < 10000; q++) {
            KeyValuePair<Vector3Int, Node> current = init;
            foreach (KeyValuePair<Vector3Int, Node> entry in open) {
                if (entry.Value.F < current.Value.F) {
                    current = entry;
                }
            }
            open.Remove(current.Key);
            closed.Add(current.Key, current.Value);
            if (current.Key == localGoal) { break; }
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    for (int z = -1; z < 2; z++) {
                        Vector3Int neighbourKey = current.Key + new Vector3Int(x, y, z);

                        if (closed.ContainsKey(neighbourKey) || !(!isGrounded || world.GetVoxelID(neighbourKey + _start + Vector3Int.down) != 0) || world.GetVoxelID(neighbourKey + _start) != 0) { continue; }

                        float G = current.Value.G + sqrtValues[new Vector3Int(x, y, z).sqrMagnitude - 1];
                        Vector3Int lll = localGoal - neighbourKey;
                        float H = Mathf.Abs(lll.x) + Mathf.Abs(lll.y) + Mathf.Abs(lll.z);
                        Node _ = new(G, H, G + H, current.Key);

                        if (!open.ContainsKey(neighbourKey)) {
                            open.Add(neighbourKey, _);
                        } else if (G < open[neighbourKey].G) {
                            open[neighbourKey] = _;
                        }
                    }
                }
            }
        }
        foreach (Vector3Int entry in closed.Keys) {
            values.Enqueue(new(Data.Vector3ToChunkVoxel(entry + _start), 19));
        }
        foreach (Vector3Int entry in open.Keys) {
            values.Enqueue(new(Data.Vector3ToChunkVoxel(entry + _start), 18));
        }
        Vector3Int search = localGoal;
        while (search != Vector3Int.zero) {
            values.Enqueue(new(Data.Vector3ToChunkVoxel(search + _start), 20));
            search = closed[search].pointer;
        }
        return values;
    }
}
