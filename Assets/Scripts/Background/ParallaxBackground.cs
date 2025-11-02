using System.Collections.Generic;
using UnityEngine;
 
[ExecuteInEditMode]
public class ParallaxBackground : MonoBehaviour
{
    public CameraController cameraController;
    List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();
 
    void Start()
    {
        if (cameraController == null)
            cameraController = Camera.main.GetComponent<CameraController>();
 
        if (cameraController != null)
        {
            cameraController.onCameraTranslate += Move;
        }
 
        SetLayers();
    }
 
    void SetLayers()
    {
        parallaxLayers.Clear();
 
        for (int i = 0; i < transform.childCount; i++)
        {
            ParallaxLayer layer = transform.GetChild(i).GetComponent<ParallaxLayer>();
 
            if (layer != null)
            {
                layer.name = "Layer-" + i;
                parallaxLayers.Add(layer);
            }
        }
    }
 
    void Move(float delta)
    {
        foreach (ParallaxLayer layer in parallaxLayers)
        {
            layer.Move(delta);
        }
    }
}