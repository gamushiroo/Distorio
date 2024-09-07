using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    public float rotationX;
    public byte selectedBlockIndex = 0;
    public MyUI myUI;

    [SerializeField] private World world;
    [SerializeField] private Hand hand;
    [SerializeField] public new Transform camera;
    [SerializeField] private GameObject blockHighlight;
    [SerializeField] private GameObject miningEffect;
    [SerializeField] private GameObject miningProgresBarObj;
    [SerializeField] private Slider miningProgresBar;
    private bool isMining = false;
    private ChunkVoxel miningPos;
    private ChunkVoxel tryPlacingPos;
    private ChunkVoxel SelectingPos;
    private float miningProgress;
    private Vector2Int chunkCoord;
    private Vector2Int lastChunkCoord;
    public Entity entity;
    private void Start () {

        entity = new(Data.player.size, world, Data.player.health);

    }
    private void Update () {

        if (entity.isAllive) {

            if (Input.GetKeyDown(KeyCode.E)) {

                myUI.Switch();

            }

            if (!myUI.inUI)  {


                isMining = Input.GetMouseButton(0) && selectedBlockIndex >= 128 && world.blockTypes[world.GetVoxelID(SelectingPos)].hasCollision;


                rotationX -= Data.mouseSens * Time.deltaTime * Input.GetAxisRaw("Mouse Y");
                rotationX = Mathf.Clamp(rotationX, -90, 90);

                entity.AddRotation(Data.mouseSens * Input.GetAxisRaw("Mouse X") * Time.deltaTime);
                entity.AddVel(Data.GetPlayerVel(), Input.GetKey(KeyCode.Space));

                if (Input.GetMouseButtonDown(0) && !world.blockTypes[world.GetVoxelID(tryPlacingPos)].hasCollision && selectedBlockIndex < 128 && world.blockTypes[world.GetVoxelID(SelectingPos)].hasCollision) {
                    SetBBBL(tryPlacingPos, selectedBlockIndex);
                }
            }

            SetValue();
            CheckPos();

        } 
        
        if (Input.GetKeyDown(KeyCode.R) && !entity.isAllive) {

            entity.Init();
        }
    }
    private void SetValue () {

        entity.Apply();

        CalculateSelectingPos();
        
        if (isMining) {
            if (!ChunkVoxel.Equal(miningPos, SelectingPos)) {
                miningPos = SelectingPos;
                miningProgress = world.blockTypes[world.GetVoxelID(SelectingPos)].hardness;
            }
            miningProgress -= Time.deltaTime * GetMiningSpeed(selectedBlockIndex - 128, 0.85f, 1f, 0.75f);
            if (miningProgress <= 0) {
                SetBBBL(SelectingPos, 0);
            }
            miningProgresBar.value = miningProgress / world.blockTypes[world.GetVoxelID(SelectingPos)].hardness;
        }

        camera.position = entity.pos + Vector3.up * 1.625f;
        camera.rotation = Quaternion.Euler(rotationX, entity.Rotation, 0);

        blockHighlight.transform.position = Data.PublicLocationDerect(SelectingPos) + Vector3.one * 0.5f;
        miningEffect.transform.position = Data.PublicLocationDerect(SelectingPos) + Vector3.one * 0.5f;

        miningEffect.SetActive(isMining);
        miningProgresBarObj.SetActive(isMining);
        blockHighlight.SetActive(world.blockTypes[world.GetVoxelID(SelectingPos)].hasCollision);
    }
    private void CheckPos () {
        chunkCoord = Data.Vector3ToChunkVoxel(entity.pos).c;
        if (!(chunkCoord == lastChunkCoord)) {
            world.CheckViewDistance(chunkCoord);
            lastChunkCoord = chunkCoord;
        }
    }
    float GetMiningSpeed (int n, float a, float l, float o) {
        return l * ((Mathf.Pow(a, n) - 1) / (a - 1)) + o;
    }
    void SetBBBL (ChunkVoxel pos, byte id) {
        if(id == 9) {

            bool canPlace = true;


            for (int x = 0; x < 2; x++) {
                for (int y = 0; y < 3; y++) {
                    for (int z = 0; z < 2; z++) {
                        ChunkVoxel checking = Data.Vector3ToChunkVoxel(Data.PublicLocationDerect(tryPlacingPos) + new Vector3Int(x, y, z) + Vector3.one * 0.5f);
                        if (world.blockTypes[world.GetVoxelID(checking)].hasCollision || entity.IsCollide(checking)) {
                            canPlace = false;
                        }

                    }
                }
            }

            if(canPlace) {
                hand.placeEase = 0;
                world.AddMod(Structure.MakeFurnace(Data.PublicLocationDerect(pos)));
            }


        } else if ( !world.blockTypes[world.GetVoxelID(tryPlacingPos)].hasCollision && !entity.IsCollide(tryPlacingPos)) {

            Queue<VoxelAndPos> queue = new();
            queue.Enqueue(new(pos, id));
            world.AddMod(queue);

            hand.placeEase = 0;
        }
    }
    private void CalculateSelectingPos () {
        Vector3 _camPos = camera.position;
        for (int i = 0; i < 300; i++) {
            if (world.blockTypes[world.GetVoxelID(Data.Vector3ToChunkVoxel(_camPos))].hasCollision) {
                break;
            }
            _camPos += camera.forward * 0.02f;
        }
        SelectingPos = Data.Vector3ToChunkVoxel(_camPos);
        tryPlacingPos = Data.Vector3ToChunkVoxel(CalculateNormal(_camPos) + _camPos);
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
