using UnityEngine;
using UnityEngine.UI;

public class SmoothFollow : MonoBehaviour
{

    public float distance = 8f;
    public float height = 1.5f;
    public float heightOffset = 1.0f;
    public float heightDamping = 4.00f;
    public float rotationDamping = 2.0f;
    public float rotationOffset = 5.0f;

    public static Transform playerCar;

    private Transform[] target;
    public RawImage rearCamerView;
    private int index = 0;

    private float wantedRotationAngle;
    private float wantedHeight;
    private float currentRotationAngle;
    private float currentHeight;
    private Quaternion currentRotation;

    private int FP = -1;

    private void Start()
    {
        if (PlayerPrefs.HasKey("FP"))
        {
            FP = PlayerPrefs.GetInt("FP", -1);
        }
    }
    private void LateUpdate()
    {
        if (!RaceMonitor.isStartedRacing)
        {
            GameObject[] cars = GameObject.FindGameObjectsWithTag("car");
            target = new Transform[cars.Length];

            for (int i = 0; i < cars.Length; i++)
            {
                target[i] = cars[i].transform;

                if(target[i] == playerCar)
                {
                    index = i;
                }
            }

            target[index].Find("RearCamera").gameObject.GetComponent<Camera>().targetTexture = (rearCamerView.texture as RenderTexture);

        }

        if (target[index] == null)
        {
            return;
        }

        if (FP == 1)
        {
            transform.position = target[index].transform.position + target[index].forward * 0.01f + target[index].up * 1.525f;
            transform.LookAt(target[index].transform.position + target[index].forward * 7.5f);
        }
        else
        {
            wantedRotationAngle = target[index].eulerAngles.y;
            wantedHeight = target[index].position.y + height;

            currentRotationAngle = transform.eulerAngles.y;
            currentHeight = transform.position.y;

            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

            currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            transform.position = target[index].position;
            transform.position -= currentRotation * Vector3.forward * distance;

            transform.position = new Vector3(transform.position.x, currentHeight + heightOffset, transform.position.z);

            transform.LookAt(target[index].transform.position + target[index].forward * rotationOffset);
        }

    }

    private void CamAngleChange()
    {
        FP = FP * -1;

        PlayerPrefs.SetInt("FP", FP);
    }

    private void CamChange()
    {
        target[index].Find("RearCamera").gameObject.GetComponent<Camera>().targetTexture = null;

        index++;

        if (index > target.Length - 1)
        {
            index = 0;
        }

        target[index].Find("RearCamera").gameObject.GetComponent<Camera>().targetTexture = (rearCamerView.texture as RenderTexture);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            CamAngleChange();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            CamChange();
        }
    }

    public void OnClickButtonFunction_CamAngleChange()
    {
        CamAngleChange();
    }

    public void OnClickButtonFunction_CamChange()
    {
        CamChange();
    }
}
