using System.Collections.Generic;
using UnityEngine;

public static class PathFinder {

    public static Queue<VoxelAndPos> FindPath (Vector3Int _start, Vector3Int _goal, World world, bool isGrounded) {

        Dictionary<Vector3Int, Node> open = new();
        Dictionary<Vector3Int, Node> closed = new();
        Queue<VoxelAndPos> values = new Queue<VoxelAndPos>();

        Vector3Int goalKey = _goal - _start;
        open.Add(Vector3Int.zero, new(0, goalKey.sqrMagnitude, goalKey.sqrMagnitude, Vector3Int.zero));



        KeyValuePair<Vector3Int, Node> init = new(Vector3Int.zero, new(Mathf.Infinity, 0, Mathf.Infinity, Vector3Int.zero));


        for (int q = 0; q < 50000; q++) {

            KeyValuePair<Vector3Int, Node> current = init;
            foreach (KeyValuePair<Vector3Int, Node> entry in open) {
                if (entry.Value.F < current.Value.F) {
                    current = entry;
                }
            }
            open.Remove(current.Key);
            closed.Add(current.Key, current.Value);

            if (current.Key == goalKey) {
                Vector3Int search = goalKey;

                while (search != Vector3Int.zero) {

                    values.Enqueue(new(Data.Vector3ToChunkVoxel(search + _start), 20));

                    search = closed[search].pointer;

                }
                return values;
            }


            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    for (int z = -1; z < 2; z++) {


                        Vector3Int neighbourKey = current.Key + new Vector3Int(x, y, z);

                        int thing = world.GetVoxelID(neighbourKey + _start);
                        int grounded = world.GetVoxelID(neighbourKey + _start + Vector3Int.down);


                        if (closed.ContainsKey(neighbourKey) || isGrounded && grounded == 0 || thing != 0) {
                            continue;
                        }

                        float G = current.Value.G + new Vector3Int(x, y, z).sqrMagnitude;
                        float H = (goalKey - neighbourKey).sqrMagnitude;
                        float F = G + H;



                        if (open.ContainsKey(neighbourKey)) {

                            if (G < open[neighbourKey].G) {

                                open[neighbourKey] = new(G, H, F, current.Key);

                            }

                        } else {

                            open.Add(neighbourKey, new(G, H, F, current.Key));

                        }
                    }
                }
            }
        }

        return values;

    }
}
