using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circuit : MonoBehaviour
{
    public GameObject[] wayPoints;

    public void OnDrawGizmos()
    {
        DrawGizmos(false);
    }

    public void OnDrawGizmosSelected()
    {
        DrawGizmos(true);
    }

    public void DrawGizmos(bool selected)
    {
        if (!selected)
        {
            return;
        }

        if (wayPoints.Length > 1)
        {
            Vector3 previous = wayPoints[0].transform.position;

            for(int i = 1; i < wayPoints.Length; i++)
            {
                Vector3 next = wayPoints[i].transform.position;
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
            Gizmos.DrawLine(previous, wayPoints[0].transform.position);
        }
    }
}
