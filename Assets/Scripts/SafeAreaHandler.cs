using UnityEngine;

/// <summary>
/// Handles safe area for mobile devices (notches, rounded corners, status bars)
/// Attach to any RectTransform that should respect safe area
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeAreaHandler : MonoBehaviour
{
    [Header("Safe Area Settings")]
    [SerializeField] private bool applyOnAwake = true;
    [SerializeField] private bool updateContinuously = false;
    [SerializeField] private bool conformX = true;
    [SerializeField] private bool conformY = true;

    private RectTransform _rectTransform;
    private Rect _lastSafeArea = new Rect(0, 0, 0, 0);
    private Vector2Int _lastScreenSize = new Vector2Int(0, 0);

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        if (applyOnAwake)
        {
            ApplySafeArea();
        }
    }

    void Update()
    {
        if (updateContinuously)
        {
            ApplySafeArea();
        }
    }

    public void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        // Check if safe area or screen size changed
        if (safeArea == _lastSafeArea && Screen.width == _lastScreenSize.x && Screen.height == _lastScreenSize.y)
        {
            return;
        }

        _lastSafeArea = safeArea;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Apply to RectTransform
        if (conformX)
        {
            _rectTransform.anchorMin = new Vector2(anchorMin.x, _rectTransform.anchorMin.y);
            _rectTransform.anchorMax = new Vector2(anchorMax.x, _rectTransform.anchorMax.y);
        }

        if (conformY)
        {
            _rectTransform.anchorMin = new Vector2(_rectTransform.anchorMin.x, anchorMin.y);
            _rectTransform.anchorMax = new Vector2(_rectTransform.anchorMax.x, anchorMax.y);
        }

        Debug.Log($"SafeArea applied: {safeArea} on screen {Screen.width}x{Screen.height}");
    }

    /// <summary>
    /// Get safe area info for debugging
    /// </summary>
    public static string GetSafeAreaInfo()
    {
        Rect safeArea = Screen.safeArea;
        return $"Screen: {Screen.width}x{Screen.height}, SafeArea: {safeArea}";
    }
}
