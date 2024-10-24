using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Item {

    protected int maxStackSize = 256;
    protected string name;


    public Item SetName(string name) {

        this.name = name;
        return this;

    }

    public int getItemStackLimit () {
        return this.maxStackSize;
    }

    public Item SetMaxStackSize (int maxStackSize) {
        this.maxStackSize = maxStackSize;
        return this;
    }
    public int GetItemStackLimit () {
        return maxStackSize;
    }
    public static void RegisterItems () {

        RegisterItem(256, new ItemSpade().SetName("iron_shovel"));
        RegisterItem(257, new ItemSpade().SetName("iron_shovel"));
        RegisterItem(258, new ItemSpade().SetName("iron_shovel"));
        RegisterItem(259, new ItemSpade().SetName("iron_shovel"));
    }

    private static void RegisterItem (int id, Item itemIn) {

        ItemRegistry.Register(id, itemIn);

    }
}
