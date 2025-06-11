using UnityEngine;
using UnityEngine.Tilemaps;

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

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("enemy collision " + collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Obstacles")) death();
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("on ground");
            move = 4f;
            if (transform.Find("WallCheckCollider").GetComponent<BoxCollider2D>().IsTouching(GameObject.Find("Grid/Ground").GetComponent<TilemapCollider2D>()) || !transform.Find("GroundCheckCollider").GetComponent<BoxCollider2D>().IsTouching(GameObject.Find("Grid/Ground").GetComponent<TilemapCollider2D>())) flip(transform);
            onGround = true;
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            flip(transform);
        }
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

    public void flip(Transform flipObject)
    {
        move *= -1;
        Vector3 localScale = flipObject.localScale;
        localScale.x *= -1;
        flipObject.localScale = localScale;
    }
}
