using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum.Utilities {
    [Serializable]
    public class GameObjectPool {
        [SerializeField]private GameObject _prefab;
        [SerializeField]private int poolSize = 1;
        private Queue<GameObject> _pool = new Queue<GameObject>();
        private Queue<GameObject> _active = new Queue<GameObject>();
        public GameObject Get() {
            GameObject obj;
            if (_pool.Count > 0) {
                // If an inactive object exists on the queue, grab it
                obj = _pool.Dequeue();
            } else if (_active.Count < poolSize) {
                // If there's still room in the active queue, create a new instance and push it on
                obj = GameObject.Instantiate(_prefab);
                obj.OnDisableAsObservable().Subscribe(_ => { _pool.Enqueue(obj); });
            } else {
                // Last resort, pull the oldest object from the active queue
                obj = _active.Dequeue();
            }
            _active.Enqueue(obj);
            return obj;
        }
    }
}