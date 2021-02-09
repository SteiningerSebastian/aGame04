using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam.eventMask = ~cam.cullingMask;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
