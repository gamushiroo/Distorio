using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack {
    public int stackSize;
    public int id;



    public ItemStack (int itemIn, int amount) {

        this.id = itemIn;
        this.stackSize = amount;

    }



    public int GetItemID () {
        return id;
    }
    public bool IsStackable () {
        return this.GetMaxStackSize() > 1;
    }
    public int GetMaxStackSize () {
        return ItemRegistry.GetItem(GetItemID()).getItemStackLimit();
    }
}
