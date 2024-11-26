using UnityEngine;

public class ItemWeapon : Item {

    private float gunCoolDown = 0;

    float projectiles = 35;
    float defSpread = 10;
    float initialVelocity = 170;


    public ItemWeapon () {

    }

    public override void OnItemLeftClick (World world, EntityPlayer playerIn) {

        float spread = defSpread * Mathf.Pow(playerIn.currentFov / playerIn.fovDef, 5);


        if (gunCoolDown >= 0.5F) {
            for (int i = 0; i < projectiles; i++) {
                float rand1 = UnityEngine.Random.Range(-180, 180) * Mathf.Deg2Rad;
                float rand2 = UnityEngine.Random.Range(-spread, spread) / 2 * Mathf.Deg2Rad;
                Vector3 ttt = new Vector3(Mathf.Cos(rand1) * Mathf.Sin(rand2), Mathf.Sin(rand1) * Mathf.Sin(rand2), Mathf.Cos(rand2)) * initialVelocity;
                Vector3 aaa = playerIn.GetRotation() * ttt;

                Vector3 fff = playerIn.GetCamPos();
                world.entityQueue.Enqueue(new EntityProjectile(fff.x, fff.y, fff.z, aaa, world));
            }
            playerIn.audioSource.PlayOneShot(world.dd, 0.5F);
            gunCoolDown = 0;
        }
    }
    public override void OnItemRightClick (World worldIn, EntityPlayer playerIn) {
        playerIn.fovTarget = 60;
    }
    public override void Update () {
        base.Update();
        gunCoolDown += Time.deltaTime;
    }
}