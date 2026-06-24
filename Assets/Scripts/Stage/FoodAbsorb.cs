using UnityEngine;

/// <summary>
/// 음식 흡수 트리거
/// 공이 닿으면 TailChain에 꼬리 추가 + HealthManager에 건강도 변화
/// </summary>
public class FoodAbsorb : MonoBehaviour
{
    [Header("Effect")]
    [SerializeField] private float healthGain = 10f;    // 건강도 증가량
    [SerializeField] private bool oneShot = true;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered && oneShot) return;
        if (!other.CompareTag("Food")) return;

        // 머리 공에서만 흡수 (꼬리 세그먼트 태그도 Food라서 머리인지 확인)
        if (other.GetComponent<FoodBall>() == null) return;

        triggered = true;

        // 꼬리 추가
        var tail = other.GetComponent<TailChain>();
        if (tail == null)
            tail = other.GetComponentInParent<TailChain>();
        tail?.AddSegment();

        // 건강도 증가
        HealthManager.Instance?.GainHealth(healthGain, isNutrient: true);

        // 흡수된 음식 오브젝트 비활성화
        gameObject.SetActive(false);

        Debug.Log($"[FoodAbsorb] {gameObject.name} 흡수됨");
    }
}
