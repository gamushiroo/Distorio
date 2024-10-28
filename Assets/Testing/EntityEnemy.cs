using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EntityEnemy : EntityLiving {



    float rotationY;

    AudioSource a;

    public EntityEnemy (World world) : base(world) {

        width = 4F;
        height = 4F;
        GenerateMesh(19);

        Vector3 pos = world.GetSpawnPoint() + Vector3.up * 12;
        SetPosition(pos.x, pos.y, pos.z);
        isZeroGravity = true;

        a = this.playerObject.AddComponent<AudioSource>();
        a.volume = 0.2f;
    }

    private protected override void Update () {

        if(Input.GetKey(KeyCode.J)) {
            rotationY += 120 * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.L)) {
            rotationY -= 120 * Time.deltaTime;
        }
        base.Update();
        if (Input.GetKeyDown(KeyCode.I)) {
            a.PlayOneShot(world.gunSound);
            world.entities.Add(new EntityProjectile(world, new Vector3((float)posX, (float)posY, (float)posZ), Quaternion.Euler(0, rotationY, 0)));


        }

        playerObject.transform.rotation = Quaternion.Euler(0, rotationY, 0);
    }

}
