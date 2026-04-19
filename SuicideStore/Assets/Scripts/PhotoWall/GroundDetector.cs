using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [Header("掉落检测")]
    public float groundY = -10f;
    public Transform spawnPoint;

    [Header("重生动画")]
    public float respawnDelay = 0.5f;
    public bool useAnimation = false;

    private bool isRespawning = false;

    void Start()
    {
        if (spawnPoint == null)
        {
            GameObject spawn = GameObject.FindGameObjectWithTag("SpawnPoint");
            if (spawn != null)
            {
                spawnPoint = spawn.transform;
            }
            else
            {
                spawnPoint = new GameObject("SpawnPoint").transform;
                spawnPoint.position = transform.position;
                spawnPoint.gameObject.tag = "SpawnPoint";
            }
        }
    }

    void Update()
    {
        if (isRespawning)
        {
            return;
        }

        if (transform.position.y < groundY)
        {
            Respawn();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeathZone"))
        {
            Respawn();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("DeathZone"))
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        if (isRespawning)
        {
            return;
        }

        StartCoroutine(RespawnCoroutine());
    }

    System.Collections.IEnumerator RespawnCoroutine()
    {
        isRespawning = true;

        gameObject.SetActive(false);
        yield return new WaitForSeconds(respawnDelay);

        transform.position = spawnPoint.position;
        transform.rotation = Quaternion.identity;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        gameObject.SetActive(true);
        isRespawning = false;
    }
}
