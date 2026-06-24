using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Food Ball Reference")]
    [SerializeField] private SpriteRenderer foodBallRenderer;

    [Header("Health Colors")]
    [SerializeField] private Color colorHealthy  = new Color(1f,    0.92f, 0.2f);
    [SerializeField] private Color colorGood     = new Color(1f,    0.6f,  0.1f);
    [SerializeField] private Color colorNeutral  = new Color(0.55f, 0.27f, 0.07f);
    [SerializeField] private Color colorBad      = new Color(0.2f,  0.6f,  0.1f);
    [SerializeField] private Color colorCritical = new Color(0.1f,  0.1f,  0.1f);

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip soundGoodDigestion;
    [SerializeField] private AudioClip soundBadDigestion;
    [SerializeField] private AudioClip soundNutrientGet;
    [SerializeField] private AudioClip soundHazardHit;

    public System.Action<float> OnHealthChanged;
    public System.Action<HealthGrade> OnGradeChanged;

    public float HealthRatio => currentHealth / maxHealth;
    public float CurrentHealth => currentHealth;
    public HealthGrade CurrentGrade => GetGrade(HealthRatio);

    private HealthGrade lastGrade = HealthGrade.Healthy;

    public enum HealthGrade { Healthy, Good, Neutral, Bad, Critical }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 씬 전환 후 새 FoodBall SpriteRenderer 찾기
        if (foodBallRenderer == null)
            foodBallRenderer = FindFirstObjectByType<FoodBall>()?.GetComponent<SpriteRenderer>();
        UpdateVisuals();
    }

    /// <summary>씬 전환 시 건강도 초기화 + 새 참조 갱신</summary>
    public void ResetForNewScene()
    {
        currentHealth = maxHealth;
        foodBallRenderer = FindFirstObjectByType<FoodBall>()?.GetComponent<SpriteRenderer>();
        UpdateVisuals();
    }

    public void GainHealth(float amount, bool isNutrient = false)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        PlaySound(isNutrient ? soundNutrientGet : soundGoodDigestion);
        UpdateVisuals();
        NotifyChange();
    }

    public void LoseHealth(float amount, bool isHazard = false)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        PlaySound(isHazard ? soundHazardHit : soundBadDigestion);
        UpdateVisuals();
        NotifyChange();
    }

    private void UpdateVisuals()
    {
        if (foodBallRenderer == null)
            foodBallRenderer = FindFirstObjectByType<FoodBall>()?.GetComponent<SpriteRenderer>();
        if (foodBallRenderer != null)
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
        if (ratio >= 0.75f) return Color.Lerp(colorGood,     colorHealthy, (ratio - 0.75f) / 0.25f);
        if (ratio >= 0.50f) return Color.Lerp(colorNeutral,  colorGood,    (ratio - 0.50f) / 0.25f);
        if (ratio >= 0.25f) return Color.Lerp(colorBad,      colorNeutral, (ratio - 0.25f) / 0.25f);
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
}
