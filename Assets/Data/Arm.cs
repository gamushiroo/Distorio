using UnityEngine;

public class Arm : MonoBehaviour {

    public Transform[] gamm = new Transform[6];
    public Vector3 target;
    public Quaternion targetRot = Quaternion.identity;
    public float[] targets = new float[6];
    public float[] now = new float[6];

    public float[] lengths = new float[6]{
        0.25f, 0.75f, 0.75f, 0.125f, 0.25f, 0.125f
    };
    float time = 0;
    bool sw = false;
    private void Start () {
        target = transform.position + Vector3.forward;
    }
    private void Update () {

        Move();
    }

    void Move () {

        for (int i = 0; i < 6; i++) {
            if (Mathf.Abs(targets[i] - now[i]) > 4) {

                now[i] += Mathf.Sign(targets[i] - now[i]) * 360 * 1.5f * Time.deltaTime;
            }
        }

        time += Time.deltaTime * 2;
        if (time > 1) {
            if (sw) {
                time--;
                target = transform.position + new Vector3(0, 0.5f, 1);
                targetRot = Quaternion.Euler(-90, 0, 0);
            } else {
                time-= 4;
                target = transform.position + new Vector3(0, 0.5f, -1);
                targetRot = Quaternion.identity;
            }
            CalculateArmsRotTarg();
            sw = !sw;

        }
        gamm[0].rotation = Quaternion.Euler(0, -now[0], 0);
        gamm[1].rotation = gamm[0].rotation * Quaternion.Euler(0, 0, -now[1]);
        gamm[2].rotation = gamm[1].rotation * Quaternion.Euler(0, 0, -now[2]);
        gamm[3].rotation = gamm[2].rotation * Quaternion.Euler(0, -now[3], 0);
        gamm[4].rotation = gamm[3].rotation * Quaternion.Euler(0, 0, -now[4]);
        gamm[5].rotation = gamm[4].rotation * Quaternion.Euler(0, -now[5], 0);

        for (int i = 0; i < 6; i++) {
            if (i != 0) {
                gamm[i].position = gamm[i - 1].position + gamm[i - 1].rotation * Vector3.up * lengths[i - 1];
            }
        }
    }
    void CalculateArmsRotTarg () {

        Vector3 sda = target - gamm[0].position;
        Vector3 tar1 = lengths[0] * Vector3.up;
        Vector3 tar4 = sda + targetRot * Vector3.up * (lengths[4] + lengths[5]);
        AB aaa = CalculateArms(tar1, tar4 - tar1);
        float bbb = Mathf.Atan2(tar4.z, tar4.x) * Mathf.Rad2Deg;
        Vector3 tar2 = tar1 + Quaternion.Euler(0, -bbb, -aaa.a) * Vector3.up * lengths[1];
        Quaternion ccc = Quaternion.Euler(0, -bbb, -aaa.a - aaa.b);
        Vector3 ddd = Quaternion.Inverse(ccc) * (sda - tar2);
        targets[0] = bbb;
        targets[1] = aaa.a;
        targets[2] = aaa.b;
        targets[3] = Mathf.Atan2(ddd.z, ddd.x) * Mathf.Rad2Deg;
        targets[4] = CalculateArmsTwo(tar2, sda);
        targets[5] = -(Quaternion.Inverse(ccc * Quaternion.Euler(0, -targets[3], -targets[4])) * targetRot).eulerAngles.y;


    }
    AB CalculateArms (Vector3 a, Vector3 b) {
        Vector3 nnn = b - a;
        nnn.y = 0;
        float dist = nnn.sqrMagnitude;
        float fff = b.y * b.y;
        float bbs = lengths[1] * lengths[1];
        float iii = 2 * lengths[1];
        float call = iii * Mathf.Sqrt(dist + fff);
        float mmm = lengths[2] + lengths[3];
        float aend = 0;
        if (call != 0) {
            aend = Mathf.Atan2(Mathf.Sqrt(dist), b.y) - Mathf.Acos(Mathf.Clamp((bbs - mmm * mmm + dist + fff) / call, -1, 1));
        }
        float bend = Mathf.PI - Mathf.Acos(Mathf.Clamp((bbs + mmm * mmm - dist - fff) / iii / mmm, -1, 1));
        return new AB(aend * Mathf.Rad2Deg, bend * Mathf.Rad2Deg);
    }
    float CalculateArmsTwo (Vector3 a, Vector3 b) {
        float dsdf = Mathf.Pow(lengths[2] + lengths[3], 2) + Mathf.Pow(lengths[4] + lengths[5], 2) - (a - b).sqrMagnitude;
        return Mathf.Acos(dsdf / -(2 * (lengths[2] + lengths[3]) * (lengths[4] + lengths[5]))) * Mathf.Rad2Deg;
    }
}
public struct AB {
    public AB (float a, float b) {
        this.a = a;
        this.b = b;
    }
    public float a;
    public float b;
}