using UnityEngine;

public class AirWall : MonoBehaviour
{
    [Header("阻挡设置")]
    public bool stopPlayerMovement = true;
    public bool showDebugMessage = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (showDebugMessage)
            {
                Debug.Log("玩家碰到空气墙，无法通过！");
            }

            if (stopPlayerMovement)
            {
                Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (showDebugMessage)
            {
                Debug.Log("玩家碰到空气墙触发体！");
            }
        }
    }
}
