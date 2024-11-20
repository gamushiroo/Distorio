using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityPlayer : EntityLiving {
    private Vec3i chunkCoord;
    private Vec3i lastChunkCoord;
    private Vector3Int tryPlacingPos;
    private Vector3Int lastTryPlacingPos = Vector3Int.zero;
    private Vector3Int SelectingPos;
    private Vector3Int miningPos;
    private readonly Transform camTransform;
    private readonly Camera cam;
    private float miningProgress;
    private readonly float mineSpeed = 2.5f;
    private float coolDown = 0;
    private bool nextFramePlaced;
    public EntityPlayer (World world) : base(world) {
        SetPositionToSpawnPoint();
        SetVelocity(0.0D, 0.0D, 0.0D);
        cam = world.camObj;
        camTransform = world.cam;
    }

    private protected override void Update () {

        if (Input.GetKeyDown(KeyCode.R)) {
            SetPositionToSpawnPoint();
            SetVelocity(0.0D, 0.0D, 0.0D);
        }

        if (Input.GetKey(KeyCode.C)) {
            cam.fieldOfView = 20;
        } else {
            cam.fieldOfView = 70;
        }

        if (Input.GetKey(KeyCode.Space) && isGrounded) {
            AddVelocity(0, Data.jumpPower, 0);
        }

        rotationPitch -= Data.mouseSens * Input.GetAxisRaw("Mouse Y") * Time.deltaTime;
        rotationYaw += Data.mouseSens * Input.GetAxisRaw("Mouse X") * Time.deltaTime;
        rotationPitch = Mathf.Clamp(rotationPitch, -90, 90);

        Vector3 f = PlayerVel() * Time.deltaTime;
        AddVelocity(f.x, f.y, f.z);

        base.Update();
        camTransform.SetPositionAndRotation(Vec3.ToVector3(posX, posY + eyeHeight, posZ), Quaternion.Euler(rotationPitch, rotationYaw, 0));
        chunkCoord = Vec3i.ToChunkCoord(posX, posY, posZ);
        if (!chunkCoord.Equals(lastChunkCoord)) {
            lastChunkCoord = chunkCoord;
            world.CheckViewDistance(chunkCoord);
        }

        if (IsTouching(15)) {
            AddHealth(3 * Time.deltaTime);
        }
        CalculateSelectingPos();
        bool isMining = Input.GetMouseButton(0) && world.blockTypes[world.GetVoxelID(SelectingPos)].hasCollision;
        if (isMining) {
            if (miningPos != SelectingPos) {
                miningPos = SelectingPos;
                miningProgress = 0;
            }
            miningProgress += Time.deltaTime * mineSpeed;
            if (miningProgress >= world.blockTypes[world.GetVoxelID(SelectingPos)].hardness) {
                world.DestroyBlock(SelectingPos);
            }
            world.miningProgresBar.value = miningProgress / world.blockTypes[world.GetVoxelID(SelectingPos)].hardness;
        }
        if (nextFramePlaced) {
            lastTryPlacingPos = tryPlacingPos;
            nextFramePlaced = false;
        }
        coolDown += Time.deltaTime;
        if (!BoundingBox.IntersectsWith(tryPlacingPos) && (Input.GetMouseButton(1) && (coolDown >= 0.3f || tryPlacingPos != lastTryPlacingPos) || Input.GetMouseButtonDown(1))) {
            if (world.SetBlock(tryPlacingPos, SelectingPos)) {
                coolDown = 0;
                lastTryPlacingPos = tryPlacingPos;
                nextFramePlaced = true;
            }
        }
        world.blockHighlight.transform.position = SelectingPos + Vector3.one * 0.5f;
        world.miningEffect.transform.position = SelectingPos + Vector3.one * 0.5f;
        //world.miningEffect.SetActive(isMining);
        world.miningProgresBarObj.SetActive(isMining);
        world.blockHighlight.SetActive(world.blockTypes[world.GetVoxelID(SelectingPos)].hasCollision);
        world.hpBar.value = health / maxHealth;
        bool IsTouching (int check) {
            List<int> ids = world.CollidingIDs(BoundingBox);
            foreach (int id in ids) {
                if (id == check) {
                    return true;
                }
            }
            return false;
        }
        void CalculateSelectingPos () {
            Vector3 _camPos = new Vector3((float)posX, (float)posY + (float)eyeHeight, (float)posZ) ;
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
        Vector3 PlayerVel () {
            return (isGrounded ? Data.resistance : Data.resistance * 0.2F) * (Quaternion.Euler(0, rotationYaw, 0) * PlayerInput() * Data.playerSpeed - new Vector3((float)motionX, 0, (float)motionZ));
            Vector3 PlayerInput () {
                int x = 0;
                int z = 0;
                float dash = 0;
                if (Input.GetKey(KeyCode.D))
                    x++;
                if (Input.GetKey(KeyCode.A))
                    x--;
                if (Input.GetKey(KeyCode.W))
                    z++;
                if (Input.GetKey(KeyCode.S))
                    z--;
                if (z == 1 && Input.GetKey(KeyCode.LeftControl))
                    dash = 1.0F / 3.0F;
                return (new Vector3(x, 0, z).normalized + dash * Vector3.forward) * (float)Math.Pow(height / defaultHeight, 1.5F);
            }
        }
    }
    private protected override void OnGrounded () {
        AddHealth(Mathf.Min(0, 13 + (float)motionY));
    }
}
