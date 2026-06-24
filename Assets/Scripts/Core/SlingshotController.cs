using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 슬링샷 컨트롤러
/// - 화면 드래그로 전체 체인을 한 덩어리로 던짐
/// - 머리 공 위치 기준으로 당긴 방향 반대로 발사
/// - 멈추면 다시 드래그 가능
/// </summary>
public class SlingshotController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FoodBall foodBall;
    [SerializeField] private TailChain tailChain;
    [SerializeField] private SlingshotUI slingshotUI;

    [Header("Slingshot Settings")]
    [SerializeField] private float maxPower = 25f;
    [SerializeField] private float maxDragDistance = 3f;
    [SerializeField] private float powerMultiplierPerSegment = 0.15f; // 꼬리 늘수록 파워 보정

    private Camera mainCam;
    private Vector2 dragStartWorld;
    private bool isDragging = false;
    private bool canLaunch = true;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        if (foodBall != null)
        {
            foodBall.OnStopped += OnChainStopped;
            foodBall.OnLaunched += OnChainLaunched;
        }
    }

    private void OnDisable()
    {
        if (foodBall != null)
        {
            foodBall.OnStopped -= OnChainStopped;
            foodBall.OnLaunched -= OnChainLaunched;
        }
    }

    private void Update()
    {
        if (!canLaunch || foodBall == null) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
            BeginDrag();
        else if (mouse.leftButton.isPressed && isDragging)
            UpdateDrag();
        else if (mouse.leftButton.wasReleasedThisFrame && isDragging)
            ReleaseDrag();
    }

    private void BeginDrag()
    {
        dragStartWorld = GetMouseWorld();
        isDragging = true;
        slingshotUI?.SetVisible(true);
    }

    private void UpdateDrag()
    {
        Vector2 dragDelta = GetClampedDelta();
        float powerRatio = dragDelta.magnitude / maxDragDistance;

        slingshotUI?.UpdateLine(foodBall.transform.position, dragDelta);
        slingshotUI?.UpdatePowerBar(powerRatio);
    }

    private void ReleaseDrag()
    {
        Vector2 dragDelta = GetClampedDelta();
        float powerRatio = dragDelta.magnitude / maxDragDistance;

        if (powerRatio > 0.05f)
        {
            LaunchChain(dragDelta.normalized, powerRatio);
        }

        isDragging = false;
        slingshotUI?.SetVisible(false);
        slingshotUI?.UpdatePowerBar(0f);
    }

    /// <summary>
    /// 전체 체인에 동시에 힘을 가함
    /// 꼬리 개수에 따라 파워 자동 보정
    /// </summary>
    private void LaunchChain(Vector2 direction, float powerRatio)
    {
        int segCount = tailChain != null ? tailChain.SegmentCount : 0;

        // 꼬리가 늘수록 파워 보정 (무거워지는 만큼 더 세게)
        float powerBoost = 1f + segCount * powerMultiplierPerSegment;
        float power = powerRatio * maxPower * powerBoost;

        // 머리에 힘
        foodBall.Rb.linearVelocity = Vector2.zero;
        foodBall.Rb.AddForce(direction * power, ForceMode2D.Impulse);

        // 꼬리 전체에 동일하게 힘 (한 덩어리처럼)
        if (tailChain != null)
        {
            for (int i = 0; i < tailChain.SegmentCount; i++)
            {
                var rb = tailChain.GetSegmentRb(i);
                if (rb == null) continue;
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(direction * power, ForceMode2D.Impulse);
            }
        }

        foodBall.NotifyLaunched();
        Debug.Log($"[Slingshot] 발사 power={power:F1} dir={direction} 꼬리={segCount}개");
    }

    private void OnChainLaunched() => canLaunch = false;
    private void OnChainStopped() => canLaunch = true;

    private Vector2 GetClampedDelta()
    {
        Vector2 delta = dragStartWorld - GetMouseWorld();
        if (delta.magnitude > maxDragDistance)
            delta = delta.normalized * maxDragDistance;
        return delta;
    }

    private Vector2 GetMouseWorld()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        return mainCam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, -mainCam.transform.position.z));
    }
}
