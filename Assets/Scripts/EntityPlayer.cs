using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityPlayer : EntityLiving {


    private Vec3i chunkCoord;
    private Vec3i lastChunkCoord;
    private readonly Camera camera;
    private readonly Transform cameraTransform;
    private readonly float fovDef = 70;

    public bool isMining;
    public float fovTarget;
    public float currentFov;

    public EntityPlayer (World world) : base(world) {


        camera = world.camObj;
        cameraTransform = world.cam;
    }
    private protected override void Initialize () {

        meshType = 1;
        base.Initialize();
        ToWorldSpawn();
        items.Add(new ItemWeapon());
        items.Add(new ItemBlock(1));


    }
    public override void Update () {

        isMining = false;
        fovTarget = fovDef;


        CalculateInput();

        base.Update();

        double t = resistance * ((Input.GetKey(KeyCode.LeftShift) ? defaultHeight / 2 : defaultHeight) - height) * Time.deltaTime;
        List<AABB> others = world.chunkManager.GetCollidingBoundingBoxes(BoundingBox.BroadPhase(0, Math.Max(t, 0), 0), ID);
        foreach (AABB other in others) {
            t = BoundingBox.CalculateYOffset(t, other);
        }
        height += t;
        eyeHeight = height * defaulteyeHeight;
        ModifyBoundingBox();

        chunkCoord = Vec3i.ToChunkCoord(posX, posY, posZ);
        if (!chunkCoord.Equals(lastChunkCoord)) {
            lastChunkCoord = chunkCoord;
            world.chunkManager.LoadChunksAround(new(chunkCoord.x, chunkCoord.z));
        }
        if (world.chunkManager.CollidingIDs(BoundingBox).Contains(3)) {
            AddHealth(3 * Time.deltaTime);
        }
        CalculateItems();
        SetGameObjectState();
    }

    void SetGameObjectState () {
        currentFov += (fovTarget - currentFov) * Time.deltaTime * resistance * 2;
        camera.fieldOfView = currentFov;
        cameraTransform.SetPositionAndRotation(GetCamPos(), GetRotation());
        world.healing.transform.position = Vec3.ToVector3(posX, posY, posZ);
        world.hpBar.value = health / maxHealth;
        world.miningProgresBarObj.SetActive(isMining);
        world.healing.SetActive(isHealed);
    }

    void CalculateItems () {

        if (Input.GetAxis("Mouse ScrollWheel") != 0) {
            currentItem += Input.GetAxis("Mouse ScrollWheel") > 0 ? -1 : 1;
            currentItem = Math.Clamp(currentItem, 0, items.Count - 1);
        }
        if (Input.GetMouseButton(0)) {
            items[currentItem].LeftMouseButton(world, this);
        }
        if (Input.GetMouseButtonDown(0)) {
            items[currentItem].LeftMouseButtonDown(world, this);
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
