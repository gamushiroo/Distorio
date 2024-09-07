using System.Collections.Generic;
using UnityEngine;

public class Entity {

    public MeshFilter meshFilter;
    public bool isAllive;
    public Vector3 pos;
    private float health;
    private float rotationY;
    private float gravityVelocity;
    private bool isGrounded;
    private Vector3 velocity;
    private Vector3 size;
    private readonly World world;
    private readonly float maxHealth;
    private readonly GameObject obj;
    private AudioSource audioSource;

    public Entity (Vector3 size, World world, float maxHealth) {

        obj = new();
        obj.AddComponent<MeshRenderer>().material = world.material;
        meshFilter = obj.AddComponent<MeshFilter>();
        audioSource = obj.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1;
        audioSource.maxDistance = 16;
        audioSource.volume = 0.05f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.dopplerLevel = 0;
        this.world = world;
        this.size = size;
        this.maxHealth = maxHealth;
        Init();
        GenerateMesh(10);
    }

    public void AddHealth(float value) {

        if (isAllive) {
            if (value < 0) {
                audioSource.PlayOneShot(world.audioClip);
            }
            health += value;
        }
    }
    public float Rotation => rotationY;
    public float Health => health;
    public void AddRotation (float rot) {
        rotationY += rot ;
    }
    public bool IsCollide (ChunkVoxel SelectedPos) {
        return AABB.ABCheck(new WWWEe(pos, size, velocity, isGrounded), Data.PublicLocationDerect(SelectedPos) + Vector3.one * 0.5f);
    }
    public void Init () {
        isAllive = true;
        gravityVelocity = 0;
        isGrounded = false;
        pos = world.GetSpawnPoint() + new Vector3(0.5f, 0, 0.5f);
        health = maxHealth;
        velocity = Vector3.zero;
        Apply();
    }
    public void AddVel (Vector3 vel, bool jump) {
        velocity += Quaternion.Euler(0, rotationY, 0) * vel * Data.playerSpeed * Time.deltaTime;
        if (jump && isGrounded) {
            gravityVelocity -= 10;
        }
    }
    public void Apply () {

        if (isAllive) {
            health += Time.deltaTime;
            gravityVelocity += 27 * Time.deltaTime;
            velocity += gravityVelocity * Time.deltaTime * Vector3.down;

            WWWEe entity = AABB.PosUpdate(new(pos, size, velocity, isGrounded), world);

            pos = entity.pos;
            velocity = entity.vel;
            isGrounded = entity.isGrounded;

            if (isGrounded) {
                if (gravityVelocity > 15) {
                    AddHealth(-Mathf.Pow(gravityVelocity, 2) / 13.5f);
                }
                gravityVelocity = 0;
            }
            health = Mathf.Min(health, maxHealth);
            if (pos.y < 16 || health <= 0) {
                isAllive = false;
            }

            obj.transform.position = pos;
            obj.transform.rotation = Quaternion.Euler(0, rotationY, 0);
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


                a.x *= size.x;
                a.y *= size.y;
                a.z *= size.z;

                a -= new Vector3(size.x, 0, size.z) / 2;

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
