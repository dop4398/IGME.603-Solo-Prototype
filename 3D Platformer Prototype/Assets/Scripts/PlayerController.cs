using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // put events here if necessary

    #region Field Declarations
    [SerializeField] private Camera camera;
    [SerializeField] private Rigidbody body;
    [SerializeField] private float acceleration = 0.01f;
    [SerializeField] private float deceleration = 0.01f;
    [SerializeField] private float maxSpeed = 1.0f;
    [SerializeField] private float jumpForce = 100.0f;
    [SerializeField] private Vector3 gravity = new Vector3(0.0f, -0.1f, 0.0f);

    private bool grounded = false;
    private Vector3 cameraForward;
    private Vector3 cameraRight;
    #endregion

    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    
    void Update()
    {
        MovePlayer();
    }

    /* TODO:
     * Move by applying a force in the given direction.
     * Deccelerate when the input stops IFF the player is on the ground.
     * Do not apply lateral force while airborne.
     * Jump on input IFF on the ground.
     * Multiple jumps in a row (~.5s between landing and jumping) will change the vertical jump force.
     * When crouched, if the jump button is pressed, do a unique jump.
     * When crouched and moving in a direction and the player jumps, do unique jumps.
     * If airborne and a collision with a wall is detected, bonk off the wall - set velocity to 0, add a force opposite to the previous velocity, deccel does the rest.
    */

    
    private void MovePlayer()
    {
        // Only apply drag if on the ground. ignore if airborne.
        // eventually will have some conditionals for applying lateral forces
        MoveLateral();

        // jump goes here
        Jump();
    }

    private void MoveLateral()
    {
        cameraForward = new Vector3(camera.transform.forward.x, 0.0f, camera.transform.forward.z);
        cameraRight = new Vector3(camera.transform.right.x, 0.0f, camera.transform.right.z);

        // while a key is down, accelerate to maxSpeed
        if (Input.GetKey(KeyCode.W))
        {
            Accelerate(cameraForward);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Accelerate(cameraForward * -1.0f);
        }

        if (Input.GetKey(KeyCode.A))
        {
            Accelerate(cameraRight * -1.0f);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Accelerate(cameraRight);
        }

        // otherwise, decelerate when grounded and a key isn't pressed
        if (grounded && !(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            Decelerate();
        }
    }

    private void Decelerate()
    {
        if (body.velocity.sqrMagnitude > 0.1f)
        {
            AddVelocity(body.velocity.normalized * maxSpeed * -deceleration); // decelerate by a percentage of maxSpeed per tick
        }
        else
        {
            body.velocity = Vector3.zero;
        }
    }

    private void Accelerate(Vector3 direction)
    {
        AddVelocity(direction.normalized * maxSpeed * acceleration);

        if (body.velocity.sqrMagnitude < maxSpeed * maxSpeed)
        {
            body.velocity = body.velocity.normalized * maxSpeed;
        }
    }

    private void AddVelocity(Vector3 v)
    {
        body.velocity += v;
    }


    private void Jump()
    {
        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            grounded = false;
            body.AddForce(transform.up * jumpForce);
        }
        else if(!grounded) // apply gravity
        {
            body.velocity += gravity;
        }
    }

    private void WallBonk()
    {
        body.velocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            grounded = true;
        }

        // check for bonk
        if (!grounded && collision.gameObject.CompareTag("Wall"))
        {
            WallBonk();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            grounded = false;
        }
    }
}
