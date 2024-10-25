using System.Collections.Generic;
using UnityEngine;

public abstract class Entity {

    private static readonly AABB ZERO_AABB = new(0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d);

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

    private AABB boundingBox;

    private float health;
    private float rotationY;
    private float gravityVelocity;
    private bool onGround;
    protected private Vector3 velocity;
    protected readonly World worldObj;
    private readonly float maxHealth;
    private readonly GameObject obj;

    public Entity (World worldIn, float maxHealth) {
        entityID = nextEntityID++;
        boundingBox = ZERO_AABB;
        width = 0.6F;
        height = 1.8F;
        worldObj = worldIn;
        SetPosition(0.0D, 0.0D, 0.0D);



        obj = new();
        obj.AddComponent<MeshRenderer>().material = worldIn.material;
        meshFilter = obj.AddComponent<MeshFilter>();
        GenerateMesh(10);

        this.maxHealth = maxHealth;
        EntityInit();
    }
    public void EntityInit () {
        gravityVelocity = 0;
        velocity = Vector3.zero;
        AAA();
        health = maxHealth;
    }
    public void AddVel (Vector3 vel, bool jump) {
        velocity += Quaternion.Euler(0, rotationY, 0) * vel * Data.playerSpeed * Time.deltaTime;
        if (jump && onGround) {
            gravityVelocity -= 10;
        }
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

    public int GetEntityId () {
        return entityID;
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
        return Data.ABCheck(new WWWEe(new((float)posX, (float)posY, (float)posZ), new(width, height, width), velocity, onGround), selectedPos + Vector3.one * 0.5f);
    }

    public virtual void AAA () {

    }
    public virtual void BBB () {

    }


    protected void SetPosition (double x, double y, double z) {
        posX = x;
        posY = y;
        posZ = z;
        float f = width / 2.0F;
        SetEntityBoundingBox(new(x - f, y, z - f, x + f, y + height, z + f));
    }
    private AABB GetEntityBoundingBox () {
        return boundingBox;
    }
    private void SetEntityBoundingBox (AABB BB) {
        boundingBox = BB;
    }
    private void MoveEntity (double _x, double _y, double _z) {
        double y = _y;
        List<AABB> AABBs = worldObj.Ajj(GetEntityBoundingBox().AddCoord(_x, _y, _z));
        foreach (AABB AABBYs in AABBs) {
            _y = AABBYs.CalculateYOffset(GetEntityBoundingBox(), _y);
        }
        SetEntityBoundingBox(GetEntityBoundingBox().Offset(0.0D, _y, 0.0D));
        foreach (AABB AABBXs in AABBs) {
            _x = AABBXs.CalculateXOffset(GetEntityBoundingBox(), _x);
        }
        SetEntityBoundingBox(GetEntityBoundingBox().Offset(_x, 0.0D, 0.0D));
        foreach (AABB AABBZs in AABBs) {
            _z = AABBZs.CalculateZOffset(GetEntityBoundingBox(), _z);
        }
        SetEntityBoundingBox(GetEntityBoundingBox().Offset(0.0D, 0.0D, _z));
        ResetPositionToBB();
        isCollidedVertically = y != _y;
        onGround = isCollidedVertically && y < 0.0D;
        if (isCollidedVertically) {
            gravityVelocity = 0;
        }
    }
    private void ResetPositionToBB () {
        posX = (GetEntityBoundingBox().minX + GetEntityBoundingBox().maxX) / 2.0D;
        posY = GetEntityBoundingBox().minY;
        posZ = (GetEntityBoundingBox().minZ + GetEntityBoundingBox().maxZ) / 2.0D;
    }


    public void Fall (float distance) {

        float i = Mathf.Pow(distance, 2) - 3.0F;
        if (i > 0) {
            //AddHealth(-i);
        }
    }

    public void Apply () {


        if (!isDead) {
            gravityVelocity += 27 * Time.deltaTime;
            velocity += gravityVelocity * Time.deltaTime * Vector3.down;

            BBB();

            MoveEntity(velocity.x, velocity.y, velocity.z);


            if (onGround) {

                Fall(gravityVelocity);
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
