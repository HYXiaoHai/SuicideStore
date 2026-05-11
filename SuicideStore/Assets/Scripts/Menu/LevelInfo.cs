using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    public int levelNumber; // 在Inspector手动填写当前是第几关（1~12）

    void Start()
    {
        if (GameManage.Instance != null)
            GameManage.Instance.currentLevel = levelNumber;
    }
}