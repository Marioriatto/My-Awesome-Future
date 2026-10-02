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
    private string dialoguesPath, saveDataPath;
    public InventoryUI inventoryUI;
    void LoadDialogues()
    {
        if (File.Exists(dialoguesPath))
        {
            string json = File.ReadAllText(dialoguesPath);
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
        if (File.Exists(saveDataPath))
        {
            string json = File.ReadAllText(saveDataPath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            PlayerStats.Instance.playerName = saveData.name;
            PlayerStats.Instance.bubbles = saveData.bubbles;
            for (int i = 0; i < 10; i++)
            {
                if (saveData.inventorySlots[i] == -1)
                    continue;
                else
                {
                    Debug.Log(saveData.inventorySlots[i] - 1);
                    inventoryUI.slots[i].SetContainer(itemDataList[saveData.inventorySlots[i] - 1]);
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
        if (File.Exists(saveDataPath))
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
                {
                    slots[i] = inventoryUI.slots[i].itemData.id;
                }
            }
            SaveData saveData = new SaveData();
            saveData.name = PlayerStats.Instance.playerName;
            saveData.bubbles = PlayerStats.Instance.bubbles;
            saveData.inventorySlots = slots;
            string json = JsonUtility.ToJson(saveData);
            File.WriteAllText(saveDataPath, json);
            isSaving = false;
        }
        else
        {
            Debug.LogWarning("No hay json");
        }        
    }
    void Awake()
    {
        dialoguesPath = Path.Combine(Application.streamingAssetsPath, "Dialogues.json");
        saveDataPath = Path.Combine(Application.streamingAssetsPath, "SaveData.json");
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
