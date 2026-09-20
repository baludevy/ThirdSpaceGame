using UnityEngine;

[CreateAssetMenu(fileName = "New Npc Dialogue", menuName = "Npc Dialogue")]
public class NpcDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogueLines;

    public bool[] autoProgressLines;
    public float[] autoProgressDelay = { 1.5f };
    public float typingSpeed = 0.05f;
    public AudioClip voicesound;
    public float voicePitch = 1.0f;
}