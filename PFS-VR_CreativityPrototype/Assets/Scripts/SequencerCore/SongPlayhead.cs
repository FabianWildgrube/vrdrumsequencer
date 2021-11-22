using UnityEngine;

public class SongPlayhead : MonoBehaviour
{
    public Transform initialPosTransform;
    public AudioClip clip;

    private float _length;
    public float length { get { return _length;  } set
        {
            _length = value;
            if (clip != null) velocity = length / clip.length; // m/s
        } }

    float velocity;
    bool moving = false;
    float travelledDistance;
    private Vector3 initialPosition { get { return initialPosTransform.position; } }

    public void StartMoving()
    {
        moving = true;
    }

    public void StopMoving()
    {
        StopAllCoroutines();
        moving = false;
        travelledDistance = 0;
        transform.position = initialPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            float distThisFrame = velocity * Time.deltaTime;
            travelledDistance += distThisFrame;
            if (travelledDistance >= _length) //loop around at the end
            {
                float overshoot = travelledDistance - _length;
                distThisFrame = overshoot;
                travelledDistance = distThisFrame;
                transform.position = initialPosition;
            }
            transform.position += transform.forward * distThisFrame;
        }
    }
}
