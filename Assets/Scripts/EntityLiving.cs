using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityLiving : Entity{



    public EntityLiving () {

        this.setPosition(this.locX, this.locY, this.locZ);

    }
}
