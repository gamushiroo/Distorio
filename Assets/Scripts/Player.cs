using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    public int selectedBlockIndex = 0;

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
    public EntityPlayer entity;
    public Inventory inventory;

    public bool isDead;
    public float mineSpeed;
    public bool inUI;

    private float rotationX;
    private float rotationY;

    public void Awake () {

        inventory = new();

    }

    private void Start () {

        entity = new(world);

    }

    private void Update () {

        if (!isDead) {
            if (!inUI) {

                rotationX -= Data.mouseSens * Input.GetAxisRaw("Mouse Y") * Time.deltaTime;
                rotationY += Data.mouseSens * Input.GetAxisRaw("Mouse X") * Time.deltaTime;
                rotationX = Mathf.Clamp(rotationX, -90, 90);

                mineSpeed = selectedBlockIndex >= 128 ? 4 : 0.8f;

                isMining = Input.GetMouseButton(0) && world.blockTypes[world.GetVoxelID(SelectingPos)].hasCollision;


                if (Input.GetKey(KeyCode.Space)) {
                    entity.TryJump();
                }
                entity.AddVel(Quaternion.Euler(0, rotationY, 0) * Data.GetPlayerVel());

            }


            entity.Update();

            CalculateSelectingPos();
            if (isMining) {
                if (!Equals(miningPos, SelectingPos)) {
                    miningPos = SelectingPos;
                    miningProgress = world.blockTypes[world.GetVoxelID(SelectingPos)].hardness;
                }
                miningProgress -= Time.deltaTime * mineSpeed;
                if (miningProgress <= 0) {
                    DestroyBlock(SelectingPos);
                }
                miningProgresBar.value = miningProgress / world.blockTypes[world.GetVoxelID(SelectingPos)].hardness;
            }
            camera.position = new Vector3((float)entity.posX, (float)entity.posY, (float)entity.posZ) + Vector3.up * 1.625f;
            camera.rotation = Quaternion.Euler(rotationX, rotationY, 0);

            blockHighlight.transform.position = SelectingPos + Vector3.one * 0.5f;
            miningEffect.transform.position = SelectingPos + Vector3.one * 0.5f;

            miningEffect.SetActive(isMining);
            miningProgresBarObj.SetActive(isMining);
            blockHighlight.SetActive(world.blockTypes[world.GetVoxelID(SelectingPos)].hasCollision && !inUI);

        }
    }

    private void DestroyBlock (Vector3 position) {
        ChunkVoxel pos = Data.Vector3ToChunkVoxel(position);
        Queue<VoxelAndPos> queue = new();
        queue.Enqueue(new(pos, 0));
        world.AddMod(queue);
    }

    public void SetBlock (Vector3 position, int id) {

        ChunkVoxel pos = Data.Vector3ToChunkVoxel(position);
        if (id != 0 && !world.blockTypes[world.GetVoxelID(tryPlacingPos)].hasCollision && !entity.IsCollide(tryPlacingPos)) {

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
