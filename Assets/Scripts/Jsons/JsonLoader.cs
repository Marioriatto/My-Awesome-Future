using UnityEngine;
using System.IO;
using System.Collections.Generic;
public class JsonLoader : MonoBehaviour
{
    public static JsonLoader Instance {get; private set;}
    public List<NPCDialoguesList> npc, dealer;
    public bool[] npcAvailability, dealerAvailability;
    public bool isSaving;
    public List<ItemData> itemDataList;
    private readonly string path = Path.Combine(Application.streamingAssetsPath, "Dialogues.json");
    public InventoryUI inventoryUI;
    void LoadDialogues()
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            DialoguesData data = JsonUtility.FromJson<DialoguesData>(json);
            foreach (DialogueType type in data.types)
            {
                if (string.Equals(type.typeName, "NPC")) npc = type.npcList;
                else if (string.Equals(type.typeName, "Dealer")) dealer = type.npcList;
            }
        }
        else
        {
            Debug.LogWarning("No hay json");
        }
    } 
    void LoadSaveData()
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            PlayerStats.Instance.playerName = saveData.name;
            PlayerStats.Instance.bubbles = saveData.bubbles;
            for (int i = 0; i < 10; i++)
            {
                if (saveData.inventorySlots[i] == -1) continue;
                else
                {
                    inventoryUI.slots[i].SetContainer(itemDataList[i]);
                }
            }
        }
        else
        {
            Debug.LogWarning("No hay json");
        }
    }
    public void WriteSaveData()
    {
        if (File.Exists(path))
        {
            isSaving = true;
            int[] slots = new int[10];
            for (int i = 0; i < 10; i++)
            {
                if (inventoryUI.slots[i].itemData == null)
                {
                    slots[i] = -1;
                }
                else
                    slots[i] = inventoryUI.slots[i].itemData.id;
            }
            SaveData saveData = new SaveData(PlayerStats.Instance.playerName,PlayerStats.Instance.bubbles, slots);
            string json = JsonUtility.ToJson(saveData);
            File.WriteAllText(path, json);
            isSaving = false;
        }
        else
        {
            Debug.LogWarning("No hay json");
        }        
    }
    void Awake()
    {
        if (inventoryUI == null) Debug.LogWarning("No inventoryUI reference in JsonLoader");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        LoadDialogues();
        npcAvailability = new bool[npc.Count];
        for (int i = 0; i < npc.Count; i++)
        {
            npcAvailability[i] = true;
        }
        dealerAvailability = new bool[dealer.Count];
        for (int i = 0; i < dealer.Count; i++)
        {
            dealerAvailability[i] = true;
        }
    }
    void Start()
    {
        LoadSaveData();
    }
}
