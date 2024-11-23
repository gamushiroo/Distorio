using UnityEngine;

public class EntityProjectile : EntityLiving {

    public EntityProjectile (double posX, double posY, double posZ, Vector3 vel, World world) : base(world) {
        UseCollision = false;
        SetPosition(posX, posY, posZ);
        AddVelocity(vel.x, vel.y, vel.z);
    }
    private protected override void Initialize () {
        defaultWidth = 0.1F;
        defaultheight = 0.1F;
        gravityMultiplier = 0;
        maxHealth = 1;
        base.Initialize();
        GenerateMesh(20);
    }
    private protected override void OnCollision () {
        Die();
    }
    public override void UpdateEntity () {
        Vector3 n = -Vec3.ToVector3(velocityX, velocityY, velocityZ) * 0.05F;
        AddVelocity(n.x, n.y, n.z);
        base.UpdateEntity();
        AddHealth(-Time.deltaTime * 2);
    }
}
