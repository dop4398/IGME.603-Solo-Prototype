using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // put events here if necessary

    #region Field Declarations
    [SerializeField] private Camera camera;
    [SerializeField] private Rigidbody body;
    [SerializeField] private float maxSpeed = 1.0f;
    [SerializeField] private float jumpForce = 100.0f;
    [SerializeField] private Vector3 gravity = new Vector3(0.0f, -0.1f, 0.0f);

    private bool grounded = false;
    private Vector3 cameraForward;
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

        if (Input.GetKeyDown(KeyCode.W))
        {
            body.AddForce(cameraForward * maxSpeed);
        } 
        else if(Input.GetKeyDown(KeyCode.S))
        {
            body.AddForce(cameraForward * -1 * maxSpeed);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            body.AddForce(camera.transform.right * -1 * maxSpeed);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            body.AddForce(camera.transform.right * maxSpeed);
        }
    }

    private void ApplyDrag()
    {
        body.velocity -= body.velocity * 0.01f;
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
