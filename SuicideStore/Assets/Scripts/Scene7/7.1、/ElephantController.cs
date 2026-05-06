using UnityEngine;

public class ElephantController : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 3f;
    public KeyCode moveKey = KeyCode.D;

    [Header("引用")]
    public Animator animator;
    public Rigidbody2D rb;
    public Scene7DialogueManager dialogueManager;

    private bool isMoving = false;

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<Scene7DialogueManager>();
        }
    }

    void Update()
    {
        HandleMovement();
        UpdateAnimation();
    }

    void HandleMovement()
    {
        if (Input.GetKey(moveKey))
        {
            isMoving = true;
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
            
            if (dialogueManager != null)
            {
                dialogueManager.CheckTriggerPosition(transform.position);
            }
        }
        else
        {
            isMoving = false;
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", isMoving);
        }
    }
}
