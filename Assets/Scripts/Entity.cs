using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity {

    private readonly MeshFilter meshFilter;

    private protected readonly World world;
    private protected readonly GameObject gameObject;
    private protected readonly Transform transform;
    private protected readonly AudioSource audioSource;

    public AABB BoundingBox { get; private set; }

    private bool waitUntilGrounded;
    private bool isDead;

    private protected bool hasGravity = true;
    private protected bool isGrounded;

    private protected float width = 0.6F;
    private protected float height = 1.8F;
    private protected float eyeHeight = 0.9F;
    private protected double motionHeight;
    private protected double motionEyeHeight;

    private protected double motionX;
    private protected double motionY;
    private protected double motionZ;
    private protected double posX;
    private protected double posY;
    private protected double posZ;

    public Entity (World worldIn) {
        world = worldIn;
        gameObject = new();
        transform = gameObject.transform;
        meshFilter = gameObject.AddComponent<MeshFilter>();
        audioSource = gameObject.AddComponent<AudioSource>();
        gameObject.AddComponent<MeshRenderer>().material = world.material;
        GenerateMesh(19);
        motionHeight = height;
        motionEyeHeight = eyeHeight;
        SetPosition(0.0D, 0.0D, 0.0D);
        SetVelocity(0.0D, 0.0D, 0.0D);
    }
    private protected void SetVelocity (double x, double y, double z) {
        motionX = x;
        motionY = y;
        motionZ = z;
    }
    private protected void AddVelocity (double x, double y, double z) {
        motionX += x;
        motionY += y;
        motionZ += z;
    }
    private protected void SetPosition (double x, double y, double z) {
        posX = x;
        posY = y;
        posZ = z;
        ModifyBoundingBox();
    }
    private protected void AddPosition (double x, double y, double z) {
        posX += x;
        posY += y;
        posZ += z;
        ModifyBoundingBox();
    }
    private void ModifyBoundingBox () {
        float f = width / 2.0F;
        BoundingBox = new(posX - f, posY, posZ - f, posX + f, posY + motionHeight, posZ + f);
    }
    private void MoveEntity (double x, double y, double z) {
        double i = x;
        double j = y;
        double k = z;
        List<AABB> others = world.GetCollidingBoundingBoxes(BoundingBox.AddCoord(x, y, z));
        void CalculateXOffset () { foreach (AABB other in others) { x = BoundingBox.CalculateXOffset(x, other); } AddPosition(x, 0.0D, 0.0D); }
        void CalculateYOffset () { foreach (AABB other in others) { y = BoundingBox.CalculateYOffset(y, other); } AddPosition(0.0D, y, 0.0D); }
        void CalculateZOffset () { foreach (AABB other in others) { z = BoundingBox.CalculateZOffset(z, other); } AddPosition(0.0D, 0.0D, z); }
        List<KeyValuePair<double, Action>> queue = new() {
            new(Math.Abs(x), CalculateXOffset),
            new(Math.Abs(y), CalculateYOffset),
            new(Math.Abs(z), CalculateZOffset)
        };
        queue.Sort((a, b) => b.Key.CompareTo(a.Key));
        foreach (KeyValuePair<double, Action> v in queue) {
            v.Value();
        }
        isGrounded = j != y && j < 0.0D;
        if (!isGrounded) {
            waitUntilGrounded = true;
        } else if (waitUntilGrounded) {
            OnGrounded();
            waitUntilGrounded = false;
        }
        if (i != x) { motionX = 0; }
        if (j != y) { motionY = 0; }
        if (k != z) { motionZ = 0; }
    }




    private protected void Die () {
        isDead = true;
    }
    private protected virtual void Update () {

        if (hasGravity) {
            AddVelocity(0, -Data.gravityScale * Time.deltaTime, 0);
        }
        MoveEntity(motionX * Time.deltaTime, motionY * Time.deltaTime, motionZ * Time.deltaTime);

        double t = Data.resistance * ((Input.GetKey(KeyCode.LeftShift) ? height / 2 : height) - motionHeight) * Time.deltaTime;
        List<AABB> others = world.GetCollidingBoundingBoxes(BoundingBox.AddCoord(0, Math.Max(t, 0), 0));
        foreach (AABB other in others) {
            t = BoundingBox.CalculateYOffset(t, other);
        }
        motionHeight += t;
        motionEyeHeight = motionHeight * eyeHeight;
        float f = width / 2.0F;
        BoundingBox = new(posX - f, posY, posZ - f, posX + f, posY + motionHeight, posZ + f);

        transform.position = new((float)posX, (float)posY, (float)posZ);
    }
    public void UpdateIfNotDead () {
        if (!isDead) {
            Update();
        }
    }
    private protected void SetPositionToSpawnPoint () {
        SetPosition(0.0D, 0.0D, 0.0D);
        while (world.GetCollidingBoundingBoxes(BoundingBox).Count != 0) {
            AddPosition(0.0D, 1.0D, 0.0D);
        }
    }
    private protected void GenerateMesh (int skin) {
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
    private protected virtual void OnGrounded () {
    }
}