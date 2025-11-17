using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Attach to dice prefab to automatically detect which face is pointing up
/// Supports standard 6-sided dice with configurable face orientations
/// </summary>
public class DiceFace : MonoBehaviour
{
    [System.Serializable]
    public class Face
    {
        [Tooltip("Local direction this face points (e.g., Vector3.up for top face)")]
        public Vector3 localDirection = Vector3.up;

        [Tooltip("Value shown on this face (1-6)")]
        [Range(1, 6)]
        public int value = 1;

        public Face(Vector3 direction, int val)
        {
            localDirection = direction.normalized;
            value = val;
        }
    }

    [Header("Dice Face Configuration")]
    [Tooltip("Define all 6 faces of the dice with their local directions and values")]
    [SerializeField] private Face[] faces = new Face[]
    {
        new Face(Vector3.up, 1),        // Top face = 1
        new Face(Vector3.down, 6),      // Bottom face = 6
        new Face(Vector3.forward, 2),   // Front face = 2
        new Face(Vector3.back, 5),      // Back face = 5
        new Face(Vector3.right, 3),     // Right face = 3
        new Face(Vector3.left, 4)       // Left face = 4
    };

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private float gizmoLength = 0.5f;

    /// <summary>
    /// Gets the value of the face currently pointing up
    /// </summary>
    public int GetTopFaceValue()
    {
        if (faces == null || faces.Length == 0)
        {
            Debug.LogWarning("DiceFace: No faces configured! Returning random value.");
            return Random.Range(1, 7);
        }

        float maxDot = -1f;
        int topValue = 1;

        foreach (var face in faces)
        {
            // Transform local direction to world space
            Vector3 worldDirection = transform.TransformDirection(face.localDirection.normalized);

            // Calculate how aligned this face is with world up
            float dot = Vector3.Dot(worldDirection, Vector3.up);

            if (dot > maxDot)
            {
                maxDot = dot;
                topValue = face.value;
            }
        }

        return topValue;
    }

    /// <summary>
    /// Gets the value of the face pointing in a specific world direction
    /// </summary>
    public int GetFaceValueInDirection(Vector3 worldDirection)
    {
        if (faces == null || faces.Length == 0)
        {
            return Random.Range(1, 7);
        }

        float maxDot = -1f;
        int faceValue = 1;

        foreach (var face in faces)
        {
            Vector3 worldFaceDir = transform.TransformDirection(face.localDirection.normalized);
            float dot = Vector3.Dot(worldFaceDir, worldDirection.normalized);

            if (dot > maxDot)
            {
                maxDot = dot;
                faceValue = face.value;
            }
        }

        return faceValue;
    }

    /// <summary>
    /// Sets up standard dice face configuration
    /// Opposite faces add up to 7 (1-6, 2-5, 3-4)
    /// </summary>
    public void SetupStandardDice()
    {
        faces = new Face[]
        {
            new Face(Vector3.up, 1),
            new Face(Vector3.down, 6),
            new Face(Vector3.forward, 2),
            new Face(Vector3.back, 5),
            new Face(Vector3.right, 3),
            new Face(Vector3.left, 4)
        };
    }

