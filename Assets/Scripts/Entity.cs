using Client;
using UnityEngine;

public enum EntityType : byte
{
    player,
}

public class Entity : MonoBehaviour
{
    public ushort entityId { get; private set; }
    [SerializeField] private EntityType type;

    public void Initialize(ushort id)
    {
        this.entityId = id;
    }

    public virtual void ApplySnapshot(EntitySnapshot from, EntitySnapshot to, float t)
    {

    }

    public EntityType GetEntityType => type;
    public void SetEntityType(EntityType type) => this.type = type;
}
