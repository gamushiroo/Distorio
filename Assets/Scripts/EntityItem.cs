
using UnityEngine;

public class EntityItem : Entity{

    public EntityItem (World world) : base ( world) {

        SetSize(0.25f, 0.25f);

    }

    private protected override void Update () {
        /*
        float radius = 3;
        double a = world.player.entity.posX - posX;
        double b = world.player.entity.posY - posY;
        double c = world.player.entity.posZ - posZ;
        Vector3 distiance = new((float)a, (float)b, (float)c);
        if (distiance.magnitude < radius) {
            velocity += 6f * Time.deltaTime * distiance.normalized;
        }
        if (distiance.magnitude < 0.5f) {
        }
        base.Update();
        */
    }
}
