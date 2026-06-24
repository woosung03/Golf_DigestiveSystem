using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 건강도 HUD - 게이지 바 + 등급 텍스트
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFill;

    [Header("Grade Display")]
    [SerializeField] private TextMeshProUGUI gradeText;

    [Header("Feedback Text")]
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private float feedbackDuration = 1f;

    private Coroutine feedbackCoroutine;

    private readonly Color colorHealthy  = new Color(1f,   0.92f, 0.2f);
    private readonly Color colorGood     = new Color(1f,   0.6f,  0.1f);
    private readonly Color colorNeutral  = new Color(0.55f,0.27f, 0.07f);
    private readonly Color colorBad      = new Color(0.2f, 0.6f,  0.1f);
    private readonly Color colorCritical = new Color(0.1f, 0.1f,  0.1f);

    private void OnEnable()
    {
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.OnHealthChanged += UpdateHealthBar;
            HealthManager.Instance.OnGradeChanged  += UpdateGrade;
        }
    }

    private void OnDisable()
    {
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.OnHealthChanged -= UpdateHealthBar;
            HealthManager.Instance.OnGradeChanged  -= UpdateGrade;
        }
    }

    private void Start()
    {
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        UpdateHealthBar(1f);
        UpdateGrade(HealthManager.HealthGrade.Healthy);
    }

    private void UpdateHealthBar(float ratio)
    {
        if (healthSlider != null) healthSlider.value = ratio;
        if (healthFill != null)   healthFill.color = GetBarColor(ratio);
    }

    private void UpdateGrade(HealthManager.HealthGrade grade)
    {
        if (gradeText == null) return;
        // 이모지 대신 일반 텍스트로 등급 표시
        gradeText.text = grade switch
        {
            HealthManager.HealthGrade.Healthy  => "S",
            HealthManager.HealthGrade.Good     => "A",
            HealthManager.HealthGrade.Neutral  => "B",
            HealthManager.HealthGrade.Bad      => "C",
            HealthManager.HealthGrade.Critical => "D",
            _ => "B"
        };
        gradeText.color = GetBarColor(
            HealthManager.Instance != null ? HealthManager.Instance.HealthRatio : 1f);
    }

    public void ShowFeedback(float amount)
    {
        if (feedbackText == null) return;
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(FeedbackRoutine(amount));
    }

    private System.Collections.IEnumerator FeedbackRoutine(float amount)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = amount >= 0
            ? $"<color=#FFD700>+{amount:F0}</color>"
            : $"<color=#FF4444>{amount:F0}</color>";
        yield return new WaitForSeconds(feedbackDuration);
        feedbackText.gameObject.SetActive(false);
    }

    private Color GetBarColor(float ratio)
    {
        if (ratio >= 0.75f) return Color.Lerp(colorGood,     colorHealthy, (ratio - 0.75f) / 0.25f);
        if (ratio >= 0.50f) return Color.Lerp(colorNeutral,  colorGood,    (ratio - 0.50f) / 0.25f);
        if (ratio >= 0.25f) return Color.Lerp(colorBad,      colorNeutral, (ratio - 0.25f) / 0.25f);
        return Color.Lerp(colorCritical, colorBad, ratio / 0.25f);
    }
}
