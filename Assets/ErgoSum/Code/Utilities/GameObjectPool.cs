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
                obj = _pool.Dequeue();
            } else if (_active.Count < poolSize) {
                obj = GameObject.Instantiate(_prefab);
                obj.OnDisableAsObservable().Subscribe(_ => { _pool.Enqueue(obj); });
                _active.Enqueue(obj);
            } else {
                obj = _active.Dequeue();
                _active.Enqueue(obj);
            }
            return obj;
        }
    }
}