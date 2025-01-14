public class Block {




    public RenderType GetRenderType () {

        return RenderType.standard;

    }
}

public enum RenderType {

    none = -1,
    standard = 0,
    liquid = 1,


}