using System;
using System.Collections.Generic;

namespace SpaceX
{
    [Serializable]
    public class Launch
    {
        public string id;
        public string name;
        public bool upcoming;
        public string date_utc;
        public string rocket;
        public List<string> ships;
        public List<string> payloads;
        public bool success;
        public string details;

        public DateTime GetLaunchDate()
        {
            if (DateTime.TryParse(date_utc, out DateTime result))
                return result;
            return DateTime.MinValue;
        }

        public bool IsUpcoming()
        {
            return upcoming || GetLaunchDate() > DateTime.UtcNow;
        }
    }

    [Serializable]
    public class Rocket
    {
        public string id;
        public string name;
        public string country;
        public string company;
        public bool active;
    }

    [Serializable]
    public class Ship
    {
        public string id;
        public string name;
        public string type;
        public string home_port;
        public string image;
        public List<string> launches;
        public bool active;

        public int GetMissionCount()
        {
            return launches != null ? launches.Count : 0;
        }
    }

    [Serializable]
    public class Payload
    {
        public string id;
        public string name;
        public string type;
        public string orbit;
        public List<string> customers;
    }

    //wrapper for arrays
    [Serializable]
    public class LaunchesResponse
    {
        public List<Launch> docs;
    }

    [Serializable]
    public class CacheEntry<T>
    {
        public T data;
        public DateTime timestamp;
        public float cacheLifetime = 600f;

        public bool IsValid()
        {
            return (DateTime.UtcNow - timestamp).TotalSeconds < cacheLifetime;
        }
    }
}