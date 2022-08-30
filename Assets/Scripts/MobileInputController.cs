using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MobileInputController : MonoBehaviour
{
    [HideInInspector] public float horizontalLeft;
    [HideInInspector] public float horizontalRight;
    [HideInInspector] public float accel;
    [HideInInspector] public float brake;

    public TextMeshProUGUI accelText;

    // Start is called before the first frame update
    void Start()
    {
        accel = 1;
        accelText.text = "D";
    }

    public void OnPointerDownLeft()
    {
        horizontalLeft = -1;
    }


    public void OnPointerUpLeft()
    {
        horizontalLeft = 0;
    }


    public void OnPointerDownRight()
    {
        horizontalRight = 1;
    }


    public void OnPointerUpRight()
    {
        horizontalRight = 0;
    }

    public void OnPointerDownBrake()
    {
        brake = 1;
    }


    public void OnPointerUpBrake()
    {
        brake = 0;
    }

    public void Acceleration()
    {
        if (accel ==-1)
        {
            accel = 1;
            accelText.text = "D";
        }
        else 
        {
            accel = -1;
            accelText.text = "R";
        }
    }

}
