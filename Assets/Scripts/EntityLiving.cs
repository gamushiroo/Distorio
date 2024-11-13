using System;
using UnityEngine;

public class EntityLiving : Entity {

    private readonly float maxHealth;
    private protected float health;
    private bool isHealing;
    private float healthBefore;

    public EntityLiving (World world) : base(world) {

        maxHealth = 20;
        health = maxHealth;

    }

    private protected void AddHealth (float value) {

        health += value;
        health = Math.Clamp(health, 0, maxHealth);
    }
    private protected override void Update () {
        base.Update();


        if(health > healthBefore) {
            Debug.Log(Time.deltaTime);
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
