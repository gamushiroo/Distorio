using System.Collections.Generic;
using UnityEngine;

public class EntityPlayer : EntityLiving {

    private Vector2Int chunkCoord;
    private Vector2Int lastChunkCoord;
    private Vector3Int tryPlacingPos;
    private Vector3Int lastTryPlacingPos;
    private Vector3Int SelectingPos;
    private bool isMining;
    private Vector3Int miningPos;
    private readonly Transform camTransform;
    private readonly Camera cam;
    private float miningProgress;
    private float mineSpeed;
    private float coolDown;
    private bool nextFramePlaced;

    public EntityPlayer (World world, Vector3 pos) : base(world) {
        lastTryPlacingPos = Vector3Int.zero;
        coolDown = 0;
        SetPosition(pos.x, pos.y, pos.z);
        cam = world.camObj;
        camTransform = world.cam;
        mineSpeed = 2.5f;

    }

    private protected override void Update () {

        cam.fieldOfView = GetFieldOfView();
        ApplyRotation();
        if (Input.GetKey(KeyCode.Space) && isGrounded) {
            AddVelocity(0, GetJumpPower(), 0);
        }

        inputVelocity += ((isGrounded ? Data.resistance : Data.resistance * 0.2F) * (Quaternion.Euler(0, rotationYaw, 0) * PlayerVel() * Data.playerSpeed - inputVelocity)) * Time.deltaTime;

        base.Update();

        camTransform.position = new((float)posX, (float)posY + GetEyeHeight(), (float)posZ);
        camTransform.rotation = Quaternion.Euler(rotationPitch, rotationYaw, 0);
        chunkCoord = Data.Vector3ToChunkVoxel(new((float)posX, (float)posY, (float)posZ)).c;
        if (chunkCoord != lastChunkCoord) {
            lastChunkCoord = chunkCoord;
            world.CheckViewDistance(chunkCoord);
        }

        if (IsTouching(15)) {
            AddHealth(3 * Time.deltaTime);
        };

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
        coolDown += Time.deltaTime;
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

        world.hpBar.value = health / maxHealth;
    }
    private float GetJumpPower () {
        return Mathf.Sqrt(2 * Data.gravityScale * (Data.jumpScale + 0.4F));
    }
    private void ApplyRotation () {
        rotationPitch -= Data.mouseSens * Input.GetAxisRaw("Mouse Y") * Time.deltaTime;
        rotationYaw += Data.mouseSens * Input.GetAxisRaw("Mouse X") * Time.deltaTime;
        rotationPitch = Mathf.Clamp(rotationPitch, -90, 90);
    }
    private float GetFieldOfView () {
        if (Input.GetKey(KeyCode.C)) {
            return 20;
        } else {
            return 70;
        }
    }
    private bool IsTouching (int check) {
        List<int> ids = CollidingIDs();
        foreach (int id in ids) {
            if (id == check) {
                return true;
            }
        }
        return false;
    }
    private float GetEyeHeight () {
        float value = 1.62F;
        if (Input.GetKey(KeyCode.LeftShift)) {
            value -= 0.08F;
        }
        return value;
    }
    private void CalculateSelectingPos () {
        Vector3 _camPos = camTransform.position;
        for (int i = 0; i < 300; i++) {
            if (world.blockTypes[world.GetVoxelID(_camPos)].hasCollision) {
                break;
            }
            _camPos += camTransform.forward * 0.02f;
        }
        SelectingPos = Vector3Int.FloorToInt(_camPos);
        tryPlacingPos = Vector3Int.FloorToInt(CalculateNormal(_camPos) + _camPos);

        Vector3Int CalculateNormal (Vector3 pp) {
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
    private protected override void OnGrounded () {

        AddHealth(Mathf.Min(0, 13 + (float)motionY));
    }
    private void DestroyBlock (Vector3 position) {
        ChunkVoxel pos = Data.Vector3ToChunkVoxel(position);
        Queue<VoxelAndPos> queue = new();
        queue.Enqueue(new(pos, 0));
        world.AddMod(queue);
    }
    private Vector3 PlayerVel () {
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
