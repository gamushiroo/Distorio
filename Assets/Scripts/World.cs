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

        Chunks.LoadChunksAround(0, 0);
        Entities.Add(new EntityPlayer(this));
    }
    private void LateUpdate () {
        Entities.Update();
        Chunks.Update();
    }
}