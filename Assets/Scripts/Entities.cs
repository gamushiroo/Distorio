using System.Collections.Generic;
using System.Linq;
public static class Entities {
    private static readonly Queue<Entity> entitiesToSummon = new();
    private static readonly List<Entity> entities = new();
    public static void Update () {
        while (entitiesToSummon.Any()) {
            entities.Add(entitiesToSummon.Dequeue());
        }
        for (int i = entities.Count - 1; i >= 0; i--) {
            if (entities[i].IsAlive) {
                entities[i].Update();
            } else {
                entities.RemoveAt(i);
            }
        }
    }
    public static void Add (Entity entity) {
        entitiesToSummon.Enqueue(entity);
    }
}