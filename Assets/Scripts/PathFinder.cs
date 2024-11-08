using System.Collections.Generic;
using UnityEngine;

public static class PathFinder {

    private static readonly int maxSteps = 5000;

    //  Cache the results of calls to Sqrt() for performance
    private static readonly float[] sqrtValues = new float[3] { 1, Mathf.Sqrt(2), Mathf.Sqrt(3) };
    private static readonly KeyValuePair<Vector3Int, Cell> init = new(Vector3Int.zero, new(Vector3Int.zero, Mathf.Infinity, 0, Mathf.Infinity));

    public static List<Vector3Int> FindPath (Vector3Int start, Vector3Int end, World world) {

        Vector3Int localEnd = end - start;
        Dictionary<Vector3Int, Cell> open = new();
        Dictionary<Vector3Int, Cell> closed = new();

        //  Open the first cell
        open.Add(Vector3Int.zero, Cell.zero);

        for (int q = 0; q < maxSteps; q++) {

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

                //  Follow the parent in each cell due to generate the path
                List<Vector3Int> path = new();
                Vector3Int pos = localEnd;
                while (pos != Vector3Int.zero) {
                    path.Add(pos + start);
                    pos = closed[pos].parent;
                }
                return path;
            }

            //  Check the neighbour
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    for (int z = -1; z < 2; z++) {

                        Vector3Int neighbour = new Vector3Int(x, y, z) + current.Key;
                        Vector3Int foo = neighbour + start;

                        //  Skip this neighbour if is closed or is unreachable
                        if (!closed.ContainsKey(neighbour) && world.GetVoxelID(foo + Vector3Int.down) != 0 && world.GetVoxelID(foo) == 0) {

                            //  Open this neighbour if it's not open. If this neighbour is already open, update this neighbour in some case
                            float G = sqrtValues[new Vector3Int(x, y, z).sqrMagnitude - 1] + current.Value.G;
                            Vector3Int a = localEnd - neighbour;
                            int H = Mathf.Abs(a.x) + Mathf.Abs(a.y) + Mathf.Abs(a.z);
                            Cell bar = new(current.Key, G, H, G + H);
                            if (!open.TryAdd(neighbour, bar) && G < open[neighbour].G) {
                                open[neighbour] = bar;
                            }
                        }
                    }
                }
            }

            //  Return if end is unreachable
            if (open.Count == 0) {
                return new();
            }
        }
        return new();
    }
}

public struct Cell {

    public Vector3Int parent;
    public float G;  //  Steps from the start till this cell
    public int H;  //  Heuristic distance from this cell till the end
    public float F;  //  G + H
    public static readonly Cell zero = new(Vector3Int.zero, 0, 0, 0);

    public Cell (Vector3Int parent, float G, int H, float F) {
        this.parent = parent;
        this.G = G;
        this.H = H;
        this.F = F;
    }
}