public class MathHelper {

    public static int FloorDouble (double value) {
        int i = (int)value;
        return value < i ? i - 1 : i;
    }
}
