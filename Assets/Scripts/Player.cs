using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    public byte selectedBlockIndex = 0;
    public Vector3 spawnPoint;


    public MyUI myUI;

    [SerializeField] private World world;
    [SerializeField] private Hand hand;
    [SerializeField] public new Transform camera;
    [SerializeField] private Transform miningEffectTransform;
    [SerializeField] private GameObject blockHighlight;
    [SerializeField] private GameObject miningProgresBarObj;
    [SerializeField] private ParticleSystem miningEffect;
    [SerializeField] private Slider miningProgresBar;

    public float health;

    private bool dead;

    private ChunkVoxel miningPos;
    private WWWEe entity;
    private BlockAndSelect SelectedPos;
    private float camMove;
    private float miningProgress;
    private float gravityVelocity;

    private float rotationX;
    private float rotationY;
    private Vector2Int chunkCoord;
    private Vector2Int lastChunkCoord;

    private void Start () {

        Spawn();

    }

    private void Update () {




        chunkCoord = Data.Vector3ToChunkVoxel(entity.pos).c;

        if (!(chunkCoord == lastChunkCoord)) {
            world.CheckViewDistance(chunkCoord);
            lastChunkCoord = chunkCoord;
        }
        if (!dead) {
            if (Input.GetKeyDown(KeyCode.E))
                myUI.Switch();
            if (!myUI.inUI) {
                AddInput();
                Aaa();
            }
            CalculateVelocity();
            SetValue();
        } else {
            if (Input.GetKeyDown(KeyCode.R))
                Spawn();
        }

        if(entity.pos.y < 16) {
            Spawn();
        }
    }


    private void Die () {
        dead = true;
    }

    private void Spawn () {

        miningProgress = 0;
        gravityVelocity = 0;
        health = Data.player.health;

        entity = new WWWEe(world.GetSpawnPoint() + new Vector3(0.5f, 0, 0.5f), Data.player.size, Vector3.zero, false);
        dead = false;

    }

    private void FixedUpdate () {


    }

    private void AddInput () {
        if (Input.GetKey(KeyCode.Space) && entity.isGrounded)
            gravityVelocity -= 10;

        rotationX -= Data.mouseSens * Time.deltaTime * Input.GetAxisRaw("Mouse Y");
        rotationY += Data.mouseSens * Time.deltaTime * Input.GetAxisRaw("Mouse X");
        rotationX = Mathf.Clamp(rotationX, -90, 90);
        entity.vel += Quaternion.Euler(0, rotationY, 0) * Data.GetPlayerVel() * Data.playerSpeed * Time.deltaTime;
    }

    private void Aaa () {

        miningEffect.Stop();
        blockHighlight.SetActive(false);
        miningProgresBarObj.SetActive(false);
        blockHighlight.SetActive(false);
        SelectedPos = GetSelectedPos();

        byte _blockID = 0;
        int _blockID2 = 0;

        if (world.chunks.ContainsKey(SelectedPos.blue.c)) {
            _blockID = world.GetVoxelID(SelectedPos.blue);
        }
        if (world.chunks.ContainsKey(SelectedPos.red.c)) {
            _blockID2 = world.GetVoxelID(SelectedPos.red);
        }


        if (world.blockTypes[_blockID].hasCollision) {

            blockHighlight.SetActive(true);


            if (selectedBlockIndex < 128) {
                if (Input.GetMouseButtonDown(0)) {
                    if (Input.GetMouseButtonDown(0) && !AABB.FFF(entity, Data.PublicLocationDerect(SelectedPos.red) + Vector3.one * 0.5f) && !world.blockTypes[_blockID2].hasCollision) {

                        hand.placeEase = 0;

                        SetBBBL(SelectedPos.red, selectedBlockIndex);
                    }
                }
            } else {

                if (Input.GetMouseButton(0)) {

                    miningProgresBarObj.SetActive(true);

                    if (miningPos.c == SelectedPos.blue.c && miningPos.v == SelectedPos.blue.v) {
                        miningProgress -= Time.deltaTime * (GetMiningSpeed(selectedBlockIndex - 128, 0.85f, 1f) + 0.75f);
                        miningEffect.Play();
                    } else {
                        miningProgress = world.blockTypes[_blockID].hardness;
                        miningPos = SelectedPos.blue;
                    }
                    if (miningProgress <= 0) {
                        SetBBBL(SelectedPos.blue, 0);
                        //Debug.Log("shit");
                    }
                    miningProgresBar.value = miningProgress / world.blockTypes[_blockID].hardness;
                }
            }
        } 
    }

    float GetMiningSpeed (int n, float a, float l) {


        return l * ((Mathf.Pow(a, n) - 1) / (a - 1));
    }

    void SetBBBL (ChunkVoxel pos, byte id) {

        if (world.chunks.ContainsKey(pos.c) && world.chunks[pos.c].IsEditable) {

            Queue<VoxelMod> queue = new Queue<VoxelMod>();
            queue.Enqueue(new(pos, id));
            world.modifications.Enqueue(queue);

        }
    }
    private void CalculateVelocity () {

        gravityVelocity += 27 * Time.deltaTime;
        entity.vel += gravityVelocity * Time.deltaTime * Vector3.down;
        entity = AABB.PosUpdate(entity, world);

        health += Time.deltaTime * 4;

        if (entity.isGrounded) {
            if (gravityVelocity > 15) {
                health -= Mathf.Pow(gravityVelocity, 2) / 13.5f;
            }
            gravityVelocity = 0;
        }

        health = Mathf.Min(health, Data.player.health);

        if (health <= 0) {
            Die();
        }
    }

    private void SetValue () {

        camMove += Input.GetKey(KeyCode.LeftControl) ? -1 : 1 * 10 * Time.deltaTime;
        camMove = Mathf.Clamp(camMove, -0.25f, 0);

        camera.position = entity.pos + Vector3.up * (1.625f + camMove);
        camera.rotation = Quaternion.Euler(rotationX, rotationY, 0);

        blockHighlight.transform.position = Data.PublicLocationDerect(SelectedPos.blue) + Vector3.one * 0.5f;
        miningEffectTransform.position = Data.PublicLocationDerect(SelectedPos.blue) + Vector3.one * 0.5f;

    }

    private BlockAndSelect GetSelectedPos () {
        Vector3 _camPos = camera.position;
        ChunkVoxel camPos = Data.Vector3ToChunkVoxel(_camPos);
        for (int i = 0; i < 300; i++) {
            camPos = Data.Vector3ToChunkVoxel(_camPos);


            if (world.chunks.ContainsKey(camPos.c)) {
                if (world.blockTypes[world.GetVoxelID(camPos)].hasCollision) {
                    break;
                }
            }

            _camPos += camera.forward * 0.02f;
        }
        return new BlockAndSelect(camPos, Data.Vector3ToChunkVoxel(II(_camPos) + _camPos));
    }

    private Vector3Int II (Vector3 pp) {

        Vector3Int normal = Vector3Int.zero;
        Vector3 p = new Vector3(pp.x < 0 ? 1 : 0, pp.y < 0 ? 1 : 0, pp.z < 0 ? 1 : 0) + new Vector3(pp.x % 1, pp.y % 1, pp.z % 1) - Vector3.one * 0.5f;
        Vector3 v = new Vector3(Mathf.Abs(p.x), Mathf.Abs(p.y), Mathf.Abs(p.z));

        if (v.x < v.z && v.y < v.z) {
            normal += Vector3Int.forward * Mathf.RoundToInt(Mathf.Sign(p.z));
        } else if (v.x < v.y) {
            normal += Vector3Int.up * Mathf.RoundToInt(Mathf.Sign(p.y));
        } else if (v.x > v.y) {
            normal += Vector3Int.right * Mathf.RoundToInt(Mathf.Sign(p.x));
        }
        return normal;
    }
}
