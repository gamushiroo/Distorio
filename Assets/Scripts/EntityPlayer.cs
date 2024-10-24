using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPlayer : Entity {

    public float rotationX;
    public byte selectedBlockIndex = 0;

    public EntityPlayer (World world, float maxHealth, Vector3 pos) : base(world, maxHealth, pos) {





    }


    public override void AAA () {

        Vector3 a = worldObj.GetSpawnPoint() + new Vector3(0.5f, 0, 0.5f);
        posX = a.x;
        posY = a.y;
        posZ = a.z;
    }
}
