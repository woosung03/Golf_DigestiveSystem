using UnityEngine;

/// <summary>
/// 음식 공 - Rigidbody2D 기반 물리 오브젝트
/// 발사, 이동 상태 감지
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class FoodBall : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float sleepThreshold = 0.3f;  // 높을수록 빨리 멈춤
    [SerializeField] private float sleepDelay = 0.3f;      // 짧을수록 빨리 멈춤
    [SerializeField] private float linearDamping = 1.5f;   // 높을수록 빨리 느려짐

    [Header("Food Stats")]
    [SerializeField] private float mass = 1f;
    [SerializeField] private float bounciness = 0.4f;
    [SerializeField] private float friction = 0.6f;

    private Rigidbody2D rb;
    private bool isMoving = false;
    private float stillTimer = 0f;

    public bool IsMoving => isMoving;
    public Rigidbody2D Rb => rb;
    public float Mass => mass;

    public System.Action OnStopped;
    public System.Action OnLaunched;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.mass = mass;
        rb.gravityScale = 1f;
        rb.linearDamping = linearDamping;
        rb.freezeRotation = false;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        var mat = new PhysicsMaterial2D("FoodMat")
        {
            bounciness = bounciness,
            friction = friction
        };
        GetComponent<CircleCollider2D>().sharedMaterial = mat;
    }

    private void Update()
    {
        DetectStopped();
    }

    public void Launch(Vector2 direction, float power)
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.AddForce(direction.normalized * power, ForceMode2D.Impulse);
        NotifyLaunched();
    }

    public void NotifyLaunched()
    {
        isMoving = true;
        stillTimer = 0f;
        OnLaunched?.Invoke();
    }

    private void DetectStopped()
    {
        if (!isMoving) return;

        if (rb.linearVelocity.magnitude < sleepThreshold)
        {
            stillTimer += Time.deltaTime;
            if (stillTimer >= sleepDelay)
            {
                isMoving = false;
                stillTimer = 0f;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                OnStopped?.Invoke();
            }
        }
        else
        {
            stillTimer = 0f;
        }
    }

    public void SetPosition(Vector2 pos)
    {
        rb.position = pos;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        isMoving = false;
    }
}