    /// <summary>
    /// Auto-configures faces by checking which face has which number
    /// Place dice with each number face-up and call this for each
    /// </summary>
    public void AutoDetectCurrentFace(int valueToAssign)
    {
        // Find which local direction is currently pointing up
        Vector3[] directions = new Vector3[]
        {
            Vector3.up, Vector3.down,
            Vector3.forward, Vector3.back,
            Vector3.right, Vector3.left
        };

        float maxDot = -1f;
        Vector3 bestDirection = Vector3.up;

        foreach (var localDir in directions)
        {
            Vector3 worldDir = transform.TransformDirection(localDir);
            float dot = Vector3.Dot(worldDir, Vector3.up);

            if (dot > maxDot)
            {
                maxDot = dot;
                bestDirection = localDir;
            }
        }

        // Update or add this face configuration
        bool found = false;
        for (int i = 0; i < faces.Length; i++)
        {
            if (Vector3.Dot(faces[i].localDirection, bestDirection) > 0.99f)
            {
                faces[i].value = valueToAssign;
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.Log($"Detected direction: {bestDirection} for value {valueToAssign}");
        }
    }

    /// <summary>
    /// Validates that all faces have unique values 1-6
    /// </summary>
    public bool ValidateConfiguration()
    {
        if (faces == null || faces.Length != 6)
        {
            Debug.LogWarning("DiceFace must have exactly 6 faces configured!");
            return false;
        }

        bool[] usedValues = new bool[7]; // Index 0 unused, 1-6 for dice values

        foreach (var face in faces)
        {
            if (face.value < 1 || face.value > 6)
            {
                Debug.LogWarning($"DiceFace has invalid value: {face.value}. Must be 1-6.");
                return false;
            }

            if (usedValues[face.value])
            {
                Debug.LogWarning($"DiceFace has duplicate value: {face.value}");
                return false;
            }

            usedValues[face.value] = true;
        }

        // Check all values 1-6 are present
        for (int i = 1; i <= 6; i++)
        {
            if (!usedValues[i])
            {
                Debug.LogWarning($"DiceFace missing value: {i}");
                return false;
            }
        }

        return true;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || faces == null) return;

        Color[] faceColors = new Color[]
        {
            Color.white,    // 1
            Color.yellow,   // 2
            Color.green,    // 3
            Color.cyan,     // 4
            Color.blue,     // 5
            Color.red       // 6
        };

        foreach (var face in faces)
        {
            Vector3 worldDir = transform.TransformDirection(face.localDirection.normalized);
            Vector3 start = transform.position;
            Vector3 end = start + worldDir * gizmoLength;

            // Color based on value
            Gizmos.color = faceColors[(face.value - 1) % faceColors.Length];
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.05f);

            // Label
            #if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Gizmos.color;
            style.fontSize = 14;
            style.fontStyle = FontStyle.Bold;
            Handles.Label(end, face.value.ToString(), style);
            #endif
        }

        // Highlight the top face
        int topValue = GetTopFaceValue();
        foreach (var face in faces)
        {
            if (face.value == topValue)
            {
                Vector3 worldDir = transform.TransformDirection(face.localDirection.normalized);
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position + worldDir * gizmoLength, 0.1f);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Show current top value
        if (faces != null && faces.Length > 0)
        {
            int topValue = GetTopFaceValue();
            #if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.yellow;
            style.fontSize = 24;
            style.fontStyle = FontStyle.Bold;
            Handles.Label(transform.position + Vector3.up * (gizmoLength + 0.3f),
                         $"Top: {topValue}", style);
            #endif
        }
    }
#endif
}

#if UNITY_EDITOR
/// <summary>
/// Custom editor for DiceFace with helper buttons
/// </summary>
[CustomEditor(typeof(DiceFace))]
public class DiceFaceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DiceFace diceFace = (DiceFace)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Setup", EditorStyles.boldLabel);

        if (GUILayout.Button("Setup Standard Dice"))
        {
            diceFace.SetupStandardDice();
            EditorUtility.SetDirty(diceFace);
        }

        if (GUILayout.Button("Validate Configuration"))
        {
            bool valid = diceFace.ValidateConfiguration();
            if (valid)
            {
                EditorUtility.DisplayDialog("Validation", "Dice configuration is valid!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation", "Dice configuration has errors! Check console.", "OK");
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Auto-Configuration", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "To auto-configure:\n" +
            "1. Place dice with face '1' pointing up\n" +
            "2. Click 'Detect Face 1'\n" +
            "3. Rotate dice to show face '2' up\n" +
            "4. Click 'Detect Face 2'\n" +
            "5. Repeat for all faces 1-6",
            MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        for (int i = 1; i <= 3; i++)
        {
            if (GUILayout.Button($"Detect Face {i}"))
            {
                diceFace.AutoDetectCurrentFace(i);
                EditorUtility.SetDirty(diceFace);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        for (int i = 4; i <= 6; i++)
        {
            if (GUILayout.Button($"Detect Face {i}"))
            {
                diceFace.AutoDetectCurrentFace(i);
                EditorUtility.SetDirty(diceFace);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Current Reading", EditorStyles.boldLabel);

        if (Application.isPlaying || Selection.activeGameObject == diceFace.gameObject)
        {
            int topValue = diceFace.GetTopFaceValue();
            EditorGUILayout.LabelField($"Top Face Value: {topValue}", EditorStyles.boldLabel);
        }
        else
        {
            EditorGUILayout.HelpBox("Select or enter Play mode to see current top face", MessageType.Info);
        }
    }
}
#endif
