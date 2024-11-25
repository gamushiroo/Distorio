using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityPlayer : EntityLiving {
    private protected float rotationPitch;
    private protected float rotationYaw;
    private Vec3i chunkCoord;
    private Vec3i lastChunkCoord;
    private Vector3Int tryPlacingPos;
    private Vector3Int lastTryPlacingPos = Vector3Int.zero;
    private Vector3Int SelectingPos;
    private Vector3Int miningPos;
    private readonly Camera camera;
    private readonly Transform cameraTransform;
    private float miningProgress;
    private readonly float mineSpeed = 2.5f;
    private float coolDown = 0;
    private bool nextFramePlaced;

    private float gunCoolDown = 0;

    float projectiles = 50;
    float spread = 10;
    float initialVelocity = 100;

    public EntityPlayer (World world) : base(world) {
        camera = world.camObj;
        cameraTransform = world.cam;
    }
    private protected override void Initialize () {

        meshType = 19;
        base.Initialize();
        ToWorldSpawn();
        projectiles = 35;
        spread = 8;
        initialVelocity = 170;


        string proj = "Projectiles: " + projectiles.ToString() + "\n";
        string spre = "Spread: " + spread.ToString() + " degrees\n";
        string inve = "Initial Velocity: " + initialVelocity.ToString() + "\n";

        world.hpText.text = proj + spre + inve;

    }
    void ToWorldSpawn () {
        SetPosition(0.0D, 0.0D, 0.0D);
        while (world.GetCollidingBoundingBoxes(BoundingBox, ID).Count != 0) {
            AddCoordinate(0.0D, 1.0D, 0.0D);
        }
    }
    public override void Update () {

        gunCoolDown += Time.deltaTime;
        rotationPitch -= Data.mouseSens * Input.GetAxisRaw("Mouse Y") * Time.deltaTime;
        rotationYaw += Data.mouseSens * Input.GetAxisRaw("Mouse X") * Time.deltaTime;
        rotationPitch = Mathf.Clamp(rotationPitch, -90, 90);

        if (Input.GetKeyDown(KeyCode.R)) {
            Initialize();
        }
        if (Input.GetKey(KeyCode.Space) && isGrounded) {
            AddForce_Impulse(0, Data.jumpPower, 0);
        }
        if (Input.GetMouseButton(0)) {
            ShootGun();
        }
        AddForce(Vec3.ToVec3(PlayerVel()));

        camera.fieldOfView = Input.GetMouseButton(1) ? 50 : 70;

        base.Update();
        double t = resistance * ((Input.GetKey(KeyCode.LeftShift) ? defaultHeight / 2 : defaultHeight) - height) * Time.deltaTime;
        List<AABB> others = world.GetCollidingBoundingBoxes(BoundingBox.AddCoord(0, Math.Max(t, 0), 0), ID);
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
        world.miningProgresBarObj.SetActive(isMining);
        world.blockHighlight.SetActive(world.blockTypes[world.GetVoxelID(SelectingPos)].hasCollision);
        world.hpBar.value = health / maxHealth;
        cameraTransform.SetPositionAndRotation(GetCamPos(), GetRotation());
        world.healing.SetActive(isHealed);
        world.healing.transform.position = transform.position;
    }
    protected void ShootGun () {

        if (gunCoolDown >= 0.5F) {

            for (int i = 0; i < projectiles; i++) {
                float rand1 = UnityEngine.Random.Range(-180, 180) * Mathf.Deg2Rad;
                float rand2 = UnityEngine.Random.Range(-spread, spread) / 2 * Mathf.Deg2Rad;
                Vector3 ttt = new Vector3(Mathf.Cos(rand1) * Mathf.Sin(rand2), Mathf.Sin(rand1) * Mathf.Sin(rand2), Mathf.Cos(rand2)) * initialVelocity;
                Vector3 aaa = GetRotation() * ttt;
                world.entityQueue.Enqueue(new EntityProjectile(posX, eyeHeight + posY, posZ, aaa, world));
            }
            audioSource.PlayOneShot(world.dd, 0.5F);
            gunCoolDown = 0;
        }
    }



    void CalculateSelectingPos () {
        Vector3 _camPos = GetCamPos();
        Vector3 _camPos2 = GetCamPos();
        for (int i = 0; i < 300; i++) {
            if (world.blockTypes[world.GetVoxelID(_camPos)].hasCollision) {
                break;
            }
            _camPos2 = _camPos;
            _camPos += GetRotation() * Vector3.forward * 0.02f;
        }
        SelectingPos = Vector3Int.FloorToInt(_camPos);
        tryPlacingPos = Vector3Int.FloorToInt(_camPos2);
    }

    Vector3 GetCamPos () {
        return Vec3.ToVector3(posX, posY + eyeHeight, posZ);
    }
    Quaternion GetRotation () {
        return Quaternion.Euler(rotationPitch, rotationYaw, 0);
    }
    Vector3 PlayerVel () {
        return (isGrounded ? resistance : resistance * 0.2F) * (Quaternion.Euler(0, rotationYaw, 0) * PlayerInput() - Vec3.ToVector3(velocityX, 0, velocityZ));
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
