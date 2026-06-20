using UnityEngine;

/// <summary>
/// 카메라가 FoodBall을 부드럽게 따라가는 팔로우 카메라
/// 공이 발사되면 따라가고, 멈추면 부드럽게 정착
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private FoodBall target;

    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Bounds (선택 - 스테이지 경계 제한)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float minX = -20f;
    [SerializeField] private float maxX = 20f;
    [SerializeField] private float minY = -10f;
    [SerializeField] private float maxY = 10f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.transform.position + offset;

        if (useBounds)
        {
            desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
            desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
    }

    public void SetTarget(FoodBall ball)
    {
        target = ball;
    }
}
