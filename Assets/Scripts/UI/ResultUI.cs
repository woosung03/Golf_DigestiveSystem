using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class ResultUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject resultPanel;

    [Header("Grade Display")]
    [SerializeField] private TextMeshProUGUI gradeText;
    [SerializeField] private TextMeshProUGUI poopNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Detail Scores")]
    [SerializeField] private TextMeshProUGUI healthScoreText;
    [SerializeField] private TextMeshProUGUI tailScoreText;
    [SerializeField] private TextMeshProUGUI nutrientScoreText;

    [Header("Buttons")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button nextButton;

    [Header("Animation")]
    [SerializeField] private float revealDelay = 0.5f;

    private void Awake()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
        if (nextButton  != null) nextButton.onClick.AddListener(OnNext);
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGradeCalculated += ShowResult;
            GameManager.Instance.OnStageFailed     += ShowFailed;
        }
        else
        {
            Debug.LogWarning("[ResultUI] GameManager.Instance is null");
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGradeCalculated -= ShowResult;
            GameManager.Instance.OnStageFailed     -= ShowFailed;
        }
    }

    private void ShowResult(GradeSystem.FinalGrade grade, float score, string poopName)
    {
        StartCoroutine(RevealResult(grade, score, poopName));
    }

    private IEnumerator RevealResult(GradeSystem.FinalGrade grade, float score, string poopName)
    {
        yield return new WaitForSeconds(revealDelay);

        if (resultPanel != null) resultPanel.SetActive(true);

        Color gradeColor  = GradeSystem.Instance?.GetGradeColor(grade) ?? Color.white;
        string description = GradeSystem.Instance?.GetPoopDescription(grade) ?? "";

        if (gradeText != null)
        {
            gradeText.text  = GradeSystem.Instance?.GetGradeText(grade) ?? "B";
            gradeText.color = gradeColor;
        }
        if (poopNameText != null)
        {
            poopNameText.text  = poopName;
            poopNameText.color = gradeColor;
        }
        if (descriptionText != null)
            descriptionText.text = description;
        if (scoreText != null)
            scoreText.text = $"{score:F0} pts";

        float healthRatio  = HealthManager.Instance?.HealthRatio ?? 0f;
        int tailCount      = FindFirstObjectByType<TailChain>()?.SegmentCount ?? 0;
        int nutrientCount  = GradeSystem.Instance?.NutrientCount ?? 0;

        if (healthScoreText  != null) healthScoreText.text  = $"Digestion Health  {healthRatio * 100f:F0}";
        if (tailScoreText    != null) tailScoreText.text    = $"Food Absorbed     {tailCount}";
        if (nutrientScoreText != null) nutrientScoreText.text = $"Nutrients Collected {nutrientCount}";
    }

    private void ShowFailed()
    {
        StartCoroutine(RevealFailed());
    }

    private IEnumerator RevealFailed()
    {
        yield return new WaitForSeconds(revealDelay);
        if (resultPanel != null) resultPanel.SetActive(true);

        if (gradeText != null)       { gradeText.text = "D"; gradeText.color = Color.gray; }
        if (poopNameText != null)    poopNameText.text    = "Bad Poop";
        if (descriptionText != null) descriptionText.text = "Try again!";
        if (scoreText != null)       scoreText.text       = "0 pts";
    }

    private void OnRetry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnNext()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            SceneManager.LoadScene(0);
    }
}
