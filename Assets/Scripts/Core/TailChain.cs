using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 꼬리 체인 관리자
/// - Tail 레이어(7번)로 꼬리끼리 충돌 무시, 지형과는 충돌 유지
/// - GetSegmentRb()로 각 세그먼트 반환 → ShotController가 전체에 힘을 가함
/// </summary>
public class TailChain : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FoodBall headBall;

    [Header("Tail Settings")]
    [SerializeField] private float segmentRadius = 0.3f;
    [SerializeField] private float segmentMass = 0.8f;
    [SerializeField] private float segmentDistance = 0.4f;

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
        Vector2 spawnPos = new Vector2(headPos.x, headPos.y - segmentDistance * (index + 1));

        GameObject seg = new GameObject($"Tail_{index}");
        seg.transform.position = spawnPos;
        seg.layer = TAIL_LAYER;
        seg.tag = "Food";

        var sr = seg.AddComponent<SpriteRenderer>();
        sr.sprite = GetSprite();
        sr.color = tailColor;
        sr.sortingOrder = headSortingOrder - (index + 1);
        float scale = segmentRadius * 2f;
        seg.transform.localScale = new Vector3(scale, scale, 1f);

        var rb = seg.AddComponent<Rigidbody2D>();
        rb.mass = segmentMass;
        rb.gravityScale = 1f;
        rb.linearDamping = 0.8f;
        rb.angularDamping = 0.8f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        var col = seg.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;
        col.sharedMaterial = tailMat;

        var headCol = headBall.GetComponent<Collider2D>();
        if (headCol != null) Physics2D.IgnoreCollision(col, headCol, true);

        var hinge = seg.AddComponent<HingeJoint2D>();
        hinge.connectedBody = anchorRb;
        hinge.autoConfigureConnectedAnchor = false;
        hinge.anchor = new Vector2(0f, 0.5f);
        hinge.connectedAnchor = new Vector2(0f, -0.5f);

        segments.Add(seg);
        Debug.Log($"[TailChain] 꼬리 추가 → 총 {segments.Count}개");
    }

    /// <summary>인덱스로 세그먼트 Rigidbody 반환</summary>
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
