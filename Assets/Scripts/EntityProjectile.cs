using UnityEngine;

public class EntityProjectile : EntityLiving {


    Quaternion rot;
    public EntityProjectile (World world, Vector3 pos, Quaternion rot) : base(world) {

        width = 0.8F;
        height = 0.8F;
        GenerateMesh(20);
        Object.Instantiate(world.particle).transform.SetParent(obj.transform);
        SetPosition(pos.x, pos.y, pos.z);
        hasGravity = false;
        this.rot = rot;
    }

    private protected override void Update () {
        base.Update();

        Vector3 aaa = rot * Vector3.forward * 20 * Time.deltaTime;
        MoveEntity(aaa.x, aaa.y, aaa.z);
        AddHealth(-Time.deltaTime);
    }
}
