using UnityEngine;

/// <summary>
/// 나쁜 음식 흡수 트리거
/// 공이 닿으면 TailChain에 꼬리 추가 + HealthManager 건강도 감소
/// 좋은 음식(FoodAbsorb)과 다르게 건강도가 내려감
/// </summary>
public class BadFoodAbsorb : MonoBehaviour
{
    [Header("Effect")]
    [SerializeField] private float healthLoss = 15f;  // 건강도 감소량
    [SerializeField] private bool oneShot = true;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered && oneShot) return;
        if (!other.CompareTag("Food")) return;
        if (other.GetComponent<FoodBall>() == null) return; // 머리만 인식

        triggered = true;

        // 꼬리는 추가됨 (먹긴 먹은 거니까)
        var tail = other.GetComponent<TailChain>();
        tail?.AddSegment();

        // 건강도 감소
        HealthManager.Instance?.LoseHealth(healthLoss, isHazard: false);

        // 흡수된 음식 비활성화
        gameObject.SetActive(false);

        Debug.Log($"[BadFoodAbsorb] {gameObject.name} 흡수 - -{healthLoss} 건강도");
    }
}
