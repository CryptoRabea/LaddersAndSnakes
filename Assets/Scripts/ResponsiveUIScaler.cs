using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Responsive UI scaler that adapts to different screen sizes and orientations
/// Provides utilities for mobile-optimized UI scaling
/// </summary>
[RequireComponent(typeof(CanvasScaler))]
public class ResponsiveUIScaler : MonoBehaviour
{
    [Header("Reference Resolution")]
    [SerializeField] private Vector2 portraitReferenceResolution = new Vector2(1080, 1920);
    [SerializeField] private Vector2 landscapeReferenceResolution = new Vector2(1920, 1080);

    [Header("Scaling Settings")]
    [SerializeField] private bool autoDetectOrientation = true;
    [SerializeField] private float portraitMatchValue = 0.5f; // 0 = width, 1 = height
    [SerializeField] private float landscapeMatchValue = 0.5f;

    [Header("Mobile Optimization")]
    [SerializeField] private bool enlargeUIOnMobile = true;
    [SerializeField] private float mobileScaleMultiplier = 1.2f;

    private CanvasScaler _canvasScaler;
    private ScreenOrientation _lastOrientation;
    private Vector2Int _lastScreenSize;

    void Awake()
    {
        _canvasScaler = GetComponent<CanvasScaler>();
        ApplyResponsiveSettings();
    }

    void Start()
    {
        _lastOrientation = Screen.orientation;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
    }

    void Update()
    {
        if (autoDetectOrientation)
        {
            // Check if orientation or screen size changed
            if (Screen.orientation != _lastOrientation ||
                Screen.width != _lastScreenSize.x ||
                Screen.height != _lastScreenSize.y)
            {
                _lastOrientation = Screen.orientation;
                _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
                ApplyResponsiveSettings();
            }
        }
    }

    public void ApplyResponsiveSettings()
    {
        if (_canvasScaler == null)
        {
            _canvasScaler = GetComponent<CanvasScaler>();
        }

        _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        bool isPortrait = IsPortrait();

        // Set reference resolution based on orientation
        _canvasScaler.referenceResolution = isPortrait ? portraitReferenceResolution : landscapeReferenceResolution;
        _canvasScaler.matchWidthOrHeight = isPortrait ? portraitMatchValue : landscapeMatchValue;

        // Apply mobile scaling if needed
        if (enlargeUIOnMobile && Application.isMobilePlatform)
        {
            _canvasScaler.scaleFactor = mobileScaleMultiplier;
        }

        Debug.Log($"UI Scaler updated - Orientation: {(isPortrait ? "Portrait" : "Landscape")}, " +
                  $"Resolution: {_canvasScaler.referenceResolution}, Match: {_canvasScaler.matchWidthOrHeight}");
    }

    public bool IsPortrait()
    {
        return Screen.height > Screen.width;
    }

    public bool IsLandscape()
    {
        return Screen.width > Screen.height;
    }

    /// <summary>
    /// Get current screen aspect ratio
    /// </summary>
    public float GetAspectRatio()
    {
        return (float)Screen.width / Screen.height;
    }

    /// <summary>
    /// Check if device has a notch (safe area is smaller than screen)
    /// </summary>
    public static bool HasNotch()
    {
        Rect safeArea = Screen.safeArea;
        return safeArea.width < Screen.width || safeArea.height < Screen.height;
    }

    /// <summary>
    /// Get recommended button size for current screen
    /// </summary>
    public float GetRecommendedButtonSize()
    {
        // Base size in pixels
        float baseSize = 120f;

        // Scale based on screen DPI
        float dpiScale = Screen.dpi > 0 ? Screen.dpi / 160f : 1f; // 160 DPI is baseline

        // Apply mobile multiplier
        if (Application.isMobilePlatform)
        {
            dpiScale *= 1.2f;
        }

        return baseSize * dpiScale;
    }

    /// <summary>
    /// Get recommended text size for current screen
    /// </summary>
    public float GetRecommendedTextSize(float baseSize)
    {
        float dpiScale = Screen.dpi > 0 ? Screen.dpi / 160f : 1f;

        if (Application.isMobilePlatform)
        {
            dpiScale *= 1.1f;
        }

        return baseSize * dpiScale;
    }
}
