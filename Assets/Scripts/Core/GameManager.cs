using UnityEngine;

/// <summary>
/// 게임 전체 상태 관리 싱글톤
/// 슬링샷 방식으로 변경 - 샷 카운트 제거, 건강도 기반 점수만 남김
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private bool stageActive = false;
    private bool stageEnded = false;
    private float stageStartTime;

    public System.Action OnStageCleared;
    public System.Action OnStageFailed;

    public bool StageActive => stageActive;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        StartStage();
    }

    public void StartStage()
    {
        stageActive = true;
        stageEnded = false;
        stageStartTime = Time.time;
    }

    /// <summary>골 트리거 도달 시 호출</summary>
    public void ClearStage()
    {
        if (stageEnded) return;
        stageEnded = true;
        stageActive = false;
        OnStageCleared?.Invoke();

        float elapsed = Time.time - stageStartTime;
        string grade = HealthManager.Instance?.GetFinalGradeText() ?? "💩";
        Debug.Log($"[GameManager] CLEAR! Time:{elapsed:F1}s 최종등급:{grade}");
    }

    public void FailStage()
    {
        if (stageEnded) return;
        stageEnded = true;
        stageActive = false;
        OnStageFailed?.Invoke();
        Debug.Log("[GameManager] FAILED");
    }
}
