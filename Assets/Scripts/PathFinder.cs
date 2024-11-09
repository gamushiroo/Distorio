using System.Collections.Generic;
using UnityEngine;

public static class PathFinder {
    private static readonly int MaxSteps = 5000;
    private static readonly float[] SqrtValues = new float[3] { 1, Mathf.Sqrt(2), Mathf.Sqrt(3) };
    private static readonly KeyValuePair<Vector3Int, Cell> Initialize = new(Vector3Int.zero, new(Vector3Int.zero, Mathf.Infinity, 0));
    public static List<Vector3Int> FindPath (Vector3Int start, Vector3Int end, World world) {
        Vector3Int localEnd = end - start;
        Dictionary<Vector3Int, Cell> open = new();
        Dictionary<Vector3Int, Cell> closed = new();
        open.Add(Vector3Int.zero, new(null, 0, 0));
        for (int q = 0; q < MaxSteps; q++) {
            KeyValuePair<Vector3Int, Cell> current = Initialize;
            foreach (KeyValuePair<Vector3Int, Cell> entry in open) {
                if (entry.Value.F < current.Value.F) {
                    current = entry;
                }
            }
            open.Remove(current.Key);
            closed.Add(current.Key, current.Value);
            if (current.Key == localEnd) {
                List<Vector3Int> path = new();
                Vector3Int? p = localEnd;
                while (p != null) {
                    path.Add((Vector3Int)p + start);
                    p = closed[(Vector3Int)p].parent;
                }
                return path;
            }
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    for (int z = -1; z < 2; z++) {
                        Vector3Int neighbour = new Vector3Int(x, y, z) + current.Key;
                        Vector3Int p = neighbour + start;
                        if (!closed.ContainsKey(neighbour) && world.GetVoxelID(p + Vector3Int.down) != 0 && world.GetVoxelID(p) == 0) {
                            float G = SqrtValues[new Vector3Int(x, y, z).sqrMagnitude - 1] + current.Value.G;
                            Cell bar = new(current.Key, G, (localEnd - neighbour).magnitude);
                            if (!open.TryAdd(neighbour, bar) && G < open[neighbour].G) {
                                open[neighbour] = bar;
                            }
                        }
                    }
                }
            }
            if (open.Count == 0) {
                return new();
            }
        }
        return new();
    }
}
public struct Cell {
    public Vector3Int? parent;
    public float G;
    public float H;
    public readonly float F => G + H;
    public Cell (Vector3Int? parent, float G, float H) {
        this.parent = parent;
        this.G = G;
        this.H = H;
    }
}