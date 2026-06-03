using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotoFlowManager : MonoBehaviour
{
    [Header("图片引用")]
    public GameObject firstImage; // 挂载 EraseCtrl_Img1 的第一张图片
    public GameObject img_Text;
    public GameObject img2;
    public GameObject img3;
    [Header("Img2限定点击区域空物体")]
    public Transform clickArea_Img2;
    [Header("跳转场景名字")]
    public string nextSceneName;

    private enum Phase { Erase, ShowText, ShowImg2, ShowImg3 }
    private Phase currentPhase = Phase.Erase;

    void Start()
    {
        if(firstImage != null) firstImage.SetActive(true);
        if(img_Text != null) img_Text.SetActive(false);
        if(img2 != null) img2.SetActive(false);
        if(img3 != null) img3.SetActive(false);
        currentPhase = Phase.Erase;
    }

    void Update()
    {
        switch (currentPhase)
        {
            case Phase.Erase:
                // 只检查擦除阶段是否完成
                if (firstImage != null && !firstImage.activeSelf && img_Text != null && img_Text.activeSelf)
                {
                    currentPhase = Phase.ShowText;
                }
                break;

            case Phase.ShowText:
                if (img_Text != null && img_Text.activeSelf && Input.GetMouseButtonDown(0))
                {
                    if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        img_Text.GetComponent<RectTransform>(),
                        Input.mousePosition,
                        Camera.main,
                        out _))
                    {
                        img_Text.SetActive(false);
                        if (img2 != null) img2.SetActive(true);
                        currentPhase = Phase.ShowImg2;
                    }
                }
                break;

            case Phase.ShowImg2:
                if (img2 != null && img2.activeSelf && Input.GetMouseButtonDown(0))
                {
                    if (clickArea_Img2 != null && clickArea_Img2.GetComponent<RectTransform>() != null)
                    {
                        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                            clickArea_Img2.GetComponent<RectTransform>(),
                            Input.mousePosition,
                            Camera.main,
                            out _))
                        {
                            img2.SetActive(false);
                            if (img3 != null) img3.SetActive(true);
                            currentPhase = Phase.ShowImg3;
                        }
                    }
                }
                break;

            case Phase.ShowImg3:
                if (img3 != null && img3.activeSelf && Input.GetMouseButtonDown(0))
                {
                    if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        img3.GetComponent<RectTransform>(),
                        Input.mousePosition,
                        Camera.main,
                        out _))
                    {
                        if (!string.IsNullOrEmpty(nextSceneName))
                        {
                            SceneManager.LoadScene(nextSceneName);
                        }
                        else
                        {
                            Debug.LogWarning("nextSceneName为空，请在Inspector中设置！");
                        }
                    }
                }
                break;
        }
    }
}