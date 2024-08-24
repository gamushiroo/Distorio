using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    public byte selectedBlockIndex = 0;
    public Vector3 spawnPoint;

    [SerializeField] private World world;
    [SerializeField] private Hand hand;
    [SerializeField] private new Transform camera;
    [SerializeField] private Transform miningEffectTransform;
    [SerializeField] private GameObject blockHighlight;
    [SerializeField] private GameObject miningProgresBarObj;
    [SerializeField] private Text hpText;
    [SerializeField] private ParticleSystem miningEffect;
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider miningProgresBar;


    private bool hasGravity = true;
    private ChunkVoxel miningPos;
    private Entity entity;
    private BlockAndSelect SelectedPos;
    private float camMove;
    private float miningProgress;
    private float gravityVelocity;
    private float health;
    private float rotationX;
    private float rotationY;
    private bool spawnRequest;
    private bool jumpRequest;
    private Vector3 playerVel;
    private Vector2Int pos;
    private Vector2Int lastPos;

    private void Start () {

        Cursor.lockState = CursorLockMode.Locked;
        spawnRequest = true;
        jumpRequest = false;
        playerVel = Vector3.zero;

    }

    private void Update () {

        pos = Data.Vector3ToChunkVoxel(camera.transform.position).c;
        if (!(pos == lastPos)) {
            world.CheckViewDistance(pos);
            lastPos = pos;
        }

        if (Input.GetKeyDown(KeyCode.G))
            hasGravity = !hasGravity;
        if (!hasGravity) {
            if (Input.GetKey(KeyCode.Space))
                entity.vel += Data.playerSpeed * Time.deltaTime * Vector3.up;
            if (Input.GetKey(KeyCode.LeftControl))
                entity.vel += Data.playerSpeed * Time.deltaTime * Vector3.down;
        }
        if (Input.GetKeyDown(KeyCode.E))
            world.InUI = !world.InUI;

        if (!world.InUI) {

            playerVel += Data.GetPlayerVel();

            if (Input.GetKey(KeyCode.Space))
                jumpRequest = true;
            if (Input.GetKeyDown(KeyCode.R))
                spawnRequest = true;

            rotationX -= Data.mouseSens * Time.deltaTime * Input.GetAxisRaw("Mouse Y");
            rotationY += Data.mouseSens * Time.deltaTime * Input.GetAxisRaw("Mouse X");
            rotationX = Mathf.Clamp(rotationX, -90, 90);

        }

        Aaa();
        CalculateVelocity();
        SetValue();

    }

    private void LateUpdate () {

        if (spawnRequest)
            Spawn();

        spawnRequest = false;
        jumpRequest = false;
        playerVel = Vector3.zero;


    }

    private void Spawn () {

        miningProgress = 0;
        gravityVelocity = 0;
        health = Data.player.health;
        hpBar.value = health / Data.player.health;
        entity = new Entity(world.GetSpawnPoint() + new Vector3(0.5f, 0, 0.5f), Data.player.size, Vector3.zero, false);

    }


    private void Aaa () {

        miningEffect.Stop();
        blockHighlight.SetActive(false);
        miningProgresBarObj.SetActive(false);

        SelectedPos = GetSelectedPos();

        byte _blockID = 0;
        int _blockID2 = 0;

        if (world.chunks.ContainsKey(SelectedPos.blue.c)) {
            _blockID = world.chunks[SelectedPos.blue.c].voxelMap[SelectedPos.blue.v.x, SelectedPos.blue.v.y, SelectedPos.blue.v.z];
        }
        if (world.chunks.ContainsKey(SelectedPos.red.c)) {
            _blockID2 = world.chunks[SelectedPos.red.c].voxelMap[SelectedPos.red.v.x, SelectedPos.red.v.y, SelectedPos.red.v.z];
        }

        blockHighlight.SetActive(_blockID != 0);

        if(_blockID != 0) {

            if (Input.GetMouseButton(0)) {

                miningProgresBarObj.SetActive(true);

                if (miningPos.c == SelectedPos.blue.c && miningPos.v == SelectedPos.blue.v) {
                    miningProgress -= Time.deltaTime * 3 / 4;
                    miningEffect.Play();
                }
                else {
                    miningProgress = world.blockTypes[_blockID].hardness;
                    miningPos = SelectedPos.blue;
                }
                if (miningProgress <= 0) {
                    SetBBBL(SelectedPos.blue, 0);
                }
                miningProgresBar.value = miningProgress / world.blockTypes[_blockID].hardness;
            }
            if (Input.GetMouseButtonDown(1) && _blockID2 == 0 && selectedBlockIndex != 0) {

                hand.placeEase = 0;

                SetBBBL(SelectedPos.red, selectedBlockIndex);
            }
        }
    }

    void SetBBBL (ChunkVoxel pos, byte id) {

        if (world.chunks.ContainsKey(pos.c) && world.chunks[pos.c].IsEditable()) {

            Queue<VoxelMod> queue = new Queue<VoxelMod>();
            queue.Enqueue(new(pos, id));
            world.modifications.Enqueue(queue);

        }
    }
    private void CalculateVelocity () {

        if (jumpRequest && entity.isGrounded) {

            gravityVelocity -= 10;


        }

        if (hasGravity) {
            gravityVelocity += 27 * Time.deltaTime;
            entity.vel += gravityVelocity * Time.deltaTime * Vector3.down;
        }

        entity.vel += Quaternion.Euler(0, rotationY, 0) * playerVel * Data.playerSpeed * Time.deltaTime;

        entity = AABB.PosUpdate(entity, world);

        health += Time.deltaTime * 4;

        if (entity.isGrounded) {
            if (gravityVelocity > 15) {
                health -= Mathf.Pow(gravityVelocity, 2) / 13.5f;
            }
            gravityVelocity = 0;
        }

        health = Mathf.Min(health, Data.player.health);

        if (health <= 0)
            spawnRequest = true;


    }

    private void SetValue () {

        camMove += Input.GetKey(KeyCode.LeftControl) ? -1 : 1 * 10 * Time.deltaTime;
        camMove = Mathf.Clamp(camMove, -0.25f, 0);

        camera.position = entity.pos + Vector3.up * ( 1.625f + camMove );
        camera.rotation = Quaternion.Euler(rotationX, rotationY, 0);

        hpText.text = Mathf.FloorToInt(health).ToString("#,#");
        hpBar.value = health / Data.player.health;
        blockHighlight.transform.position = Data.PublicLocationDerect(SelectedPos.blue) + Vector3.one * 0.5f;
        miningEffectTransform.position = Data.PublicLocationDerect(SelectedPos.blue) + Vector3.one * 0.5f;

    }

    private BlockAndSelect GetSelectedPos () {
        Vector3 _camPos = camera.position;
        ChunkVoxel camPos = Data.Vector3ToChunkVoxel(_camPos);
        for (int i = 0; i < 300; i++) {
            camPos = Data.Vector3ToChunkVoxel(_camPos);


            if (world.chunks.ContainsKey(camPos.c)) {
                if (world.chunks[camPos.c].voxelMap[camPos.v.x, camPos.v.y, camPos.v.z] != 0) {
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
        }
        else if (v.x < v.y) {
            normal += Vector3Int.up * Mathf.RoundToInt(Mathf.Sign(p.y));
        }
        else if (v.x > v.y) {
            normal += Vector3Int.right * Mathf.RoundToInt(Mathf.Sign(p.x));
        }
        return normal;
    }
}
