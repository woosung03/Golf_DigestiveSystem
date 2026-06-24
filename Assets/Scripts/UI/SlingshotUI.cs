using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 슬링샷 UI
/// - 드래그 방향 화살표 선
/// - 파워 바
/// </summary>
public class SlingshotUI : MonoBehaviour
{
    [Header("Drag Line")]
    [SerializeField] private LineRenderer dragLine;

    [Header("Power Bar")]
    [SerializeField] private Slider powerSlider;
    [SerializeField] private Image powerFill;
    [SerializeField] private Color lowColor  = Color.green;
    [SerializeField] private Color highColor = Color.red;

    private void Awake()
    {
        if (dragLine != null)
        {
            dragLine.positionCount = 2;
            dragLine.startWidth = 0.1f;
            dragLine.endWidth = 0.05f;
            dragLine.useWorldSpace = true;
        }
        SetVisible(false);
    }

    public void UpdateLine(Vector2 ballPos, Vector2 dragDelta)
    {
        if (dragLine == null) return;
        // 공 위치에서 드래그 방향 반대(발사 방향)로 선 표시
        dragLine.SetPosition(0, ballPos);
        dragLine.SetPosition(1, ballPos + dragDelta.normalized *
            Mathf.Min(dragDelta.magnitude, 3f));
    }

    public void UpdatePowerBar(float ratio)
    {
        if (powerSlider != null) powerSlider.value = ratio;
        if (powerFill != null)
            powerFill.color = Color.Lerp(lowColor, highColor, ratio);
    }

    public void SetVisible(bool visible)
    {
        if (dragLine != null) dragLine.enabled = visible;
    }
}
