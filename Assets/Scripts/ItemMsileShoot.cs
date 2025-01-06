using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMsileShoot : Item {

    public override void LeftMouseButton (World world, Entity playerIn) {

        Vector3 playerCamPos = playerIn.GetCamPos();
        Quaternion playerRot = playerIn.GetRotation();

        world.entityQueue.Enqueue(new Misile(playerCamPos.x, playerCamPos.y, playerCamPos.z, Vector3.forward * 100, world));
        playerIn.audioSource.PlayOneShot(world.dd, 0.1F);
    }
}
