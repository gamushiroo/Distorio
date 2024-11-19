using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathHelper  {

    public static int FloorDouble (double value) {
        int i = (int)value;
        return value < i ? i - 1 : i;
    }
}
