using UnityEngine;

public class GradeSystem : MonoBehaviour
{
    public static GradeSystem Instance { get; private set; }

    [Header("Weight Settings")]
    [SerializeField] private float healthWeight   = 0.6f;
    [SerializeField] private float tailWeight     = 0.2f;
    [SerializeField] private float nutrientWeight = 0.2f;

    [Header("Tail Score Settings")]
    [SerializeField] private int maxTailForFullScore = 8;

    private int nutrientCount = 0;
    private int maxNutrientCount = 5;

    // 씬 누적 데이터 (전체 스테이지 통산)
    private int totalNutrientCount = 0;
    private int totalTailCount = 0;

    public int NutrientCount => nutrientCount;
    public int TotalNutrientCount => totalNutrientCount;

    public enum FinalGrade { S, A, B, C, D }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>스테이지 시작 시 초기화 (누적 데이터는 유지)</summary>
    public void Init(int totalNutrients)
    {
        nutrientCount = 0;
        maxNutrientCount = Mathf.Max(1, totalNutrients);
    }

    public void AddNutrient()
    {
        nutrientCount++;
        totalNutrientCount++;
    }

    public float CalculateFinalScore(float healthRatio, int tailCount)
    {
        totalTailCount += tailCount;

        float healthScore   = healthRatio * 100f;
        float tailScore     = Mathf.Clamp01((float)tailCount / maxTailForFullScore) * 100f;
        float nutrientScore = Mathf.Clamp01((float)nutrientCount / maxNutrientCount) * 100f;

        float total = healthScore   * healthWeight
                    + tailScore     * tailWeight
                    + nutrientScore * nutrientWeight;

        Debug.Log($"[GradeSystem] Health={healthScore:F0} Tail={tailScore:F0} Nutrient={nutrientScore:F0} Total={total:F0}");
        return total;
    }

    public FinalGrade GetGrade(float score)
    {
        if (score >= 90f) return FinalGrade.S;
        if (score >= 70f) return FinalGrade.A;
        if (score >= 50f) return FinalGrade.B;
        if (score >= 30f) return FinalGrade.C;
        return FinalGrade.D;
    }

    public string GetGradeText(FinalGrade grade) => grade switch
    {
        FinalGrade.S => "S",
        FinalGrade.A => "A",
        FinalGrade.B => "B",
        FinalGrade.C => "C",
        FinalGrade.D => "D",
        _ => "B"
    };

    public string GetPoopName(FinalGrade grade) => grade switch
    {
        FinalGrade.S => "Golden Poop",
        FinalGrade.A => "Healthy Poop",
        FinalGrade.B => "Normal Poop",
        FinalGrade.C => "Weird Poop",
        FinalGrade.D => "Bad Poop",
        _ => "Normal Poop"
    };

    public Color GetGradeColor(FinalGrade grade) => grade switch
    {
        FinalGrade.S => new Color(1f, 0.84f, 0f),
        FinalGrade.A => new Color(0.4f, 0.8f, 0.4f),
        FinalGrade.B => new Color(0.6f, 0.4f, 0.2f),
        FinalGrade.C => new Color(0.5f, 0.5f, 0.5f),
        FinalGrade.D => new Color(0.2f, 0.2f, 0.2f),
        _ => Color.white
    };

    public string GetPoopDescription(FinalGrade grade) => grade switch
    {
        FinalGrade.S => "Perfect digestion! Your gut is super healthy!",
        FinalGrade.A => "Healthy poop! Well digested!",
        FinalGrade.B => "Not bad. Could eat better next time!",
        FinalGrade.C => "Poor digestion. Take care of your gut!",
        FinalGrade.D => "Bad poop... Try again!",
        _ => ""
    };
}
