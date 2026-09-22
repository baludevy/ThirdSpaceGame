using UnityEngine;

public enum EntityType : byte
{
    player,
}

public class Entity : MonoBehaviour
{
    public ushort id { get; private set; }
    [SerializeField] private EntityType type;

    public void Initialize(ushort id)
    {
        this.id = id;
    }
    
    public EntityType GetEntityType => type;
}
