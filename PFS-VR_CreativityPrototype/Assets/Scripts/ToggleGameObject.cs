using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleGameObject : MonoBehaviour
{
    public GameObject gameObjectToToggle;

    public void toggleObjectActive()
    {
        gameObjectToToggle.SetActive(!gameObjectToToggle.activeInHierarchy);
    }
}
