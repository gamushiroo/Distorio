using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class World : MonoBehaviour {
    public GameObject particle;
    public PathRenderer pathrend;
    public Slider hpBar;
    public Text hpText;
    public AudioClip eeeeeee;
    public AudioClip dd;
    public AudioSource audioSource;
    public Material[] materials = new Material[2];
    public List<BlockType> blockTypes = new();
    public List<ItemType> itemTypes = new();
    public List<Entity> entities = new();
    public Transform backGround;
    public Transform cam;
    public Camera camObj;
    public Hand hand;
    public GameObject miningProgresBarObj;
    public UserInterface userInter;
    public GameObject blockHighlight;
    public GameObject miningEffect; public Slider miningProgresBar;
    private Vector2 offset;
    public GameObject healing;


    public Queue<Entity> entityQueue = new();

    private readonly Dictionary<Vector2Int, Chunk> chunks = new();
    private readonly Queue<Queue<VoxelAndPos>> modifications = new();
    private readonly List<Chunk> chunksToUpdate = new();
    private readonly Queue<Chunk> chunksToDraw = new();



    private void Awake () {
        Item.RegisterItems();
        InitNoise();
        GenerateWorld();
        ModifyChunks();
        UpdateChunks();

        void GenerateWorld () {
            for (int x = -Data.ChunkLoadRange; x < Data.ChunkLoadRange; x++) {
                for (int y = -Data.ChunkLoadRange; y < Data.ChunkLoadRange; y++) {
                    Vector2Int pos = new(x, y);
                    chunks.Add(pos, new(pos, this));
                    chunks[pos].GenerateTerrainData();
                    chunksToUpdate.Add(chunks[pos]);
                }
            }
        }
    }
    private void Start () {
        userInter.inventory = new();
        entities.Add(new EntityPlayer(this));
        //entities.Add(new EntityTester(this));

    }

    public void DestroyBlock (Vector3 position) {
        ChunkVoxel pos = Data.Vector3ToChunkVoxel(position);
        Queue<VoxelAndPos> queue = new();
        queue.Enqueue(new(pos, 0));
        AddMod(queue);
    }
    /*
    public MovingObjectPosition rayTraceBlocks (Vec3 eyePos, Vec3 to, bool stopOnLiquid, bool returnLastUncollidableBlock) {
        int toX = MathHelper.FloorDouble(to.xCoord);
        int toY = MathHelper.FloorDouble(to.yCoord);
        int toZ = MathHelper.FloorDouble(to.zCoord);
        int eyeX = MathHelper.FloorDouble(eyePos.xCoord);
        int eyeY = MathHelper.FloorDouble(eyePos.yCoord);
        int eyeZ = MathHelper.FloorDouble(eyePos.zCoord);
        BlockPos blockpos = new BlockPos(eyeX, eyeY, eyeZ);
        IBlockState iblockstate = this.getBlockState(blockpos);
        Block block = iblockstate.getBlock();

        if (( block.getCollisionBoundingBox(this, blockpos, iblockstate) != null) && block.canCollideCheck(iblockstate, stopOnLiquid)) {
            MovingObjectPosition movingobjectposition = block.collisionRayTrace(this, blockpos, eyePos, to);

            if (movingobjectposition != null) {
                return movingobjectposition;
            }
        }

        MovingObjectPosition movingobjectposition2 = null;
        int k1 = 200;

        while (k1-- >= 0) {

            if (eyeX == toX && eyeY == toY && eyeZ == toZ) {
                return returnLastUncollidableBlock ? movingobjectposition2 : null;
            }

            bool flag2 = true;
            bool flag = true;
            bool flag1 = true;
            double d0 = 999.0D;
            double d1 = 999.0D;
            double d2 = 999.0D;

            if (toX > eyeX) {
                d0 = eyeX + 1.0D;
            } else if (toX < eyeX) {
                d0 = eyeX + 0.0D;
            } else {
                flag2 = false;
            }

            if (toY > eyeY) {
                d1 = eyeY + 1.0D;
            } else if (toY < eyeY) {
                d1 = eyeY + 0.0D;
            } else {
                flag = false;
            }

            if (toZ > eyeZ) {
                d2 = eyeZ + 1.0D;
            } else if (toZ < eyeZ) {
                d2 = eyeZ + 0.0D;
            } else {
                flag1 = false;
            }

            double d3 = 999.0D;
            double d4 = 999.0D;
            double d5 = 999.0D;
            double diffX = to.xCoord - eyePos.xCoord;
            double diffY = to.yCoord - eyePos.yCoord;
            double diffZ = to.zCoord - eyePos.zCoord;

            if (flag2) {
                d3 = (d0 - eyePos.xCoord) / diffX;
            }

            if (flag) {
                d4 = (d1 - eyePos.yCoord) / diffY;
            }

            if (flag1) {
                d5 = (d2 - eyePos.zCoord) / diffZ;
            }

            if (d3 == -0.0D) {
                d3 = -1.0E-4D;
            }

            if (d4 == -0.0D) {
                d4 = -1.0E-4D;
            }

            if (d5 == -0.0D) {
                d5 = -1.0E-4D;
            }

            EnumFacing enumfacing;

            if (d3 < d4 && d3 < d5) {
                enumfacing = toX > eyeX ? EnumFacing.WEST : EnumFacing.EAST;
                eyePos = new Vec3(d0, eyePos.yCoord + diffY * d3, eyePos.zCoord + diffZ * d3);
            } else if (d4 < d5) {
                enumfacing = toY > eyeY ? EnumFacing.DOWN : EnumFacing.UP;
                eyePos = new Vec3(eyePos.xCoord + diffX * d4, d1, eyePos.zCoord + diffZ * d4);
            } else {
                enumfacing = toZ > eyeZ ? EnumFacing.NORTH : EnumFacing.SOUTH;
                eyePos = new Vec3(eyePos.xCoord + diffX * d5, eyePos.yCoord + diffY * d5, d2);
            }

            eyeX = MathHelper.FloorDouble(eyePos.xCoord) - (enumfacing == EnumFacing.EAST ? 1 : 0);
            eyeY = MathHelper.FloorDouble(eyePos.yCoord) - (enumfacing == EnumFacing.UP ? 1 : 0);
            eyeZ = MathHelper.FloorDouble(eyePos.zCoord) - (enumfacing == EnumFacing.SOUTH ? 1 : 0);

            blockpos = new BlockPos(eyeX, eyeY, eyeZ);
            IBlockState iblockstate1 = this.getBlockState(blockpos);
            Block block1 = iblockstate1.getBlock();

            if ( block1.getCollisionBoundingBox(this, blockpos, iblockstate1) != null) {
                if (block1.canCollideCheck(iblockstate1, stopOnLiquid)) {
                    MovingObjectPosition movingobjectposition1 = block1.collisionRayTrace(this, blockpos, eyePos, to);

                    if (movingobjectposition1 != null) {
                        return movingobjectposition1;
                    }
                } else {
                    movingobjectposition2 = new MovingObjectPosition(MovingObjectPosition.MovingObjectType.MISS, eyePos, enumfacing, blockpos);
                }
            }
        }
        return returnLastUncollidableBlock ? movingobjectposition2 : null;
    }
    */

    private void LateUpdate () {

        //hpText.text  = ((int)(1f / Time.unscaledDeltaTime)).ToString();
        while (entityQueue.Count > 0) {
            entities.Add(entityQueue.Dequeue());
        }
        for (int i = entities.Count - 1; i >= 0; i--) {
            if (entities[i].IsAlive) {
                entities[i].Update();
            } else {
                entities.RemoveAt(i);
            }
        }
        foreach (Entity entity in entities) {
        }
        ModifyChunks();
        UpdateChunks();
        if (chunksToDraw.Count > 0 && chunksToDraw.Peek().IsEditable) {
            chunksToDraw.Dequeue().GenerateMesh();
        }
    }
    public void CheckViewDistance (Vec3i playerPos) {
        List<Vector2Int> previouslyActiveChunks = chunks.Keys.ToList();
        for (int x = playerPos.x - Data.ChunkLoadRange; x < playerPos.x + Data.ChunkLoadRange; x++) {
            for (int z = playerPos.z - Data.ChunkLoadRange; z < playerPos.z + Data.ChunkLoadRange; z++) {
                Vector2Int pos = new(x, z);
                if (!chunks.ContainsKey(pos)) {
                    chunks.Add(pos, new(pos, this));
                    chunks[pos].GenerateTerrainData();
                    chunksToUpdate.Add(chunks[pos]);
                } else {
                    if (!chunks[pos].IsTerrainMapGenerated) {
                        chunks[pos].GenerateTerrainData();
                        chunksToUpdate.Add(chunks[pos]);
                    }
                    chunks[pos].SetActiveState(true);
                }
                for (int i = 0; i < previouslyActiveChunks.Count; i++) {
                    if (previouslyActiveChunks[i] == pos) {
                        previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }
        }
        foreach (Vector2Int c in previouslyActiveChunks) {
            chunks[c].SetActiveState(false);
        }
    }
    public List<int> CollidingIDs (AABB aabb) {
        List<int> a = new();
        for (int x = (int)Math.Floor(aabb.minX); x < (int)Math.Ceiling(aabb.maxX); x++) {
            for (int y = (int)Math.Floor(aabb.minY); y < (int)Math.Ceiling(aabb.maxY); y++) {
                for (int z = (int)Math.Floor(aabb.minZ); z < (int)Math.Ceiling(aabb.maxZ); z++) {
                    a.Add(GetVoxelID(new(x, y, z)));
                }
            }
        }
        return a;
    }
    public List<AABB> GetCollidingBoundingBoxes (AABB aabb, int? self) {
        List<AABB> a = new();
        for (int x = (int)Math.Floor(aabb.minX); x < (int)Math.Ceiling(aabb.maxX); x++) {
            for (int y = (int)Math.Floor(aabb.minY); y < (int)Math.Ceiling(aabb.maxY); y++) {
                for (int z = (int)Math.Floor(aabb.minZ); z < (int)Math.Ceiling(aabb.maxZ); z++) {
                    if (blockTypes[GetVoxelID(new(x, y, z))].hasCollision) {
                        a.Add(new(x, y, z, x + 1, y + 1, z + 1));
                    }
                }
            }
        }
        foreach (Entity entity in entities) {
            if (self == null || self != null && entity.ID != self && aabb.IntersectsWith(entity.BoundingBox)) {
                a.Add(entity.BoundingBox);
            }
        }
        return a;
    }
    public byte GetVoxel (Vector3Int pos) {
        byte VoxelValue = 0;

        if (pos.y < 40) {
            VoxelValue = 7;
        }
        int terrainHeight = Mathf.FloorToInt(GetHeight(new(pos.x, pos.z)));
        switch (pos.y - terrainHeight) {
            case < -4:
                VoxelValue = 3;
                break;
            case < -1:
                VoxelValue = 2;
                break;
            case < 0:
                VoxelValue = 1;
                break;
            default:
                break;
        }
        if (VoxelValue == 1) {
            lock (modifications) {
                if (Noise.Get2DPerlin(new(pos.x, pos.z), 0.0158f) > 0) {
                    float www = Mathf.Max(0, Noise.Get2DPerlin(new(pos.x, pos.z), 0.052f) + 0.5f) * 32 + 2;
                    if (new System.Random().Next(0, Mathf.FloorToInt(www)) == 0) {
                        Queue<VoxelAndPos> a = new();
                        a.Enqueue(new(Data.Vector3ToChunkVoxel(pos + Vector3Int.up), 15));
                        modifications.Enqueue(a);
                    }
                }
                /*
                if (new System.Random().Next(0, (int)MathF.Pow(2, 12)) == 0) {
                    modifications.Enqueue(Structure.MakeCave(pos));
                }
                */
                if (Noise.Get2DPerlin(new(pos.x, pos.z), Data.treeZoneScale) > Data.treeZoneThreshold) {
                    if (Noise.Get2DPerlin(new(pos.x + 50, pos.z + 50), Data.treePlacementScale) > Data.treePlacementThreshold) {
                        modifications.Enqueue(Structure.MakeTree(pos + Vector3Int.up));
                    }
                }
            }
        }
        return VoxelValue;

        static float GetHeight (Vector2Int pos) {
            float terrainHeight = 0;
            for (int i = 0; i < 4; i++) {
                terrainHeight += Noise.Get2DPerlin(pos, Data.terrainScale / Mathf.Pow(2, i));
            }
            return terrainHeight * Data.terrainHeight * (Mathf.Pow(2, Noise.Get2DPerlin(pos, 0.029F) * 4) + Mathf.Pow(2, Noise.Get2DPerlin(pos, 0.005F) * 4) / 2) / (Noise.Get2DPerlin(pos, 0.05f) / 5000 + 1) + Data.solidGroundHeight;
        }
    }
    public bool SetBlock (Vector3 position, Vector3 selectingPos) {
        if (userInter.selectedBlockIndex != 0 && !blockTypes[GetVoxelID(position)].hasCollision && blockTypes[GetVoxelID(selectingPos)].hasCollision) {
            Queue<VoxelAndPos> queue = new();
            queue.Enqueue(new(Data.Vector3ToChunkVoxel(position), userInter.selectedBlockIndex));
            AddMod(queue);
            hand.placeEase = 0;
            return true;
        }
        return false;
    }
    public int GetVoxelID (Vector3 position) {
        ChunkVoxel pos = Data.Vector3ToChunkVoxel(position);
        return chunks.ContainsKey(pos.c) ? chunks[pos.c].GetVoxelIDChunk(pos.v) : 0;
    }
    public void AddMod (Queue<VoxelAndPos> aadd) {
        modifications.Enqueue(aadd);
    }
    private void InitNoise () {
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
        offset = new(UnityEngine.Random.Range(-66666.6f, 66666.6f), UnityEngine.Random.Range(-66666.6f, 66666.6f));
        Noise.SetOffset(offset);
    }
    private void ModifyChunks () {
        lock (modifications) {
            while (modifications.Count > 0) {
                Queue<VoxelAndPos> queue = modifications.Dequeue();
                while (queue.Count > 0) {
                    VoxelAndPos vmod = queue.Dequeue();
                    if (!chunks.ContainsKey(vmod.pos.c)) {
                        chunks.Add(vmod.pos.c, new(vmod.pos.c, this));
                    }
                    chunks[vmod.pos.c].EnqueueVoxelMod(vmod);
                    if (!chunksToUpdate.Contains(chunks[vmod.pos.c])) {
                        chunksToUpdate.Add(chunks[vmod.pos.c]);
                    }
                }
            }
        }
    }
    private void UpdateChunks () {
        if (chunksToUpdate.Count > 0) {
            for (int i = chunksToUpdate.Count - 1; i >= 0; i--) {
                if (chunksToUpdate[i].IsEditable) {
                    chunksToUpdate[i].UpdateChunk();
                    chunksToDraw.Enqueue(chunksToUpdate[i]);
                    chunksToUpdate.RemoveAt(i);
                }
            }
        }
    }
}
[System.Serializable]
public class BlockType {
    public string blockName;
    public bool isSolid;
    public bool hasCollision;
    public bool hasInventory;
    public float hardness;
    public Sprite sprite;
    public byte meshTypes;
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;
    public int GetTextureID (int faceIndex) {
        return faceIndex switch {
            0 => backFaceTexture,
            1 => frontFaceTexture,
            2 => topFaceTexture,
            3 => bottomFaceTexture,
            4 => leftFaceTexture,
            5 => rightFaceTexture,
            _ => 0,
        };
    }
}