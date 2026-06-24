using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TailChain tailChain;
    [SerializeField] private int totalNutrientsInStage = 5;

    private bool stageActive = false;
    private bool stageEnded = false;
    private float stageStartTime;

    public System.Action OnStageCleared;
    public System.Action OnStageFailed;
    public System.Action<GradeSystem.FinalGrade, float, string> OnGradeCalculated;

    public bool StageActive => stageActive;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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

        // 씬 전환 후 TailChain 새로 찾기
        if (tailChain == null)
            tailChain = FindFirstObjectByType<TailChain>();

        GradeSystem.Instance?.Init(totalNutrientsInStage);
    }

    public void ClearStage()
    {
        if (stageEnded) return;
        stageEnded = true;
        stageActive = false;

        float healthRatio = HealthManager.Instance?.HealthRatio ?? 0.5f;
        int tailCount = tailChain != null ? tailChain.SegmentCount : 0;

        float score = GradeSystem.Instance?.CalculateFinalScore(healthRatio, tailCount) ?? 50f;
        GradeSystem.FinalGrade grade = GradeSystem.Instance?.GetGrade(score) ?? GradeSystem.FinalGrade.B;
        string poopName = GradeSystem.Instance?.GetPoopName(grade) ?? "Normal Poop";

        OnStageCleared?.Invoke();
        OnGradeCalculated?.Invoke(grade, score, poopName);

        float elapsed = Time.time - stageStartTime;
        Debug.Log($"[GameManager] CLEAR! Time:{elapsed:F1}s Score:{score:F0} Grade:{grade} ({poopName})");
    }

    public void FailStage()
    {
        if (stageEnded) return;
        stageEnded = true;
        stageActive = false;
        OnStageFailed?.Invoke();
        Debug.Log("[GameManager] FAILED");
    }

    /// <summary>씬 전환 시 호출 - 이벤트 초기화 + 새 씬 참조 갱신</summary>
    public void OnNewSceneLoaded()
    {
        OnStageCleared = null;
        OnStageFailed = null;
        OnGradeCalculated = null;
        tailChain = FindFirstObjectByType<TailChain>();
        StartStage();
    }
}
