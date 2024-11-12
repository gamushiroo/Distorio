using System.Collections.Generic;
using UnityEngine;

public class EntityPlayer : EntityLiving {

    private Vector2Int chunkCoord;
    private Vector2Int lastChunkCoord;
    private Vector3Int tryPlacingPos;
    private Vector3Int lastTryPlacingPos;
    private Vector3Int SelectingPos;
    private float rotationX;
    private float rotationY;
    private bool isMining;
    private Vector3Int miningPos;
    private readonly Transform cam;
    private readonly Camera camObj;
    private float miningProgress;
    private float mineSpeed;
    private Vector3 forceAcc;
    private float distance;
    private float coolDown;
    private bool nextFramePlaced;


    public EntityPlayer (World world, Vector3 pos) : base(world) {
        lastTryPlacingPos = Vector3Int.zero;
        coolDown = 0;
        SetPosition(pos.x, pos.y, pos.z);
        camObj = world.camObj;
        cam = world.cam;
        mineSpeed = 2.5f;

    }

    private protected override void Update () {


        if (Input.GetKey(KeyCode.C)) {
            camObj.fieldOfView = 20;
        } else {
            camObj.fieldOfView = 70;
        }
        coolDown += Time.deltaTime;
        rotationX -= Data.mouseSens * Input.GetAxisRaw("Mouse Y") * Time.deltaTime;
        rotationY += Data.mouseSens * Input.GetAxisRaw("Mouse X") * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, -90, 90);

        if (Input.GetKey(KeyCode.Space) && isGrounded) {
            velocity += Vector3.up * Mathf.Sqrt(2 * Data.gravityScale * (Data.jumpScale + 0.4F));
        }

        forceAcc = Vector3.zero;
        forceAcc = Data.resistance * (Quaternion.Euler(0, rotationY, 0) * PlayerVel() * Data.playerSpeed - inputVelocity);
        inputVelocity += forceAcc * Time.deltaTime;
        distance += inputVelocity.magnitude * Time.deltaTime;
        base.Update();

        cam.position = new Vector3((float)posX, (float)posY, (float)posZ) + Vector3.up * EyeHeight();
        cam.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        chunkCoord = Data.Vector3ToChunkVoxel(new((float)posX, (float)posY, (float)posZ)).c;
        if (chunkCoord != lastChunkCoord) {
            lastChunkCoord = chunkCoord;
            world.CheckViewDistance(chunkCoord);
        }

        CalculateSelectingPos();

        isMining = Input.GetMouseButton(0) && world.blockTypes[world.GetVoxelID(SelectingPos)].hasCollision;
        if (isMining) {
            if (miningPos != SelectingPos) {
                miningPos = SelectingPos;
                miningProgress = 0;
            }
            miningProgress += Time.deltaTime * mineSpeed;
            if (miningProgress >= world.blockTypes[world.GetVoxelID(SelectingPos)].hardness) {
                DestroyBlock(SelectingPos);
            }
            world.miningProgresBar.value = miningProgress / world.blockTypes[world.GetVoxelID(SelectingPos)].hardness;
        }

        if (nextFramePlaced) {
            lastTryPlacingPos = tryPlacingPos;
            nextFramePlaced = false;
        }
        if (!IsCollide(tryPlacingPos)) {
            if (Input.GetMouseButton(1) && coolDown >= 0.3f || Input.GetMouseButtonDown(1) || Input.GetMouseButton(1) && tryPlacingPos != lastTryPlacingPos) {
                if (world.SetBlock(tryPlacingPos, SelectingPos)) {
                    coolDown = 0;
                    lastTryPlacingPos = tryPlacingPos;
                    nextFramePlaced = true;
                }
            }
        }

        world.blockHighlight.transform.position = SelectingPos + Vector3.one * 0.5f;
        world.miningEffect.transform.position = SelectingPos + Vector3.one * 0.5f;

        world.miningEffect.SetActive(isMining);
        world.miningProgresBarObj.SetActive(isMining);
        world.blockHighlight.SetActive(world.blockTypes[world.GetVoxelID(SelectingPos)].hasCollision);

        world.hpBar.value = health / 20;
        world.hpText.text = forceAcc.magnitude.ToString("F") + "m/s^2\n" + (inputVelocity).magnitude.ToString("F") + "m/s\n" + distance.ToString("F") + "m\n";
    }
    private float EyeHeight () {
        float value = 1.62F;
        if (Input.GetKey(KeyCode.LeftShift)) {
            value -= 0.08F;
        }
        return value;
    }
    private void CalculateSelectingPos () {
        Vector3 _camPos = cam.position;
        for (int i = 0; i < 300; i++) {
            if (world.blockTypes[world.GetVoxelID(_camPos)].hasCollision) {
                break;
            }
            _camPos += cam.forward * 0.02f;
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
    private protected override void OnGrounded () {

        AddHealth(Mathf.Min(0, 13 + velocity.y));
    }
    private void DestroyBlock (Vector3 position) {
        ChunkVoxel pos = Data.Vector3ToChunkVoxel(position);
        Queue<VoxelAndPos> queue = new();
        queue.Enqueue(new(pos, 0));
        world.AddMod(queue);
    }

    public static Vector3 PlayerVel () {

        int x = 0;
        int y = 0;
        int z = 0;
        float dash = 0;
        float shift = 1;

        if (Input.GetKey(KeyCode.D))
            x++;
        if (Input.GetKey(KeyCode.A))
            x--;
        if (Input.GetKey(KeyCode.E))
            y++;
        if (Input.GetKey(KeyCode.Q))
            y--;
        if (Input.GetKey(KeyCode.W))
            z++;
        if (Input.GetKey(KeyCode.S))
            z--;
        if (z == 1 && Input.GetKey(KeyCode.LeftControl))
            dash = 1.0F / 3.0F;
        if (Input.GetKey(KeyCode.LeftShift))
            shift = 0.4F;

        return (new Vector3(x, y, z).normalized + dash * Vector3.forward) * shift;

    }
}
