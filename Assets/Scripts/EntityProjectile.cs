using UnityEngine;

public class EntityProjectile : EntityLiving {


    Quaternion rot;
    public EntityProjectile (World world, Vector3 pos, Quaternion rot) : base(world) {

        _width = 0.8F;
        _height = 0.8F;
        GenerateMesh(20);
        Object.Instantiate(world.particle).transform.SetParent(transform);
        SetPosition(pos.x, pos.y, pos.z);
        gravityMultiplier = 0;
        this.rot = rot;
    }

    public override void Update () {
        base.Update();

        Vector3 aaa = rot * Vector3.forward * 20 * Time.deltaTime;
        //MoveEntity(aaa.x, aaa.y, aaa.z);
        AddHealth(-Time.deltaTime);
    }
}
