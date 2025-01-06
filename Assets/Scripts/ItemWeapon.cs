using UnityEngine;

public class ItemWeapon : Item {

    private float gunCoolDown = 0;

    float projectiles = 35;
    float defSpread = 10;
    float initialVelocity = 170;


    public ItemWeapon () {

    }

    public override void LeftMouseButton (World world, Entity playerIn) {

        // float eee = Mathf.Pow(playerIn.currentFov / playerIn.fovDef, 5) / 2 * Mathf.Deg2Rad;
        float spread = defSpread / 2 * Mathf.Deg2Rad;
        Vector3 playerCamPos = playerIn.GetCamPos();
        Quaternion playerRot = playerIn.GetRotation();

        if (gunCoolDown >= 0.5F) {
            for (int i = 0; i < projectiles; i++) {
                float rand1 = Random.Range(-180, 180) * Mathf.Deg2Rad;
                float rand2 = Random.Range(-spread, spread);
                Vector3 ttt = new Vector3(Mathf.Cos(rand1) * Mathf.Sin(rand2), Mathf.Sin(rand1) * Mathf.Sin(rand2), Mathf.Cos(rand2)) * initialVelocity;
                world.entityQueue.Enqueue(new EntityProjectile(playerCamPos.x, playerCamPos.y, playerCamPos.z, playerRot * ttt, world));
            }
            playerIn.audioSource.PlayOneShot(world.dd, 0.5F);
            gunCoolDown = 0;
        }
    }
    public override void RightMouseButton (World worldIn, EntityPlayer playerIn) {
        playerIn.fovTarget = 60;
    }
    public override void Update () {
        gunCoolDown += Time.deltaTime;
    }
}