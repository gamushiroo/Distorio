using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block  {

    protected bool fullBlock;
    protected float blockHardness;
    public float slipperiness;

    public bool IsFullBlock () {
        return fullBlock;
    }
}
