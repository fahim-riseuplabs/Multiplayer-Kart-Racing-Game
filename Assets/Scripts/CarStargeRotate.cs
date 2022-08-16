using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarStargeRotate : MonoBehaviour
{
  
    private void Update()
    {

        this.transform.Rotate(new Vector3(0, 1*Time.deltaTime*25, 0));
    }
}
