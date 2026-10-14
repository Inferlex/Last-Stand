using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    private Canvas canvas;
    private new Camera camera; //it showed warning if I didnt put new

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        Camera[] cameras = FindObjectsOfType<Camera>();
        camera = cameras[0];
        canvas.worldCamera = camera;
    }
}
