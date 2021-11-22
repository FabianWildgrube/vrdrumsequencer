using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuExample : MonoBehaviour
{
    public Transform initialPos;

    public void resetPosition()
    {
        transform.position = initialPos.position;
        transform.rotation = initialPos.rotation;
    }
}
