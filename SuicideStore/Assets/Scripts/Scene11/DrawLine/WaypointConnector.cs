using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaypointConnector : MonoBehaviour
{
    [Header("点位设置")]
    public List<Transform> waypoints1;
    public List<Transform> waypoints2;
    public float radius = 0.5f;

    [Header("照片")]
    public SpriteRenderer image1;
    public SpriteRenderer image2;

    [Header("文案")]
    public TMP_Text text1;
    public TMP_Text text2;
    public TMP_Text text3;
    public TMP_Text text4;

    [Header("按钮与特效")]
    public Button levelButton;
    public ParticleSystem particleEffect;

    [Header("动画时长")]
    public float fadeDuration = 0.5f;
    public float showHideDuration = 0.2f;
    public float passEffectDuration = 0.3f;

    [Header("引用")]
    public MspPaint paintSystem;

    private enum GameState
    {
        Level1_WaitStart,
        Level1_Drawing,
        Level1_Complete,
        Level2_WaitFirstClick,
        Level2_WaitSecondClick,
        Level3_WaitClick,
        Level3_Drawing,
        Level3_Complete
    }
    private GameState currentState;

    private List<Transform> currentWaypoints;
    private int nextWaypointIndex = 0;
    private bool isWaitingForStart = true;

    private void Start()
    {
        if (paintSystem == null)
            paintSystem = FindObjectOfType<MspPaint>();

        if (paintSystem == null)
        {
            Debug.LogError("WaypointConnector: 未找到 MspPaint 组件");
            return;
        }

        paintSystem.OnDrawingPositionUpdated += OnDrawingPosition;
        paintSystem.OnDrawingFinished += OnDrawingFinished;

        // 初始UI状态
        image1.color = new Color(image1.color.r, image1.color.g, image1.color.b, 0);
        image2.color = new Color(image2.color.r, image2.color.g, image2.color.b, 0);
        text1.alpha = 1;
        text2.alpha = 0;
        text3.alpha = 0;
        text4.alpha = 0;

        SetButtonInteractable(false);
        if (particleEffect != null) particleEffect.Stop();

        // 初始隐藏所有 waypoints2 的点位（确保 Level3 开始前不可见）
        HideAllWaypoints(waypoints2);

        // 设置第一组点位，只显示第一个点
        currentWaypoints = waypoints1;
        ResetWaypointsDisplay(currentWaypoints);

        currentState = GameState.Level1_WaitStart;
        isWaitingForStart = true;
    }

    private void Update()
    {
        if ((currentState == GameState.Level1_WaitStart || currentState == GameState.Level3_WaitClick)&& !paintSystem.IsDrawing)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = GetMousePosition();
                if (currentWaypoints.Count > 0 && Vector3.Distance(mousePos, currentWaypoints[0].position) <= radius)
                {
                    isWaitingForStart = false;
                    paintSystem.StartDrawing(mousePos);
                    if (currentWaypoints.Count > 1)
                        ShowWaypoint(currentWaypoints, 1, true);
                    if (currentState == GameState.Level1_WaitStart)
                        currentState = GameState.Level1_Drawing;
                    else if (currentState == GameState.Level3_WaitClick)
                        currentState = GameState.Level3_Drawing;
                }
            }
        }
    }
    //画画的点位
    private void OnDrawingPosition(Vector3 currentMouseWorldPos)
    {
        if (currentState != GameState.Level1_Drawing && currentState != GameState.Level3_Drawing)
            return;
        if (nextWaypointIndex >= currentWaypoints.Count) return;

        Transform target = currentWaypoints[nextWaypointIndex];
        if (Vector3.Distance(currentMouseWorldPos, target.position) <= radius)
        {
            MarkWaypointPassed(currentWaypoints, nextWaypointIndex);
            nextWaypointIndex++;

            if (nextWaypointIndex < currentWaypoints.Count)
                ShowWaypoint(currentWaypoints, nextWaypointIndex, true);

            if (nextWaypointIndex >= currentWaypoints.Count)
            {
                if (currentState == GameState.Level1_Drawing)
                    Level1Win();
                else if (currentState == GameState.Level3_Drawing)
                    Level3Win();
            }
            return;
        }

        for (int i = nextWaypointIndex + 1; i < currentWaypoints.Count; i++)
        {
            if (Vector3.Distance(currentMouseWorldPos, currentWaypoints[i].position) <= radius)
            {
                TriggerMistake();
                break;
            }
        }
    }

    private void OnDrawingFinished()
    {
        if (currentState != GameState.Level1_Drawing && currentState != GameState.Level3_Drawing)
            return;
        if (nextWaypointIndex < currentWaypoints.Count)
            TriggerMistake();
    }

    private void TriggerMistake()
    {
        if (paintSystem == null) return;
        paintSystem.RetractLine(() =>
        {
            nextWaypointIndex = 0;
            isWaitingForStart = true;
            ResetWaypointsDisplay(currentWaypoints);

            // 根据当前使用的点位组恢复正确的等待状态
            if (currentWaypoints == waypoints1)
                currentState = GameState.Level1_WaitStart;
            else if (currentWaypoints == waypoints2)
                currentState = GameState.Level3_WaitClick;
        });
    }

    private void Level1Win()
    {
        paintSystem.RetractLine(null);
        if (paintSystem.IsDrawing) paintSystem.RetractLine(null);

        currentState = GameState.Level1_Complete;
        isWaitingForStart = false;

        // 隐藏所有 waypoints1 的点位（确保彻底消失）
        HideAllWaypoints(waypoints1);

        image1.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(text1.DOFade(0, fadeDuration));
            seq.Append(text2.DOFade(1, fadeDuration));
            seq.OnComplete(() =>
            {
                SetButtonInteractable(true);
                currentState = GameState.Level2_WaitFirstClick;
            });
            seq.Play();
        });
    }

    private void Level3Win()
    {
        paintSystem.RetractLine(null);
        currentState = GameState.Level3_Complete;

        Sequence finalSeq = DOTween.Sequence();
        finalSeq.Join(image2.DOFade(1, fadeDuration));
        finalSeq.Join(text4.DOFade(1, fadeDuration));
        finalSeq.OnComplete(() =>
        {
            Debug.Log("全关卡完成！");
            CompleteLevel();
        });
        finalSeq.Play();
    }
    public void CompleteLevel()
    {
        // 通知 GameManage 当前关卡通关
        GameManage.Instance.CompleteCurrentLevel();
        // 可选：自动进入下一关第一场景（如果希望无缝衔接）
        int nextLevel = GameManage.Instance.currentLevel + 1;
        if (nextLevel <= 12)
        {
            string nextScene = GameManage.Instance.GetFirstSceneOfLevel(nextLevel);
            if (!string.IsNullOrEmpty(nextScene))
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.Log("恭喜通关全部12大关！");
        }
    }
    public void OnButtonClick()
    {
        switch (currentState)
        {
            case GameState.Level2_WaitFirstClick:
                image1.DOFade(0, fadeDuration).OnComplete(() =>
                {
                    currentState = GameState.Level2_WaitSecondClick;
                });
                break;

            case GameState.Level2_WaitSecondClick:
                Debug.Log("点击 level2 -2click");
                Sequence seq = DOTween.Sequence();
                seq.Append(text2.DOFade(0, fadeDuration));
                seq.Append(text3.DOFade(1, fadeDuration));
                seq.OnComplete(() =>
                {
                    if (particleEffect != null) particleEffect.Play();
                    currentState = GameState.Level3_WaitClick;
                });
                seq.Play();
                break;

            case GameState.Level3_WaitClick:
                Debug.Log("点击 level3 -click");
                text3.DOFade(0, fadeDuration).OnComplete(() =>
                {
                    SetButtonInteractable(false);
                    currentWaypoints = waypoints2;
                    nextWaypointIndex = 0;
                    isWaitingForStart = true;
                    ResetWaypointsDisplay(currentWaypoints);   // 只显示第一个点
                    currentState = GameState.Level3_WaitClick;
                });
                break;

            default:
                Debug.Log("当前状态按钮无效: " + currentState);
                break;
        }
    }

    private void SetButtonInteractable(bool interactable)
    {
        if (levelButton != null)
            levelButton.interactable = interactable;
    }

    /// <summary>
    /// 重置某组点位：只显示第一个，其余隐藏
    /// </summary>
    private void ResetWaypointsDisplay(List<Transform> waypoints)
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            waypoints[i].gameObject.SetActive(true);
            if (i == 0)
                ShowWaypoint(waypoints, i, true);
            else
                ShowWaypoint(waypoints, i, false);
        }
    }

    //隐藏一组所有点位
    private void HideAllWaypoints(List<Transform> waypoints)
    {
        foreach (Transform wp in waypoints)
        {
            if (wp == null) continue;
            wp.gameObject.SetActive(false);
            //SpriteRenderer sr = wp.GetComponent<SpriteRenderer>();
            //if (sr != null)
            //{

            //    sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0);
            //}
        }
    }
    //显示点位
    private void ShowWaypoint(List<Transform> waypoints, int idx, bool show)
    {
        Transform wp = waypoints[idx];
        if (wp == null) return;

        float targetAlpha = show ? 1f : 0f;
        SpriteRenderer sr = wp.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.DOFade(targetAlpha, showHideDuration);
    }
    //经过动画
    private void MarkWaypointPassed(List<Transform> waypoints, int index)
    {
        Transform wp = waypoints[index];
        if (wp == null) return;

        // 2. 在第一个子物体上播放特效
        if (wp.childCount > 0)
        {
            Transform effect = wp.GetChild(0);
            SpriteRenderer effectSr = effect.GetComponent<SpriteRenderer>();
            if (effectSr != null)
            {
                effect.localScale = Vector3.zero;
                effectSr.color = new Color(effectSr.color.r, effectSr.color.g, effectSr.color.b, 1f);

                Sequence seq = DOTween.Sequence();
                seq.Join(effect.DOScale(0.6f, passEffectDuration).SetEase(Ease.OutCirc));
                seq.Join(effectSr.DOFade(0f, passEffectDuration));
                seq.Play();
            }
        }
    }

    private Vector3 GetMousePosition()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        return hit.collider != null ? hit.point : (Vector3)mousePos;
    }
}