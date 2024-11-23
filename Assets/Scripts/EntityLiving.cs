using System;
public class EntityLiving : Entity {
    private protected int maxHealth = 20;
    private protected float health;
    private protected bool isHealed;
    private float healthBefore;
    public EntityLiving (World world) : base(world) {
    }
    private protected override void Initialize () {
        base.Initialize();
        health = maxHealth;
    }
    private protected void AddHealth (float value) {
        health += value;
        health = Math.Min(health, maxHealth);
    }
    public override void Update () {
        base.Update();
        isHealed = health > healthBefore;
        healthBefore = health;
        if (health <= 0) {
            Kill();
        }
    }
}