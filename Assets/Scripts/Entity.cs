using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity {

    private readonly MeshFilter meshFilter;
    private AABB boundingBox;


    private bool alreadyGrounded;
    private bool isDead;

    private protected bool isZeroGravity;
    private protected float width;
    private protected float height;
    private protected bool isGrounded;
    private protected Vector3 velocity;
    private protected Vector3 acceleration;
    private protected readonly GameObject playerObject;
    private protected readonly Transform playerTransform;
    private protected readonly World world;
    private protected double posX;
    private protected double posY;
    private protected double posZ;
    private protected Vector3 inputVelocity;

    public Entity (World world) {

        this.world = world;
        playerObject = new();
        playerObject.AddComponent<MeshRenderer>().material = world.material;
        playerTransform = playerObject.transform;
        meshFilter = playerObject.AddComponent<MeshFilter>();
        width = 0.6F;
        height = 1.8F;
        GenerateMesh(10);
        velocity = Vector3.zero;
        SetPosition(0.0D, 0.0D, 0.0D);

    }

    public void UpdateIfNotDead () {
        if (!isDead) {
            Update();
        }
    }

    private protected void AddPosition (double x, double y, double z) {
        SetPosition(posX + x, posY + y, posZ + z);
    }
    private void TryMoveEntity (double x, double y, double z) {

        double _x = x;
        double _y = y;
        double _z = z;

        List<AABB> boundingBoxes = world.CollidingBoundingBoxes(boundingBox.Extend(x, y, z));

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
            velocity.x = 0;
            inputVelocity.x = 0;
        }
        if (_y != y) {
            velocity.y = 0;
            inputVelocity.y = 0;
        }
        if (_z != z) {
            velocity.z = 0;
            inputVelocity.z = 0;
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
    private protected bool IsCollide (Vector3Int selectedPos) {
        return Data.ABCheck(boundingBox, new(selectedPos.x, selectedPos.y, selectedPos.z, selectedPos.x + 1, selectedPos.y + 1, selectedPos.z + 1));
    }
    private protected void AddForce (Vector3 vel) {
        acceleration += vel;
    }
    private protected void Die () {
        isDead = true;
    }
    private protected void SetSize (float width, float height) {
        if (width != this.width || height != this.height) {
            this.width = width;
            this.height = height;

            float f = width / 2.0F;
            boundingBox = new(f, 0, f, f, height, f);
            GenerateMesh(10);
        }
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
            acceleration += Data.gravityScale * Vector3.down;
        }
        velocity += acceleration * Time.deltaTime;

        Vector3 a = inputVelocity + velocity;
        TryMoveEntity(a.x * Time.deltaTime, a.y * Time.deltaTime, a.z * Time.deltaTime);
        acceleration = Vector3.zero;
    }
    private protected virtual void OnGrounded () {
    }
}