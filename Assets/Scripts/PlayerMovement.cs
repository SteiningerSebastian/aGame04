using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rbThis;
    //void FixedUpdate()
    //{
    //    //Attractor attractor = Attractor.FindClosestAttractor(rbThis);
    //    //this.transform.parent = attractor.transform;
    //    //transform.RotateAround(attractor.rb.position, Vector3.up, (360 / ) * Time.deltaTime);
    //    //transform.Rotate(0, (360 / attractor.rotationTimeInS) * Time.deltaTime, 0, Space.Self);
    //}

    public float moveSpeed;

    private Vector3 moveDirection;

    void Update()
    {
        moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
    }

    void FixedUpdate()
    {
        rbThis.MovePosition(GetComponent<Rigidbody>().position + transform.TransformDirection(moveDirection) * moveSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision");
    }
}
