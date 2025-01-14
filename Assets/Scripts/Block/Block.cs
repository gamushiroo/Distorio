public class Block {




    public BlockRenderType GetRenderType () {

        return BlockRenderType.standard;

    }
}

public enum BlockRenderType {

    none = -1,
    standard = 0,
    liquid = 1,


}