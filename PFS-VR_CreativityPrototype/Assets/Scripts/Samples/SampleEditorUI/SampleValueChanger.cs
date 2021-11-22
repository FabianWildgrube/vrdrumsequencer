using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;

/// <summary>
/// An element that can be dragged along a line between a min and max position to change a single float value of a sample (from 0.0 - 1.0)
/// 
/// Assumes that Movement is handled via a MRTK ObjectManipulator and MRTK Constraint components.
/// LateUpdate() is used to prevent movement beyond min and max points.
/// </summary>
public class SampleValueChanger : MonoBehaviour
{
    public GameObject visualisation;
    public Renderer visualisationRenderer;
    public float emissiveIntensity = 1f;
    [Range(0.0f, 1.0f)]
    [Tooltip("Value with wich the color of the visualisation is multiplied whenever the user grabs the visualisation")]
    public float mainpulationColorMultiplier = 0.95f;
    public float manipulationEmissiveColorMultiplier = 0.99f;
    public SimpleLineDataProvider lineDataProvider;
    public MixedRealityLineRenderer lineRenderer;
    public TMPro.TextMeshPro debugLabel;
    public bool showDebugLabel = false;

    private int index;
    private Sample sample;
    private float value => sample.getValue(index);
    private Vector3 farthestPosition => center.position - transform.forward * maxDistanceFromCenter;
    private Vector3 nearestPosition => center.position - transform.forward * minDistanceFromCenter;
    private Transform center;
    private float rangeOfMotion => maxDistanceFromCenter - minDistanceFromCenter;
    private Transform minLimitSphere;
    private Transform maxLimitSphere;
    private float maxDistanceFromCenter => maxLimitSphere.lossyScale.x * 0.5f;
    private float minDistanceFromCenter => minLimitSphere.lossyScale.x * 0.5f;
    private bool manipulationActive;

    private float lineWidthAtNormalScale;

    private Color color;

    /// Initializes this changer object to the appropriate position within the fibbonaci sphere representing its initial value
    /// Expected to be called immediately after instantiation
    public void init(int index, Sample sample, Transform center, Vector3 positionOnUnitFibSphere, Transform closestDistanceLimitInfo, Transform farthestDistanceLimitInfo, Color c)
    {
        this.index = index;
        this.sample = sample;
        this.sample.OnClipChanged += this.handleSampleDefinitionChanged;

        debugLabel.text = value.ToString();

        this.center = center;
        transform.position = positionOnUnitFibSphere;

        minLimitSphere = closestDistanceLimitInfo;
        maxLimitSphere = farthestDistanceLimitInfo;
        lineDataProvider.EndPoint = new MixedRealityPose(new Vector3(0, 0, rangeOfMotion));

        lineDataProvider.EndPoint = new MixedRealityPose(new Vector3(0, 0, rangeOfMotion));

        manipulationActive = false;

        this.color = c;

        lineWidthAtNormalScale = lineRenderer.WidthMultiplier;

        lineDataProvider.enabled = false;
    }

    void Start()
    {
        //do this in Start() so that all transforms have already been applied
        updateToNewBasePosition(transform.position);

        visualisationRenderer.material.EnableKeyword("_EMISSION");
        visualisationRenderer.material.color = color;
        visualisationRenderer.material.SetColor("_EmissionColor", new Color(color.r * emissiveIntensity, color.g * emissiveIntensity, color.b * emissiveIntensity, 1f));

        debugLabel.gameObject.SetActive(showDebugLabel);
    }

    ///Place at the furthest point; rotate so that forward is the axis we can move along; move the visualisation to the correct position
    public void updateToNewBasePosition(Vector3 positionOnUnitFibSphere)
    {
        transform.position = positionOnUnitFibSphere;
        transform.LookAt(center);
        transform.position = farthestPosition;
        transform.LookAt(center);

        enforceCorrectPositionForVisualisation();
    }

    /// inform the value changer if its scale changed, so it can adjust the line width accordingly
    /// Expects a parameter > 1.0f if scaled up and < 1.0f if scaled down
    public void updateScale(float newScale)
    {
        lineRenderer.WidthMultiplier = lineWidthAtNormalScale * newScale;
    }

    public void HandleManipulationStarted(ManipulationEventData eventData)
    {
        Color c = visualisationRenderer.material.color;
        Color _emissionColor = visualisationRenderer.material.GetColor("_EmissionColor");
        visualisationRenderer.material.SetColor("_EmissionColor", new Color(_emissionColor.r * manipulationEmissiveColorMultiplier, _emissionColor.g * manipulationEmissiveColorMultiplier, _emissionColor.b * manipulationEmissiveColorMultiplier, 1f));
        visualisationRenderer.material.color = new Color(c.r * mainpulationColorMultiplier, c.g * mainpulationColorMultiplier, c.b * mainpulationColorMultiplier);
        enforceCorrectPositionForVisualisation();
        manipulationActive = true;
        lineDataProvider.enabled = true;
    }

    public void Update()
    {
        if (showDebugLabel)
        {
            float currentValue = Vector3.Distance(visualisation.transform.position, farthestPosition) / rangeOfMotion;
            debugLabel.text = currentValue.ToString();
        }
    }

    public void LateUpdate()
    {
        if (manipulationActive)
        {
            //restrict movement during drags
            if (Vector3.Distance(visualisation.transform.position, center.position) > maxDistanceFromCenter)
            {
                //too far away
                visualisation.transform.position = farthestPosition;
                visualisation.transform.LookAt(center);
            } else if (Vector3.Distance(visualisation.transform.position, center.position) < minDistanceFromCenter)
            {
                //too close
                visualisation.transform.position = nearestPosition;
                visualisation.transform.LookAt(center);
            }
        }
    }

    public void HandleManipulationEnded(ManipulationEventData eventData)
    {
        manipulationActive = false;
        lineDataProvider.enabled = false;

        float newValue = Vector3.Distance(visualisation.transform.position, farthestPosition) / rangeOfMotion;
        sample.update(index, newValue);
        if (showDebugLabel) debugLabel.text = value.ToString();
        UsageLogger.log(UserAction.SAMPLE_MODIFIED);
    }

    public void handleSampleDefinitionChanged(AudioClip clip)
    {
        if (showDebugLabel) debugLabel.text = value.ToString();
        enforceCorrectPositionForVisualisation();
    }

    private void enforceCorrectPositionForVisualisation()
    {
        visualisation.transform.position = farthestPosition + (transform.forward * value * rangeOfMotion);
        visualisation.transform.LookAt(center);
    }
}
