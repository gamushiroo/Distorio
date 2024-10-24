using System.Collections.Generic;
using UnityEngine;

public static class AABB {


    public static bool ABCheck (WWWEe posAndVel_, Vector3 ttt) {

        return AABBCheck(posAndVel_, new WWWEe(ttt, Vector3.one, Vector3.zero, false));

    }

    public static bool AABBCheck (WWWEe b1, WWWEe b2) {

        Vector3 b1p = b1.pos + b1.size;
        Vector3 b2p = b2.pos + b2.size;

        return !( b2.pos.x >= b1p.x || b1.pos.x >= b2p.x || b2.pos.y >= b1p.y || b1.pos.y >= b2p.y || b2.pos.z >= b1p.z || b1.pos.z >= b2p.z );

    }
}