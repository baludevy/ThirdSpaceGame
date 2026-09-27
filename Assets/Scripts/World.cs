using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Object
{
    public ushort id;
   public ObjectType type;

   public GameObject go;
}
public enum ObjectType
{
    chest,
}

public struct World
{
    public List<Object> objects;
    public List<Entity> entities;
}