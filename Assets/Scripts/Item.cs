public class Item {

    protected int maxStackSize = 512;
    protected string name;


    public Item SetName (string name) {
        this.name = name;
        return this;
    }
    public string GetName () {
        return name;
    }

    public Item SetMaxStackSize (int maxStackSize) {
        this.maxStackSize = maxStackSize;
        return this;
    }
    public int GetItemStackLimit () {
        return maxStackSize;
    }
    public static void RegisterItems () {

        RegisterItem(0, new Item().SetName("Stone").SetMaxStackSize(64));
        RegisterItem(1, new Item().SetName("Dirt").SetMaxStackSize(64));
        RegisterItem(2, new Item().SetName("Gravel").SetMaxStackSize(64));
        RegisterItem(3, new Item().SetName("Sand").SetMaxStackSize(64));

        RegisterItem(256, new ItemSpade().SetName("iron_shovel"));
        RegisterItem(257, new ItemSpade().SetName("iron_shovel"));
        RegisterItem(258, new ItemSpade().SetName("iron_shovel"));
        RegisterItem(259, new ItemSpade().SetName("iron_shovel"));
    }

    private static void RegisterItem (int id, Item itemIn) {

        ItemRegistry.Register(id, itemIn);

    }
}
