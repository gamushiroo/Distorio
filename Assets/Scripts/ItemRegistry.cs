using Org.BouncyCastle.Security;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemRegistry  {

    private static Dictionary<int, Item> register = new();

    public static void Register (int id, Item itemIn) {

        register.Add(id, itemIn);
    }

    public static Item GetItem (int id) {


        return register[id];

    }
}
