using System.Collections.Generic;
using UnityEngine;

public static class PathFinder {

    //  Cache the results of calls to Sqrt() for performance
    static readonly float[] sqrtValues = new float[3] { 1, Mathf.Sqrt(2), Mathf.Sqrt(3) };
    static readonly KeyValuePair<Vector3Int, Cell> init = new(Vector3Int.zero, new(Vector3Int.zero, Mathf.Infinity, 0, Mathf.Infinity));

    public static Queue<VoxelAndPos> FindPath (Vector3Int start, Vector3Int end, World world) {

        Queue<VoxelAndPos> values = new();
        Dictionary<Vector3Int, Cell> open = new();
        Dictionary<Vector3Int, Cell> closed = new();
        Vector3Int localEnd = end - start;


        //  Open the first cell
        open.Add(Vector3Int.zero, new(Vector3Int.zero, 0, 0, 0));

        //  not while(true) because it might do a infinite loop xdddd
        for (int q = 0; q < 10000; q++) {

            //  Find the cell with the lowest F from open cells, and set it to current
            KeyValuePair<Vector3Int, Cell> current = init;
            foreach (KeyValuePair<Vector3Int, Cell> entry in open) {
                if (entry.Value.F < current.Value.F) {
                    current = entry;
                }
            }

            //  Close this cell
            open.Remove(current.Key);
            closed.Add(current.Key, current.Value);

            //  Break if this cell is end
            if (current.Key == localEnd) {
                break;
            }

            //  Check the neighbour. 3 by 3.
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    for (int z = -1; z < 2; z++) {

                        Vector3Int currentNeighbour = new Vector3Int(x, y, z) + current.Key;

                        //  Skip this neighbour if is unreachable or is closed
                        if (closed.ContainsKey(currentNeighbour) || !(world.GetVoxelID(currentNeighbour + start + Vector3Int.down) != 0) || world.GetVoxelID(currentNeighbour + start) != 0) {
                            continue;
                        }

                        Vector3Int lll = localEnd - currentNeighbour;
                        float G = current.Value.G + sqrtValues[new Vector3Int(x, y, z).sqrMagnitude - 1];
                        float H = Mathf.Abs(lll.x) + Mathf.Abs(lll.y) + Mathf.Abs(lll.z);
                        Cell _ = new(current.Key, G, H, G + H);

                        if (!open.ContainsKey(currentNeighbour)) {
                            open.Add(currentNeighbour, _);
                        } else if (G < open[currentNeighbour].G) {
                            open[currentNeighbour] = _;
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


        //  Generate some stuffs and return it
        foreach (Vector3Int _pos in closed.Keys) {
            values.Enqueue(new(Data.Vector3ToChunkVoxel(_pos + start), 19));
        }
        foreach (Vector3Int _pos in open.Keys) {
            values.Enqueue(new(Data.Vector3ToChunkVoxel(_pos + start), 18));
        }
        foreach (Vector3Int _pos in path) {
            values.Enqueue(new(Data.Vector3ToChunkVoxel(_pos + start), 20));
        }
        return values;
    }
}

[System.Serializable]
public struct Cell {

    public Vector3Int parent;  //  Parent of this cell
    public float G;  //  Steps from the start node till the current node
    public float H;  //  Heuristic distance from this cell till the end node
    public float F;  //  G + H

    public Cell (Vector3Int parent, float G, float H, float F) {

        this.parent = parent;
        this.G = G;
        this.H = H;
        this.F = F;

    }
}