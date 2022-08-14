using UnityEngine;

public class AvoidDetector : MonoBehaviour
{
    public float avoidPath = 0;
    public float avoidTime = 0;
    public float wanderDistance = 4; // distance between cars
    public float avoidLenght = 1; // delay 1 sec

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag != "car") return;

        avoidTime = 0;

    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag != "car") return;

        Rigidbody otherCarRB = collision.rigidbody;
        avoidTime = Time.time + 1;

        Vector3 otherCarLocalTarget = transform.InverseTransformPoint(otherCarRB.transform.position);
        float OtherCarAngle = Mathf.Atan2(otherCarLocalTarget.x, otherCarLocalTarget.z);

        avoidPath = wanderDistance * -Mathf.Sign(OtherCarAngle);

    }
}
