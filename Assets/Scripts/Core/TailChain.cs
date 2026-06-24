using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 꼬리 체인 관리자
/// - 스폰 시 중력 0 + 머리 오른쪽에 가로로 생성
/// - 일정 시간 후 중력 복구 → 자연스럽게 아래로 늘어짐
/// - Tail 레이어(7번)로 꼬리끼리 충돌 무시
/// </summary>
public class TailChain : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FoodBall headBall;

    [Header("Tail Settings")]
    [SerializeField] private float segmentRadius   = 0.3f;
    [SerializeField] private float segmentMass     = 0.8f;
    [SerializeField] private float segmentDistance = 0.4f;

    [Header("Spawn Settings")]
    [SerializeField] private float gravityRestoreDelay = 0.8f; // 중력 복구까지 대기 시간

    [Header("Tail Visual")]
    [SerializeField] private Color tailColor = new Color(0.55f, 0.27f, 0.07f);
    [SerializeField] private Sprite tailSprite;

    private List<GameObject> segments = new List<GameObject>();
    private PhysicsMaterial2D tailMat;
    private int headSortingOrder = 0;

    private const int TAIL_LAYER = 7;

    private void Awake()
    {
        tailMat = new PhysicsMaterial2D("TailMat")
        {
            bounciness = 0.1f,
            friction = 0.6f
        };

        var headSr = headBall?.GetComponent<SpriteRenderer>();
        if (headSr != null) headSortingOrder = headSr.sortingOrder;

        Physics2D.IgnoreLayerCollision(TAIL_LAYER, TAIL_LAYER, true);
    }

    public void AddSegment()
    {
        GameObject anchorObj = segments.Count == 0
            ? headBall.gameObject
            : segments[segments.Count - 1];

        Rigidbody2D anchorRb = anchorObj.GetComponent<Rigidbody2D>();

        int index = segments.Count;
        Vector2 headPos = headBall.transform.position;

        // 머리 왼쪽 방향으로 가로 배치 (지형 끼임 방지)
        Vector2 spawnPos = new Vector2(
            headPos.x - segmentDistance * (index + 1),
            headPos.y
        );

        GameObject seg = new GameObject($"Tail_{index}");
        seg.transform.position = spawnPos;
        seg.layer = TAIL_LAYER;
        seg.tag = "Food";

        // 비주얼
        var sr = seg.AddComponent<SpriteRenderer>();
        sr.sprite = GetSprite();
        sr.color = tailColor;
        sr.sortingOrder = headSortingOrder - (index + 1);
        float scale = segmentRadius * 2f;
        seg.transform.localScale = new Vector3(scale, scale, 1f);

        // 물리 - 처음엔 중력 0
        var rb = seg.AddComponent<Rigidbody2D>();
        rb.mass = segmentMass;
        rb.gravityScale = 0f; // 스폰 직후 중력 없음
        rb.linearDamping = 0.8f;
        rb.angularDamping = 0.8f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        var col = seg.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;
        col.sharedMaterial = tailMat;

        // 머리와 충돌 무시
        var headCol = headBall.GetComponent<Collider2D>();
        if (headCol != null) Physics2D.IgnoreCollision(col, headCol, true);

        // HingeJoint2D 연결
        var hinge = seg.AddComponent<HingeJoint2D>();
        hinge.connectedBody = anchorRb;
        hinge.autoConfigureConnectedAnchor = false;
        hinge.anchor = new Vector2(0.5f, 0f);       // 이 세그먼트 오른쪽
        hinge.connectedAnchor = new Vector2(-0.5f, 0f); // 앞 세그먼트 왼쪽

        segments.Add(seg);

        // 일정 시간 후 중력 복구
        StartCoroutine(RestoreGravity(rb));

        Debug.Log($"[TailChain] 꼬리 추가 → 총 {segments.Count}개");
    }

    /// <summary>
    /// 스폰 후 gravityRestoreDelay 초 뒤 중력 복구
    /// HingeJoint로 연결돼 있어서 자연스럽게 아래로 늘어짐
    /// </summary>
    private IEnumerator RestoreGravity(Rigidbody2D rb)
    {
        yield return new WaitForSeconds(gravityRestoreDelay);
        if (rb != null)
            rb.gravityScale = 1f;
    }

    public Rigidbody2D GetSegmentRb(int index)
    {
        if (index < 0 || index >= segments.Count) return null;
        return segments[index]?.GetComponent<Rigidbody2D>();
    }

    public void ClearTail()
    {
        foreach (var seg in segments)
            if (seg != null) Destroy(seg);
        segments.Clear();
    }

    public int SegmentCount => segments.Count;
    public float TotalMass => headBall.Mass + segmentMass * segments.Count;

    private Sprite GetSprite()
    {
        if (tailSprite != null) return tailSprite;
        return CreateCircleSprite();
    }

    private Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        float center = size / 2f;
        float radius = size / 2f - 1f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                tex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
            }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
