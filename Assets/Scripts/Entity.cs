using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity {

    private readonly MeshFilter meshFilter;
    private AABB boundingBox;

    private bool alreadyGrounded;
    private bool isDead;

    private protected readonly GameObject obj;
    private protected readonly Transform playerTransform;
    private protected readonly World world;

    private protected float rotationPitch;
    private protected float rotationYaw;
    private protected bool isZeroGravity;
    private protected bool isGrounded;
    private protected float width;
    private protected float height;
    private protected double motionX, motionY, motionZ;
    private protected double posX, posY, posZ;

    public Entity (World world) {

        this.world = world;
        obj = new();
        obj.AddComponent<MeshRenderer>().material = world.material;
        playerTransform = obj.transform;
        meshFilter = obj.AddComponent<MeshFilter>();
        width = 0.6F;
        height = 1.8F;
        GenerateMesh(10);

        SetVelocity(0.0D, 0.0D, 0.0D);
        SetPosition(0.0D, 0.0D, 0.0D);
    }

    public void UpdateIfNotDead () {
        if (!isDead) {
            Update();
        }
    }

    protected List<int> CollidingIDs () {
        return CollidingIDs(boundingBox);
    }
    protected void AddVelocity (double x, double y, double z) {
        motionX += x;
        motionY += y;
        motionZ += z;
    }
    protected void SetVelocity (double x, double y, double z) {
        motionX = x;
        motionY = y;
        motionZ = z;
    }

    public List<int> CollidingIDs (AABB aabb) {
        List<int> a = new();
        for (int x = (int)Math.Floor(aabb.minX); x < (int)Math.Ceiling(aabb.maxX); x++) {
            for (int y = (int)Math.Floor(aabb.minY); y < (int)Math.Ceiling(aabb.maxY); y++) {
                for (int z = (int)Math.Floor(aabb.minZ); z < (int)Math.Ceiling(aabb.maxZ); z++) {
                    a.Add(world.GetVoxelID(new(x, y, z)));
                }
            }
        }
        return a;
    }
    protected void TryMoveEntity (double x, double y, double z) {

        List<AABB> boundingBoxes = CollidingBoundingBoxes(boundingBox.Extend(x, y, z));
        double _x = x;
        double _y = y;
        double _z = z;

        CalculateYOffset();

        if (Math.Abs(x) > Math.Abs(z)) {
            CalculateXOffset();
            CalculateZOffset();
        } else {
            CalculateZOffset();
            CalculateXOffset();
        }

        isGrounded = _y != y && y < 0.0D;

        if (!isGrounded) {
            alreadyGrounded = false;
        } else if (!alreadyGrounded) {
            OnGrounded();
            alreadyGrounded = true;
        }
        if (_x != x) {
            motionX = 0;
        }
        if (_y != y) {
            motionY = 0;
        }
        if (_z != z) {
            motionZ = 0;
        }
        List<AABB> CollidingBoundingBoxes (AABB aabb) {
            List<AABB> a = new();
            for (int x = (int)Math.Floor(aabb.minX); x < (int)Math.Ceiling(aabb.maxX); x++) {
                for (int y = (int)Math.Floor(aabb.minY); y < (int)Math.Ceiling(aabb.maxY); y++) {
                    for (int z = (int)Math.Floor(aabb.minZ); z < (int)Math.Ceiling(aabb.maxZ); z++) {
                        if (world.blockTypes[world.GetVoxelID(new(x, y, z))].hasCollision) {
                            a.Add(new(x, y, z, x + 1, y + 1, z + 1));
                        }
                    }
                }
            }
            return a;
        }
        void CalculateXOffset () {
            foreach (AABB value in boundingBoxes) {
                _x = value.CalculateXOffset(boundingBox, _x);
            }
            AddPosition(_x, 0.0D, 0.0D);
        }
        void CalculateYOffset () {
            foreach (AABB value in boundingBoxes) {
                _y = value.CalculateYOffset(boundingBox, _y);
            }
            AddPosition(0.0D, _y, 0.0D);
        }
        void CalculateZOffset () {
            foreach (AABB value in boundingBoxes) {
                _z = value.CalculateZOffset(boundingBox, _z);
            }
            AddPosition(0.0D, 0.0D, _z);
        }
        void AddPosition (double x, double y, double z) {
            SetPosition(posX + x, posY + y, posZ + z);
        }
    }
    private protected bool IsCollide (Vector3Int selectedPos) {
        return boundingBox.ABCheck(new(selectedPos.x, selectedPos.y, selectedPos.z, selectedPos.x + 1, selectedPos.y + 1, selectedPos.z + 1));
    }
    private protected void Die () {
        isDead = true;
    }
    private protected void SetPosition (double x, double y, double z) {

        posX = x;
        posY = y;
        posZ = z;
        float f = width / 2.0F;
        boundingBox = new(posX - f, posY, posZ - f, posX + f, posY + height, posZ + f);
        playerTransform.position = new((float)posX, (float)posY, (float)posZ);

    }
    private protected virtual void Update () {
        if (!isZeroGravity) {
            Vector3 sss = Data.gravityScale * Time.deltaTime * Vector3.down;
            AddVelocity(sss.x, sss.y, sss.z);
        }

        TryMoveEntity(motionX * Time.deltaTime, motionY * Time.deltaTime, motionZ * Time.deltaTime);
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
}