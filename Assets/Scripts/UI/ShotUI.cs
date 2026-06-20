using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 인게임 HUD: 남은 샷 수, 파워바, 점수 표시
/// </summary>
public class ShotUI : MonoBehaviour
{
    [Header("Shot Counter")]
    [SerializeField] private TextMeshProUGUI shotsRemainingText;

    [Header("Power Bar")]
    [SerializeField] private Slider powerSlider;
    [SerializeField] private Image powerFill;
    [SerializeField] private Color lowPowerColor = Color.green;
    [SerializeField] private Color highPowerColor = Color.red;

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Stage Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private TextMeshProUGUI resultScoreText;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnShotCountChanged += UpdateShotsRemaining;
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnStageCleared += ShowClearResult;
            GameManager.Instance.OnStageFailed += ShowFailResult;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnShotCountChanged -= UpdateShotsRemaining;
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnStageCleared -= ShowClearResult;
            GameManager.Instance.OnStageFailed -= ShowFailResult;
        }
    }

    private void Start()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        if (powerSlider != null) powerSlider.value = 0f;
        UpdateShotsRemaining(GameManager.Instance != null ? GameManager.Instance.MaxShots : 10);
    }

    public void UpdatePowerBar(float ratio)
    {
        if (powerSlider == null) return;
        powerSlider.value = ratio;
        if (powerFill != null)
            powerFill.color = Color.Lerp(lowPowerColor, highPowerColor, ratio);
    }

    private void UpdateShotsRemaining(int remaining)
    {
        if (shotsRemainingText != null)
            shotsRemainingText.text = $"Shots: {remaining}";
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    private void ShowClearResult()
    {
        if (resultPanel == null) return;
        resultPanel.SetActive(true);
        if (resultTitleText != null) resultTitleText.text = "CLEAR!";
        if (resultScoreText != null)
            resultScoreText.text = $"Score: {GameManager.Instance?.Score}";
    }

    private void ShowFailResult()
    {
        if (resultPanel == null) return;
        resultPanel.SetActive(true);
        if (resultTitleText != null) resultTitleText.text = "TRY AGAIN";
        if (resultScoreText != null) resultScoreText.text = "";
    }
}
