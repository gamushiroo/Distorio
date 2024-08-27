using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity {

    private static int entityCount;
    private int id;

    public double locX;
    public double locY;
    public double locZ;

    public float height;
    public float width;
    public float length;

    public bool dead;

    public int getId () {
        return this.id;
    }

    public void d (int i) {
        this.id = i;
    }

    public Entity () {
        this.id = entityCount++;
        this.width = 0.6F;
        this.length = 1.8F;

    }


    public void setPosition (double locX, double locY, double locZ) {

        this.locX = locX;
        this.locY = locY;
        this.locZ = locZ;

    }


    public void die () {
        this.dead = true;
    }

    public bool isAlive () {
        return !this.dead;
    }
}
