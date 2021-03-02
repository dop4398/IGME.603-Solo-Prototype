using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public GameObject parent;

    private float moveTimer = 0.0f;
    private Vector3 direction;
    [SerializeField] private float moveForce = 5.0f;

    void Start()
    {
        
    }

    void Update()
    {
        moveTimer += Time.deltaTime;

        if(moveTimer > 2.0f)
        {
            direction = Random.onUnitSphere;
            direction.y = 0.0f;
            parent.GetComponent<Rigidbody>().AddForce(direction * moveForce, ForceMode.Impulse);
            moveTimer = 0.0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(parent);
        }
    }
}
