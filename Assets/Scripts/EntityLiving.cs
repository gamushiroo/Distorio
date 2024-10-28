using UnityEngine;

public class EntityLiving : Entity {

    private readonly float maxHealth;
    private protected float health;

    public EntityLiving (World world) : base(world) {

        maxHealth = 20;
        health = maxHealth;

    }

    private protected void AddHealth (float value) {

        health += value;
    }
    private protected override void Update () {
        base.Update();
        if (health <= 0) {
            Die();
        }
    }
}
