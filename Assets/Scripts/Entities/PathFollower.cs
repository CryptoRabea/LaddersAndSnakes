using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LAS.Entities
{
    public class PathFollower : MonoBehaviour
    {
        public IEnumerator MoveAlongPath(Transform piece, List<Vector3> positions, float speed, AnimationCurve curve, Action onComplete = null)
        {
            if (positions == null || positions.Count == 0) { onComplete?.Invoke(); yield break; }
            int segment = 0;
            while (segment < positions.Count - 1)
            {
                Vector3 a = positions[segment]; Vector3 b = positions[segment + 1];
                float segLen = Vector3.Distance(a, b); float segTime = segLen / speed; float t = 0f;
                while (t < segTime)
                {
                    t += Time.deltaTime; float norm = Mathf.Clamp01(t / segTime); float curved = curve.Evaluate(norm);
                    piece.position = Vector3.Lerp(a, b, curved);
                    yield return null;
                }
                segment++;
            }
            piece.position = positions[positions.Count - 1]; onComplete?.Invoke();
        }
    }
}