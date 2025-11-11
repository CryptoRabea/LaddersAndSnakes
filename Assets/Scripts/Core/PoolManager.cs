using System.Collections.Generic;
using UnityEngine;


namespace LAS.Core
{
    public class PoolManager
    {
        readonly Dictionary<GameObject, Queue<GameObject>> pool = new Dictionary<GameObject, Queue<GameObject>>();
        readonly Transform root;
        public PoolManager(Transform root = null) { this.root = root ?? new GameObject("Pools").transform; }
        public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            if (!pool.TryGetValue(prefab, out var q)) { q = new Queue<GameObject>(); pool.Add(prefab, q); }
            GameObject go;
            if (q.Count > 0) { go = q.Dequeue(); go.SetActive(true); }
            else { go = GameObject.Instantiate(prefab, root); }
            go.transform.position = pos; go.transform.rotation = rot;
            var poolable = go.GetComponent<IPoolable>(); poolable?.OnSpawn();
            return go;
        }
        public void Despawn(GameObject prefab, GameObject instance)
        {
            instance.SetActive(false); var poolable = instance.GetComponent<IPoolable>(); poolable?.OnDespawn();
            if (!pool.TryGetValue(prefab, out var q)) { q = new Queue<GameObject>(); pool.Add(prefab, q); }
            q.Enqueue(instance);
        }
    }
}