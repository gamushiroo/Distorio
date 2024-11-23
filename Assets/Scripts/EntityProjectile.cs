using UnityEngine;

public class EntityProjectile : EntityLiving {

    public EntityProjectile (double posX, double posY, double posZ, Vector3 vel, World world) : base(world) {
        UseCollision = false;
        SetPosition(posX, posY, posZ);
        AddImpulseForce(vel.x, vel.y, vel.z);
    }
    private protected override void Initialize () {
        defaultWidth = 0.1F;
        defaultHeight = 0.1F;
        gravityMultiplier = 0;
        maxHealth = 1;
        base.Initialize();
        GenerateMesh(20);
    }
    private protected override void OnCollision () {
        Kill();
    }
    public override void Update () {
        Vector3 n = -Vec3.ToVector3(velocityX, velocityY, velocityZ) * 7;
        AddForce(n.x, n.y, n.z);
        base.Update();
        width = defaultWidth * health;
        height = defaultHeight * health;
        transform.localScale = Vec3.ToVector3(width / defaultWidth, height / defaultHeight, width / defaultWidth);
        AddHealth(-Time.deltaTime * 1.5F);
    }
}
