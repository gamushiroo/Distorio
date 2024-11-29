using UnityEngine;

public class ItemBlock : Item {


    private Vector3Int tryPlacingPos;
    private Vector3Int SelectingPos;
    private Vector3Int miningPos;
    private float miningProgress = 0;
    private float coolDown = 0;

    private int itemID = 0;
    public ItemBlock (int itemID) {
        this.itemID = itemID;
    }

    public override void LeftMouseButton (World world, EntityPlayer player) {
        CalculateSelectingPos(world, player);
        if (coolDown >= 0.3f && TryPlace(world, player)) {
            coolDown = 0;
        }
    }
    public override void RightMouseButton (World world, EntityPlayer player) {

        CalculateSelectingPos(world, player);
        BlockType eee = world.blockTypes[world.GetVoxelID(SelectingPos)];
        if (eee.hasCollision) {
            if (miningPos != SelectingPos) {
                miningPos = SelectingPos;
                miningProgress = 0;
            }
            if (miningProgress >= eee.hardness) {
                world.DestroyBlock(SelectingPos);
            }
            miningProgress += Time.deltaTime * 2.5F;
            world.miningProgresBar.value = miningProgress / eee.hardness;
            player.isMining = true;
        }
    }
    public override void LeftMouseButtonDown (World world, EntityPlayer player) {
        if (coolDown != 0 && TryPlace(world, player)) {
            coolDown = 0;
        }
    }
    public override void Update () {
        coolDown += Time.deltaTime;
    }

    bool TryPlace (World world, EntityPlayer player) {
        CalculateSelectingPos(world, player);
        return !player.BoundingBox.IntersectsWith(Vec3i.ToVec3i(tryPlacingPos)) && world.SetBlock(tryPlacingPos, SelectingPos, itemID);
    }
    void CalculateSelectingPos (World world, EntityPlayer player) {
        Vector3 _camPos = player.GetCamPos();
        Vector3 _camPos2 = _camPos;
        Quaternion _camRot = player.GetRotation();
        for (int i = 0; i < 300; i++) {
            if (world.blockTypes[world.GetVoxelID(_camPos)].hasCollision) {
                break;
            }
            _camPos2 = _camPos;
            _camPos += _camRot * Vector3.forward * 0.02f;
        }
        SelectingPos = Vector3Int.FloorToInt(_camPos);
        tryPlacingPos = Vector3Int.FloorToInt(_camPos2);
    }
}
