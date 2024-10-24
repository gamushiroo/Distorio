using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    public float rotationX;
    public byte selectedBlockIndex = 0;

    [SerializeField] private World world;
    [SerializeField] private Hand hand;
    [SerializeField] public new Transform camera;
    [SerializeField] private GameObject blockHighlight;
    [SerializeField] private GameObject miningEffect;
    [SerializeField] private GameObject miningProgresBarObj;
    [SerializeField] private Slider miningProgresBar;
    private bool isMining = false;
    private Vector3Int miningPos;
    public Vector3Int tryPlacingPos;
    public Vector3Int SelectingPos;
    private float miningProgress;
    private Vector2Int chunkCoord;
    private Vector2Int lastChunkCoord;
    public Entity entity;
    public Inventory inventory;

    public float miningSpeed;
    public bool locked;

    public void Awake () {
        locked = false;
        inventory = new();

    }

    private void Start () {

        entity = new EntityPlayer(world, Data.player.health, Vector3.zero);

    }
    private void Update () {

        if (!entity.isDead) {

            if (!locked) {

                if (selectedBlockIndex >= 128) {
                    miningSpeed = 4;

                } else {
                    miningSpeed = 0.8f;
                }



                isMining = Input.GetMouseButton(0) && world.blockTypes[world.GetVoxelID(SelectingPos)].hasCollision;


                rotationX -= Data.mouseSens * Time.deltaTime * Input.GetAxisRaw("Mouse Y");
                rotationX = Mathf.Clamp(rotationX, -90, 90);

                entity.AddRotation(Data.mouseSens * Input.GetAxisRaw("Mouse X") * Time.deltaTime);
                entity.AddVel(Data.GetPlayerVel(), Input.GetKey(KeyCode.Space));

            }

            SetValue();

        }

        if (Input.GetKeyDown(KeyCode.R) && entity.isDead) {

            entity.EntityInit();

        }
    }
    private void SetValue () {

        entity.Apply();

        CalculateSelectingPos();

        if (isMining) {
            if (!Equals(miningPos, SelectingPos)) {
                miningPos = SelectingPos;
                miningProgress = world.blockTypes[world.GetVoxelID(SelectingPos)].hardness;
            }
            miningProgress -= Time.deltaTime * miningSpeed;
            if (miningProgress <= 0) {
                SetBBBL(SelectingPos, 0);
            }
            miningProgresBar.value = miningProgress / world.blockTypes[world.GetVoxelID(SelectingPos)].hardness;
        }
        camera.position = new Vector3((float)entity.posX, (float)entity.posY, (float)entity.posZ) + Vector3.up * 1.625f;
        camera.rotation = Quaternion.Euler(rotationX, entity.Rotation, 0);

        blockHighlight.transform.position = SelectingPos + Vector3.one * 0.5f;

        miningEffect.transform.position = SelectingPos + Vector3.one * 0.5f;

        miningEffect.SetActive(isMining);
        miningProgresBarObj.SetActive(isMining);
        blockHighlight.SetActive(world.blockTypes[world.GetVoxelID(SelectingPos)].hasCollision && !locked);

        chunkCoord = Data.Vector3ToChunkVoxel(new((float)entity.posX, (float)entity.posY, (float)entity.posZ)).c;
        if (!(chunkCoord == lastChunkCoord)) {
            world.CheckViewDistance(chunkCoord);
            lastChunkCoord = chunkCoord;
        }
    }
    public void SetBBBL (Vector3 position, byte id) {

        ChunkVoxel pos = Data.Vector3ToChunkVoxel(position);


        Debug.Log(Time.deltaTime);

        if (id == 0) {
            Queue<VoxelAndPos> queue = new();
            queue.Enqueue(new(pos, id));
            world.AddMod(queue);
            world.SummonEntity(position + Vector3.one * 0.5f);


        } else if (!world.blockTypes[world.GetVoxelID(tryPlacingPos)].hasCollision && !entity.IsCollide(tryPlacingPos)) {

            Queue<VoxelAndPos> queue = new();
            queue.Enqueue(new(pos, id));
            world.AddMod(queue);

            hand.placeEase = 0;
        }
    }
    private void CalculateSelectingPos () {
        Vector3 _camPos = camera.position;
        for (int i = 0; i < 300; i++) {
            if (world.blockTypes[world.GetVoxelID(_camPos)].hasCollision) {
                break;
            }
            _camPos += camera.forward * 0.02f;
        }
        SelectingPos = Vector3Int.FloorToInt(_camPos);
        tryPlacingPos = Vector3Int.FloorToInt(CalculateNormal(_camPos) + _camPos);
    }
    private Vector3Int CalculateNormal (Vector3 pp) {

        Vector3Int normal = Vector3Int.zero;
        Vector3 p = new Vector3(pp.x < 0 ? 1 : 0, pp.y < 0 ? 1 : 0, pp.z < 0 ? 1 : 0) + new Vector3(pp.x % 1, pp.y % 1, pp.z % 1) - Vector3.one * 0.5f;
        Vector3 v = new(Mathf.Abs(p.x), Mathf.Abs(p.y), Mathf.Abs(p.z));

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
