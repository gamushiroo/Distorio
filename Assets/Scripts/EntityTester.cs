using UnityEngine;

public class EntityTester : EntityLiving {




    float projectiles = 35;
    float spread = 8;
    float initialVelocity = 170;

    private protected float rotationPitch;
    private protected float rotationYaw;





    public EntityTester (World world) : base(world) {
    }


    public override void Update () {
        base.Update();

        if (Input.GetKeyDown(KeyCode.F)) {
            ShootGun();
        }
    }

    private protected override void Initialize () {
        base.Initialize();
        ToWorldSpawn();
        GenerateMesh(19);
    }
    protected void ShootGun () {



        for (int i = 0; i < projectiles; i++) {
            float rand1 = UnityEngine.Random.Range(-180, 180) * Mathf.Deg2Rad;
            float rand2 = UnityEngine.Random.Range(-spread, spread) / 2 * Mathf.Deg2Rad;
            Vector3 ttt = new Vector3(Mathf.Cos(rand1) * Mathf.Sin(rand2), Mathf.Sin(rand1) * Mathf.Sin(rand2), Mathf.Cos(rand2)) * initialVelocity;
            Vector3 aaa = Quaternion.Euler(rotationPitch, rotationYaw, 0) * ttt;
            world.entityQueue.Enqueue(new EntityProjectile(posX, eyeHeight + posY, posZ, aaa, world));
        }
        audioSource.PlayOneShot(world.dd, 0.5F);
    }
}
