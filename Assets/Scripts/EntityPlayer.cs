using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityPlayer : EntityLiving {
    private protected float rotationPitch;
    private protected float rotationYaw;
    private Vec3i chunkCoord;
    private Vec3i lastChunkCoord;
    private Vector3Int SelectingPos;
    private Vector3Int miningPos;
    private readonly Camera camera;
    private readonly Transform cameraTransform;
    private float miningProgress;
    private readonly float mineSpeed = 2.5f;
    bool isMining;

    public float fovDef = 70;
    public float fovTarget;
    public float currentFov;

    public int currentItem = 0;
    Item[] items = new Item[2] { new ItemWeapon(), new ItemBlock() };

    public EntityPlayer (World world) : base(world) {
        camera = world.camObj;
        cameraTransform = world.cam;
    }
    private protected override void Initialize () {

        meshType = 19;
        base.Initialize();
        ToWorldSpawn();


    }
    void ToWorldSpawn () {
        SetPosition(0.0D, 0.0D, 0.0D);
        while (world.GetCollidingBoundingBoxes(BoundingBox, ID).Count != 0) {
            AddCoordinate(0.0D, 1.0D, 0.0D);
        }
    }
    public override void Update () {

        currentFov += (fovTarget - currentFov) * Time.deltaTime * resistance * 2;
        camera.fieldOfView = currentFov;
        fovTarget = fovDef;


        CalculateInput();
        base.Update();

        double t = resistance * ((Input.GetKey(KeyCode.LeftShift) ? defaultHeight / 2 : defaultHeight) - height) * Time.deltaTime;
        List<AABB> others = world.GetCollidingBoundingBoxes(BoundingBox.BroadPhase(0, Math.Max(t, 0), 0), ID);
        foreach (AABB other in others) {
            t = BoundingBox.CalculateYOffset(t, other);
        }
        height += t;
        eyeHeight = height * defaulteyeHeight;
        ModifyBoundingBox();

        chunkCoord = Vec3i.ToChunkCoord(posX, posY, posZ);
        if (!chunkCoord.Equals(lastChunkCoord)) {
            lastChunkCoord = chunkCoord;
            world.CheckViewDistance(chunkCoord);
        }
        if (world.CollidingIDs(BoundingBox).Contains(15)) {
            AddHealth(3 * Time.deltaTime);
        }
        CalculateItems();
        CalculateSelectingPos();
        isMining = Input.GetMouseButton(0) && GetVoxelID().hasCollision;
        if (isMining) {
            if (miningPos != SelectingPos) {
                miningPos = SelectingPos;
                miningProgress = 0;
            }
            miningProgress += Time.deltaTime * mineSpeed;
            world.miningProgresBar.value = miningProgress / GetVoxelID().hardness;
            if (miningProgress >= GetVoxelID().hardness) {
                world.DestroyBlock(SelectingPos);
            }
        }
        SetGameObjectState();
    }

    void SetGameObjectState () {
        cameraTransform.SetPositionAndRotation(GetCamPos(), GetRotation());
        world.blockHighlight.transform.position = SelectingPos + Vector3.one * 0.5f;
        world.miningEffect.transform.position = SelectingPos + Vector3.one * 0.5f;
        world.healing.transform.position = Vec3.ToVector3(posX, posY, posZ);
        world.hpBar.value = health / maxHealth;
        world.miningProgresBarObj.SetActive(isMining);
        world.blockHighlight.SetActive(GetVoxelID().hasCollision);
        world.healing.SetActive(isHealed);
    }

    void CalculateItems () {

        if (Input.GetAxis("Mouse ScrollWheel") != 0) {
            currentItem += Input.GetAxis("Mouse ScrollWheel") > 0 ? -1 : 1;
            currentItem = Math.Clamp(currentItem, 0, items.Length - 1);
        }
        for (int i = 0; i < items.Length; i++) {
            items[i].Update();
        }
        if (Input.GetMouseButton(0)) {
            items[currentItem].LeftMouseButton(world, this);
        }
        if (Input.GetMouseButton(1)) {
            items[currentItem].RightMouseButton(world, this);
        }
        if (Input.GetMouseButtonDown(1)) {
            items[currentItem].RightMouseButtonDown(world, this);
        }

    }

    void CalculateInput () {
        rotationPitch -= Data.mouseSens * Input.GetAxisRaw("Mouse Y") * Time.deltaTime;
        rotationYaw += Data.mouseSens * Input.GetAxisRaw("Mouse X") * Time.deltaTime;
        rotationPitch = Mathf.Clamp(rotationPitch, -90, 90);
        if (Input.GetKeyDown(KeyCode.R)) {
            Initialize();
        }
        if (Input.GetKey(KeyCode.Space) && isGrounded) {
            AddForce_Impulse(0, Data.jumpPower, 0);
        }
        AddForce(Vec3.ToVec3(PlayerVel()));
    }

    private BlockType GetVoxelID () {
        return world.blockTypes[world.GetVoxelID(SelectingPos)];
    }



    void CalculateSelectingPos () {
        Vector3 _camPos = GetCamPos();
        for (int i = 0; i < 300; i++) {
            if (world.blockTypes[world.GetVoxelID(_camPos)].hasCollision) {
                break;
            }
            _camPos += GetRotation() * Vector3.forward * 0.02f;
        }
        SelectingPos = Vector3Int.FloorToInt(_camPos);
    }
    public Vector3 GetCamPos () {
        return Vec3.ToVector3(posX, posY + eyeHeight, posZ);
    }
    public Quaternion GetRotation () {
        return Quaternion.Euler(rotationPitch, rotationYaw, 0);
    }
    Vector3 PlayerVel () {
        Vector3 a = Vector3.zero;
        if (!inTheWater) {
            a = (isGrounded ? resistance : resistance * 0.2F) * (Quaternion.Euler(0, rotationYaw, 0) * PlayerInput() - Vec3.ToVector3(velocityX, 0, velocityZ));
        } else {
            a = resistance * 0.12F * (Quaternion.Euler(rotationPitch, rotationYaw, 0) * PlayerInput());
        }

        return a;
    }
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
        return (float)Math.Pow(height / defaultHeight, 1.5F) * Data.playerSpeed * (new Vector3(x, 0, z).normalized + dash * Vector3.forward);
    }
    private protected override void OnGrounded () {
        AddHealth(Mathf.Min(0, 13 + (float)velocityY));
    }
}
