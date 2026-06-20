using UnityEngine;

/// <summary>
/// 골프 조준 & 발사 시스템
/// - 공 위에서만 드래그 시작 가능
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
    [SerializeField] private float ballClickRadius = 0.6f; // 공 클릭 인식 반경 (월드 단위)

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

        if (Input.GetMouseButtonDown(0))
            TryBeginDrag();
        else if (Input.GetMouseButton(0) && isDragging)
            UpdateDrag();
        else if (Input.GetMouseButtonUp(0) && isDragging)
            ReleaseDrag();
    }

    // 공 근처를 클릭했을 때만 드래그 시작
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
        Vector2 dragDelta = dragStartWorld - mouseWorld; // 당긴 반대 = 발사 방향

        // 최대 드래그 거리 클램프
        if (dragDelta.magnitude > maxDragDistance)
            dragDelta = dragDelta.normalized * maxDragDistance;

        float powerRatio = dragDelta.magnitude / maxDragDistance;
        float power = powerRatio * maxPower;

        // 궤적 미리보기: Impulse이므로 초기속도 = force / mass
        Vector2 initialVelocity = dragDelta.normalized * (power / foodBall.Mass);
        trajectoryPredictor?.UpdateTrajectory(foodBall.transform.position, initialVelocity);

        shotUI?.UpdatePowerBar(powerRatio);

        // 드래그 방향 시각화용 선 (선택)
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
        // 샷 초과 체크는 GameManager에 위임
        if (GameManager.Instance != null && !GameManager.Instance.ShotsRemaining)
        {
            GameManager.Instance.FailStage();
            return;
        }
        canShoot = true;
    }

    private Vector2 GetMouseWorld()
    {
        Vector3 mp = Input.mousePosition;
        mp.z = -mainCam.transform.position.z;
        return mainCam.ScreenToWorldPoint(mp);
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
