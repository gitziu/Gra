using UnityEngine;

public class WallCheckCollider : MonoBehaviour
{
    private EnemyController ec;

    void Awake()
    {
        ec = transform.parent.GetComponent<EnemyController>();
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!ec.onGround) return;
        if (collision.gameObject.CompareTag("Collectible")) return;
        if (collision.gameObject.CompareTag("Exit")) return;
        if (collision.gameObject.CompareTag("Player")) return;
        Debug.Log("Entering collison with wall");
        Debug.Log(collision.gameObject.name);
        ec.flip(transform.parent);
        //ec.flip(transform.parent.Find("visual"));
    }
}
