public class ItemBlock : Item {


    protected Block block;





    public ItemBlock (Block block) {
        this.block = block;
    }
    public static int getIdFromBlock (Block blockIn) {
        return 0;
        //return blockRegistry.getIDForObject(blockIn);
    }
}
