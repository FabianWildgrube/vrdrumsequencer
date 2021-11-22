using UnityEngine;

[RequireComponent(typeof(TMPro.TextMeshPro))]
public class LoopnameTxtField : MonoBehaviour
{
    public Loop loop;
    private TMPro.TextMeshPro textElement;

    // Start is called before the first frame update
    void Start()
    {
        textElement = GetComponent<TMPro.TextMeshPro>();
        textElement.text = loop.loopName;
        loop.OnNameChanged += this.updateName;
    }

    private void OnDestroy()
    {
        loop.OnNameChanged -= this.updateName;
    }

    void updateName(string newName)
    {
        textElement.text = newName;
    }
}
