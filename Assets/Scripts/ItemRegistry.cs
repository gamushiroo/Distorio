using System.Collections.Generic;

public static class ItemRegistry {

    private static Dictionary<int, Item> register = new();

    public static void Register (int id, Item itemIn) {

        register.Add(id, itemIn);
    }

    public static Item GetItem (int id) {


        return register[id];

    }
}
