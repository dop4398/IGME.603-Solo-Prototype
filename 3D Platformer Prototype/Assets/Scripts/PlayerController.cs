using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class PlayerController : MonoBehaviour
{
    // put events here if necessary

    #region Field Declarations
    [SerializeField] private Camera camera;
    [SerializeField] private Rigidbody body;
    private Vector3 cameraForward;
    private Vector3 cameraRight;

    [SerializeField] private float acceleration = 0.01f;
    [SerializeField] private float deceleration = 0.01f;
    private float maxSpeed = 1.0f;
    [SerializeField] private float runSpeed = 10.0f;
    [SerializeField] private float crouchSpeed = 5.0f;

    [SerializeField] private float jumpForce = 100.0f;
    [SerializeField] private Vector3 gravity = new Vector3(0.0f, -0.1f, 0.0f);

    [SerializeField] private Vector3 velocity = Vector3.zero;
    [SerializeField] private Vector2 velocity2D = Vector2.zero;

    [SerializeField] private bool grounded = false;
    [SerializeField] private bool moving = false;
    [SerializeField] private float jumpTimer = 0.0f;
    [SerializeField] private float jumpTimeLimit = 0.5f;
    [SerializeField] private int jumpStage = 0;

    #endregion

    void Start()
    {
        body = GetComponent<Rigidbody>();
        velocity2D = new Vector2(body.velocity.x, body.velocity.z);
        Cursor.lockState = CursorLockMode.Locked;
    }

    
    void Update()
    {
        MovePlayer();

        LockMouse();
    }

    /* TODO:
     ** Move by applying a force in the given direction.
     ** Deccelerate when the input stops IFF the player is on the ground.
     ** Do not apply lateral force while airborne.
     ** Jump on input IFF on the ground.
     ** Multiple jumps in a row (~.5s between landing and jumping) will change the vertical jump force.
     ** When crouched, if the jump button is pressed, do a unique jump.
     * When crouched and moving in a direction and the player jumps, do unique jumps.
     ** If airborne and a collision with a wall is detected, bonk off the wall - set velocity to 0, add a force opposite to the previous velocity, deccel does the rest.
    */

    #region Player Movement
    private void MovePlayer()
    {
        // Only apply drag if on the ground. ignore if airborne.
        // eventually will have some conditionals for applying lateral forces
        MoveLateral();

        // jump goes here
        Jump();

        // reset if stuck
        if(Input.GetKeyDown(KeyCode.R))
        {
            transform.position = new Vector3(12.0f, 2.0f, 8.0f);
        }
    }

    private void MoveLateral()
    {
        cameraForward = new Vector3(camera.transform.forward.x, 0.0f, camera.transform.forward.z);
        cameraRight = new Vector3(camera.transform.right.x, 0.0f, camera.transform.right.z);

        if(Input.GetKey(KeyCode.LeftControl))
        {
            maxSpeed = crouchSpeed;
        }
        else
        {
            maxSpeed = runSpeed;
        }

        // while a key is down, accelerate to maxSpeed
        if (Input.GetKey(KeyCode.W))
        {
            Accelerate(cameraForward);
            moving = true;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Accelerate(cameraForward * -1.0f);
            moving = true;
        }
        

        if (Input.GetKey(KeyCode.A))
        {
            Accelerate(cameraRight * -1.0f);
            moving = true;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Accelerate(cameraRight);
            moving = true;
        }

        // otherwise, decelerate when grounded and a key isn't pressed
        if (grounded && !(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            Decelerate();
            moving = false;
        }

        if (Input.GetKeyUp(KeyCode.W))
        {

        }
    }

    private void Jump()
    {
        if (grounded)
        {
            jumpTimer += Time.deltaTime;
        }

        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            if (Input.GetKey(KeyCode.LeftControl)) // crouch jumps
            {
                if (moving)
                {
                    body.AddForce(transform.up * jumpForce * 0.5f);
                    AddVelocity(body.velocity.normalized * jumpForce);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Jumps/Back Jump");
                }
                else
                {
                    body.AddForce(transform.up * jumpForce * 2.0f);
                    body.AddForce(camera.transform.forward * jumpForce * -0.25f);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Jumps/Back Jump");
                }
            }
            else
            {
                switch (jumpStage)
                {
                    case 1:
                        if (jumpTimer < jumpTimeLimit)
                        {
                            jumpStage = 2;
                            body.AddForce(transform.up * jumpForce * 1.5f);
                            FMODUnity.RuntimeManager.PlayOneShot("event:/Jumps/Jump2");
                        }
                        else
                        {
                            body.AddForce(transform.up * jumpForce);
                            FMODUnity.RuntimeManager.PlayOneShot("event:/Jumps/Jump1");
                        }
                        break;
                    case 2:
                        if (jumpTimer < jumpTimeLimit)
                        {
                            body.AddForce(transform.up * jumpForce * 2.0f);
                            FMODUnity.RuntimeManager.PlayOneShot("event:/Jumps/Jump3");
                        }
                        else
                        {
                            body.AddForce(transform.up * jumpForce);
                            FMODUnity.RuntimeManager.PlayOneShot("event:/Jumps/Jump1");
                        }
                        jumpStage = 0;
                        break;
                    default:
                        body.AddForce(transform.up * jumpForce);
                        jumpStage = 1;
                        FMODUnity.RuntimeManager.PlayOneShot("event:/Jumps/Jump1");
                        break;
                }
                jumpTimer = 0.0f;
            }
            grounded = false;
        }
        else if (!grounded) // apply gravity
        {
            AddVelocity(gravity);
        }
        else
        {
            body.velocity = new Vector3(body.velocity.x, 0.0f, body.velocity.z);
        }
    }

    private void WallBonk(Vector3 v)
    {
        AddVelocity(new Vector3(v.x, 0.0f, v.z).normalized * maxSpeed * 50.0f);
        FMODUnity.RuntimeManager.PlayOneShot("event:/Wall Bonk");
    }

    private void EnemyStomp()
    {
        body.AddForce(transform.up * jumpForce * 0.5f);
        FMODUnity.RuntimeManager.PlayOneShot("event:/Stomp");
    }
    #endregion

    #region Helper Methods
    // make lateral movement use 2D vectors??
    private void Accelerate(Vector3 direction)
    {       
        if(grounded)
        {
            AddVelocity(direction.normalized * maxSpeed * acceleration);
        }
        else
        {
            AddVelocity(direction.normalized * maxSpeed * acceleration * 0.25f);
        }

        velocity2D = new Vector2(body.velocity.x, body.velocity.z);

        if (velocity2D.sqrMagnitude > maxSpeed * maxSpeed)
        {
            velocity2D = velocity2D.normalized * maxSpeed;
            body.velocity = new Vector3(velocity2D.x, body.velocity.y, velocity2D.y);
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
    private void Decelerate(Vector3 direction)
    {
        if (body.velocity.sqrMagnitude > 0.1f)
        {
            AddVelocity(direction.normalized * maxSpeed * -deceleration); // decelerate by a percentage of maxSpeed per tick
        }
        else
        {
            body.velocity = Vector3.zero;
        }
    }

    private void AddVelocity(Vector3 v)
    {
        body.velocity += v * Time.deltaTime;
    }

    private void LockMouse()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }
    }
    #endregion


    #region Collision Resolution
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            grounded = true;
        }

        // check for bonk
        if (!grounded && collision.gameObject.CompareTag("Wall"))
        {
            
            WallBonk(collision.relativeVelocity);
        }

        if(collision.gameObject.CompareTag("Enemy"))
        {
            EnemyStomp();
        }

        body.drag = 1.0f;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            grounded = false;
        }

        body.drag = 0.0f;
    }
    #endregion
}
