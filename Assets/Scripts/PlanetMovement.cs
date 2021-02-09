using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetMovement : MonoBehaviour
{
    public Rigidbody sun;
    public float orbitTimeInS = 1;
    public float rotationTimeInS = 1;
    // Start is called before the first frame update
    private void FixedUpdate()
    {
        transform.RotateAround(sun.position, Vector3.up, (360 / orbitTimeInS) * Time.deltaTime);
        transform.Rotate(0, (360/rotationTimeInS) * Time.deltaTime, 0, Space.Self);
    }
}
