using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementRestrictor : MonoBehaviour
{
    public float maxDistanceFromCenter;
    public float minDistanceFromCenter;
    public Transform center;
    public Transform farthestPosition;
    public Transform nearestPosition;

    public void LateUpdate()
    {
        //restrict movement during drags
        if (Vector3.Distance(transform.position, center.position) > maxDistanceFromCenter)
        {
            //too far away
            transform.position = farthestPosition.position;
            transform.LookAt(center);
        }
        else if (Vector3.Distance(transform.position, center.position) < minDistanceFromCenter)
        {
            //too close
            transform.position = nearestPosition.position;
            transform.LookAt(center);
        }
    }
}
