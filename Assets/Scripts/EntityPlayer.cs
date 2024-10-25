using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPlayer : Entity {

    public float rotationX;
    public byte selectedBlockIndex = 0;

    public EntityPlayer (World world, float maxHealth) : base(world, maxHealth) {

    }


    public override void AAA () {

        Vector3 a = worldObj.GetSpawnPoint() + new Vector3(0.5f, 1, 0.5f);
        SetPosition(a.x, a.y, a.z);
    }
}
