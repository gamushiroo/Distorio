using UnityEngine;

public class EntityLiving : Entity {

    private float maxHealth;
    public float health;
    private bool isDead;

    public EntityLiving (World world) : base(world) {

        maxHealth = 20;
        health = maxHealth;


    }
    public void AddHealth (float value) {

        health += value;
    }

    public override void Update () {
        if (!isDead) {
            base.Update();
            if (health <= 0) {
                Die();
            }
        }
    }
    public void Die () {
        isDead = true;
        world.player.isDead = true;
        obj.SetActive(false);
    }
    protected override void OnLanded () {
        double i = Mathf.Pow((float)gravityVelocity, 2) / (2 * Data.gravityScale) - 4;
        AddHealth(-Mathf.Max((float)i, 0));
    }
}
