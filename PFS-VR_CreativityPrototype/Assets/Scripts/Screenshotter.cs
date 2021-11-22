using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Screenshotter : MonoBehaviour
{
    public KeyCode screenshotKey;

    public int width = 5000;
    public int height = 3000;

    [Header("Panorama Series Settings")]
    public int panoramaUpDownStart = 12;
    public int panoramaUpDownSteps = 4;
    public int panoramaUpDownStepAngle = 12;
    public int panoramaYSteps = 8;

    [Header("Output")]
    public string outputDirectory;
    public string outputFileName;

    private RenderTexture renderT;

    void Start()
    {
        _camera = GetComponent<Camera>();
        renderT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
    }

    public void setPosition(Vector3 pos, Quaternion rot)
    {
        Camera.transform.position = pos;
        Camera.transform.rotation = rot;
    }

    private Camera Camera
    {
        get
        {
            if (!_camera)
            {
                _camera = Camera.main;
            }
            return _camera;
        }
    }
    private Camera _camera;

    private void LateUpdate()
    {
        if (Input.GetKeyDown(screenshotKey))
        {
            Capture();
        }
    }

    WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    public void Capture()
    {
        StartCoroutine(capture(outputFileName));
    }

    public void Capture(string fileName)
    {
        StartCoroutine(capture(fileName));
    }

    public void CapturePanorama()
    {
        StartCoroutine(panoramaSeries());
    }

    public void Capture360CubeImage()
    {
        byte[] bytes = I360Render.Capture(width, true, Camera);
        if (bytes != null)
        {
            string path = Path.Combine(Path.Combine(outputDirectory, outputFileName + "_360panorama.jpg"));
            File.WriteAllBytes(path, bytes);
            Debug.Log("360 render saved to " + path);
        }
    }

    private IEnumerator panoramaSeries()
    {
        for (int j = 0; j < panoramaUpDownSteps; j++)
        {
            float eulerX = (j > 0 ? Camera.transform.rotation.eulerAngles.x - panoramaUpDownStepAngle : panoramaUpDownStart); // tilt up (starting from slightly looking down)
            for (int i = 0; i < panoramaYSteps; i++)
            {
                Camera.transform.rotation = Quaternion.Euler(
                    eulerX,
                    (i > 0 ? Camera.transform.rotation.eulerAngles.y + (360 / (1.0f * panoramaYSteps)) : 0), // reset to 0 for first, then go around the circle
                    Camera.transform.rotation.eulerAngles.z);
                yield return capture(outputFileName + "_" + j + "_" + i);
            }
        }
    }

    private IEnumerator capture(string outputFileName)
    {
        yield return frameEnd;
        Camera.targetTexture = renderT;

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderT;

        Camera.Render();

        Texture2D screenshot = new Texture2D(renderT.width, renderT.height, TextureFormat.ARGB32, false, true);
        screenshot.ReadPixels(new Rect(0, 0, screenshot.width, screenshot.height), 0, 0);
        screenshot.Apply();

        Camera.targetTexture = null;
        RenderTexture.active = currentRT;

        byte[] bytes = screenshot.EncodeToJPG();
        Destroy(screenshot);

        Directory.CreateDirectory(outputDirectory);
        File.WriteAllBytes(Path.Combine(outputDirectory, outputFileName + ".jpg"), bytes);
        Debug.Log("Save screenshot " + outputFileName);
    }
}
