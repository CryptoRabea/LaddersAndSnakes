using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manual dice roller with hold-to-shake, release-to-throw mechanics
/// Place this on a UI Button or GameObject in your scene
/// </summary>
public class ManualDiceRoller : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Dice Setup")]
    [SerializeField] private GameObject dicePrefab;
    [SerializeField] private Transform diceThrowPosition;
    [SerializeField] private int numberOfDice = 1; // 1 or 2 dice

    [Header("Shake Settings")]
    [SerializeField] private float shakeIntensity = 0.5f;
    [SerializeField] private float shakeSpeed = 20f;

    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 5f;
    [SerializeField] private float throwTorque = 100f;
    [SerializeField] private float settleTime = 2f;

    [Header("Physics")]
    [SerializeField] private bool usePhysics = true;
    [SerializeField] private LayerMask groundLayer;

    private List<GameObject> currentDice = new List<GameObject>();
    private bool isHolding = false;
    private bool isRolling = false;
    private System.Action<int> onRollComplete;

    void Update()
    {
        if (isHolding && currentDice.Count > 0)
        {
            // Shake dice while holding
            foreach (var dice in currentDice)
            {
                if (dice != null)
                {
                    float shakeX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
                    float shakeY = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeIntensity;
                    float shakeZ = Mathf.Sin(Time.time * shakeSpeed * 0.7f) * shakeIntensity;

                    dice.transform.localRotation = Quaternion.Euler(shakeX * 30, shakeY * 30, shakeZ * 30);
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isRolling)
        {
            StartHolding();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isHolding)
        {
            ThrowDice();
        }
    }

    /// <summary>
    /// Call this to roll dice (can also be called from button OnClick)
    /// </summary>
    public void RollDice(System.Action<int> callback)
    {
        if (isRolling) return;

        onRollComplete = callback;
        StartHolding();

        // Auto-throw after short delay if not using pointer events
        if (!isHolding)
        {
            Invoke(nameof(ThrowDice), 0.5f);
        }
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
            if (dicePrefab == null) continue;

            Vector3 offset = new Vector3(i * 0.6f - (numberOfDice - 1) * 0.3f, 0, 0);
            GameObject dice = Instantiate(dicePrefab, spawnPos + offset, Quaternion.identity);

            // Disable physics while holding
            Rigidbody rb = dice.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
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
                rb.isKinematic = false;

                // Apply throw force
                Vector3 randomDir = new Vector3(
                    Random.Range(-0.3f, 0.3f),
                    1f,
                    Random.Range(0.5f, 1f)
                ).normalized;

                rb.AddForce(randomDir * throwForce, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * throwTorque);
            }
        }

        StartCoroutine(WaitForSettle());
    }

    IEnumerator ThrowWithAnimation()
    {
        float elapsed = 0f;
        float duration = 1f;

        foreach (var dice in currentDice)
        {
            if (dice == null) continue;

            Vector3 startPos = dice.transform.position;
            Vector3 endPos = startPos + new Vector3(Random.Range(-1f, 1f), -1f, Random.Range(1f, 2f));

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                dice.transform.position = Vector3.Lerp(startPos, endPos, t);
                dice.transform.Rotate(Random.insideUnitSphere * 360 * Time.deltaTime);

                yield return null;
            }
        }

        yield return new WaitForSeconds(0.5f);

        int total = ReadDiceValues();
        onRollComplete?.Invoke(total);
        isRolling = false;
    }

    IEnumerator WaitForSettle()
    {
        yield return new WaitForSeconds(settleTime);

        int total = ReadDiceValues();
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
                total += diceFace.GetTopFaceValue();
            }
            else
            {
                // Random value if no DiceFace component
                total += Random.Range(1, 7);
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

    public void SetNumberOfDice(int count)
    {
        numberOfDice = Mathf.Clamp(count, 1, 2);
    }

    void OnDestroy()
    {
        ClearDice();
    }
}
