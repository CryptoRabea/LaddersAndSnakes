using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manual dice roller with hold-to-shake, release-to-throw mechanics
/// Uses Unity New Input System for mobile and desktop support
/// Supports touch, mouse, and keyboard input with haptic feedback
/// </summary>
public class ManualDiceRoller : MonoBehaviour
{
    [Header("Dice Setup")]
    [SerializeField] private GameObject dicePrefab;
    [SerializeField] private Transform diceThrowPosition;
    [SerializeField] private int numberOfDice = 1; // 1 or 2 dice

    [Header("Shake Settings")]
    [SerializeField] private float shakeIntensity = 0.5f;
    [SerializeField] private float shakeSpeed = 20f;

    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 8f;
    [SerializeField] private float throwTorque = 300f;
    [SerializeField] private float settleTime = 2.5f;
    [SerializeField] private Vector3 throwDirection = new Vector3(0, -0.3f, 1f);

    [Header("Physics")]
    [SerializeField] private bool usePhysics = true;
    [SerializeField] private float drag = 0.5f;
    [SerializeField] private float angularDrag = 0.5f;

    [Header("Mobile Optimization")]
    [SerializeField] private bool enableHapticFeedback = true;
    [SerializeField] private bool enableSwipeToRoll = false;

    private List<GameObject> currentDice = new List<GameObject>();
    private bool isHolding = false;
    private bool isRolling = false;
    private System.Action<int> onRollComplete;
    private MobileInputHelper mobileInput;

    // New Input System actions
    private InputAction dicePressAction;
    private InputAction touchPositionAction;
    private InputAction alternateRollAction;

    void Awake()
    {
        // Setup mobile input helper
        mobileInput = gameObject.AddComponent<MobileInputHelper>();

        if (enableSwipeToRoll)
        {
            mobileInput.OnSwipeUp += OnSwipeDetected;
        }
    }

    void OnEnable()
    {
        // Initialize Input Actions
        if (dicePressAction == null)
        {
            dicePressAction = new InputAction("DicePress", binding: "<Touchscreen>/primaryTouch/press");
            dicePressAction.AddBinding("<Mouse>/leftButton");
        }

        if (touchPositionAction == null)
        {
            touchPositionAction = new InputAction("TouchPosition", binding: "<Touchscreen>/primaryTouch/position");
            touchPositionAction.AddBinding("<Mouse>/position");
        }

        if (alternateRollAction == null)
        {
            alternateRollAction = new InputAction("AlternateRoll", binding: "<Keyboard>/space");
        }

        // Setup callbacks
        dicePressAction.started += OnDicePressStarted;
        dicePressAction.canceled += OnDicePressCanceled;
        alternateRollAction.performed += OnAlternateRoll;

        // Enable actions
        dicePressAction.Enable();
        touchPositionAction.Enable();
        alternateRollAction.Enable();
    }

    void OnDisable()
    {
        // Cleanup callbacks
        if (dicePressAction != null)
        {
            dicePressAction.started -= OnDicePressStarted;
            dicePressAction.canceled -= OnDicePressCanceled;
            dicePressAction.Disable();
        }

        if (alternateRollAction != null)
        {
            alternateRollAction.performed -= OnAlternateRoll;
            alternateRollAction.Disable();
        }

        if (touchPositionAction != null)
        {
            touchPositionAction.Disable();
        }
    }

    void Update()
    {
        if (!isHolding) return;  // prevents shaking after release
        if (currentDice.Count == 0) return;

        foreach (var dice in currentDice)
        {
            if (dice == null) continue;

            float shakeX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
            float shakeY = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeIntensity;
            float shakeZ = Mathf.Sin(Time.time * shakeSpeed * 0.7f) * shakeIntensity;

            dice.transform.localRotation = Quaternion.Euler(shakeX * 30, shakeY * 30, shakeZ * 30);
        }
    }

    // New Input System callbacks
    private void OnDicePressStarted(InputAction.CallbackContext context)
    {
        if (!isRolling && !isHolding)
        {
            StartHolding();

            // Haptic feedback on press
            if (enableHapticFeedback && mobileInput != null)
            {
                mobileInput.TriggerHapticFeedback(HapticFeedbackType.LightImpact);
            }
        }
    }

    private void OnDicePressCanceled(InputAction.CallbackContext context)
    {
        if (isHolding)
        {
            ThrowDice();

            // Haptic feedback on throw
            if (enableHapticFeedback && mobileInput != null)
            {
                mobileInput.TriggerHapticFeedback(HapticFeedbackType.MediumImpact);
            }
        }
    }

    private void OnAlternateRoll(InputAction.CallbackContext context)
    {
        if (!isRolling && !isHolding)
        {
            StartCoroutine(AutoRollSequence());
        }
    }

    private void OnSwipeDetected()
    {
        if (!isRolling && !isHolding)
        {
            StartCoroutine(AutoRollSequence());
        }
    }

    /// <summary>
    /// Call this to roll dice (for non-interactive rolling)
    /// </summary>
    public void RollDice(System.Action<int> callback)
    {
        if (isRolling) return;

        onRollComplete = callback;
        StartCoroutine(AutoRollSequence());
    }

    IEnumerator AutoRollSequence()
    {
        StartHolding();
        yield return new WaitForSeconds(0.5f);
        ThrowDice();
    }

    void StartHolding()
    {
        if (isRolling) return;

        isHolding = true;
        ClearDice();
        SpawnDice();
    }

