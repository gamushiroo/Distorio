using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Misile : EntityLiving {
    public Misile (double posX, double posY, double posZ, Vector3 vel, World world) : base(world) {
        UseCollision = false;
        AddForce_Impulse(vel.x, vel.y, vel.z);
        SetPosition(posX, posY, posZ);
    }
    private protected override void Initialize () {
        defaultWidth = 0.8F;
        defaultHeight = 0.8F;
        gravityMultiplier = 0;
        maxHealth = 10;
        meshType = 20;
        base.Initialize();
    }
    public override void Update () {
        Vector3 n = -Vec3.ToVector3(velocityX, velocityY, velocityZ) * 7;
        AddForce(0, 0, 100);
        base.Update();
        AddHealth(-Time.deltaTime);
    }
}
