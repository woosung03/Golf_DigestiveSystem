using UnityEngine;

/// <summary>
/// 골프 궤적 예측기
/// 포물선 수식으로 예측점 계산 후 LineRenderer로 표시
/// initialVelocity = Impulse / mass 로 넘겨받음
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPredictor : MonoBehaviour
{
    [Header("Prediction Settings")]
    [SerializeField] private int pointCount = 40;
    [SerializeField] private float timeStep = 0.04f;

    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = pointCount;
        lr.startWidth = 0.08f;
        lr.endWidth = 0.02f;
        lr.useWorldSpace = true;
        SetVisible(false);
    }

    /// <summary>
    /// startPos: 발사 위치
    /// initialVelocity: 발사 직후 속도 (= Impulse / mass)
    /// </summary>
    public void UpdateTrajectory(Vector3 startPos, Vector2 initialVelocity)
    {
        Vector2 pos = startPos;
        Vector2 vel = initialVelocity;
        Vector2 gravity = Physics2D.gravity;

        for (int i = 0; i < pointCount; i++)
        {
            lr.SetPosition(i, new Vector3(pos.x, pos.y, 0f));
            vel += gravity * timeStep;
            pos += vel * timeStep;
        }
    }

    public void SetVisible(bool visible)
    {
        lr.enabled = visible;
    }
}
