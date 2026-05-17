using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickToLoadScene : MonoBehaviour
{
    [Header("场景设置")]
    [SerializeField] private string sceneName = "NextScene";   // 目标场景名称

    [Header("可选：点击反馈")]
    [SerializeField] private AudioClip clickSound;             // 点击音效（可选）

    private void OnMouseDown()
    {
        // 播放音效
        if (clickSound != null)
            AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);

        // 加载场景
        SceneManager.LoadScene(sceneName);
    }
}