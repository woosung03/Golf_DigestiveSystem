using UnityEngine;

/// <summary>
/// 프로토타입 코스 자동 생성기
/// Play 전에 에디터에서 실행하거나 Start()에서 자동 생성
/// 
/// 코스 구조:
/// 
///  [시작]                         [골]
///   ●                              ⛳
///   ──────╮        ╭──────╮       ──
///         ╰────────╯      ╰───────
/// 
/// X: -8 ~ +10, 총 길이 약 18유닛
/// </summary>
public class CourseBuilder : MonoBehaviour
{
    [Header("Auto Build on Start")]
    [SerializeField] private bool buildOnStart = true;

    [Header("Materials (선택)")]
    [SerializeField] private PhysicsMaterial2D groundMaterial;

    private void Start()
    {
        if (buildOnStart) BuildCourse();
    }

    public void BuildCourse()
    {
        // 기존 코스 오브젝트 제거
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        // ── 지형 포인트 정의 ──────────────────────────────────
        // 바닥 전체를 하나의 긴 EdgeCollider2D로 구성
        Vector2[] floorPoints = new Vector2[]
        {
            new Vector2(-9f,   0f),   // 왼쪽 시작
            new Vector2(-4f,   0f),   // 평지
            new Vector2(-3f,  -2f),   // 첫 번째 경사 하강
            new Vector2( 0f,  -2f),   // 첫 번째 골짜기 바닥
            new Vector2( 1f,   0f),   // 경사 상승
            new Vector2( 4f,   0f),   // 두 번째 평지
            new Vector2( 5f,  -1.5f), // 두 번째 경사 하강
            new Vector2( 7f,  -1.5f), // 두 번째 골짜기 바닥
            new Vector2( 8f,   0f),   // 경사 상승
            new Vector2(11f,   0f),   // 골 앞 평지
        };

        // 왼쪽 벽
        Vector2[] leftWallPoints = new Vector2[]
        {
            new Vector2(-9f,  5f),
            new Vector2(-9f,  0f),
        };

        // 오른쪽 벽
        Vector2[] rightWallPoints = new Vector2[]
        {
            new Vector2(11f,  0f),
            new Vector2(11f,  5f),
        };

        // ── 생성 ─────────────────────────────────────────────
        CreateEdge("Floor",     floorPoints,     groundMaterial);
        CreateEdge("LeftWall",  leftWallPoints,  groundMaterial);
        CreateEdge("RightWall", rightWallPoints, groundMaterial);

        // 골 트리거 위치 자동 배치 (오른쪽 끝 바닥 위)
        PlaceGoal(new Vector2(10.5f, 0.5f));

        Debug.Log("[CourseBuilder] 코스 생성 완료");
    }

    private void CreateEdge(string objName, Vector2[] points, PhysicsMaterial2D mat)
    {
        GameObject go = new GameObject(objName);
        go.transform.parent = transform;
        go.layer = LayerMask.NameToLayer("Default");

        var edge = go.AddComponent<EdgeCollider2D>();
        edge.points = points;
        if (mat != null) edge.sharedMaterial = mat;
    }

    private void PlaceGoal(Vector2 pos)
    {
        // 기존 StageGoal이 씬에 있으면 위치만 이동
        var existing = FindFirstObjectByType<StageGoal>();
        if (existing != null)
        {
            existing.transform.position = new Vector3(pos.x, pos.y, 0f);
            return;
        }

        // 없으면 새로 생성
        GameObject goalGO = new GameObject("Goal");
        goalGO.transform.parent = transform;
        goalGO.transform.position = new Vector3(pos.x, pos.y, 0f);

        var col = goalGO.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 2f);

        goalGO.AddComponent<StageGoal>();
    }
}
