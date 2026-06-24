using UnityEngine;

/// <summary>
/// 게임 전체 상태 관리 싱글톤
/// 슬링샷 방식 - 건강도 + 꼬리길이 + 영양소 기반 등급 평가
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TailChain tailChain;
    [SerializeField] private int totalNutrientsInStage = 5; // 스테이지 총 영양소 개수

    private bool stageActive = false;
    private bool stageEnded = false;
    private float stageStartTime;

    public System.Action OnStageCleared;
    public System.Action OnStageFailed;
    public System.Action<GradeSystem.FinalGrade, float, string> OnGradeCalculated;
    // 파라미터: 등급, 점수, 변 이름

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
        GradeSystem.Instance?.Init(totalNutrientsInStage);
    }

    /// <summary>골 트리거 도달 시 호출</summary>
    public void ClearStage()
    {
        if (stageEnded) return;
        stageEnded = true;
        stageActive = false;

        // 최종 점수 계산
        float healthRatio = HealthManager.Instance?.HealthRatio ?? 0.5f;
        int tailCount = tailChain != null ? tailChain.SegmentCount : 0;

        float score = GradeSystem.Instance?.CalculateFinalScore(healthRatio, tailCount) ?? 50f;
        GradeSystem.FinalGrade grade = GradeSystem.Instance?.GetGrade(score) ?? GradeSystem.FinalGrade.B;
        string poopName = GradeSystem.Instance?.GetPoopName(grade) ?? "보통변";

        OnStageCleared?.Invoke();
        OnGradeCalculated?.Invoke(grade, score, poopName);

        float elapsed = Time.time - stageStartTime;
        Debug.Log($"[GameManager] CLEAR! Time:{elapsed:F1}s 점수:{score:F0} 등급:{grade} ({poopName})");
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
