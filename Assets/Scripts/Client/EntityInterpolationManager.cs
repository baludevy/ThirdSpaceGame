using System.Collections.Generic;
using UnityEngine;

namespace Client
{
    public class EntityInterpolationManager : MonoBehaviour
    {
        public static EntityInterpolationManager Instance;

        private readonly Dictionary<ushort, SnapshotBuffer> buffers = new();

        private bool hasClock;
        private uint firstTick;
        private uint latestTick;
        private float clockOffset;

        private const float worldUpdateInterval = 0.02f;
        private const int interpolationTicks = 1;
        private const float interpolationSeconds = interpolationTicks * worldUpdateInterval;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void AddSnapshot(Entity entity, uint tick, EntitySnapshot snapshot)
        {
            if (entity == null) return;

            if (!hasClock)
            {
                hasClock = true;
                firstTick = tick;
                latestTick = tick;
                clockOffset = Time.time;
            }

            snapshot.tick = tick;
            snapshot.time = (tick - firstTick) * worldUpdateInterval;

            if (tick > latestTick)
            {
                latestTick = tick;
                clockOffset = Mathf.Lerp(clockOffset, Time.time - snapshot.time, 0.1f);
            }

            if (!buffers.TryGetValue(entity.entityId, out SnapshotBuffer buffer))
            {
                buffer = new SnapshotBuffer(entity);
                buffers.Add(entity.entityId, buffer);
            }

            buffer.Add(snapshot);
        }

        private void Update()
        {
            if (!hasClock) return;

            float renderTime = Time.time - clockOffset - interpolationSeconds;

            foreach (SnapshotBuffer buffer in buffers.Values)
                buffer.Interpolate(renderTime);
        }

        public void RemoveEntity(ushort entityId)
        {
            buffers.Remove(entityId);
        }

        private class SnapshotBuffer
        {
            private readonly Entity entity;
            private readonly List<EntitySnapshot> samples = new();

            public SnapshotBuffer(Entity entity)
            {
                this.entity = entity;
            }

            public void Add(EntitySnapshot snapshot)
            {
                if (samples.Count > 0 && snapshot.tick <= samples[samples.Count - 1].tick)
                    return;

                samples.Add(snapshot);
            }

            public void Interpolate(float renderTime)
            {
                if (entity == null || samples.Count < 2) return;

                while (samples.Count > 2 && samples[1].time <= renderTime)
                    samples.RemoveAt(0);

                EntitySnapshot from = samples[0];
                EntitySnapshot to = samples[1];
                float t = Mathf.InverseLerp(from.time, to.time, renderTime);
                Vector2 position = Vector2.Lerp(from.position, to.position, t);

                entity.transform.position = new Vector3(position.x, position.y, entity.transform.position.z);
                entity.ApplySnapshot(from, to, t);
            }
        }
    }

    public class EntitySnapshot
    {
        public uint tick;
        public float time;
        public Vector2 position;
    }
}