using UnityEngine;

/// <summary>
/// 게임 전체 상태 관리 싱글톤
/// 샷 카운트, 점수, 스테이지 흐름 관리
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Stage Settings")]
    [SerializeField] private int maxShots = 10;

    private int shotCount = 0;
    private int score = 0;
    private float stageStartTime;
    private bool stageActive = false;
    private bool stageEnded = false;

    public System.Action<int> OnShotCountChanged;  // 파라미터: 남은 샷 수
    public System.Action<int> OnScoreChanged;
    public System.Action OnStageCleared;
    public System.Action OnStageFailed;

    public int ShotCount => shotCount;
    public int MaxShots => maxShots;
    public int Score => score;
    public bool StageActive => stageActive;
    public bool ShotsRemaining => shotCount < maxShots;

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
        shotCount = 0;
        score = 0;
        stageActive = true;
        stageEnded = false;
        stageStartTime = Time.time;
        OnShotCountChanged?.Invoke(maxShots);
    }

    /// <summary>ShotController 발사 시 호출</summary>
    public void AddShot()
    {
        if (!stageActive || stageEnded) return;
        shotCount++;
        OnShotCountChanged?.Invoke(maxShots - shotCount);
        Debug.Log($"[GameManager] Shot {shotCount}/{maxShots}");
    }

    /// <summary>StageGoal 트리거 도달 시 호출</summary>
    public void ClearStage()
    {
        if (stageEnded) return;
        stageEnded = true;
        stageActive = false;

        float elapsed = Time.time - stageStartTime;
        int shotBonus = Mathf.Max(0, (maxShots - shotCount) * 100);
        int timeBonus = Mathf.Max(0, Mathf.RoundToInt((60f - elapsed) * 10f));
        score = shotBonus + timeBonus;

        OnScoreChanged?.Invoke(score);
        OnStageCleared?.Invoke();
        Debug.Log($"[GameManager] CLEAR! Shots:{shotCount} Time:{elapsed:F1}s Score:{score}");
    }

    /// <summary>공이 멈췄는데 샷이 소진됐을 때 ShotController가 호출</summary>
    public void FailStage()
    {
        if (stageEnded) return;
        stageEnded = true;
        stageActive = false;
        OnStageFailed?.Invoke();
        Debug.Log("[GameManager] FAILED - shots exhausted");
    }
}
