using System.Collections.Generic;
using UnityEngine;

public static class PathFinder {

    //  Cache the results of calls to Sqrt() for performance
    private static readonly float[] sqrtValues = new float[3] { 1, Mathf.Sqrt(2), Mathf.Sqrt(3) };
    private static readonly KeyValuePair<Vector3Int, Cell> init = new(Vector3Int.zero, new(Vector3Int.zero, Mathf.Infinity, 0, Mathf.Infinity));

    public static Queue<VoxelAndPos> FindPath (Vector3Int start, Vector3Int end, World world) {

        Vector3Int localEnd = end - start;
        Dictionary<Vector3Int, Cell> open = new();
        Dictionary<Vector3Int, Cell> closed = new();

        //  Open the first cell
        open.Add(Vector3Int.zero, new(Vector3Int.zero, 0, 0, 0));

        for (int q = 0; q < 10000; q++) {

            //  Find the cell with the lowest F from open cells
            KeyValuePair<Vector3Int, Cell> current = init;
            foreach (KeyValuePair<Vector3Int, Cell> entry in open) {
                if (entry.Value.F < current.Value.F) {
                    current = entry;
                }
            }

            //  Close this cell
            open.Remove(current.Key);
            closed.Add(current.Key, current.Value);

            if (current.Key == localEnd) {
                break;
            }

            //  Check the neighbour
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    for (int z = -1; z < 2; z++) {

                        Vector3Int currentNeighbour = new Vector3Int(x, y, z) + current.Key;

                        //  Skip this if is closed or is unreachable
                        if (closed.ContainsKey(currentNeighbour) || world.GetVoxelID(currentNeighbour + start + Vector3Int.down) == 0 || world.GetVoxelID(currentNeighbour + start) != 0) {
                            continue;
                        }

                        float G = sqrtValues[new Vector3Int(x, y, z).sqrMagnitude - 1] + current.Value.G;
                        Vector3Int a = localEnd - currentNeighbour;
                        int H = Mathf.Abs(a.x) + Mathf.Abs(a.y) + Mathf.Abs(a.z);

                        if (!open.ContainsKey(currentNeighbour)) {
                            open.Add(currentNeighbour, new Cell(current.Key, G, H, G + H));
                        } else if (G < open[currentNeighbour].G) {
                            open[currentNeighbour] = new Cell(current.Key, G, H, G + H);
                        }
                    }
                }
            }
        }

        //  Follow the parent in each cell due to generate the path
        List<Vector3Int> path = new();
        Vector3Int pos = localEnd;
        while (pos != Vector3Int.zero) {
            path.Add(pos);
            pos = closed[pos].parent;
        }

        //  Generate stuff
        Queue<VoxelAndPos> value = new();
        foreach (Vector3Int _pos in closed.Keys) {
            value.Enqueue(new(Data.Vector3ToChunkVoxel(_pos + start), 19));
        }
        foreach (Vector3Int _pos in open.Keys) {
            value.Enqueue(new(Data.Vector3ToChunkVoxel(_pos + start), 18));
        }
        foreach (Vector3Int _pos in path) {
            value.Enqueue(new(Data.Vector3ToChunkVoxel(_pos + start), 20));
        }
        return value;
    }
}

[System.Serializable]
public struct Cell {

    public Vector3Int parent;
    public float G;  //  Steps from the start node till this
    public int H;  //  Heuristic distance from this till the end
    public float F;  //  G + H

    public Cell (Vector3Int parent, float G, int H, float F) {
        this.parent = parent;
        this.G = G;
        this.H = H;
        this.F = F;
    }
}