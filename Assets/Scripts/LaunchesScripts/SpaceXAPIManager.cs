using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SpaceX
{
    public class SpaceXAPIManager : MonoBehaviour
    {
        private static SpaceXAPIManager _instance;
        public static SpaceXAPIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SpaceXAPIManager");
                    _instance = go.AddComponent<SpaceXAPIManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private const string BASE_URL = "https://api.spacexdata.com/v4";
        private const float CACHE_LIFETIME = 600f; //10 minutes

        private Dictionary<string, CacheEntry<object>> cache = new Dictionary<string, CacheEntry<object>>();

        //clean expired when accessing cache
        private T GetFromCache<T>(string cacheKey) where T : class
        {
            if (cache.ContainsKey(cacheKey))
            {
                if (cache[cacheKey].IsValid())
                {
                    return cache[cacheKey].data as T;
                }
                else
                {
                    cache.Remove(cacheKey);
                }
            }
            return null;
        }

        //add to cache with automatic cleanup
        private void AddToCache(string cacheKey, object data)
        {
            cache[cacheKey] = new CacheEntry<object>
            {
                data = data,
                timestamp = DateTime.UtcNow,
                cacheLifetime = CACHE_LIFETIME
            };
        }

        //fetch all launches
        public IEnumerator GetLaunches(Action<List<Launch>> onSuccess, Action<string> onError)
        {
            string url = $"{BASE_URL}/launches";
            string cacheKey = "launches";

            var cachedData = GetFromCache<List<Launch>>(cacheKey);
            if (cachedData != null)
            {
                onSuccess?.Invoke(cachedData);
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string json = "{\"docs\":" + request.downloadHandler.text + "}";
                        LaunchesResponse response = JsonUtility.FromJson<LaunchesResponse>(json);
                        AddToCache(cacheKey, response.docs);

                        onSuccess?.Invoke(response.docs);
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"Parse error: {e.Message}");
                    }
                }
                else
                {
                    onError?.Invoke($"Request error: {request.error}");
                }
            }
        }

        //fetch specific rocket
        public IEnumerator GetRocket(string rocketId, Action<Rocket> onSuccess, Action<string> onError)
        {
            if (string.IsNullOrEmpty(rocketId))
            {
                onError?.Invoke("Invalid rocket ID");
                yield break;
            }

            string url = $"{BASE_URL}/rockets/{rocketId}";
            string cacheKey = $"rocket_{rocketId}";

            var cachedData = GetFromCache<Rocket>(cacheKey);
            if (cachedData != null)
            {
                onSuccess?.Invoke(cachedData);
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        Rocket rocket = JsonUtility.FromJson<Rocket>(request.downloadHandler.text);
                        AddToCache(cacheKey, rocket);
                        onSuccess?.Invoke(rocket);
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"Parse error: {e.Message}");
                    }
                }
                else
                {
                    onError?.Invoke($"Request error: {request.error}");
                }
            }
        }

        //fetch specific ship
        public IEnumerator GetShip(string shipId, Action<Ship> onSuccess, Action<string> onError)
        {
            string url = $"{BASE_URL}/ships/{shipId}";
            string cacheKey = $"ship_{shipId}";

            var cachedData = GetFromCache<Ship>(cacheKey);
            if (cachedData != null)
            {
                onSuccess?.Invoke(cachedData);
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        Ship ship = JsonUtility.FromJson<Ship>(request.downloadHandler.text);
                        AddToCache(cacheKey, ship);
                        onSuccess?.Invoke(ship);
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"Parse error: {e.Message}");
                    }
                }
                else
                {
                    onError?.Invoke($"Request error: {request.error}");
                }
            }
        }

        //fetch multiple ships
        public IEnumerator GetShips(List<string> shipIds, Action<List<Ship>> onSuccess, Action<string> onError)
        {
            List<Ship> ships = new List<Ship>();

            foreach (string shipId in shipIds)
            {
                bool shipFetched = false;
                string shipError = null;

                yield return GetShip(shipId,
                    ship => { ships.Add(ship); shipFetched = true; },
                    error => { shipError = error; shipFetched = true; }
                );

                float timeout = 0f;
                while (!shipFetched && timeout < 5f)
                {
                    timeout += Time.deltaTime;
                    yield return null;
                }

                if (!shipFetched)
                {
                    Debug.LogError($"Timeout fetching ship {shipId}");
                }
            }

            onSuccess?.Invoke(ships);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                ClearCache();
            }
        }

        public void ClearCache()
        {
            cache.Clear();
        }
    }
}