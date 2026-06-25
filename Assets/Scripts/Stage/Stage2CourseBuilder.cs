using UnityEngine;

/// <summary>
/// Stage 2 - 위/십이지장 코스 빌더
/// 동키콩 스타일 플랫폼 맵
/// - 층마다 바닥 + 구멍으로 아래로 내려가는 구조
/// - 트램펄린은 구멍 반대편에 배치 (지그재그 유도)
/// 
/// 구조:
/// ╭─────────────────╮
/// │ 입구             │
/// │ ████ 구멍 █████ │ ← 1층
/// │      ████       │ ← 트램펄린
/// │ █████ 구멍 ████ │ ← 2층
/// │ ████            │ ← 트램펄린
/// │ ████ 구멍 █████ │ ← 3층
/// ╰──╮              │
///    │ 십이지장      │
/// </summary>
public class Stage2CourseBuilder : MonoBehaviour
{
    [Header("Auto Build")]
    [SerializeField] private bool buildOnStart = true;

    [Header("Materials")]
    [SerializeField] private PhysicsMaterial2D groundMaterial;

    [Header("Stomach Settings")]
    [SerializeField] private float stomachWidth  = 14f;
    [SerializeField] private float stomachHeight = 20f;

    [Header("Duodenum Settings")]
    [SerializeField] private float duodenumWidth = 3f;
    [SerializeField] private float duodenumDepth = 6f;

    [Header("Platform Settings")]
    [SerializeField] private int   floorCount   = 3;      // 층 수
    [SerializeField] private float holeWidth    = 3.5f;   // 구멍 너비
    [SerializeField] private float floorHeight  = 0.3f;   // 바닥 두께

    [Header("Trampoline Settings")]
    [SerializeField] private float trampolineWidth  = 3f;
    [SerializeField] private float trampolineHeight = 0.3f;

    [Header("Food Prefabs")]
    [SerializeField] private GameObject[] goodFoodPrefabs;
    [SerializeField] private GameObject[] badFoodPrefabs;
    [SerializeField] private int goodFoodCount = 3;
    [SerializeField] private int badFoodCount  = 2;

    // 좌표 캐싱
    private float stLeft, stRight, stTop, stBottom;
    private float duoLeft, duoRight, duoBottom;

    private void Start()
    {
        if (buildOnStart) BuildCourse();
    }

    public void BuildCourse()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        CalculateCoords();
        BuildOuterWalls();
        BuildPlatforms();
        SpawnFood();
        PlaceGoal();

