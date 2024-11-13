public class ItemStack {
    public int stackSize;
    public int id;
    public ItemStack (int itemIn, int amount) {
        id = itemIn;
        stackSize = amount;
    }
    public int GetItemID () {
        return id;
    }
    public bool IsStackable () {
        return GetMaxStackSize() > 1;
    }
    public int GetMaxStackSize () {
        return ItemRegistry.GetItem(GetItemID()).GetItemStackLimit();
    }
}
