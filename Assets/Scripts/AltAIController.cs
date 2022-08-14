using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltAIController : MonoBehaviour
{
    public Circuit circuit;

    private Vector3 target;
    private Vector3 direction;
    
    private int currentWP = 0;

    private float speed = 20.0f;
    private float accuracy =1.0f;
    private float rotSpeed = 4.0f;
    private float distanceTarget;

    // Start is called before the first frame update
    void Start()
    {
        target = circuit.wayPoints[currentWP].transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        distanceTarget = Vector3.Distance(target, this.transform.position);

        direction = target - transform.position;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);

        transform.Translate(0, 0, speed * Time.deltaTime);

        if (distanceTarget < accuracy)
        {
            currentWP++;
            if(currentWP >= circuit.wayPoints.Length)
            {
                currentWP = 0;
            }
            target = circuit.wayPoints[currentWP].transform.position;
        }
        
    }
}
