using UnityEngine;

public class EntityEnemy : EntityLiving {



    float rotationY;


    public EntityEnemy (World world, Vector3 pos) : base(world) {

        defaultWidth = 4F;
        defaultHeight = 4F;
        GenerateMesh(19);

        SetPosition(pos.x, pos.y, pos.z);
        gravityMultiplier = 0;

    }

    public override void Update () {

        if (Input.GetKey(KeyCode.J)) {
            rotationY += 120 * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.L)) {
            rotationY -= 120 * Time.deltaTime;
        }
        base.Update();
        if (Input.GetKeyDown(KeyCode.I)) {
            //world.entities.Add(new EntityProjectile(world, new Vector3((float)posX, (float)posY, (float)posZ), Quaternion.Euler(0, rotationY, 0)));


        }

        transform.rotation = Quaternion.Euler(0, rotationY, 0);
    }

}
