using System;
using System.Collections.Generic;
using UnityEngine;
public abstract class Entity {
    public bool IsAlive { get; private set; } = true;
    public AABB BoundingBox { get; private set; }
    private bool waitUntilGrounded;
    private readonly MeshFilter meshFilter;
    private protected readonly World world;
    private protected readonly GameObject gameObject;
    private protected readonly Transform transform;
    private protected readonly AudioSource audioSource;
    private protected bool isGrounded;
    private protected float gravityMultiplier = 1;
    private protected float defaultWidth = 0.6F;
    private protected float defaultHeight = 1.8F;
    private protected float defaulteyeHeight = 0.9F;
    private protected double width;
    private protected double height;
    private protected double eyeHeight;
    private protected double velocityX;
    private protected double velocityY;
    private protected double velocityZ;
    private protected double posX;
    private protected double posY;
    private protected double posZ;
    public int ID;
    private static int currentID = 0;
    private protected bool UseCollision = true;
    protected int meshType = 0;
    public static readonly float resistance = 14.0F;
    public Entity (World worldIn) {
        ID = currentID++;
        world = worldIn;
        gameObject = new();
        transform = gameObject.transform;
        meshFilter = gameObject.AddComponent<MeshFilter>();
        audioSource = gameObject.AddComponent<AudioSource>();
        gameObject.AddComponent<MeshRenderer>().material = world.materials[0];
        Initialize();
    }
    private protected virtual void Initialize () {
        width = defaultWidth;
        height = defaultHeight;
        eyeHeight = defaulteyeHeight;
        GenerateMesh();
        void GenerateMesh () {
            int vertexIndex = 0;
            List<Vector3> vertices = new();
            List<int> triangles = new();
            List<Vector2> uvs = new();
            for (int p = 0; p < 6; p++) {
                for (int i = 0; i < 4; i++) {
                    Vector3 a = Data.voxelVerts[Data.blockMesh[p, i]];
                    a.x *= defaultWidth;
                    a.y *= defaultHeight;
                    a.z *= defaultWidth;
                    a -= new Vector3(defaultWidth, 0, defaultWidth) / 2;
                    vertices.Add(a);
                    uvs.Add((Data.voxelUVs[i] + Data.TexturePos(world.blockTypes[meshType].GetTextureID(p))) / Data.TextureSize);
                }
                for (int i = 0; i < 6; i++) {
                    triangles.Add(Data.order[i] + vertexIndex);
                }
                vertexIndex += 4;
            }
            meshFilter.sharedMesh = Data.MakeMesh(vertices, triangles, uvs);
        }
    }
    public virtual void Update () {
        AddForce(0, -Data.gravityScale * gravityMultiplier, 0);
        Move();
        transform.position = Vec3.ToVector3(posX, posY, posZ);
    }
    void Move () {
        double x = velocityX * Time.deltaTime;
        double y = velocityY * Time.deltaTime;
        double z = velocityZ * Time.deltaTime;
        if (UseCollision) {
            double i = x;
            double j = y;
            double k = z;
            List<AABB> p = world.GetCollidingBoundingBoxes(BoundingBox.AddCoord(x, y, z), ID);
            List<KeyValuePair<double, Action>> l = new() {
            new(Math.Abs(x), CalculateXOffset),
            new(Math.Abs(y), CalculateYOffset),
            new(Math.Abs(z), CalculateZOffset)
            };
            l.Sort((a, b) => b.Key.CompareTo(a.Key));
            foreach (KeyValuePair<double, Action> v in l) {
                v.Value();
            }
            isGrounded = j != y && j < 0.0D;
            if (!isGrounded) {
                waitUntilGrounded = true;
            } else if (waitUntilGrounded) {
                OnGrounded();
                waitUntilGrounded = false;
            }
            if (i != x) { velocityX = 0; }
            if (j != y) { velocityY = 0; }
            if (k != z) { velocityZ = 0; }
            if (i != x || j != y || k != z) {
                OnCollision();
            }
            void CalculateXOffset () { foreach (AABB q in p) { x = BoundingBox.CalculateXOffset(x, q); } AddCoordinate(x, 0.0D, 0.0D); }
            void CalculateYOffset () { foreach (AABB q in p) { y = BoundingBox.CalculateYOffset(y, q); } AddCoordinate(0.0D, y, 0.0D); }
            void CalculateZOffset () { foreach (AABB q in p) { z = BoundingBox.CalculateZOffset(z, q); } AddCoordinate(0.0D, 0.0D, z); }
        } else {
            AddCoordinate(x, y, z);
        }
    }
    private protected void AddForce (double x, double y, double z) {
        velocityX += x * Time.deltaTime;
        velocityY += y * Time.deltaTime;
        velocityZ += z * Time.deltaTime;
    }
    private protected void AddForce (Vec3 force) {
        velocityX += force.xCoord * Time.deltaTime;
        velocityY += force.yCoord * Time.deltaTime;
        velocityZ += force.zCoord * Time.deltaTime;
    }
    private protected void SetVelocity (double x, double y, double z) {
        velocityX = x;
        velocityY = y;
        velocityZ = z;
    }
    private protected void AddForce_Impulse (double x, double y, double z) {
        velocityX += x;
        velocityY += y;
        velocityZ += z;
    }
    private protected void SetPosition (double x, double y, double z) {
        posX = x;
        posY = y;
        posZ = z;
        ModifyBoundingBox();
    }
    private protected void AddCoordinate (double x, double y, double z) {
        posX += x;
        posY += y;
        posZ += z;
        ModifyBoundingBox();
    }
    private protected void Kill () {
        UnityEngine.Object.Destroy(gameObject);
        IsAlive = false;
    }









    private protected virtual void OnCollision () {
    }
    private protected void ModifyBoundingBox () {
        double f = width / 2.0F;
        BoundingBox = new(posX - f, posY, posZ - f, posX + f, posY + height, posZ + f);
    }
    private protected virtual void OnGrounded () {
    }
}