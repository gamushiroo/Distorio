using UnityEngine;
using UnityEngine.UI;

public class World : MonoBehaviour {

    [SerializeField] private Camera camObj;

    public GameObject particle;
    public Slider hpBar;
    public Text hpText;
    public AudioClip eeeeeee;
    public AudioClip dd;
    public AudioSource audioSource;
    public Transform backGround;
    public GameObject miningProgresBarObj;
    public GameObject blockHighlight;
    public GameObject miningEffect; 
    public Slider miningProgresBar;
    private Vector2 offset;
    public GameObject healing;


    private void Awake () {
        Cursor.lockState = CursorLockMode.Locked;
        Random.InitState((int)System.DateTime.Now.Ticks);
        Noise.offset = new(Random.Range(-66666.6f, 66666.6f), Random.Range(-66666.6f, 66666.6f));
        MyResources.materials = new Material[2] { (Material)Resources.Load("Block"), (Material)Resources.Load("Water") };
        MyResources.camera = camObj;
        MyResources.cameraTransform = camObj.transform;
        Chunks.LoadChunksAround(new(0, 0));
        Entities.Add(new EntityPlayer(this));
    }
    private void LateUpdate () {
        Entities.Update();
        Chunks.Update();
    }
}
[System.Serializable]
public class BlockType {
    public BlockType (string blockName, bool isSolid, bool hasCollision, float hardness, byte meshTypes, int backFaceTexture, int frontFaceTexture, int topFaceTexture, int bottomFaceTexture, int leftFaceTexture, int rightFaceTexture) {
        this.blockName = blockName;
        this.isSolid = isSolid;
        this.hasCollision = hasCollision;
        this.hardness = hardness;
        this.meshTypes = meshTypes;
        this.backFaceTexture = backFaceTexture;
        this.frontFaceTexture = frontFaceTexture;
        this.topFaceTexture = topFaceTexture;
        this.bottomFaceTexture = bottomFaceTexture;
        this.leftFaceTexture = leftFaceTexture;
        this.rightFaceTexture = rightFaceTexture;
    }
    public string blockName;
    public bool isSolid;
    public bool hasCollision;
    public float hardness;
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