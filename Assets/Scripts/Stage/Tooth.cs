using UnityEngine;

/// <summary>
/// 치아 장애물
/// - 공이 닿으면 건강도 + (씹히는 효과)
/// - BoxCollider2D로 넓적한 어금니 형태
/// - 위 치아(천장에서 아래로) / 아래 치아(바닥에서 위로) 구분
/// </summary>
public class Tooth : MonoBehaviour
{
    [Header("Tooth Settings")]
    [SerializeField] private float healthGain = 8f;     // 닿으면 건강도 증가
    [SerializeField] private bool isUpperTooth = true;  // true=위 치아, false=아래 치아

    [Header("Cooldown")]
    [SerializeField] private float contactCooldown = 0.5f; // 연속 접촉 방지
    private float lastContactTime = -999f;

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Food")) return;
        if (Time.time - lastContactTime < contactCooldown) return;

        lastContactTime = Time.time;
        HealthManager.Instance?.GainHealth(healthGain, isNutrient: false);

        Debug.Log($"[Tooth] 씹힘! +{healthGain} 건강도");
    }
}
