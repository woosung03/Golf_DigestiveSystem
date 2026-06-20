using UnityEngine;

/// <summary>
/// 스테이지 골 트리거 - 음식이 도달하면 스테이지 클리어
/// Collider2D (IsTrigger=true) 필요
/// </summary>
public class StageGoal : MonoBehaviour
{
    [Header("Goal Settings")]
    [SerializeField] private string foodTag = "Food";
    [SerializeField] private GameObject goalVFX; // 선택적 연출

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag(foodTag)) return;

        triggered = true;

        if (goalVFX != null)
            Instantiate(goalVFX, transform.position, Quaternion.identity);

        GameManager.Instance?.ClearStage();
    }
}
