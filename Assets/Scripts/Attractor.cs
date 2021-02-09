using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor : MonoBehaviour
{
    public Rigidbody rb;
    public int exponent = 24;
    public static List<Attractor> Attractors;
    static float G = 6.67259f * Mathf.Pow(10, -11);
    public bool canAttract = true;
    //radius/scale 696340km/10000 
    void Attract(Attractor objToAttract)
    {
        Rigidbody rbToAttract = objToAttract.rb;
        Vector3 direction = rb.position - rbToAttract.position;
        float distance = direction.magnitude;
        if (distance == 0)
        {
            return;
        }

        float forceMagnitude = (rb.mass * rbToAttract.mass) * Mathf.Pow(10, exponent) / Mathf.Pow(distance, 2) * G;

        Vector3 gForce = direction.normalized * forceMagnitude;

        rbToAttract.AddForce(gForce);
    }

    private void OnEnable()
    {
        if (Attractors == null)
        {
            Attractors = new List<Attractor>();
        }
        Attractors.Add(this);
    }

    private void OnDisable()
    {
        Attractors.Remove(this);
    }

    private void FixedUpdate()
    {
        if (canAttract == true)
        {
            foreach (Attractor attractor in Attractors)
            {
                if (attractor != this && attractor.tag != "Planet")
                {
                    Attract(attractor);
                }
            }
        }
        /*transform.parent = attractorClose.transform;
        Vector3 v = transform.position - attractorClose.transform.position;
        transform.rotation = Quaternion.FromToRotation(Vector3.up, v);

        Vector3 gravityUp = (transform.position - attractorClose.transform.position).normalized;
        Vector3 localUp = transform.up;

        Quaternion targetRotation = Quaternion.FromToRotation(localUp, gravityUp) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 50f * Time.deltaTime);
    }*/
    }
    public static Attractor FindClosestAttractor(Rigidbody rb)
    {
        Attractor attractorClose = Attractors[0];
        float minForceMagnitude = -1;
        foreach (Attractor attractor in Attractors)
        {
            if (attractor.rb.transform != rb.transform && attractor.canAttract == true)
            {
                Vector3 direction = rb.position - attractor.rb.position;
                float distance = direction.magnitude;
                if (distance == 0)
                {
                    distance = 0.001f;
                }

                float forceMagnitude = (rb.mass * attractor.rb.mass) * Mathf.Pow(10, attractor.exponent) / Mathf.Pow(distance, 2) * G;
                forceMagnitude = Mathf.Abs(forceMagnitude);
                if (forceMagnitude > minForceMagnitude)
                {
                    attractorClose = attractor;
                }
            }
        }
        if (attractorClose.rb.transform != rb.transform)
        {
            return attractorClose;
        }
        else
        {
            return null;
        }
    }
}