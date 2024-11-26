public class Item {
    protected string name;
    public Item SetName (string name) {
        this.name = name;
        return this;
    }
    public string GetName () {
        return name;
    }
    public static void RegisterItems () {
        RegisterItem(0, new Item().SetName("Stone"));
        RegisterItem(1, new Item().SetName("Dirt"));
        RegisterItem(2, new Item().SetName("Gravel"));
        RegisterItem(3, new Item().SetName("Sand"));
        RegisterItem(256, new ItemSpade().SetName("iron_shovel"));
        RegisterItem(257, new ItemSpade().SetName("iron_shovel"));
        RegisterItem(258, new ItemSpade().SetName("iron_shovel"));
        RegisterItem(259, new ItemSpade().SetName("iron_shovel"));
    }
    public virtual void Update () {
    }
    private static void RegisterItem (int id, Item itemIn) {
        ItemRegistry.Register(id, itemIn);
    }
    public virtual void OnItemLeftClick (World worldIn, EntityPlayer playerIn) {
    }
    public virtual void OnItemRightClick (World worldIn, EntityPlayer playerIn) {
    }
}
