using UnityEngine;

public class BeatGridLine : MonoBehaviour
{
    public GameObject visualisation;
    public TMPro.TextMeshPro label;

    public void setHeight(float newHeight)
    {
        Vector3 oldScale = visualisation.transform.localScale;
        visualisation.transform.localScale = new Vector3(oldScale.x, newHeight, oldScale.z);
    }

    public void setLabel(string text)
    {
        if (label != null)
        {
            label.text = text;
        }
    }
}
