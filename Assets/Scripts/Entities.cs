using System.Collections.Generic;
using System.Linq;
public static class Entities {
    private static readonly Queue<Entity> entityQueue = new();
    private static readonly List<Entity> entities = new();
    public static void Update () {
        while (entityQueue.Any()) {
            entities.Add(entityQueue.Dequeue());
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
        entityQueue.Enqueue(entity);
    }
}