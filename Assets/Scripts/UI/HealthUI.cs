using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 건강도 HUD - 이모지 아이콘 + 게이지 바
/// HealthManager 이벤트 구독해서 자동 업데이트
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFill;

    [Header("Grade Display")]
    [SerializeField] private TextMeshProUGUI gradeText;   // 이모지 등급 표시

    [Header("Feedback Text (잠깐 뜨는 +/- 표시)")]
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private float feedbackDuration = 1f;

    private Coroutine feedbackCoroutine;

    // 등급별 색상 (HealthManager 색상과 동일하게)
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
        gradeText.text = grade switch
        {
            HealthManager.HealthGrade.Healthy  => "💛",
            HealthManager.HealthGrade.Good     => "🟠",
            HealthManager.HealthGrade.Neutral  => "💩",
            HealthManager.HealthGrade.Bad      => "🟢",
            HealthManager.HealthGrade.Critical => "☠️",
            _ => "💩"
        };
    }

    /// <summary>외부에서 호출 - "+10" "-5" 같은 피드백 텍스트 잠깐 표시</summary>
    public void ShowFeedback(float amount)
    {
        if (feedbackText == null) return;
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(FeedbackRoutine(amount));
    }

    private System.Collections.IEnumerator FeedbackRoutine(float amount)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = amount >= 0 ? $"<color=#FFD700>+{amount:F0}</color>"
                                        : $"<color=#FF4444>{amount:F0}</color>";
        yield return new WaitForSeconds(feedbackDuration);
        feedbackText.gameObject.SetActive(false);
    }

    private Color GetBarColor(float ratio)
    {
        if (ratio >= 0.75f) return Color.Lerp(colorGood, colorHealthy, (ratio - 0.75f) / 0.25f);
        if (ratio >= 0.50f) return Color.Lerp(colorNeutral, colorGood, (ratio - 0.50f) / 0.25f);
        if (ratio >= 0.25f) return Color.Lerp(colorBad, colorNeutral, (ratio - 0.25f) / 0.25f);
        return Color.Lerp(colorCritical, colorBad, ratio / 0.25f);
    }
}
