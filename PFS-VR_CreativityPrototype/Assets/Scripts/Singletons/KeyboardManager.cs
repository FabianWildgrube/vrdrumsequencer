using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardManager : MonoBehaviour
{
    public static KeyboardManager instance;

    [HideInInspector]
    public Keyboard keyboard;

    [SerializeField]
    private GameObject keyboardPrototype;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            var kbGO = Instantiate(keyboardPrototype);
            keyboard = kbGO.GetComponent<Keyboard>();
            if (keyboard == null)
            {
                Debug.LogError("Keyboard manager has a wrong keyboard prefab!");
            }
            kbGO.SetActive(false);
        }
    }
}
