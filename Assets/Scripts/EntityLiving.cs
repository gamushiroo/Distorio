using System;
using UnityEngine;

public class EntityLiving : Entity {

    private protected readonly float maxHealth = 20;
    private protected float health;
    private float healthBefore;

    public EntityLiving (World world) : base(world) {

        health = maxHealth;

    }

    private protected void AddHealth (float value) {

        health += value;
        health = Math.Clamp(health, 0, maxHealth);
    }
    public override void Update () {
        base.Update();


        if (health > healthBefore) {
            world.healing.SetActive(true);
        } else {
            world.healing.SetActive(false);
        }
        healthBefore = health;
        world.healing.transform.position = new((float)posX, (float)posY, (float)posZ);
        if (health == 0) {
            Die();
        }
    }
}