        Debug.Log("[Stage2CourseBuilder] Stage2 생성 완료");
    }

    private void CalculateCoords()
    {
        stLeft   = -7f;
        stRight  = stLeft + stomachWidth;
        stTop    = 10f;
        stBottom = stTop - stomachHeight;

        duoLeft   = stLeft;
        duoRight  = stLeft + duodenumWidth;
        duoBottom = stBottom - duodenumDepth;
    }

    private void BuildOuterWalls()
    {
        // 상단 입구
        CreateEdge("Top", new Vector2[]
        {
            new Vector2(stLeft,  stTop),
            new Vector2(stRight, stTop),
        }, groundMaterial);

        // 오른쪽 벽
        CreateEdge("Right", new Vector2[]
        {
            new Vector2(stRight, stTop),
            new Vector2(stRight, stBottom),
        }, groundMaterial);

        // 하단 바닥 (십이지장 오른쪽부터)
        CreateEdge("Bottom", new Vector2[]
        {
            new Vector2(duoRight, stBottom),
            new Vector2(stRight,  stBottom),
        }, groundMaterial);

        // 왼쪽 벽
        CreateEdge("Left", new Vector2[]
        {
            new Vector2(stLeft, stTop),
            new Vector2(stLeft, stBottom),
        }, groundMaterial);

        // 십이지장 왼쪽 벽
        CreateEdge("Duo_Left", new Vector2[]
        {
            new Vector2(duoLeft, stBottom),
            new Vector2(duoLeft, duoBottom),
        }, groundMaterial);

        // 십이지장 오른쪽 벽
        CreateEdge("Duo_Right", new Vector2[]
        {
            new Vector2(duoRight, stBottom),
            new Vector2(duoRight, duoBottom),
        }, groundMaterial);

        // 십이지장 바닥
        CreateEdge("Duo_Bottom", new Vector2[]
        {
            new Vector2(duoLeft,  duoBottom),
            new Vector2(duoRight, duoBottom),
        }, groundMaterial);
    }

    /// <summary>
    /// 층마다 바닥(구멍 포함) + 트램펄린 배치
    /// 구멍과 트램펄린은 번갈아가며 좌/우 배치
    /// </summary>
    private void BuildPlatforms()
    {
        float yStep = stomachHeight / (floorCount + 1);

        for (int i = 0; i < floorCount; i++)
        {
            float y = stTop - yStep * (i + 1);
            bool holeOnLeft = (i % 2 == 0); // 짝수층: 구멍 왼쪽, 홀수층: 구멍 오른쪽

            BuildFloorWithHole($"Floor_{i}", y, holeOnLeft);
            BuildTrampoline($"Trampoline_{i}", y - yStep * 0.5f, !holeOnLeft);
        }
    }

    /// <summary>
    /// 구멍 있는 바닥 생성
    /// 구멍 왼쪽이면: [구멍][━━━━━━━━━━━]
    /// 구멍 오른쪽이면: [━━━━━━━━━━━][구멍]
    /// </summary>
    private void BuildFloorWithHole(string name, float y, bool holeOnLeft)
    {
        float totalWidth = stomachWidth;
        float solidWidth = totalWidth - holeWidth;

        if (holeOnLeft)
        {
            // 구멍이 왼쪽 → 오른쪽에 바닥
            float solidStart = stLeft + holeWidth;
            CreateEdge(name, new Vector2[]
            {
                new Vector2(solidStart, y),
                new Vector2(stRight,    y),
            }, groundMaterial);
        }
        else
        {
            // 구멍이 오른쪽 → 왼쪽에 바닥
            float solidEnd = stRight - holeWidth;
            CreateEdge(name, new Vector2[]
            {
                new Vector2(stLeft,   y),
                new Vector2(solidEnd, y),
            }, groundMaterial);
        }
    }

    /// <summary>
    /// 트램펄린 생성 - 구멍 반대편에 배치
    /// </summary>
    private void BuildTrampoline(string name, float y, bool onLeft)
    {
        float x = onLeft
            ? stLeft  + trampolineWidth / 2f + 0.5f
            : stRight - trampolineWidth / 2f - 0.5f;

        GameObject go = new GameObject(name);
        go.transform.parent = transform;
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(trampolineWidth, trampolineHeight, 1f);
        go.layer = LayerMask.NameToLayer("Default");

        // 비주얼
        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.2f, 0.8f, 0.8f);
        sr.sortingOrder = 1;
        sr.sprite = CreateRectSprite();

        // 콜라이더 + 바운시 매테리얼
        var col = go.AddComponent<BoxCollider2D>();
        col.sharedMaterial = new PhysicsMaterial2D("TrampolineMat")
        {
            bounciness = 0.9f,
            friction   = 0f
        };

        go.AddComponent<Trampoline>();
    }

    private void SpawnFood()
    {
        float xMin = stLeft  + 1f;
        float xMax = stRight - 1f;
        float yMin = stBottom + 1f;
        float yMax = stTop    - 1f;

        SpawnRandom(goodFoodPrefabs, goodFoodCount, xMin, xMax, yMin, yMax);
        SpawnRandom(badFoodPrefabs,  badFoodCount,  xMin, xMax, yMin, yMax);
    }

    private void SpawnRandom(GameObject[] prefabs, int count,
        float xMin, float xMax, float yMin, float yMax)
    {
        if (prefabs == null || prefabs.Length == 0 || count == 0) return;
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = new Vector2(
                Random.Range(xMin, xMax),
                Random.Range(yMin, yMax)
            );
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            if (prefab != null)
                Instantiate(prefab, pos, Quaternion.identity).transform.parent = transform;
        }
    }

    private void PlaceGoal()
    {
        float goalX = (duoLeft + duoRight) / 2f;
        float goalY = duoBottom + 0.5f;

        var existing = FindFirstObjectByType<StageGoal>();
        if (existing != null)
        {
            existing.transform.position = new Vector3(goalX, goalY, 0f);
            return;
        }

        GameObject goalGO = new GameObject("Goal");
        goalGO.transform.parent = transform;
        goalGO.transform.position = new Vector3(goalX, goalY, 0f);

        var col = goalGO.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(duodenumWidth, 1f);

        goalGO.AddComponent<StageGoal>();
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

    private Sprite CreateRectSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
