using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class CheckPointCreator : MonoBehaviour
{
    public Circuit circut;
    private GameObject tracker;
    public GameObject firstCP;
    public GameObject cpPrefab;
    public float cpDistance = 0.5f;
    private int currentTrackerWP = 0;
    private bool isTrackerStarted = false;
    private int number = 1;
    private float lastCPTimer;

    public void CreateCheckPoints()
    {
        tracker = GameObject.Find("CPPLACER");

        if (tracker == null)
        {
            tracker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            DestroyImmediate(tracker.GetComponent<Collider>());
        }

        tracker.transform.position = firstCP.transform.position;
        tracker.gameObject.name = "CPPLACER";
        lastCPTimer = Time.time + cpDistance;
        currentTrackerWP = 0;
        number = 1;
        isTrackerStarted = true;
    }

    void PlaceCheckPoint()
    {
        GameObject cp = Instantiate(cpPrefab);
        cp.transform.position = tracker.transform.position;
        cp.transform.rotation = tracker.transform.rotation;
        cp.transform.parent = this.transform;
        cp.gameObject.name = "" + number;
        number++;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isTrackerStarted) return;

        Quaternion rotation = Quaternion.LookRotation(circut.wayPoints[currentTrackerWP].transform.position - tracker.transform.position);
        tracker.transform.rotation = Quaternion.Slerp(tracker.transform.rotation, rotation, Time.deltaTime * 2);

        tracker.transform.Translate(0, 0, 1);

        if (Vector3.Distance(tracker.transform.position, circut.wayPoints[currentTrackerWP].transform.position) < 1)
        {
            currentTrackerWP++;

            if (currentTrackerWP >= circut.wayPoints.Length)
            {
                isTrackerStarted = false;
            }
        }

        if (lastCPTimer < Time.time)
        {
            PlaceCheckPoint();

            lastCPTimer = Time.time + cpDistance;
        }

        EditorApplication.QueuePlayerLoopUpdate();
    }
}
