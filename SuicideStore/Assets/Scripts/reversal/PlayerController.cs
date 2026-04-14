using UnityEngine;

public class ReversalPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject maskObject;
    
    private Rigidbody2D rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Update()
    {
        float horizontalInput = 0;
        if (Input.GetKey(KeyCode.A))
            horizontalInput = -1;
        else if (Input.GetKey(KeyCode.D))
            horizontalInput = 1;
        
        if (rb != null)
        {
            Vector2 movement = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
            rb.velocity = movement;
        }
        
        if (maskObject != null)
        {
            maskObject.transform.position = transform.position;
        }
    }
}