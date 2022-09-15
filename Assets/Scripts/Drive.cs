using Photon.Pun;
using UnityEngine;

public class Drive : MonoBehaviour
{
    public WheelCollider[] wheelColliders;
    public Transform[] wheels;

    public float torque = 200f;
    public float maxStreetAngle = 30f;
    public float maxBrakeTorque = 5000f;

    public AudioSource skidAudioSource;
    public AudioSource highAccelAudio;

    public Transform skidTrailPrefab;
    private Transform[] skidTrails = new Transform[4];

    public ParticleSystem smokePrefab;
    private ParticleSystem[] skidSmokes = new ParticleSystem[4];

    public GameObject brakeLight;

    public Rigidbody rb;
    public float gearLenght = 3;
    public float currentSpeed { get { return rb.velocity.magnitude * gearLenght; } }
    public float lowPitch = 1f;
    public float highPitch = 6f;
    public int numberGears = 5;
    private float rpm;
    private int currentGear = 1;
    private float currentGearPerc;
    private float streetAngle;
    private float breakTorque;
    public float maxSpeed = 200;

    public GameObject playerNamePrefab;
    public Renderer jeepMesh;
    public string networkName = "";

    private Transform playerNameParentUI;
    private Vector3 wheelColiderPosition;
    private Quaternion WheelColiderQuaternion;
    private NameUIController nameUIController;
    private string[] nameNPC = { "Ratul", "Rafiq", "Masud", "Emraan", "Ovi", "Sunny", "Sumon" };

    private PhotonView photonView;

    public void CalculateEngineSound()
    {
        float gearPercentage = (1 / (float)numberGears);
        float targetGearFactor = Mathf.InverseLerp(gearPercentage * currentGear, gearPercentage * (currentGear + 1), Mathf.Abs(currentSpeed / maxSpeed));

        currentGearPerc = Mathf.Lerp(currentGearPerc, targetGearFactor, Time.captureDeltaTime * 5f);

        var gearNumberFactor = currentGear / (float)numberGears;
        rpm = Mathf.Lerp(gearNumberFactor, 1, currentGearPerc);

        float speedPercentage = Mathf.Abs(currentSpeed / maxSpeed);
        float upperGearMax = (1 / (float)numberGears) * (currentGear + 1);
        float downGearMax = (1 / (float)numberGears) * currentGear;

        if (currentGear > 0 && speedPercentage < downGearMax)
        {
            currentGear--;
        }

        if (speedPercentage > upperGearMax && (currentGear < (numberGears - 1)))
        {
            currentGear++;
        }

        float pitch = Mathf.Lerp(lowPitch, highPitch, rpm);
        highAccelAudio.pitch = Mathf.Min(highPitch, pitch) * 0.25f;
    }

    [PunRPC]
    private void BrakeLightOff()
    {
        brakeLight.SetActive(false);
    }

    [PunRPC]
    private void BrakeLightOn()
    {
        brakeLight.SetActive(true);
    }

    [PunRPC]
    private void InstantiateSmoke()
    {
        for (int i = 0; i < 4; i++)
        {
            skidSmokes[i] = Instantiate(smokePrefab);
            skidSmokes[i].Stop();
        }
    }

    [PunRPC]
    private void EmitSmoke(Vector3 postion, int i)
    {
        skidSmokes[i].transform.position = postion;
        skidSmokes[i].Emit(1);
    }

