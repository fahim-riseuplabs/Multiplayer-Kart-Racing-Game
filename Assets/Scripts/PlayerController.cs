using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Drive drive;
    private float lastMovingTime = 0;
    private RaycastHit hit;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private CheckPointManager checkPointManager;
    private MobileInputController mobileInputController;
    private float finishedSteer;

    private float accelInput;
    private float streetAngleInput;
    private float brakeInput;
    // Start is called before the first frame update
    void Start()
    {
        mobileInputController = FindObjectOfType<MobileInputController>();
        drive = GetComponent<Drive>();
        checkPointManager = drive.rb.GetComponent<CheckPointManager>();
        finishedSteer = Random.Range(-1, 1);
    }

    void ResetCarLayer()
    {
        drive.rb.gameObject.layer = 0;

        this.GetComponent<Ghost>().HoverOff();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            
            accelInput = Input.GetAxis("Vertical");
            streetAngleInput = Input.GetAxis("Horizontal");
            brakeInput = Input.GetAxis("Jump");

            print("dawdwqdqad" + Input.GetAxis("Horizontal"));

        }

        if(Application.platform == RuntimePlatform.Android)
        {

            if (brakeInput == 0)
            {
                accelInput = Mathf.Lerp(accelInput, mobileInputController.accel, Time.deltaTime * 2.5f);
            }

            if (mobileInputController.horizontalLeft == 0)
            {
                streetAngleInput = Mathf.Lerp(streetAngleInput, mobileInputController.horizontalRight, Time.deltaTime * 2.5f);
            }
            else if (mobileInputController.horizontalRight == 0)
            {
                streetAngleInput = Mathf.Lerp(streetAngleInput, mobileInputController.horizontalLeft, Time.deltaTime * 2.5f);
            }
            else if (mobileInputController.horizontalLeft != 0 && mobileInputController.horizontalRight != 0)
            {
                streetAngleInput = Mathf.Lerp(streetAngleInput, 0, Time.deltaTime * 2.5f);
            }

            brakeInput = mobileInputController.brake;

        }

        if (!RaceMonitor.isStartedRacing)
        {
            accelInput = 0;
        }

        if(checkPointManager.lap == RaceMonitor.totalLaps + 1)
        {
            drive.highAccelAudio.Stop();
            drive.Driving(0, finishedSteer, 1);

            return;
        }

        if (drive.rb.velocity.magnitude > 1 || !RaceMonitor.isStartedRacing)
        {
            lastMovingTime = Time.time;
        }

        if (Physics.Raycast(drive.rb.gameObject.transform.position, -Vector3.up, out hit, 10))
        {
            if (hit.collider.gameObject.tag == "road")
            {
                lastPosition = drive.rb.transform.position;
                lastRotation = drive.rb.transform.rotation;
            }
        }

        if (Time.time > lastMovingTime + 4)
        {
            drive.rb.transform.position = checkPointManager.lastCheckPoint.transform.position + Vector3.up * 2;
            drive.rb.transform.rotation = checkPointManager.lastCheckPoint.transform.rotation;

            //drive.rb.transform.position = lastPosition + new Vector3(Random.Range(-2, 2), 0.25f, Random.Range(-2, 2));
            //drive.rb.transform.rotation = lastRotation * Quaternion.Euler(1, Random.Range(-180, 180), 1);

            drive.rb.gameObject.layer = 6;

            this.GetComponent<Ghost>().HoverOn();

            Invoke("ResetCarLayer", 5);
        }

        if (Vector3.Distance(drive.rb.transform.position, checkPointManager.lastCheckPoint.transform.position) > 40 || drive.rb.transform.position.y < -5 || drive.rb.transform.position.y > 10)
        {
            drive.rb.transform.position = checkPointManager.lastCheckPoint.transform.position + Vector3.up * 2;
            drive.rb.transform.rotation = checkPointManager.lastCheckPoint.transform.rotation;

            drive.rb.gameObject.layer = 6;

            this.GetComponent<Ghost>().HoverOn();

            Invoke("ResetCarLayer", 5);
        }

        drive.Driving(accelInput, streetAngleInput, brakeInput);
        drive.CheckForSKid();
        drive.CalculateEngineSound();
    }
}
