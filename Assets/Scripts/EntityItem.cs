
using UnityEngine;

public class EntityItem : Entity{

    public EntityItem (World world, float maxHealth) : base ( world,  maxHealth) {

        SetSize(0.25f, 0.25f);

    }

    public override void BBB () {
        float radius = 3;

        double a = worldObj.player.entity.posX - posX;
        double b = worldObj.player.entity.posY - posY;
        double c = worldObj.player.entity.posZ - posZ;

        Vector3 distiance = new((float)a, (float)b, (float)c);

        if (distiance.magnitude < radius) {
            velocity = distiance.normalized * Time.deltaTime * 6f;

        }
        if (distiance.magnitude < 0.5f) {
            Die();
        }
    }
}
