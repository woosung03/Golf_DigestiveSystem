using UnityEngine;

/// <summary>
/// Stage 1 - 입/식도 코스 빌더
/// 프리팹 참조해서 음식/장애물 자동 배치
/// 
/// 구조:
///  [시작: 입 구간] 가로 이동
///  ●──────────────────╮  ← 윗 천장
///  ════════════════╮  │  ← 아랫 바닥 (오른쪽에서 꺾임)
///                  │  │
///                  │  │  ← 식도 (세로 구간, 좁은 통로)
///                  │  │
///                  ╰──╯
///                    ↓ [골] 위 입구
/// </summary>
public class CourseBuilder : MonoBehaviour
{
    [Header("Auto Build on Start")]
    [SerializeField] private bool buildOnStart = true;

    [Header("Materials")]
    [SerializeField] private PhysicsMaterial2D groundMaterial;

    [Header("Stage 1 Settings")]
    [SerializeField] private float mouthWidth     = 16f;
    [SerializeField] private float mouthHeight    = 3f;
    [SerializeField] private float esophagusDepth = 12f;
    [SerializeField] private float esophagusWidth = 3f;

    [Header("Food Prefabs")]
    [SerializeField] private GameObject[] goodFoodPrefabs;  // 건강한 음식 (꼬리 +, 건강도 +)
    [SerializeField] private GameObject[] badFoodPrefabs;   // 나쁜 음식 (꼬리 +, 건강도 -)

    [Header("Hazard Prefabs")]
    [SerializeField] private GameObject[] hazardPrefabs;    // 장애물 (건강도 -)

    [Header("Spawn Settings")]
    [SerializeField] private int goodFoodCount = 4;
    [SerializeField] private int badFoodCount  = 2;
    [SerializeField] private int hazardCount   = 2;

    // 좌표 캐싱
    private float mouthLeft, mouthRight, mouthTop, mouthBottom;
    private float esoLeft, esoRight, esoTop, esoBottom;

    private void Start()
    {
        if (buildOnStart) BuildCourse();
    }

    public void BuildCourse()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        CalculateCoords();
        BuildWalls();
        SpawnItems();
        PlaceGoal(new Vector2((esoLeft + esoRight) / 2f, esoBottom + 0.5f));

        Debug.Log($"[CourseBuilder] Stage1 생성 완료 | 시작: ({mouthLeft + 1f}, {mouthBottom + 0.5f})");
    }

    private void CalculateCoords()
    {
        mouthLeft   = -8f;
        mouthRight  = mouthLeft + mouthWidth;
        mouthTop    = 2f;
        mouthBottom = mouthTop - mouthHeight;

        esoLeft   = mouthRight - esophagusWidth;
        esoRight  = mouthRight;
        esoTop    = mouthBottom;
        esoBottom = esoTop - esophagusDepth;
    }

    private void BuildWalls()
    {
        CreateEdge("Mouth_Ceiling", new Vector2[]
        {
            new Vector2(mouthLeft,  mouthTop),
            new Vector2(mouthRight, mouthTop),
        }, groundMaterial);

        CreateEdge("Mouth_Floor", new Vector2[]
        {
            new Vector2(mouthLeft, mouthBottom),
            new Vector2(esoLeft,   mouthBottom),
        }, groundMaterial);

        CreateEdge("Left_Wall", new Vector2[]
        {
            new Vector2(mouthLeft, mouthTop),
            new Vector2(mouthLeft, mouthBottom),
        }, groundMaterial);

        CreateEdge("Eso_LeftWall", new Vector2[]
        {
            new Vector2(esoLeft, mouthBottom),
            new Vector2(esoLeft, esoBottom),
        }, groundMaterial);

        CreateEdge("Eso_RightWall", new Vector2[]
        {
            new Vector2(esoRight, mouthTop),
            new Vector2(esoRight, esoBottom),
        }, groundMaterial);

        CreateEdge("Eso_Floor", new Vector2[]
        {
            new Vector2(esoLeft,  esoBottom),
            new Vector2(esoRight, esoBottom),
        }, groundMaterial);
    }

    private void SpawnItems()
    {
        // 입 구간 X 범위 (왼쪽 벽 ~ 식도 왼쪽 벽)
        float spawnXMin = mouthLeft  + 1f;
        float spawnXMax = esoLeft    - 1f;
        float spawnY    = mouthBottom + 0.6f; // 바닥 바로 위

        // 좋은 음식 배치 (균등 간격)
        SpawnPrefabsInRow(goodFoodPrefabs, goodFoodCount,
            spawnXMin, spawnXMax, spawnY);

        // 나쁜 음식 배치 (좋은 음식 사이사이)
        SpawnPrefabsInRow(badFoodPrefabs, badFoodCount,
            spawnXMin + 1f, spawnXMax - 1f, spawnY + 1f);

        // 식도 구간 장애물 배치
        float esoSpawnX = (esoLeft + esoRight) / 2f;
        float esoSpawnYMin = esoTop   - 1f;
        float esoSpawnYMax = esoBottom + 2f;
        SpawnPrefabsInColumn(hazardPrefabs, hazardCount,
            esoSpawnX, esoSpawnYMin, esoSpawnYMax);
    }

    /// <summary>가로 방향으로 균등 배치</summary>
    private void SpawnPrefabsInRow(GameObject[] prefabs, int count,
        float xMin, float xMax, float y)
    {
        if (prefabs == null || prefabs.Length == 0 || count == 0) return;

        float step = (xMax - xMin) / (count + 1);
        for (int i = 0; i < count; i++)
        {
            float x = xMin + step * (i + 1);
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            SpawnPrefab(prefab, new Vector2(x, y));
        }
    }

    /// <summary>세로 방향으로 균등 배치</summary>
    private void SpawnPrefabsInColumn(GameObject[] prefabs, int count,
        float x, float yMin, float yMax)
    {
        if (prefabs == null || prefabs.Length == 0 || count == 0) return;

        float step = (yMax - yMin) / (count + 1);
        for (int i = 0; i < count; i++)
        {
            float y = yMin + step * (i + 1);
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            SpawnPrefab(prefab, new Vector2(x, y));
        }
    }

    private void SpawnPrefab(GameObject prefab, Vector2 pos)
    {
        if (prefab == null) return;
        GameObject go = Instantiate(prefab, pos, Quaternion.identity);
        go.transform.parent = transform;
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
        var existing = FindFirstObjectByType<StageGoal>();
        if (existing != null)
        {
            existing.transform.position = new Vector3(pos.x, pos.y, 0f);
            return;
        }

        GameObject goalGO = new GameObject("Goal");
        goalGO.transform.parent = transform;
        goalGO.transform.position = new Vector3(pos.x, pos.y, 0f);

        var col = goalGO.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(esophagusWidth, 1f);

        goalGO.AddComponent<StageGoal>();
    }
}
