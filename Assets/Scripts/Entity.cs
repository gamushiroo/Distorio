using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Entity {

    private static readonly AxisAlignedBB ZERO_AABB = new(0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d);

    private static int nextEntityID;
    private readonly int entityID;

    public MeshFilter meshFilter;
    bool isCollidedVertically;
    public bool isDead;

    public double posX;
    public double posY;
    public double posZ;

    public float width;
    public float height;

    private AxisAlignedBB boundingBox;

    private float health;
    private float rotationY;
    private float gravityVelocity;
    private bool onGround;
    protected private Vector3 velocity;
    protected readonly World worldObj;
    private readonly float maxHealth;
    private readonly GameObject obj;
    private AudioSource audioSource;

    public Entity ( World worldObj, float maxHealth, Vector3 pos) {


        isDead = false;
        entityID = nextEntityID++;
        boundingBox = ZERO_AABB;

        width = 0.6f;
        height = 1.8f;

        SetPosition(pos.x, pos.y, pos.z);

        obj = new();
        obj.AddComponent<MeshRenderer>().material = worldObj.material;
        meshFilter = obj.AddComponent<MeshFilter>();
        audioSource = obj.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1;
        audioSource.maxDistance = 16;
        audioSource.volume = 0.05f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.dopplerLevel = 0;
        this.worldObj = worldObj;
        this.maxHealth = maxHealth;
        GenerateMesh(10);
        EntityInit();
    }


    protected void SetSize (float width, float height) {
        if (width != this.width || height != this.height) {
            this.width = width;
            this.height = height;

            float f = width / 2.0F;
            SetEntityBoundingBox(new(f, 0, f, f, height, f));
            GenerateMesh(10);
        }
    }

    public AxisAlignedBB GetEntityBoundingBox () {
        return boundingBox;
    }
    protected void SetPosition (double x, double y, double z) {
        posX = x;
        posY = y;
        posZ = z;
        float f = width / 2.0F;
        SetEntityBoundingBox(new(x - f, y, z - f, x + f, y + height, z + f));
    }
    private void SetEntityBoundingBox (AxisAlignedBB BB) {
        boundingBox = BB;
    }
    public int GetEntityId () {
        return this.entityID;
    }

    public void AddHealth (float value) {

        if (!isDead) {
            health += value;
        }
    }
    public float Rotation => rotationY;
    public float Health => health;
    public void AddRotation (float rot) {
        rotationY += rot;
    }

    public void Die () {
        isDead = true;
        obj.SetActive(false);
    }
    public bool IsCollide (Vector3Int selectedPos) {



        return AABB.ABCheck(new WWWEe(new((float)posX, (float)posY, (float)posZ), new(width, height, width), velocity, onGround), selectedPos + Vector3.one * 0.5f);
    }

    public virtual void AAA () {

    }
    public virtual void BBB () {

    }

    public void EntityInit () {

        gravityVelocity = 0;
        onGround = false;

        AAA();


        health = maxHealth;
        velocity = Vector3.zero;
        Apply();
    }
    public void AddVel (Vector3 vel, bool jump) {
        velocity += Quaternion.Euler(0, rotationY, 0) * vel * Data.playerSpeed * Time.deltaTime;
        if (jump && onGround) {
            gravityVelocity -= 10;
        }
    }

    private void ResetPositionToBB () {
        posX = (GetEntityBoundingBox().minX + GetEntityBoundingBox().maxX) / 2.0D;
        posY = GetEntityBoundingBox().minY;
        posZ = (GetEntityBoundingBox().minZ + GetEntityBoundingBox().maxZ) / 2.0D;
    }

    private void MoveEntity (double x, double y, double z) {


        double d0 = posX;
        double d1 = posY;
        double d2 = posZ;

        double d3 = x;
        double d4 = y;
        double d5 = z;

        List<AxisAlignedBB> list1 = worldObj.Ajj(GetEntityBoundingBox().AddCoord(x, y, z));
        foreach (AxisAlignedBB axisalignedbb1 in list1) {
            y = axisalignedbb1.CalculateYOffset(GetEntityBoundingBox(), y);
        }
        SetEntityBoundingBox(GetEntityBoundingBox().Offset(0.0D, y, 0.0D));
        foreach (AxisAlignedBB axisalignedbb2 in list1) {
            x = axisalignedbb2.CalculateXOffset(GetEntityBoundingBox(), x);
        }
        SetEntityBoundingBox(GetEntityBoundingBox().Offset(x, 0.0D, 0.0D));
        foreach (AxisAlignedBB axisalignedbb13 in list1) {
            z = axisalignedbb13.CalculateZOffset(GetEntityBoundingBox(), z);
        }
        SetEntityBoundingBox(GetEntityBoundingBox().Offset(0.0D, 0.0D, z));

        ResetPositionToBB();

        //Debug.Log(new Vector3((float)x, (float)y, (float)z));


        isCollidedVertically = d4 != y;
        onGround = isCollidedVertically && d4 < 0.0D;
        if( isCollidedVertically && d4 > 0.0D) {
            gravityVelocity = 0;
        }





        //WWWEe entity = AABB.PosUpdate(new(new((float)posX, (float)posY, (float)posZ), new(width, height, width), new((float)x, (float)y, (float)z), false), worldObj);



    }



    public void Apply () {


        if (!isDead) {
            health += Time.deltaTime;
            gravityVelocity += 27 * Time.deltaTime;
            velocity += gravityVelocity * Time.deltaTime * Vector3.down;

            BBB();

            MoveEntity(velocity.x, velocity.y, velocity.z);


            if (onGround) {
                if (gravityVelocity > 15) {
                    AddHealth(-Mathf.Pow(gravityVelocity, 2) / 13.5f);
                }
                gravityVelocity = 0;
            }
            health = Mathf.Min(health, maxHealth);
            if (health <= 0) {
                isDead = true;
            }

            obj.transform.position = new((float)posX, (float)posY, (float)posZ);
            obj.transform.rotation = Quaternion.Euler(0, rotationY, 0);


            velocity = Vector3.zero;
        }
    }

    void GenerateMesh (byte skin) {

        int vertexIndex = 0;
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();
        for (int p = 0; p < 6; p++) {
            for (int i = 0; i < 4; i++) {

                Vector3 a = Data.voxelVerts[Data.blockMesh[p, i]];


                a.x *= width;
                a.y *= height;
                a.z *= width;

                a -= new Vector3(width, 0, width) / 2;

                vertices.Add(a);
                uvs.Add((Data.voxelUVs[i] + Data.TexturePos(worldObj.blockTypes[skin].GetTextureID(p))) / Data.TextureSize);
            }
            for (int i = 0; i < 6; i++)
                triangles.Add(Data.order[i] + vertexIndex);
            vertexIndex += 4;
        }
        Mesh mesh = new() {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray()
        };
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;
    }
}
