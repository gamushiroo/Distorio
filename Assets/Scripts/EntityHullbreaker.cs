using UnityEngine;

public class EntityHullbreaker : EntityLiving {


    int amount = 7;
    int current = 0;
    float wait = 0;
    float deg = 0;
    float ttDeg = 0;

    public EntityHullbreaker (World world) : base(world) {
        gravityMultiplier = 0;
        for (int i = 0; i < amount * 2; i++) {
            Entity a = new EntityLiving(world) {
                pearent = this,
                gravityMultiplier = 0
            };
            a.SetSize(2, 2);
            a.items.Add(new ItemMsileShoot());
            child.Add(a);
            world.entityQueue.Enqueue(a);
        }
    }
    private protected override void Initialize () {
        SetSize(8, 8);
        base.Initialize();
        ToWorldSpawn();
        AddPosition(0, 15, 0);
    }
    public override void Update () {


        deg += Time.deltaTime * (Input.GetMouseButton(0) ? 1 : -1) * 5;

        deg = Mathf.Clamp(deg, 0, 7F);

        ttDeg += deg * Time.deltaTime;

        for (int i = 0; i < child.Count; i++) {

            double a = Mathf.Cos(ttDeg * (i >= amount ? -1 : 1) + (360 / amount * i) * Mathf.Deg2Rad) * (i >= amount ? 10 : 6);
            double b = Mathf.Sin(ttDeg * (i >= amount ? -1 : 1) + (360 / amount * i) * Mathf.Deg2Rad) * (i >= amount ? 10 : 6);
            child[i].SetPosition(posX + a, posY + b, posZ + i >= amount ? 15 : 10);

        }
        if (wait > 0.1f  ) {
            if (Input.GetMouseButton(0)) {
                child[-current + amount - 1].items[0].LeftMouseButton(world, child[-current + amount - 1]);
                child[current + amount].items[0].LeftMouseButton(world, child[current + amount]);
                current++;
                if (current >= amount) {
                    current = 0;
                }
                wait = 0;
            }
        }
        base.Update();
        wait += Time.deltaTime;
    }
}