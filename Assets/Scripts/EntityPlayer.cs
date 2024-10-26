using UnityEngine;

public class EntityPlayer : EntityLiving {

    private Vector2Int chunkCoord;
    private Vector2Int lastChunkCoord;

    public EntityPlayer (World world) : base(world) {

        Vector3 a = world.GetSpawnPoint();
        SetPosition(a.x, a.y, a.z);

    }

    public override void Update () {

        base.Update();

        chunkCoord = Data.Vector3ToChunkVoxel(new((float)posX, (float)posY, (float)posZ)).c;
        if (chunkCoord != lastChunkCoord) {
            world.CheckViewDistance(chunkCoord);
            lastChunkCoord = chunkCoord;
        }
    }
}
