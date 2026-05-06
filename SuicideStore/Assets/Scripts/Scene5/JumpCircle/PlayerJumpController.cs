using UnityEngine;

public class PlayerJumpController : MonoBehaviour
{
    public Animator animator;
    public string jumpTriggerName = "Jump";
    public bool canJump = false;
    void Update()
    {
        if (!JumpGameManager.Instance.gameCompleted&&canJump && Input.GetKeyDown(KeyCode.Space))
        {
            PerformJump();
        }
    }
    
    public void SetCanJump(bool canJump)
    {
        this.canJump = canJump;
    }
    public void PerformJump()
    {
        if (animator != null)
            animator.SetTrigger(jumpTriggerName);
    }

    // 动画事件：落地瞬间调用（在动画片段中添加Animation Event）
    public void OnLand()
    {
        JumpGameManager.Instance?.OnPlayerJumpLand();
    }
}