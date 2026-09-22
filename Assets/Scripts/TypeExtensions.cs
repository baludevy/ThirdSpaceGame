using LiteNetLib.Utils;
using UnityEngine;

public static class TypeExtensions
{
    public static void Put(this NetDataWriter writer, Vector3 vector)
    {
        writer.Put(vector.x);
        writer.Put(vector.y);
        writer.Put(vector.z);
    }

    public static void Put(this NetDataWriter writer, Vector2 vector)
    {
        writer.Put(vector.x);
        writer.Put(vector.y);
    }

    public static void Put(this NetDataWriter writer, Quaternion quaternion)
    {
        writer.Put(quaternion.x);
        writer.Put(quaternion.y);
        writer.Put(quaternion.z);
        writer.Put(quaternion.w);
    }

    public static Vector3 GetVector3(this NetDataReader reader) =>
        new Vector3(
            reader.GetFloat(),
            reader.GetFloat(),
            reader.GetFloat()
        );

    public static Vector2 GetVector2(this NetDataReader reader) =>
        new Vector2(
            reader.GetFloat(),
            reader.GetFloat()
        );

    public static Quaternion GetQuaternion(this NetDataReader reader) =>
        new Quaternion(
            reader.GetFloat(),
            reader.GetFloat(),
            reader.GetFloat(),
            reader.GetFloat()
        );
}
