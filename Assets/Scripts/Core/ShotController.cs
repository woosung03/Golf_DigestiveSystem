using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 골프 조준 & 발사 시스템 (New Input System)
/// - 공 근처를 클릭했을 때만 드래그 시작
/// - 드래그 반대 방향으로 발사 (슬링샷 방식)
/// - 공이 움직이는 동안 입력 차단
/// </summary>
public class ShotController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FoodBall foodBall;
    [SerializeField] private TrajectoryPredictor trajectoryPredictor;
    [SerializeField] private ShotUI shotUI;

    [Header("Shot Settings")]
    [SerializeField] private float maxPower = 15f;
    [SerializeField] private float maxDragDistance = 2.5f;
    [SerializeField] private float ballClickRadius = 0.6f;

    private Camera mainCam;
    private Vector2 dragStartWorld;
    private bool isDragging = false;
    private bool canShoot = true;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        if (foodBall != null)
        {
            foodBall.OnStopped += OnBallStopped;
            foodBall.OnLaunched += OnBallLaunched;
        }
    }

    private void OnDisable()
    {
        if (foodBall != null)
        {
            foodBall.OnStopped -= OnBallStopped;
            foodBall.OnLaunched -= OnBallLaunched;
        }
    }

    private void Update()
    {
        if (foodBall == null || !canShoot) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
            TryBeginDrag();
        else if (mouse.leftButton.isPressed && isDragging)
            UpdateDrag();
        else if (mouse.leftButton.wasReleasedThisFrame && isDragging)
            ReleaseDrag();
    }

    private void TryBeginDrag()
    {
        Vector2 mouseWorld = GetMouseWorld();
        float dist = Vector2.Distance(mouseWorld, foodBall.transform.position);
        if (dist > ballClickRadius) return;

        dragStartWorld = mouseWorld;
        isDragging = true;
        trajectoryPredictor?.SetVisible(true);
    }

    private void UpdateDrag()
    {
        Vector2 mouseWorld = GetMouseWorld();
        Vector2 dragDelta = dragStartWorld - mouseWorld;

        if (dragDelta.magnitude > maxDragDistance)
            dragDelta = dragDelta.normalized * maxDragDistance;

        float powerRatio = dragDelta.magnitude / maxDragDistance;
        float power = powerRatio * maxPower;

        Vector2 initialVelocity = dragDelta.normalized * (power / foodBall.Mass);
        trajectoryPredictor?.UpdateTrajectory(foodBall.transform.position, initialVelocity);
        shotUI?.UpdatePowerBar(powerRatio);

        Debug.DrawLine(foodBall.transform.position,
                       (Vector2)foodBall.transform.position + dragDelta, Color.yellow);
    }

    private void ReleaseDrag()
    {
        Vector2 mouseWorld = GetMouseWorld();
        Vector2 dragDelta = dragStartWorld - mouseWorld;

        if (dragDelta.magnitude > maxDragDistance)
            dragDelta = dragDelta.normalized * maxDragDistance;

        float powerRatio = dragDelta.magnitude / maxDragDistance;

        if (powerRatio > 0.05f)
        {
            float power = powerRatio * maxPower;
            foodBall.Launch(dragDelta.normalized, power);
            GameManager.Instance?.AddShot();
        }

        isDragging = false;
        trajectoryPredictor?.SetVisible(false);
        shotUI?.UpdatePowerBar(0f);
    }

    private void OnBallLaunched() => canShoot = false;

    private void OnBallStopped()
    {
        if (GameManager.Instance != null && !GameManager.Instance.ShotsRemaining)
        {
            GameManager.Instance.FailStage();
            return;
        }
        canShoot = true;
    }

    private Vector2 GetMouseWorld()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -mainCam.transform.position.z));
        return worldPos;
    }

    public void SetFoodBall(FoodBall ball)
    {
        if (foodBall != null)
        {
            foodBall.OnStopped -= OnBallStopped;
            foodBall.OnLaunched -= OnBallLaunched;
        }
        foodBall = ball;
        if (foodBall != null)
        {
            foodBall.OnStopped += OnBallStopped;
            foodBall.OnLaunched += OnBallLaunched;
        }
    }
}