    public void Driving(float accelInput, float streetAngleInput, float brakeTorqueInput)
    {
        accelInput = Mathf.Clamp(accelInput, -1, 1);
        streetAngleInput = Mathf.Clamp(streetAngleInput, -1, 1);
        brakeTorqueInput = Mathf.Clamp(brakeTorqueInput, 0, 1);

        float thurstTorque = 0;

        if (currentSpeed < maxSpeed)
        {
            thurstTorque = accelInput * torque;
        }

        streetAngle = streetAngleInput * maxStreetAngle;
        breakTorque = brakeTorqueInput * maxBrakeTorque;

        if (breakTorque != 0)
        {
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("BrakeLightOn", RpcTarget.All, null);
            }
            else
            {
                brakeLight.SetActive(true);
            }
        }
        else
        {
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("BrakeLightOff", RpcTarget.All, null);
            }
            else
            {
                brakeLight.SetActive(false);
            }
        }

        for (int i = 0; i < wheelColliders.Length; i++)
        {
            wheelColliders[i].motorTorque = thurstTorque;

            if (i < 2)
            {
                wheelColliders[i].steerAngle = streetAngle;
                wheelColliders[i].brakeTorque = breakTorque;
            }

            //wheelColliders[i].GetWorldPose(out wheelColiderPosition, out WheelColiderQuaternion);

            //wheels[i].position = wheelColiderPosition;
            //wheels[i].localRotation = WheelColiderQuaternion;
        }
    }

    public void CheckForSKid()
    {
        int skidNumber = 0;

        for (int i = 0; i < 4; i++)
        {
            WheelHit wheelHit;
            wheelColliders[i].GetGroundHit(out wheelHit);

            if (Mathf.Abs(wheelHit.forwardSlip) >= 0.5f || Mathf.Abs(wheelHit.sidewaysSlip) >= 0.25f)
            {
                skidNumber++;
                if (!skidAudioSource.isPlaying)
                {
                    skidAudioSource.Play();
                }

                if (PhotonNetwork.IsConnected)
                {
                    Vector3 position = new Vector3(0, 0.5f, 0) + (wheelColliders[i].transform.position - Vector3.up * wheelColliders[i].radius);
                    photonView.RPC("EmitSmoke", RpcTarget.All, position, i);
                }
                else
                {
                    skidSmokes[i].transform.position = new Vector3(0, 0.5f, 0) + (wheelColliders[i].transform.position - Vector3.up * wheelColliders[i].radius);
                    skidSmokes[i].Emit(1);
                }

                //SkidTrailStart(i);
            }
            else
            {
                //skidTrailEnd(i);
            }
        }

        if (skidNumber == 0 && skidAudioSource.isPlaying)
        {
            skidAudioSource.Stop();
        }
    }

    public void SkidTrailStart(int i)
    {
        if (skidTrails[i] == null)
        {
            skidTrails[i] = Instantiate(skidTrailPrefab);

            skidTrails[i].parent = wheelColliders[i].transform;
            skidTrails[i].localRotation = Quaternion.Euler(90, 0, 0);
            skidTrails[i].localPosition = -Vector3.up * wheelColliders[i].radius;
        }
    }

    public void skidTrailEnd(int i)
    {
        if (skidTrails[i] == null)
        {
            return;
        }
        else
        {
            Transform holder = skidTrails[i];
            skidTrails[i] = null;
            holder.parent = null;
            holder.rotation = Quaternion.Euler(90, 0, 0);
            Destroy(holder.gameObject, 30f);
        }
    }

    private void Start()
    {
        playerNameParentUI = GameObject.Find("PlayerNameParentUI").transform;
        photonView = GetComponent<PhotonView>();

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("InstantiateSmoke", RpcTarget.All, null); 
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                skidSmokes[i] = Instantiate(smokePrefab);
                skidSmokes[i].Stop();
            }
        }

        brakeLight.SetActive(false);

        GameObject playerName = Instantiate(playerNamePrefab);

        playerName.transform.SetParent(playerNameParentUI);

        nameUIController = playerName.GetComponent<NameUIController>();
        nameUIController.target = rb.transform;

        if (GetComponent<AIControllerWithTracker>().enabled)
        {
            if (networkName != "")
            {
                nameUIController.nameText.text = networkName;
            }
            else
            {
                nameUIController.nameText.text = nameNPC[Random.Range(0, nameNPC.Length)];
            }
        }
        else if (PhotonNetwork.IsConnected && !GetComponent<AIControllerWithTracker>().enabled)
        {
            nameUIController.nameText.text = PhotonNetwork.LocalPlayer.NickName;
        }
        else if (!PhotonNetwork.IsConnected)
        {
            nameUIController.nameText.text = PlayerPrefs.GetString("PlayerName");
        }

        nameUIController.carRend = jeepMesh;
    }

    private void Update()
    {
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            wheelColliders[i].GetWorldPose(out wheelColiderPosition, out WheelColiderQuaternion);

            wheels[i].position = wheelColiderPosition;
            wheels[i].localRotation = WheelColiderQuaternion;
        }
    }
}
