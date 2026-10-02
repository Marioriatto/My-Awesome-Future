using System.Collections.Generic;
[System.Serializable]
public class SaveData
{
    public string name;
    public int bubbles;
    public int[] inventorySlots = new int[10];
    public SaveData (string name, int bubbles, int[] inventorySlots)
    {
        this.name = name;
        this.bubbles = bubbles;
        this.inventorySlots = inventorySlots;
    }
}
[System.Serializable]
// Dialogue Struct
public class Dialogues
{
    public List<string> lines;
}
[System.Serializable]
// NPC Dialogues Mapping Struct
public class NPCDialoguesList
{
    public string name;
    public List<Dialogues> dialogues;
}
[System.Serializable]
// NPC Clasification Struct
public class DialogueType
{
    public string typeName;
    public List<NPCDialoguesList> npcList;
}
[System.Serializable]
// NPC Types Struct
public class DialoguesData
{
    public List<DialogueType> types;    
}