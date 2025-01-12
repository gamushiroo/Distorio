using System;
using System.Collections.Generic;
using UnityEngine;
public abstract class Entity {
    public int ID;
    public readonly AudioSource audioSource;
    public bool IsAlive { get; private set; } = true;
    public AABB BoundingBox { get; private set; }
    private protected float gravityMultiplier = 1;
    private protected List<Item> items = new();
    private protected int currentItem = 0;
    private protected Entity pearent;
    private protected List<Entity> child = new();
    private bool waitUntilGrounded;
    private readonly MeshFilter meshFilter;
    private protected readonly World world;
    private protected readonly GameObject gameObject;
    private protected readonly Transform transform;
    private protected bool isGrounded;
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
    private protected bool UseCollision = true;
    private static int currentID = 0;
    private protected bool inTheWater;
    private protected int meshType = 1;
    private protected readonly float resistance = 14.0F;
    private protected float rotationPitch = 0;
    private protected float rotationYaw = 0;
    public Entity (World worldIn) {
        ID = currentID++;
        world = worldIn;
        gameObject = new();
        transform = gameObject.transform;
        meshFilter = gameObject.AddComponent<MeshFilter>();
        audioSource = gameObject.AddComponent<AudioSource>();
        gameObject.AddComponent<MeshRenderer>().material = MyResources.materials[0];
        Initialize();
    }
    public Vector3 GetCamPos () {
        return Vec3.ToVector3(posX, posY + eyeHeight, posZ);
    }
    public Quaternion GetRotation () {
        return Quaternion.Euler(rotationPitch, rotationYaw, 0);
    }
    public void SetSize (float width, float height) {
        defaultWidth = width;
        defaultHeight = height;
        this.width = defaultWidth;
        this.height = defaultHeight;
        GenerateMesh();
    }
    private protected virtual void Initialize () {
        width = defaultWidth;
        height = defaultHeight;
        eyeHeight = defaulteyeHeight;
        GenerateMesh();
    }
    void GenerateMesh () {
        int vertexIndex = 0;
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();
        float tt = defaultWidth / 2;
        for (int p = 0; p < 6; p++) {
            for (int i = 0; i < 4; i++) {
                Vector3 b = MyResources.voxelVerts[MyResources.blockMesh[p, i]];
                vertices.Add(new(b.x * defaultWidth - tt, b.y * defaultHeight, b.z * defaultWidth - tt));
                uvs.Add((MyResources.voxelUVs[i] + MyResources.TexturePos(MyResources.blockTypes[meshType].GetTextureID(p))) / MyResources.TextureSize);
            }
            for (int i = 0; i < 6; i++) {
                triangles.Add(MyResources.order[i] + vertexIndex);
            }
            vertexIndex += 4;
        }
        meshFilter.sharedMesh = MyResources.MakeMesh(vertices, triangles, uvs);
    }
    public virtual void Update () {
        for (int i = 0; i < items.Count; i++) {
            items[i].Update();
        }
        if (pearent == null) {
            inTheWater = Chunks.GetCollidingBlockIDs(BoundingBox).Contains(7);
            if (inTheWater) {
                AddForce(velocityX * -2, velocityY * -2, velocityZ * -2);
            } else {
                AddForce(0, -MyResources.gravityScale * gravityMultiplier, 0);
            }
            Move();
        }
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
            List<AABB> p = Chunks.GetCollidingBlocks(BoundingBox.BroadPhase(x, y, z));
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
            if (i != x) { 
                velocityX = 0; 
            }
            if (j != y) { 
                velocityY = 0; 
            }
            if (k != z) {
                velocityZ = 0;
            }
            if (i != x || j != y || k != z) {
                OnCollision();
            }
            void CalculateXOffset () { foreach (AABB q in p) { x = BoundingBox.CalculateXOffset(x, q); } AddPosition(x, 0.0D, 0.0D); }
            void CalculateYOffset () { foreach (AABB q in p) { y = BoundingBox.CalculateYOffset(y, q); } AddPosition(0.0D, y, 0.0D); }
            void CalculateZOffset () { foreach (AABB q in p) { z = BoundingBox.CalculateZOffset(z, q); } AddPosition(0.0D, 0.0D, z); }
        } else {
            AddPosition(x, y, z);
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
    public void SetPosition (double x, double y, double z) {
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