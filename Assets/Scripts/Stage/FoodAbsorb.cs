using UnityEngine;

/// <summary>
/// 음식 흡수 트리거
/// 공이 닿으면 TailChain에 꼬리 추가 + HealthManager 건강도 증가 + GradeSystem 영양소 카운트
/// </summary>
public class FoodAbsorb : MonoBehaviour
{
    [Header("Effect")]
    [SerializeField] private float healthGain = 10f;
    [SerializeField] private bool isNutrient = true;
    [SerializeField] private bool oneShot = true;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered && oneShot) return;
        if (!other.CompareTag("Food")) return;
        if (other.GetComponent<FoodBall>() == null) return; // 머리만 인식

        triggered = true;

        // 꼬리 추가
        var tail = other.GetComponent<TailChain>();
        tail?.AddSegment();

        // 건강도 증가
        HealthManager.Instance?.GainHealth(healthGain, isNutrient);

        // 영양소 카운트
        if (isNutrient)
            GradeSystem.Instance?.AddNutrient();

        // 흡수된 음식 비활성화
        gameObject.SetActive(false);

        Debug.Log($"[FoodAbsorb] {gameObject.name} 흡수 완료");
    }
}
