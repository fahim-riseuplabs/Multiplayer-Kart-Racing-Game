using UnityEngine;

public class AIControllerWithTracker : MonoBehaviour
{
    public Circuit Circuit;
    public float brakingSensitivity = 1;
    private Drive drive;
    public float steeringSensitivity = 0.01f;
    public float AccelSensitivity = 0.3f;

    private GameObject tracker;
    private int currentTrackerWP = 0;
    //private int ResetCurrentTrakerWP = 0;
    public float lookAhead = 10f;
    private AvoidDetector avoidDetector;
    private CheckPointManager checkPointManager;

    private float lastTimeMoving = 0;
    private float prevTorque;
    private float finishedSteer;

    // Start is called before the first frame update
    void Start()
    {
        if(Circuit == null)
        {
            Circuit = GameObject.FindGameObjectWithTag("circut").GetComponent<Circuit>();
        }

        drive = GetComponent<Drive>();
        avoidDetector = drive.rb.GetComponent<AvoidDetector>();

        tracker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        DestroyImmediate(tracker.GetComponent<Collider>());

        tracker.GetComponent<MeshRenderer>().enabled = false;
        tracker.transform.position = drive.rb.transform.position;
        tracker.transform.rotation = drive.rb.transform.rotation;

        checkPointManager = drive.rb.GetComponent<CheckPointManager>();
        finishedSteer = Random.Range(-1.0f, 1.0f);
        if (checkPointManager != null)
        {
            
        }
    }

    public void ProgressTreacker()
    {
        Debug.DrawLine(drive.rb.gameObject.transform.position, tracker.transform.position);

        if (Vector3.Distance(drive.rb.gameObject.transform.position, tracker.transform.position) > lookAhead)
        {
            return;
        }

        tracker.transform.LookAt(Circuit.wayPoints[currentTrackerWP].transform.position);
        tracker.transform.Translate(0, 0, 1.0f);

        if (Vector3.Distance(tracker.transform.position, Circuit.wayPoints[currentTrackerWP].transform.position) < 1)
        {
            currentTrackerWP++;

            if (currentTrackerWP >= Circuit.wayPoints.Length)
            {
                currentTrackerWP = 0;
                //ResetCurrentTrakerWP = 0;
            }
        }
    }

    void ResetCarLayer()
    {
        drive.rb.gameObject.layer = 0;

        //this.GetComponent<Ghost>().HoverOff();
    }

    // Update is called once per frame
    void Update()
    {
        if (!RaceMonitor.isStartedRacing)
        {
            lastTimeMoving = Time.time;
            return;
        }

        if (checkPointManager.lap == RaceMonitor.totalLaps + 1)
        {
            drive.highAccelAudio.Stop();
            drive.Driving(0, finishedSteer, 1);

            return;
        }

        ProgressTreacker();
        Vector3 localTarget;

        if (drive.rb.velocity.magnitude > 1)
        {
            lastTimeMoving = Time.time;
        }

        if (Time.time > lastTimeMoving + 4 || Vector3.Distance(drive.rb.transform.position, checkPointManager.lastCheckPoint.transform.position) > 40 || drive.rb.transform.position.y < -5 || drive.rb.transform.position.y > 10)
        {

            //if (currentTrackerWP != 0 && ResetCurrentTrakerWP < currentTrackerWP)
            //{
            //    ResetCurrentTrakerWP = currentTrackerWP - 1;
            //    currentTrackerWP = ResetCurrentTrakerWP;
            //}

            tracker.transform.position = checkPointManager.lastCheckPoint.transform.position;

            tracker.transform.rotation = checkPointManager.lastCheckPoint.transform.rotation;

            drive.rb.gameObject.transform.position = tracker.transform.position + Vector3.up * 2;

            drive.rb.gameObject.transform.rotation = tracker.transform.rotation;




            //drive.rb.gameObject.transform.position = Circuit.wayPoints[ResetCurrentTrakerWP].transform.position + Vector3.up + new Vector3(Random.Range(-1, 1), 0,Random.Range(-1, 1));

            //tracker.transform.position = drive.rb.gameObject.transform.position;
            drive.rb.gameObject.layer = 6;

            //this.GetComponent<Ghost>().HoverOn();

            Invoke("ResetCarLayer", 7);
        }

        if (Time.time < avoidDetector.avoidTime)
        {
            localTarget = tracker.transform.right * avoidDetector.avoidPath;
        }
        else
        {
            localTarget = drive.rb.gameObject.transform.InverseTransformPoint(tracker.transform.position);
        }

        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(drive.currentSpeed);

        float accel = 1f;
        float brake = 0;

        float speedFactor = drive.currentSpeed / drive.maxSpeed;
        float corner = Mathf.Clamp(targetAngle, 0, 90);
        float cornerFactor = corner / 90f;

        if (corner > 10f && speedFactor > 0.1f)
        {
            brake = Mathf.Lerp(0, 1 + speedFactor * brakingSensitivity, cornerFactor);
        }

        if (corner > 20 && speedFactor > 0.2f)
        {
            accel = Mathf.Lerp(0, 1 * AccelSensitivity, 1 - cornerFactor);
        }

        prevTorque = drive.torque;

        if (speedFactor < 0.3f && drive.rb.transform.forward.y > 0.1f)
        {
            drive.torque *= 4;
            accel = 1f;
            brake = 0;
        }

        drive.Driving(accel, steer, brake);

        drive.CheckForSKid();
        drive.CalculateEngineSound();

        drive.torque = prevTorque;
    }
}
