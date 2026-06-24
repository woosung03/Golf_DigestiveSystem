using UnityEngine;

/// <summary>
/// Stage 1 - 입/식도 코스 빌더
/// 프리팹 참조해서 음식/치아/장애물 자동 배치
/// 
/// 구조:
///  [시작: 입 구간] 가로 이동
///  ●──────────────────╮  ← 윗 천장
///  ════════════════╮  │  ← 아랫 바닥 (오른쪽에서 꺾임)
///                  │  │
///                  │  │  ← 식도 (세로 구간)
///                  ╰──╯
///                    ↓ [골]
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
    [SerializeField] private GameObject[] goodFoodPrefabs;
    [SerializeField] private GameObject[] badFoodPrefabs;

    [Header("Tooth Settings")]
    [SerializeField] private int   toothPairCount  = 3;    // 치아 쌍 개수
    [SerializeField] private float toothWidth      = 1.2f; // 어금니 너비
    [SerializeField] private float toothHeight     = 0.6f; // 어금니 높이
    [SerializeField] private float toothGap        = 1.0f; // 위아래 치아 사이 간격
    [SerializeField] private Color toothColor      = Color.white;

    [Header("Spawn Settings")]
    [SerializeField] private int goodFoodCount = 4;
    [SerializeField] private int badFoodCount  = 2;

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
        SpawnTeeth();
        SpawnFood();
        PlaceGoal(new Vector2((esoLeft + esoRight) / 2f, esoBottom + 0.5f));

        Debug.Log($"[CourseBuilder] Stage1 생성 완료");
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

    /// <summary>
    /// 입 구간에 치아 쌍 배치
    /// 위 치아: 천장에서 아래로 돌출
    /// 아래 치아: 바닥에서 위로 돌출
    /// 사이 간격(toothGap)으로 공이 통과
    /// </summary>
    private void SpawnTeeth()
    {
        float spawnXMin = mouthLeft  + 2f;
        float spawnXMax = esoLeft    - 2f;
        float step = (spawnXMax - spawnXMin) / (toothPairCount + 1);

        float mouthCenter = (mouthTop + mouthBottom) / 2f;
        float upperToothBottom = mouthCenter + toothGap / 2f; // 위 치아 하단
        float lowerToothTop    = mouthCenter - toothGap / 2f; // 아래 치아 상단

        for (int i = 0; i < toothPairCount; i++)
        {
            float x = spawnXMin + step * (i + 1);

            // 위 치아 (천장에서 아래로)
            CreateTooth($"UpperTooth_{i}", x,
                upperToothBottom + toothHeight / 2f,  // 중심 Y
                true);

            // 아래 치아 (바닥에서 위로)
            CreateTooth($"LowerTooth_{i}", x,
                lowerToothTop - toothHeight / 2f,     // 중심 Y
                false);
        }
    }

    private void CreateTooth(string objName, float x, float centerY, bool isUpper)
    {
        GameObject go = new GameObject(objName);
        go.transform.parent = transform;
        go.transform.position = new Vector3(x, centerY, 0f);
        go.layer = LayerMask.NameToLayer("Default");
        go.tag = "Untagged";

        // 비주얼
        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = toothColor;
        sr.sortingOrder = 1;
        // 기본 흰 사각형 스프라이트 (나중에 치아 스프라이트로 교체)
        sr.sprite = CreateRectSprite();
        go.transform.localScale = new Vector3(toothWidth, toothHeight, 1f);

        // 콜라이더
        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one; // localScale으로 크기 조정

        // Tooth 스크립트
        var tooth = go.AddComponent<Tooth>();
    }

    private void SpawnFood()
    {
        float spawnXMin = mouthLeft  + 1f;
        float spawnXMax = esoLeft    - 1f;
        float spawnY    = mouthBottom + 0.5f;

        SpawnPrefabsInRow(goodFoodPrefabs, goodFoodCount, spawnXMin, spawnXMax, spawnY);
        SpawnPrefabsInRow(badFoodPrefabs,  badFoodCount,  spawnXMin, spawnXMax, spawnY + 1.2f);
    }

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

    private void SpawnPrefab(GameObject prefab, Vector2 pos)
    {
        if (prefab == null) return;
        Instantiate(prefab, pos, Quaternion.identity).transform.parent = transform;
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

    private Sprite CreateRectSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
