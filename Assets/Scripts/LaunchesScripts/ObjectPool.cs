using System.Collections.Generic;
using UnityEngine;

namespace SpaceX
{
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Queue<T> pool = new Queue<T>();
        private readonly List<T> activeObjects = new List<T>();

        public ObjectPool(T prefab, Transform parent, int initialSize = 10)
        {
            this.prefab = prefab;
            this.parent = parent;

            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }

        private T CreateNewObject()
        {
            T obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
            return obj;
        }

        public T Get()
        {
            T obj;

            if (pool.Count == 0)
            {
                obj = CreateNewObject();
            }

            obj = pool.Dequeue();
            obj.gameObject.SetActive(true);
            activeObjects.Add(obj);
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null) return;

            obj.gameObject.SetActive(false);
            activeObjects.Remove(obj);
            pool.Enqueue(obj);

            if (obj is LaunchListItem listItem)
            {
                listItem.ResetItem();
            }
        }

        public void ReturnAll()
        {
            var activeObjectsCopy = new List<T>(activeObjects);

            foreach (var obj in activeObjectsCopy)
            {
                Return(obj);
            }

            activeObjects.Clear();
        }

        public List<T> GetActiveObjects()
        {
            return new List<T>(activeObjects);
        }
    }
}