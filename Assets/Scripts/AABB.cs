using System.Collections.Generic;
using UnityEngine;

public static class AABB {

    public static WWWEe PosUpdate (WWWEe posAndVel_, World world) {

        posAndVel_.isGrounded = false;
        float remainingtime = 1.0f;
        int max = 0;

        while (remainingtime > 0.0f || max > 10) {

            List<AABBData> boxColliderList = new();
            AABBData aabb = new AABBData(1, Vector3Int.zero);
            float value = Mathf.Infinity;

            for (int x = -2; x < 2; x++) {
                for (int y = -2; y < 3; y++) {
                    for (int z = -2; z < 2; z++) {

                        Vector3Int pos = Vector3Int.FloorToInt(posAndVel_.pos + new Vector3Int(x, y, z));

                        if (world.blockTypes[world.GetVoxelID(pos)].hasCollision) {

                            boxColliderList.Add(SweptAABB(posAndVel_, new WWWEe(pos, Vector3.one, Vector3.zero, false)));

                        }
                    }
                }
            }

            for (int i = 0; i < boxColliderList.Count; i++) {

                if (boxColliderList[i].entryTime < value) {

                    value = boxColliderList[i].entryTime;
                    int index = i;
                    aabb = boxColliderList[index];

                }
            }

            remainingtime -= aabb.entryTime;
            posAndVel_.pos += aabb.entryTime * posAndVel_.vel;

            float xz = posAndVel_.vel.x * aabb.normal.z + posAndVel_.vel.z * aabb.normal.x;
            float xy = posAndVel_.vel.x * aabb.normal.y + posAndVel_.vel.y * aabb.normal.x;
            float yz = posAndVel_.vel.y * aabb.normal.z + posAndVel_.vel.z * aabb.normal.y;
            posAndVel_.vel = new Vector3(xz * aabb.normal.z + xy * aabb.normal.y, xy * aabb.normal.x + yz * aabb.normal.z, xz * aabb.normal.x + yz * aabb.normal.y) * remainingtime;


            if (aabb.normal.y == 1) {
                posAndVel_.isGrounded = true;

            }

            max++;
        }

        posAndVel_.vel = Vector3.zero;

        return posAndVel_;

    }

    public static bool ABCheck (WWWEe posAndVel_, Vector3 ttt) {

        return AABBCheck(posAndVel_, new WWWEe(ttt, Vector3.one, Vector3.zero, false));

    }

    private static AABBData SweptAABB (WWWEe entity, WWWEe staticBox) {

        entity.pos -= new Vector3(entity.size.x * 0.5f, 0, entity.size.z * 0.5f);

        Vector3 normal = Vector3.zero;
        Vector3 entry = new(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);

        if (AABBCheck(BroadPhaseBox(entity), staticBox)) {

            Vector3 innn = staticBox.pos - ( entity.pos + entity.size );
            Vector3 outt = ( staticBox.pos + staticBox.size ) - entity.pos;

            Vector3  entryDistance = new(entity.vel.x >= 0.0f ? innn.x : outt.x, entity.vel.y >= 0.0f ? innn.y : outt.y, entity.vel.z >= 0.0f ? innn.z : outt.z);

            if (entity.vel.x != 0.0f)
                entry.x = entryDistance.x / entity.vel.x;
            if (entity.vel.y != 0.0f)
                entry.y = entryDistance.y / entity.vel.y;
            if (entity.vel.z != 0.0f)
                entry.z = entryDistance.z / entity.vel.z;

            if (entry.x > entry.y) {
                if (entry.x > entry.z) {
                    normal.x = -Mathf.Sign(entity.vel.x);
                    normal.y = 0.0f;
                    normal.z = 0.0f;
                }
                else {
                    normal.z = -Mathf.Sign(entity.vel.z);
                    normal.x = 0.0f;
                    normal.y = 0.0f;
                }
            }
            else {
                if (entry.y < entry.z) {

                    normal.z = -Mathf.Sign(entity.vel.z);
                    normal.x = 0.0f;
                    normal.y = 0.0f;
                }
                else {
                    normal.y = -Mathf.Sign(entity.vel.y);
                    normal.x = 0.0f;
                    normal.z = 0.0f;
                }
            }
            /*
            if (entry.x < entry.z && entry.y < entry.z) {
                if (entryDistance.z < 0.0f) {
                    normal += Vector3Int.back;
                }
                else {
                    normal += Vector3Int.forward;
                }
            }
            else if (entry.x > entry.y) {
                if (entryDistance.x < 0.0f) {
                    normal += Vector3Int.left;
                }
                else {
                    normal += Vector3Int.right;
                }
            }
            else if (entry.x < entry.y) {
                if (entryDistance.y < 0.0f) {
                    normal += Vector3Int.down;
                }
                else {
                    normal += Vector3Int.up;
                }
            }

            */
        }
        float entryTime = Mathf.Max(entry.x, entry.y, entry.z);
        if ( entry.x < 0.0f && entry.y < 0.0f && entry.z < 0.0f)
            return new AABBData(1.0f, Vector3Int.zero);

        return new AABBData(entryTime, normal);

    }

    private static WWWEe BroadPhaseBox (WWWEe b) {

        WWWEe box = new() {

            pos = new(b.vel.x >= 0 ? b.pos.x : b.pos.x + b.vel.x,
                      b.vel.y >= 0 ? b.pos.y : b.pos.y + b.vel.y,
                      b.vel.z >= 0 ? b.pos.z : b.pos.z + b.vel.z),
            size = new(b.vel.x >= 0 ? b.vel.x + b.size.x : b.size.x - b.vel.x,
                       b.vel.y >= 0 ? b.vel.y + b.size.y : b.size.y - b.vel.y,
                       b.vel.z >= 0 ? b.vel.z + b.size.z : b.size.z - b.vel.z)

        };

        return box;

    }

    public static bool AABBCheck (WWWEe b1, WWWEe b2) {

        Vector3 b1p = b1.pos + b1.size;
        Vector3 b2p = b2.pos + b2.size;

        return !( b2.pos.x >= b1p.x || b1.pos.x >= b2p.x || b2.pos.y >= b1p.y || b1.pos.y >= b2p.y || b2.pos.z >= b1p.z || b1.pos.z >= b2p.z );

    }
}