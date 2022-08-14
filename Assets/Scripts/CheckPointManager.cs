using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    public int lap = 0;
    public int checkPoint = -1;
    private int checkPointCount = 0;
    private int nextCheckPoint = 0;
    public GameObject lastCheckPoint;
    public float enteredTime;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] checkPoints = GameObject.FindGameObjectsWithTag("checkpoint");
        checkPointCount = checkPoints.Length;

        foreach (GameObject checkPoint in checkPoints)
        {
            if (checkPoint.name == "0")
            {
                lastCheckPoint = checkPoint;
                break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "checkpoint")
        {

            int thisCPNumber = int.Parse(other.gameObject.name);

            if (thisCPNumber == nextCheckPoint && lap < RaceMonitor.totalLaps+1)
            {
                lastCheckPoint = other.gameObject;
                checkPoint = thisCPNumber;
                enteredTime = Time.time;

                if (checkPoint == 0)
                {
                    lap++;
                }

                nextCheckPoint++;

                if (nextCheckPoint >= checkPointCount)
                {
                    nextCheckPoint = 0;
                }
            }
        }
    }
}
