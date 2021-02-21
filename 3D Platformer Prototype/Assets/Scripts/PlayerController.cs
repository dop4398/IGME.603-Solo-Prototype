using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // put events here if necessary

    #region Field Declarations
    public Camera camera;
    [SerializeField] private float mass;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float direction;
    [SerializeField] private float acceleration;


    #endregion

    void Start()
    {
        
    }

    
    void Update()
    {
        
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
}
