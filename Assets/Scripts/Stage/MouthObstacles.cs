using UnityEngine;

/// <summary>
/// Mouth 스테이지 전용 장애물들
/// </summary>

// ── 1. 이빨 장애물 (정적 바운스) ──────────────────────────────────────
public class ToothObstacle : MonoBehaviour
{
    // PhysicsMaterial2D를 Collider2D에 직접 설정 (bounciness 0.7 권장)
    // 별도 로직 없음 - 물리 재질로 충분
}

// ── 2. 침 슬라이드 (미끄러운 경사면) ──────────────────────────────────
public class SalivaSlide : MonoBehaviour
{
    [SerializeField] private float frictionOverride = 0.05f;

    private void Start()
    {
        var col = GetComponent<Collider2D>();
        if (col == null) return;

        PhysicsMaterial2D mat = new PhysicsMaterial2D("SalivaMat");
        mat.friction = frictionOverride;
        mat.bounciness = 0.1f;
        col.sharedMaterial = mat;
    }
}

// ── 3. 위험 구역 (음식을 시작 지점으로 리셋) ──────────────────────────
public class HazardZone : MonoBehaviour
{
    [SerializeField] private string foodTag = "Food";
    [SerializeField] private Transform resetPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(foodTag)) return;

        FoodBall ball = other.GetComponent<FoodBall>();
        if (ball == null) return;

        if (resetPoint != null)
            ball.SetPosition(resetPoint.position);
        else
            ball.SetPosition(transform.position + Vector3.up * 2f);

        Debug.Log("[HazardZone] Food reset to start position.");
    }
}
