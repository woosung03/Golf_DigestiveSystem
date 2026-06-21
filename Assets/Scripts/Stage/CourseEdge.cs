using UnityEngine;

/// <summary>
/// EdgeCollider2D로 골프 코스 지형을 그리는 헬퍼
/// Inspector에서 points 배열 수정해서 지형 조정 가능
/// </summary>
[RequireComponent(typeof(EdgeCollider2D))]
public class CourseEdge : MonoBehaviour
{
    [Header("Course Points (월드 좌표)")]
    [SerializeField] private Vector2[] points = new Vector2[]
    {
        new Vector2(-8f,  -2f),   // 0: 시작 왼쪽 벽 하단
        new Vector2(-8f,   2f),   // 1: 시작 왼쪽 벽 상단
    };

    private EdgeCollider2D edgeCol;

    private void Awake()
    {
        edgeCol = GetComponent<EdgeCollider2D>();
        edgeCol.points = points;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        edgeCol = GetComponent<EdgeCollider2D>();
        if (edgeCol != null && points.Length >= 2)
            edgeCol.points = points;
    }

    private void OnDrawGizmos()
    {
        if (points == null || points.Length < 2) return;
        Gizmos.color = Color.green;
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 a = transform.TransformPoint(points[i]);
            Vector3 b = transform.TransformPoint(points[i + 1]);
            Gizmos.DrawLine(a, b);
        }
    }
#endif
}
