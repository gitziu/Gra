using UnityEngine;

public class GroundCheckCollider : MonoBehaviour
{
    private EnemyController ec;

    void Awake()
    {
        ec = transform.parent.GetComponent<EnemyController>();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!ec.onGround) return;
        if (collision.gameObject.CompareTag("Collectible")) return;
        if (collision.gameObject.CompareTag("Obstacles")) return;
        if (collision.gameObject.CompareTag("Exit")) return;
        Debug.Log("Exiting collision with ground");
        Debug.Log(collision.gameObject.name);
        ec.flip(transform.parent);
        //ec.flip(transform.parent.Find("visual"));
    }

}
