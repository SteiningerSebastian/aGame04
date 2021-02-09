using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helpers;

namespace Helpers
{
    public class MultiplierSteps
    {
        public int Multiplier
        {
            get
            {
                return steps[index];
            }
        }
        private int[] steps;
        private int index;
        public MultiplierSteps(int[] steps, int startIndex = 0)
        {
            this.steps = steps;
            index = startIndex;
        }

        public void Plus()
        {
            if (index < steps.Length-1)
            {
                index++;
            }
        }

        public void Minus()
        {
            if (index > 0)
            {
                index--;
            }
        }
    }
}

public class StarShipController : MonoBehaviour
{
    public Rigidbody rb;
    public float force = 10;
    public float velMaxForward;
    public float velMaxBackward;
    public float velMaxSide;
    public float velMaxUpDown;
    public float velRownl;
    public float jumpSpeed;
    private bool jumpStop;
    private float stopMultiplier = 10;
    public ParticleSystem pso1;
    public ParticleSystem pso2;
    private MultiplierSteps mlps;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        mlps = new MultiplierSteps(new int[13] { 10, 25, 50, 100, 500, 1000, 2000, 5000, 10000, 50000, 100000, 1000000, 10000000 }, 2);
        //pso1.Stop();
        //pso2.Stop();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");
        transform.Rotate(Vector3.up, x);
        transform.Rotate(Vector3.left, y);
        Vector3 v3Force = new Vector3(0.0f, 0.0f, 0.0f);
        Cursor.lockState = CursorLockMode.Locked;
        Vector3 v3Speed = transform.forward * jumpSpeed;

        if(Input.mouseScrollDelta.y > 0)
        {
            mlps.Plus();
        }else if (Input.mouseScrollDelta.y < 0)
        {
            mlps.Minus();
        }

        if (Input.GetKey("j"))
        {
            rb.velocity = v3Speed;
            jumpStop = true;
            pso1.Play();
            pso2.Play();
        }
        else if (jumpStop == true)
        {
            jumpStop = false;
            rb.velocity = Vector3.zero;
            pso1.Stop();
            pso2.Stop();
        }
        else
        {
            if (Input.GetKey("w"))
            {
                float maxVelDiv = 1 - Vector3.Project(rb.velocity, transform.forward).magnitude / velMaxForward;
                if (maxVelDiv < 0)
                {
                    maxVelDiv *= -stopMultiplier;
                }
                v3Force = transform.forward * force / maxVelDiv;
                AddForce(v3Force * Time.deltaTime);
                pso1.Play();
                pso2.Play();
            }
            else
            {
                pso1.Stop();
                pso2.Stop();
            }
            if (Input.GetKey("s"))
            {
                float maxVelDiv = 1 - Vector3.Project(rb.velocity, transform.forward * -1).magnitude / velMaxBackward;
                if (maxVelDiv < 0)
                {
                    maxVelDiv *= -stopMultiplier;
                }
                v3Force = transform.forward * -1 * force / maxVelDiv;
                AddForce(v3Force * Time.deltaTime);
            }
            if (Input.GetKey("a"))
            {
                float maxVelDiv = 1 - Vector3.Project(rb.velocity, transform.right * -1).magnitude / velMaxSide;
                if (maxVelDiv < 0)
                {
                    maxVelDiv *= -stopMultiplier;
                }
                v3Force = transform.right * -1 * force / maxVelDiv;
                AddForce(v3Force * Time.deltaTime);
            }
            if (Input.GetKey("d"))
            {
                float maxVelDiv = 1 - Vector3.Project(rb.velocity, transform.right).magnitude / velMaxSide;
                if (maxVelDiv < 0)
                {
                    maxVelDiv *= -stopMultiplier;
                }
                v3Force = transform.right * force / maxVelDiv;
                AddForce(v3Force * Time.deltaTime);
            }
            if (Input.GetKey("space"))
            {
                float maxVelDiv = 1 - Vector3.Project(rb.velocity, transform.up).magnitude / velMaxUpDown;
                if (maxVelDiv < 0)
                {
                    maxVelDiv *= -stopMultiplier;
                }
                v3Force = transform.up * force / maxVelDiv;
                AddForce(v3Force * Time.deltaTime);
            }
            if (Input.GetKey("left shift"))
            {
                float maxVelDiv = 1 - Vector3.Project(rb.velocity, transform.up * -1).magnitude / velMaxUpDown;
                if (maxVelDiv < 0)
                {
                    maxVelDiv *= -stopMultiplier;
                }
                v3Force = transform.up * -1 * force / maxVelDiv;
                AddForce(v3Force * Time.deltaTime);
            }
            if (Input.GetKey("q"))
            {
                transform.Rotate(Vector3.forward, velRownl * Time.deltaTime / 10);
            }
            if (Input.GetKey("e"))
            {
                transform.Rotate(Vector3.forward, -velRownl * Time.deltaTime / 10);
            }
        }
    }

    private void AddForce(Vector3 v3Force)
    {
        if (Input.GetKey("left ctrl") && Input.GetKey("w"))
        {
            v3Force *= mlps.Multiplier;
        }
        rb.AddForce(v3Force * Time.deltaTime);
    }
}
