using UnityEngine;

/// <summary>
/// 트램펄린 - 위 연동운동 표현
/// 공이 닿으면 위로 튕김
/// 연속 튕김 횟수에 따라 건강도 변화
/// 1~2회: 변화 없음 (정상 소화)
/// 3~4회: 건강도 - (과한 자극)
/// 5회+:  건강도 -- (오바이트)
/// </summary>
public class Trampoline : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float bounceForce = 15f;       // 튕기는 힘
    [SerializeField] private float bounceAngleVariance = 15f; // 튕기는 각도 랜덤 범위

    [Header("Health Settings")]
    [SerializeField] private int   safeBouncCount  = 2;    // 이 횟수까지는 안전
    [SerializeField] private int   dangerBounceCount = 4;  // 이 횟수부터 위험
    [SerializeField] private float normalHealthLoss  = 10f; // 3~4회 건강도 감소
    [SerializeField] private float dangerHealthLoss  = 20f; // 5회+ 건강도 감소

    [Header("Reset Settings")]
    [SerializeField] private float bounceResetTime = 3f;   // 이 시간 동안 안 튕기면 카운트 리셋

    private int bounceCount = 0;
    private float lastBounceTime = -999f;

    private void Update()
    {
        // 일정 시간 튕기지 않으면 카운트 리셋
        if (bounceCount > 0 && Time.time - lastBounceTime > bounceResetTime)
        {
            bounceCount = 0;
            Debug.Log("[Trampoline] 튕김 카운트 리셋");
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Food")) return;

        Rigidbody2D rb = col.rigidbody;
        if (rb == null) return;

        bounceCount++;
        lastBounceTime = Time.time;

        // 위로 튕기는 방향 (약간 랜덤)
        float angle = 90f + Random.Range(-bounceAngleVariance, bounceAngleVariance);
        Vector2 bounceDir = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );

        // 현재 속도 제거 후 튕김
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(bounceDir * bounceForce, ForceMode2D.Impulse);

        // 건강도 변화
        ApplyHealthEffect();

        Debug.Log($"[Trampoline] {bounceCount}회 튕김!");
    }

    private void ApplyHealthEffect()
    {
        if (bounceCount <= safeBouncCount)
        {
            // 정상 소화 - 변화 없음
            return;
        }
        else if (bounceCount <= dangerBounceCount)
        {
            // 과한 자극
            HealthManager.Instance?.LoseHealth(normalHealthLoss, isHazard: false);
            Debug.Log($"[Trampoline] 과한 자극! -{normalHealthLoss}");
        }
        else
        {
            // 오바이트 직전
            HealthManager.Instance?.LoseHealth(dangerHealthLoss, isHazard: true);
            Debug.Log($"[Trampoline] 오바이트! -{dangerHealthLoss}");
        }
    }

    // Gizmos로 트램펄린 범위 표시
    private void OnDrawGizmos()
    {
        Gizmos.color = bounceCount > safeBouncCount ? Color.red : Color.cyan;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
