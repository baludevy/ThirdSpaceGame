using System.Collections.Generic;
using UnityEngine;

namespace Client
{
    public class EntityInterpolationManager : MonoBehaviour
    {
        public static EntityInterpolationManager Instance;

        private Dictionary<ushort, SnapshotBuffer> buffers = new();

        // updates are sent in fixedupdate
        private static float interpolationDelay => Time.fixedDeltaTime;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            float renderTime = Time.time - interpolationDelay;

            foreach (SnapshotBuffer buffer in buffers.Values)
            {
                if (!buffer.HasTwoSnapshots())
                    continue;

                buffer.Interpolate(renderTime);
            }
        }

        public void AddSnapshot(Entity entity, EntitySnapshot snapshot)
        {
            if (!buffers.TryGetValue(entity.entityId, out SnapshotBuffer buffer))
            {
                buffer = new SnapshotBuffer(entity);
                buffers.Add(entity.entityId, buffer);
            }

            buffer.Add(snapshot);
        }

        private class SnapshotBuffer
        {
            public Entity entity;

            public EntitySnapshot from;
            public EntitySnapshot to;

            public SnapshotBuffer(Entity entity)
            {
                this.entity = entity;
            }

            public void Add(EntitySnapshot snapshot)
            {
                from = to;
                to = snapshot;
            }

            public void Interpolate(float renderTime)
            {
                float duration = to.time - from.time;

                if (duration <= 0f)
                    return;

                float t = Mathf.InverseLerp(from.time, to.time, renderTime);

                entity.transform.position = Vector3.Lerp(from.position, to.position, t);

                entity.ApplySnapshot(from, to, t);
            }

            public bool HasTwoSnapshots()
            {
                return from != null && to != null;
            }
        }
    }

    public class EntitySnapshot
    {
        public float time;
        public Vector2 position;
    }
}
