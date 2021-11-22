using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideObjectHandler : MonoBehaviour
{
    public void hide()
    {
        gameObject.SetActive(false);
    }
}
