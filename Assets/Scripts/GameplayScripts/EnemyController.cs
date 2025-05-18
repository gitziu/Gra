using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem.iOS;
using System;

public class EnemyController : MonoBehaviour
{

    public float move = 0f;
    public bool onGround = false;
    private float firstDirection;
    private Rigidbody2D rb;
    private Collider2D c2d;

    void Awake()
    {
        rb = transform.GetComponent<Rigidbody2D>();
        c2d = transform.GetComponent<BoxCollider2D>();
        
    }
    void Start()
    {

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacles")) death();
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("on ground");
            move = 4f;
            onGround = true;
        }
        if (collision.gameObject.CompareTag("Enemy")) flip();
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            onGround = false;
            move = 0f;
        }
    }

    public void death()
    {
        //space for death animation
        Destroy(gameObject);
    }

    void Update()
    {
        rb.linearVelocity = new Vector2(move, 0f);
    }

    public void flip()
    {
        move *= -1;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }
}
