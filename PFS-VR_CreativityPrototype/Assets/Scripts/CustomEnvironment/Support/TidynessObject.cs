using UnityEngine;

public class TidynessObject : MonoBehaviour
{
    [SerializeField]
    [Tooltip("First transform should be the untidyest, last one the most tidy. Must not be children of this gameobject!")]
    Transform[] tidynessStates;

    //interpolate the new position and rotation from the two closest adjacent tidynessStates
    //(seeing newValue as the marker where along the tidyness-"scale" we are)
    public void applyTidynessValue(float newValue)
    {
        int nrOfIntervals = tidynessStates.Length - 1;
        int prevIdx = Mathf.FloorToInt(newValue * nrOfIntervals);
        int nextIdx = Mathf.CeilToInt(newValue * nrOfIntervals);

        if (prevIdx == nextIdx)
        {
            transform.position = tidynessStates[prevIdx].position;
            transform.rotation = tidynessStates[prevIdx].rotation;
            return;
        }

        Transform prevTrans = tidynessStates[prevIdx];
        Transform nextTrans = tidynessStates[nextIdx];

        float normalizedStep = 1.0f / nrOfIntervals;
        float prevNormalizedValue = prevIdx * normalizedStep;
        float nextNormalizedValue = nextIdx * normalizedStep;
        float progressWithinSteps = (newValue - prevNormalizedValue) / (nextNormalizedValue - prevNormalizedValue);

        transform.position = Vector3.Lerp(prevTrans.position, nextTrans.position, progressWithinSteps);
        transform.rotation = Quaternion.Slerp(prevTrans.rotation, nextTrans.rotation, progressWithinSteps);
    }

    public void OnDrawGizmos()
    {
        if (tidynessStates != null && tidynessStates.Length > 1)
        {
            for (int i = 1; i < tidynessStates.Length; ++i)
            {
                Vector3 currentPos = tidynessStates[i-1].position;
                Vector3 nextPos = tidynessStates[i].position;
                Gizmos.DrawSphere(currentPos, .06f);
                Gizmos.DrawLine(currentPos, nextPos);
            }

            Gizmos.DrawSphere(tidynessStates[tidynessStates.Length - 1].position, .06f);
        }
    }
     /*
    IEnumerator Move()
    {
        while (true)
        {
            if (moving)
            {
                transform.position = Vector3.MoveTowards(transform.position, waypoints[nextWaypointIdx], patrolSpeed * Time.deltaTime);
                if (transform.position == waypoints[nextWaypointIdx])
                {
                    nextWaypointIdx = (nextWaypointIdx + 1) % waypoints.Length;
                    moving = false;
                    yield return new WaitForSeconds(restDuration);
                }
                yield return null;
            }
            else
            {
                //get rotation towards next waypoint
                Vector3 dir = (waypoints[nextWaypointIdx] - transform.position).normalized;
                float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                //rotate there
                Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnAngleSpeed * Time.deltaTime);
                //once we're there moving = true;
                if (transform.rotation == targetRotation)
                {
                    moving = true;
                }
                yield return null;
            }

        }
    }
     */
}
