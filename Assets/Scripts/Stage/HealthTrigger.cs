using UnityEngine;

/// <summary>
/// 건강도 변화 트리거 존
/// Collider2D (IsTrigger=true) 부착 필요
/// 
/// 사용 예:
/// - 위액 웅덩이: amount=-20, isHazard=true
/// - 영양소 존:   amount=+15, isNutrient=true
/// - 좋은 소화 구간: amount=+10
/// </summary>
public class HealthTrigger : MonoBehaviour
{
    [Header("Health Effect")]
    [SerializeField] private float amount = 10f;        // 양수=회복, 음수=감소
    [SerializeField] private bool isNutrient = false;   // 영양소 사운드
    [SerializeField] private bool isHazard = false;     // 위험 사운드

    [Header("One Shot (한 번만 발동)")]
    [SerializeField] private bool oneShot = true;
    private bool triggered = false;

    [Header("Visual Feedback")]
    [SerializeField] private HealthUI healthUI;         // 선택 - 피드백 텍스트용

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Food")) return;
        if (oneShot && triggered) return;

        triggered = true;

        if (amount >= 0)
            HealthManager.Instance?.GainHealth(amount, isNutrient);
        else
            HealthManager.Instance?.LoseHealth(-amount, isHazard);

        healthUI?.ShowFeedback(amount);

        Debug.Log($"[HealthTrigger] {gameObject.name} → {(amount >= 0 ? "+" : "")}{amount} health");
    }

    // oneShot=false일 때 나갔다 다시 들어오면 재발동 가능하게
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Food")) return;
        if (!oneShot) triggered = false;
    }
}
