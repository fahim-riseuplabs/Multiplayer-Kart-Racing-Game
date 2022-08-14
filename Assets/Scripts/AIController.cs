using UnityEngine;

public class AIController : MonoBehaviour
{
    
    public float brakingSensitivity = 1;
    private Drive drive;
    public float steeringSensitivity = 0.01f;
    public float AccelSensitivity = 0.3f;
    private Vector3 target;
    private Vector3 nextTarget;
    private int currentWayPoint = 0;
    private float totalDistanceToTarget;
    private bool isJumped = false;

    private Circuit Circuit;

    // Start is called before the first frame update
    void Start()
    {
        if(Circuit == null)
        {
            Circuit = GameObject.FindGameObjectWithTag("circut").GetComponent<Circuit>();
        }

        drive = GetComponent<Drive>();
        target = Circuit.wayPoints[currentWayPoint].transform.position;
        nextTarget = Circuit.wayPoints[currentWayPoint+1].transform.position;
        totalDistanceToTarget = Vector3.Distance(target, drive.rb.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 localTarget = drive.rb.gameObject.transform.InverseTransformPoint(target);
        Vector3 nextLocalTarget = drive.rb.gameObject.transform.InverseTransformPoint(nextTarget);

        float distanceToTarget = Vector3.Distance(target, drive.rb.gameObject.transform.position);

        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        float nextTargetAngle = Mathf.Atan2(nextLocalTarget.x, nextLocalTarget.z) * Mathf.Rad2Deg;

        float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(drive.currentSpeed);

        float distanceFactor = distanceToTarget / totalDistanceToTarget;
        float speedFactor = drive.currentSpeed / drive.maxSpeed;

        float accel = Mathf.Lerp(AccelSensitivity,1,distanceFactor);
        float brake = Mathf.Lerp((-1- Mathf.Abs(nextTargetAngle))* brakingSensitivity, 1+ speedFactor, 1 - distanceFactor);

        if (Mathf.Abs(nextTargetAngle) > 20)
        {
            brake += 0.8f;
            accel -= 0.8f;
        }

        if (isJumped)
        {
            brake = 0;
            accel = 1;
            Debug.Log(isJumped);
        }

        drive.Driving(accel, steer, brake);

        if (distanceToTarget < 5)
        {
            currentWayPoint++;

            if (currentWayPoint >= Circuit.wayPoints.Length)
            {
                currentWayPoint = 0;
            }
            target = Circuit.wayPoints[currentWayPoint].transform.position;
            totalDistanceToTarget = Vector3.Distance(target, drive.rb.transform.position);
            
            if(currentWayPoint == Circuit.wayPoints.Length - 1)
            {
                nextTarget = Circuit.wayPoints[0].transform.position;
            }
            else
            {
                nextTarget = Circuit.wayPoints[currentWayPoint+1].transform.position;
            }

            if (drive.rb.transform.InverseTransformPoint(target).y > 5)
            {
                isJumped = true;
            }
            else
            {
                isJumped = false;
            }
        }

        drive.CheckForSKid();
        drive.CalculateEngineSound();
    }
}
