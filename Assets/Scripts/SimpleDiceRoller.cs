using UnityEngine;
using System.Collections;

/// <summary>
/// Simple dice roller that can work with 3D dice models
/// Supports both visual dice objects and simple number generation
/// </summary>
public class SimpleDiceRoller : MonoBehaviour
{
    [Header("Dice Visual (Optional)")]
    [SerializeField] private GameObject diceModel;
    [SerializeField] private Transform diceSpawnPoint;

    [Header("Animation Settings")]
    [SerializeField] private float rollDuration = 1f;
    [SerializeField] private float spinSpeed = 720f;
    [SerializeField] private bool usePhysics = false;

    [Header("Dice Display")]
    [SerializeField] private Vector3 displayPosition = new Vector3(0, 1, 0);
    [SerializeField] private Vector3 displayRotation = new Vector3(0, 45, 0);

    private GameObject currentDice;
    private bool isRolling = false;

    /// <summary>
    /// Roll the dice and return result (1-6)
    /// </summary>
    public IEnumerator RollDice(System.Action<int> onComplete)
    {
        if (isRolling)
        {
            onComplete?.Invoke(Random.Range(1, 7));
            yield break;
        }

        isRolling = true;
        int result = Random.Range(1, 7);

        if (diceModel != null)
        {
            yield return StartCoroutine(AnimateDiceRoll(result));
        }
        else
        {
            // Simple delay without visual
            yield return new WaitForSeconds(0.5f);
        }

        isRolling = false;
        onComplete?.Invoke(result);
    }

    IEnumerator AnimateDiceRoll(int result)
    {
        // Clear previous dice
        if (currentDice != null)
        {
            Destroy(currentDice);
        }

        // Spawn dice
        Vector3 spawnPos = diceSpawnPoint != null ? diceSpawnPoint.position : displayPosition;
        currentDice = Instantiate(diceModel, spawnPos, Quaternion.identity);

        if (usePhysics && currentDice.GetComponent<Rigidbody>() != null)
        {
            // Use physics-based roll
            Rigidbody rb = currentDice.GetComponent<Rigidbody>();
            rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * spinSpeed, ForceMode.Impulse);

            yield return new WaitForSeconds(rollDuration);

            // Stop physics
            rb.isKinematic = true;
        }
        else
        {
            // Simple spin animation
            float elapsed = 0f;
            Quaternion startRot = currentDice.transform.rotation;

            while (elapsed < rollDuration)
            {
                elapsed += Time.deltaTime;
                float spin = spinSpeed * Time.deltaTime;
                currentDice.transform.Rotate(Vector3.right * spin, Space.World);
                currentDice.transform.Rotate(Vector3.up * spin * 0.7f, Space.World);

                yield return null;
            }
        }

        // Set final rotation to show result
        SetDiceToShowNumber(currentDice, result);

        // Move to display position
        currentDice.transform.position = displayPosition;
    }

    /// <summary>
    /// Set dice rotation to show a specific number
    /// Note: This is a simple rotation - for proper face display,
    /// you'll need to customize based on your dice model
    /// </summary>
    void SetDiceToShowNumber(GameObject dice, int number)
    {
        // Default rotations for standard dice (customize for your model)
        Quaternion[] faceRotations = new Quaternion[]
        {
            Quaternion.Euler(0, 0, 0),      // 1 (front)
            Quaternion.Euler(0, 180, 0),    // 2 (back)
            Quaternion.Euler(0, 90, 0),     // 3 (right)
            Quaternion.Euler(0, -90, 0),    // 4 (left)
            Quaternion.Euler(90, 0, 0),     // 5 (top)
            Quaternion.Euler(-90, 0, 0)     // 6 (bottom)
        };

        if (number >= 1 && number <= 6)
        {
            dice.transform.rotation = faceRotations[number - 1] * Quaternion.Euler(displayRotation);
        }
    }

    /// <summary>
    /// Clear the dice visual
    /// </summary>
    public void ClearDice()
    {
        if (currentDice != null)
        {
            Destroy(currentDice);
        }
    }

    public bool IsRolling => isRolling;
}
