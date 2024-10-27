using UnityEngine;

public class EntityLiving : Entity {

    private readonly float maxHealth;
    public float health;

    public EntityLiving (World world) : base(world) {

        maxHealth = 20;
        health = maxHealth;


    }
    public void AddHealth (float value) {

        health += value;
    }


    private protected override void Update () {
        base.Update();
        if (health <= 0) {
            Die();
        }
    }
    private protected override void OnLanded () {
        double i = Mathf.Pow((float)gravityVelocity, 2) / (2 * Data.gravityScale) - 4;
        AddHealth(-Mathf.Max((float)i, 0));
    }
}
