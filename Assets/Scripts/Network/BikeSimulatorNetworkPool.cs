using System.Collections.Generic;
using Fusion;
using PathologicalGames;
using UnityEngine;

namespace Network
{
    public class BikeSimulatorNetworkPool : MonoBehaviour, INetworkObjectPool
    {
        private Dictionary<string, string> _poolsByPrefab = new Dictionary<string, string>();
        private Dictionary<NetworkObject, SpawnPool> _poolsByInstance = new Dictionary<NetworkObject, SpawnPool>();
        
        
        public void RegisterNetworkedPrefab(string prefabName, string poolName)
        {
            if (_poolsByPrefab.ContainsKey(prefabName))
                return;
            _poolsByPrefab[prefabName] = poolName;
        }

        public NetworkObject AcquireInstance(NetworkRunner runner, NetworkPrefabInfo info)
        {
            NetworkObject prefab;
            if (NetworkProjectConfig.Global.PrefabTable.TryGetPrefab(info.Prefab, out prefab))
            {
                SpawnPool pool = GetPool(prefab);
                if (!pool)
                {
                    return Instantiate(prefab, Vector3.zero, Quaternion.identity);
                }
                NetworkObject newt = pool.Spawn(prefab.transform).GetComponent<NetworkObject>();
                if (!_poolsByInstance.ContainsKey(newt))
                {
                    _poolsByInstance[newt] = pool;
                }
                return newt;
            }

            UnityEngine.Debug.LogError("No prefab for " + info.Prefab);
            return null;
        }

        public void ReleaseInstance(NetworkRunner runner, NetworkObject instance, bool isSceneObject)
        {
            if (instance != null)
            {
                if (_poolsByInstance.TryGetValue(instance, out SpawnPool pool))
                {
                    pool.Despawn(instance.transform);
                }
                else
                {
                    instance.gameObject.SetActive(false); // Should always disable before re-parenting, or we will dirty it twice
                    instance.transform.SetParent(null, false);
                    Destroy(instance.gameObject);
                }
            }
        }
        
        public SpawnPool GetPool<T>(T prefab) where T : NetworkObject
        {
            if (!_poolsByPrefab.TryGetValue(prefab.name, out string poolName))
            {
//                UnityEngine.Debug.LogError($"Prefab: {prefab.name} not network pooled!");
                return null;
            }

            return PoolManager.Pools[poolName];
        }

        public void ClearPools()
        {
            foreach (string poolName in _poolsByPrefab.Values)
            {
                SpawnPool pool = PoolManager.Pools[poolName];
                //pool.Clear();
            }

            foreach (SpawnPool pool in _poolsByInstance.Values)
            {
                //pool.Clear();
            }

            _poolsByPrefab = new Dictionary<string, string>();
        }
    }
}