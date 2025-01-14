using UnityEngine;

public class Test : MonoBehaviour {



    Chunk a ;
    bool ttt;


    private void Start () {
        a = new(Vector3Int.zero);
        a.GenerateTerrainData();
    }

    private void Update () {

        if (a.IsEditable && !ttt) {
            a.GenerateMesh();
            ttt= true;
        }
    }
}