    void SpawnDice()
    {
        currentDice.Clear();

        Vector3 spawnPos = diceThrowPosition != null ? diceThrowPosition.position : transform.position;

        for (int i = 0; i < numberOfDice; i++)
        {
            if (dicePrefab == null)
            {
                Debug.LogError("ManualDiceRoller: Dice prefab is not assigned!");
                continue;
            }

            Vector3 offset = new Vector3(i * 0.6f - (numberOfDice - 1) * 0.3f, 0, 0);
            GameObject dice = Instantiate(dicePrefab, spawnPos + offset, Random.rotation);

            // Setup or verify Rigidbody
            Rigidbody rb = dice.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = dice.AddComponent<Rigidbody>();
            }

            // Configure Rigidbody
            rb.mass = 0.1f;
            rb.linearDamping = drag;
            rb.angularDamping = angularDrag;
            rb.useGravity = true;
            rb.isKinematic = true; // Start kinematic while holding

            // Ensure collider exists
            if (dice.GetComponent<Collider>() == null)
            {
                BoxCollider collider = dice.AddComponent<BoxCollider>();
                Debug.LogWarning("ManualDiceRoller: Added missing collider to dice!");
            }

            currentDice.Add(dice);
        }
    }

    void ThrowDice()
    {
        if (!isHolding || currentDice.Count == 0) return;

        isHolding = false;
        isRolling = true;



        if (usePhysics)
        {
            ThrowWithPhysics();
        }
        else
        {
            StartCoroutine(ThrowWithAnimation());
        }
    }

    void ThrowWithPhysics()
    {
        foreach (var dice in currentDice)
        {
            if (dice == null) continue;

            Rigidbody rb = dice.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Enable physics
                rb.isKinematic = false;
                rb.useGravity = true;

                // Calculate throw direction with randomness
                Vector3 baseDir = throwDirection.normalized;
                Vector3 randomDir = new Vector3(
                    baseDir.x + Random.Range(-0.2f, 0.2f),
                    baseDir.y + Random.Range(-0.1f, 0.2f),
                    baseDir.z + Random.Range(-0.2f, 0.2f)
                ).normalized;

                // Apply force
                rb.linearVelocity = Vector3.zero; // Clear any existing velocity
                rb.angularVelocity = Vector3.zero;
                rb.AddForce(randomDir * throwForce, ForceMode.VelocityChange);

                // Apply torque for spin
                Vector3 randomTorque = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)
                ).normalized * throwTorque;
                rb.AddTorque(randomTorque, ForceMode.VelocityChange);

                Debug.Log($"Dice thrown with force: {randomDir * throwForce}, torque: {randomTorque}");
            }
            else
            {
                Debug.LogError("ManualDiceRoller: Dice has no Rigidbody!");
            }
        }

        StartCoroutine(WaitForSettle());
    }

    IEnumerator ThrowWithAnimation()
    {
        float duration = 1.5f;
        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> endPositions = new List<Vector3>();

        foreach (var dice in currentDice)
        {
            if (dice == null) continue;

            startPositions.Add(dice.transform.position);
            Vector3 endPos = dice.transform.position + new Vector3(
                Random.Range(-1f, 1f),
                -2f,
                Random.Range(1f, 3f)
            );
            endPositions.Add(endPos);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curve = Mathf.Sin(t * Mathf.PI); // Arc trajectory

            for (int i = 0; i < currentDice.Count; i++)
            {
                if (currentDice[i] == null) continue;

                Vector3 pos = Vector3.Lerp(startPositions[i], endPositions[i], t);
                pos.y += curve * 1f; // Add arc height
                currentDice[i].transform.position = pos;
                currentDice[i].transform.Rotate(Random.insideUnitSphere * 720 * Time.deltaTime);
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        int total = ReadDiceValues();
        onRollComplete?.Invoke(total);
        isRolling = false;
    }

    IEnumerator WaitForSettle()
    {
        // Wait for dice to settle
        yield return new WaitForSeconds(settleTime);

        // Stop all dice movement
        foreach (var dice in currentDice)
        {
            if (dice == null) continue;

            Rigidbody rb = dice.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true; // Freeze in place
            }
        }

        // Read values and complete
        int total = ReadDiceValues();
        Debug.Log($"Dice settled. Total value: {total}");

        onRollComplete?.Invoke(total);
        isRolling = false;
    }

    int ReadDiceValues()
    {
        int total = 0;

        foreach (var dice in currentDice)
        {
            if (dice == null) continue;

            // Try to read value from DiceFace component
            DiceFace diceFace = dice.GetComponent<DiceFace>();
            if (diceFace != null)
            {
                int value = diceFace.GetTopFaceValue();
                total += value;
                Debug.Log($"Dice face value: {value}");
            }
            else
            {
                // Random value if no DiceFace component
                int value = Random.Range(1, 7);
                total += value;
                Debug.LogWarning($"DiceFace component missing! Using random value: {value}");
            }
        }

        return total;
    }

    public void ClearDice()
    {
        foreach (var dice in currentDice)
        {
            if (dice != null)
            {
                Destroy(dice);
            }
        }
        currentDice.Clear();
    }

    public bool IsRolling() => isRolling;
    public bool IsHolding() => isHolding;

    public void SetNumberOfDice(int count)
    {
        numberOfDice = Mathf.Clamp(count, 1, 2);
    }

    void OnDestroy()
    {
        ClearDice();

        // Cleanup input actions
        if (dicePressAction != null)
        {
            dicePressAction.Dispose();
        }

        if (touchPositionAction != null)
        {
            touchPositionAction.Dispose();
        }

        if (alternateRollAction != null)
        {
            alternateRollAction.Dispose();
        }
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        if (diceThrowPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(diceThrowPosition.position, 0.2f);

            // Show throw direction
            Gizmos.color = Color.red;
            Vector3 throwDir = throwDirection.normalized * 2f;
            Gizmos.DrawRay(diceThrowPosition.position, throwDir);
        }
    }
}





