using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity {

    private protected readonly GameObject obj;
    private protected readonly Transform objTransform;
    private protected readonly World world;
    private readonly MeshFilter meshFilter;

    public AABB BoundingBox { get; private set; }

    private protected bool hasGravity = true;
    private protected bool isGrounded;
    private bool alreadyGrounded;
    private bool isDead;


    private protected float rotationPitch;
    private protected float rotationYaw;
    private protected float width = 0.6F;
    private protected float defaultHeight = 1.8F;
    private protected double height = 1.8F;
    private protected double eyeHeight = 1.8F;
    private protected double motionX;
    private protected double motionY;
    private protected double motionZ;
    private protected double posX;
    private protected double posY;
    private protected double posZ;

    public Entity (World world) {

        this.world = world;
        obj = new();
        objTransform = obj.transform;
        obj.AddComponent<MeshRenderer>().material = world.material;
        meshFilter = obj.AddComponent<MeshFilter>();
        GenerateMesh(10);

        SetVelocity(0.0D, 0.0D, 0.0D);
        SetPosition(0.0D, 0.0D, 0.0D);
    }

    public void UpdateIfNotDead () {
        if (!isDead) {
            Update();
        }
    }
    protected void SetPositionToSpawnPoint () {
        SetPosition(0.0D, 0.0D, 0.0D);
        while (world.GetCollidingBoundingBoxes(BoundingBox).Count != 0) {
            AddPosition(0.0D, 1.0D, 0.0D);
        }
    }
    protected void SetVelocity (double x, double y, double z) {
        motionX = x;
        motionY = y;
        motionZ = z;
    }
    protected void AddVelocity (double x, double y, double z) {
        motionX += x;
        motionY += y;
        motionZ += z;
    }
    private protected void SetPosition (double x, double y, double z) {
        posX = x;
        posY = y;
        posZ = z;
        float f = width / 2.0F;
        BoundingBox = new(posX - f, posY, posZ - f, posX + f, posY + height, posZ + f);
    }
    private protected void SetSize (float width, float height) {
        float f = width / 2.0F;
        BoundingBox = new(posX - f, posY, posZ - f, posX + f, posY + height, posZ + f);
    }
    protected void AddPosition (double x, double y, double z) => SetPosition(posX + x, posY + y, posZ + z);
    protected void MoveEntity (double x, double y, double z) {

        List<AABB> others = world.GetCollidingBoundingBoxes(BoundingBox.AddCoord(x, y, z));
        double _x = x;
        double _y = y;
        double _z = z;

        foreach (AABB other in others) {
            y = BoundingBox.CalculateYOffset(y, other);
        }
        AddPosition(0.0D, y, 0.0D);
        if (Math.Abs(x) > Math.Abs(z)) {
            CalculateXOffset();
            CalculateZOffset();
        } else {
            CalculateZOffset();
            CalculateXOffset();
        }
        void CalculateXOffset () {
            foreach (AABB other in others) {
                x = BoundingBox.CalculateXOffset(x, other);
            }
            AddPosition(x, 0.0D, 0.0D);
        }
        void CalculateZOffset () {
            foreach (AABB other in others) {
                z = BoundingBox.CalculateZOffset(z, other);
            }
            AddPosition(0.0D, 0.0D, z);
        }
        isGrounded = y != _y && _y < 0.0D;
        if (!isGrounded) {
            alreadyGrounded = false;
        } else if (!alreadyGrounded) {
            OnGrounded();
            alreadyGrounded = true;
        }
        if (x != _x) {
            motionX = 0;
        }
        if (y != _y) {
            motionY = 0;
        }
        if (z != _z) {
            motionZ = 0;
        }
    }
    private protected void Die () {
        isDead = true;
    }
    private protected virtual void Update () {


        if (hasGravity) {
            AddVelocity(0, -Data.gravityScale * Time.deltaTime, 0);
        }
        MoveEntity(motionX * Time.deltaTime, motionY * Time.deltaTime, motionZ * Time.deltaTime);


        double t = Data.resistance * ((Input.GetKey(KeyCode.LeftShift) ? defaultHeight / 2 : defaultHeight) - height) * Time.deltaTime;
        List<AABB> others = world.GetCollidingBoundingBoxes(BoundingBox.AddCoord(0, Math.Max(t, 0), 0));
        foreach (AABB other in others) {
            t = BoundingBox.CalculateYOffset(t, other);
        }
        height += t;
        eyeHeight = height * 9.0F / 10.0F;
        float f = width / 2.0F;
        BoundingBox = new(posX - f, posY, posZ - f, posX + f, posY + height, posZ + f);





        objTransform.position = new((float)posX, (float)posY, (float)posZ);
    }
    private protected virtual void OnGrounded () {
    }
    private protected void GenerateMesh (byte skin) {
        int vertexIndex = 0;
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();
        for (int p = 0; p < 6; p++) {
            for (int i = 0; i < 4; i++) {
                Vector3 a = Data.voxelVerts[Data.blockMesh[p, i]];
                a.x *= width;
                a.y *= (float)height;
                a.z *= width;
                a -= new Vector3(width, 0, width) / 2;
                vertices.Add(a);
                uvs.Add((Data.voxelUVs[i] + Data.TexturePos(world.blockTypes[skin].GetTextureID(p))) / Data.TextureSize);
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