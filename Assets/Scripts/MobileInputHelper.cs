using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Helper class for mobile input including gestures and haptic feedback
/// Provides utilities for swipe detection, pinch, and haptics
/// </summary>
public class MobileInputHelper : MonoBehaviour
{
    [Header("Swipe Detection")]
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private float swipeTimeThreshold = 0.5f;

    [Header("Haptic Feedback")]
    [SerializeField] private bool enableHaptics = true;

    // Swipe events
    public System.Action<Vector2> OnSwipe;
    public System.Action OnSwipeLeft;
    public System.Action OnSwipeRight;
    public System.Action OnSwipeUp;
    public System.Action OnSwipeDown;

    // Touch events
    public System.Action<Vector2> OnTouchStarted;
    public System.Action<Vector2> OnTouchMoved;
    public System.Action<Vector2> OnTouchEnded;
    public System.Action<float> OnLongPress; // Passes hold duration

    private Vector2 _touchStartPos;
    private float _touchStartTime;
    private bool _isTouching = false;
    private float _longPressThreshold = 0.5f;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        DetectTouchInput();
    }

    void DetectTouchInput()
    {
        // Use enhanced touch system
        if (Touch.activeFingers.Count > 0)
        {
            var touch = Touch.activeFingers[0].currentTouch;

            switch (touch.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    HandleTouchBegan(touch.screenPosition);
                    break;

                case UnityEngine.InputSystem.TouchPhase.Moved:
                    HandleTouchMoved(touch.screenPosition);
                    break;

                case UnityEngine.InputSystem.TouchPhase.Ended:
                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    HandleTouchEnded(touch.screenPosition);
                    break;
            }

            // Check for long press
            if (_isTouching && Time.time - _touchStartTime > _longPressThreshold)
            {
                OnLongPress?.Invoke(Time.time - _touchStartTime);
            }
        }
    }

    void HandleTouchBegan(Vector2 position)
    {
        _touchStartPos = position;
        _touchStartTime = Time.time;
        _isTouching = true;
        OnTouchStarted?.Invoke(position);
    }

    void HandleTouchMoved(Vector2 position)
    {
        OnTouchMoved?.Invoke(position);
    }

    void HandleTouchEnded(Vector2 position)
    {
        if (!_isTouching) return;

        _isTouching = false;
        OnTouchEnded?.Invoke(position);

        // Check for swipe
        float duration = Time.time - _touchStartTime;
        Vector2 swipeDelta = position - _touchStartPos;
        float swipeDistance = swipeDelta.magnitude;

        if (swipeDistance > swipeThreshold && duration < swipeTimeThreshold)
        {
            DetectSwipeDirection(swipeDelta);
        }
    }

    void DetectSwipeDirection(Vector2 swipeDelta)
    {
        OnSwipe?.Invoke(swipeDelta);

        // Determine primary direction
        if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
        {
            // Horizontal swipe
            if (swipeDelta.x > 0)
            {
                OnSwipeRight?.Invoke();
                TriggerHapticFeedback(HapticFeedbackType.LightImpact);
            }
            else
            {
                OnSwipeLeft?.Invoke();
                TriggerHapticFeedback(HapticFeedbackType.LightImpact);
            }
        }
        else
        {
            // Vertical swipe
            if (swipeDelta.y > 0)
            {
                OnSwipeUp?.Invoke();
                TriggerHapticFeedback(HapticFeedbackType.LightImpact);
            }
            else
            {
                OnSwipeDown?.Invoke();
                TriggerHapticFeedback(HapticFeedbackType.LightImpact);
            }
        }
    }

    /// <summary>
    /// Trigger haptic feedback on mobile devices
    /// </summary>
    public void TriggerHapticFeedback(HapticFeedbackType type = HapticFeedbackType.Selection)
    {
        if (!enableHaptics) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

            switch (type)
            {
                case HapticFeedbackType.Selection:
                    vibrator.Call("vibrate", 10L); // 10ms
                    break;
                case HapticFeedbackType.LightImpact:
                    vibrator.Call("vibrate", 20L); // 20ms
                    break;
                case HapticFeedbackType.MediumImpact:
                    vibrator.Call("vibrate", 40L); // 40ms
                    break;
                case HapticFeedbackType.HeavyImpact:
                    vibrator.Call("vibrate", 60L); // 60ms
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Haptic feedback failed: {e.Message}");
        }
#elif UNITY_IOS && !UNITY_EDITOR
        // iOS haptic feedback would go here
        // Requires iOS native plugin for proper haptic engine access
        Handheld.Vibrate();
#endif
    }

    /// <summary>
    /// Check if device supports haptic feedback
    /// </summary>
    public static bool SupportsHaptics()
    {
#if UNITY_ANDROID || UNITY_IOS
        return SystemInfo.supportsVibration;
#else
        return false;
#endif
    }

    /// <summary>
    /// Check if running on mobile device
    /// </summary>
    public static bool IsMobileDevice()
    {
        return Application.isMobilePlatform;
    }

    /// <summary>
    /// Get touch count
    /// </summary>
    public static int GetTouchCount()
    {
        return Touch.activeFingers.Count;
    }
}

public enum HapticFeedbackType
{
    Selection,      // Light tap (10ms)
    LightImpact,    // Light impact (20ms)
    MediumImpact,   // Medium impact (40ms)
    HeavyImpact     // Heavy impact (60ms)
}
