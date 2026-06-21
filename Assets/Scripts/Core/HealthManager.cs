using UnityEngine;

/// <summary>
/// 음식 건강도 관리 - 공 색변화 + 사운드 피드백 연동
/// 건강도에 따라 공 색이 노랑 → 주황 → 갈색 → 녹색 → 검정 으로 변함
/// </summary>
public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Food Ball Reference")]
    [SerializeField] private SpriteRenderer foodBallRenderer;

    [Header("Health Colors")]
    [SerializeField] private Color colorHealthy    = new Color(1f,   0.92f, 0.2f);  // 노랑 (100%)
    [SerializeField] private Color colorGood       = new Color(1f,   0.6f,  0.1f);  // 주황 (75%)
    [SerializeField] private Color colorNeutral    = new Color(0.55f,0.27f, 0.07f); // 갈색 (50%)
    [SerializeField] private Color colorBad        = new Color(0.2f, 0.6f,  0.1f);  // 녹색 (25%)
    [SerializeField] private Color colorCritical   = new Color(0.1f, 0.1f,  0.1f);  // 검정 (0%)

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip soundGoodDigestion;  // 좋은 소화 (냠냠)
    [SerializeField] private AudioClip soundBadDigestion;   // 나쁜 소화 (으엑)
    [SerializeField] private AudioClip soundNutrientGet;    // 영양소 획득
    [SerializeField] private AudioClip soundHazardHit;      // 위험 구역 충돌

    // 이벤트
    public System.Action<float> OnHealthChanged;  // 현재 health 비율 (0~1)
    public System.Action<HealthGrade> OnGradeChanged;

    public float HealthRatio => currentHealth / maxHealth;
    public float CurrentHealth => currentHealth;
    public HealthGrade CurrentGrade => GetGrade(HealthRatio);

    private HealthGrade lastGrade = HealthGrade.Healthy;

    public enum HealthGrade
    {
        Healthy,   // 100~76%
        Good,      // 75~51%
        Neutral,   // 50~26%
        Bad,       // 25~11%
        Critical   // 10~0%
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        UpdateVisuals();
    }

    // ── 외부에서 호출하는 메서드 ──────────────────────────

    /// <summary>좋은 소화 행동 (위액 통과, 영양소 흡수 등)</summary>
    public void GainHealth(float amount, bool isNutrient = false)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        PlaySound(isNutrient ? soundNutrientGet : soundGoodDigestion);
        UpdateVisuals();
        NotifyChange();
    }

    /// <summary>나쁜 소화 행동 (포크 찔림, 과다 자극 등)</summary>
    public void LoseHealth(float amount, bool isHazard = false)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        PlaySound(isHazard ? soundHazardHit : soundBadDigestion);
        UpdateVisuals();
        NotifyChange();
    }

    // ── 내부 처리 ─────────────────────────────────────────

    private void UpdateVisuals()
    {
        if (foodBallRenderer == null) return;
        foodBallRenderer.color = GetHealthColor(HealthRatio);
    }

    private void NotifyChange()
    {
        OnHealthChanged?.Invoke(HealthRatio);

        HealthGrade grade = GetGrade(HealthRatio);
        if (grade != lastGrade)
        {
            lastGrade = grade;
            OnGradeChanged?.Invoke(grade);
        }
    }

    private Color GetHealthColor(float ratio)
    {
        // 구간별 Lerp
        if (ratio >= 0.75f)
            return Color.Lerp(colorGood, colorHealthy, (ratio - 0.75f) / 0.25f);
        else if (ratio >= 0.50f)
            return Color.Lerp(colorNeutral, colorGood, (ratio - 0.50f) / 0.25f);
        else if (ratio >= 0.25f)
            return Color.Lerp(colorBad, colorNeutral, (ratio - 0.25f) / 0.25f);
        else
            return Color.Lerp(colorCritical, colorBad, ratio / 0.25f);
    }

    private HealthGrade GetGrade(float ratio)
    {
        if (ratio > 0.75f) return HealthGrade.Healthy;
        if (ratio > 0.50f) return HealthGrade.Good;
        if (ratio > 0.25f) return HealthGrade.Neutral;
        if (ratio > 0.10f) return HealthGrade.Bad;
        return HealthGrade.Critical;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    /// <summary>스테이지 클리어 시 최종 등급 반환</summary>
    public string GetFinalGradeText()
    {
        return CurrentGrade switch
        {
            HealthGrade.Healthy  => "💛 황금변",
            HealthGrade.Good     => "🟤 건강변",
            HealthGrade.Neutral  => "💩 보통변",
            HealthGrade.Bad      => "🟢 이상변",
            HealthGrade.Critical => "☠️ 불량변",
            _ => "💩 보통변"
        };
    }
}
