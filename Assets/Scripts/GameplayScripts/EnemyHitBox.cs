using UnityEngine;

public class EnemyHitBox : MonoBehaviour
{

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            transform.parent.GetComponent<EnemyController>().death();
        }
    }

}
