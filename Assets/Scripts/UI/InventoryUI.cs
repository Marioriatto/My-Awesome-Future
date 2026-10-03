using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class InventoryUI : MonoBehaviour
{
    // References
    [SerializeField] PlayerController playerController;
    [SerializeField] GameObject slotPrefab;
    // Inventory Slots
    public Slot[] slots;
    private Slot selectedSlot, swappingSlot; // UI Choosing
    
    // Dialogues and options for interactions and options
    [SerializeField] OptionsBubble optionsBubble;
    private TextMeshProUGUI bubbleCountText;
    [SerializeField] RegularDialogueBubble regularDialogueBubble;
    [SerializeField] RectTransform bubbleCountRectTransform;
    
    // UI Animations
    private RectTransform rectTransform;
    private Vector2 shownPosition, hiddenPosition;
    private int swappingRow, swappingCol;
    public int selectingRow, selectingCol;
    private Coroutine currentAnimatePanel, currentCooldown;
    [System.NonSerialized] public Coroutine currentAnimation;
    private Vector2 startPosition = new Vector2(-650f, 50f);
    private readonly Vector2 spacing = new Vector2(330f, 300f);

    // Inventory States
    private bool isSwappingSlots, isInventoryPanelAnimated;
    public bool isCoolingAction, isSelectingSlotOptions, isSelling, isBuying, isPlayerInventory;
    private bool isThisOpen, isViewingItem;

    void SpawnSlots()
    {
        // Creates Slot UI GameObjects
        // Set up Slots Object List
        int limit;
        if (isPlayerInventory)
        {
            limit = 10;
        }
        else
        {
            limit = 15;
            startPosition += new Vector2(0f,300f);
        }
        for (int i = 0; i < limit; i++)
        {
            GameObject slot = Instantiate(slotPrefab, transform);
            Slot slotScript = slot.GetComponent<Slot>();
            if (slotScript == null) Debug.LogWarning("there is no slotscript for "+i+"th slot instance");
            slotScript.id = i;
            slotScript.playerController = playerController.gameObject;
            slots[i] = slotScript;
            int row = i / 5;
            int col = i % 5;

            RectTransform slotRectTransform = slot.GetComponent<RectTransform>();
            slotRectTransform.anchoredPosition = new Vector2(
                startPosition.x + col * spacing.x,
                startPosition.y - row * spacing.y
            );
        }
        if (isPlayerInventory)
            JsonLoader.Instance.LoadSaveData();
    }
    void Awake()
    {
        // Slots List Length Setup
        if (isPlayerInventory) slots = new Slot[10];
        else slots = new Slot[15];
        // UI Tools
        rectTransform = GetComponent<RectTransform>();
        bubbleCountText = GetComponentInChildren<TextMeshProUGUI>();
        hiddenPosition = new Vector2(0f, -3500f);
        shownPosition = new Vector2(0f, 0f);
        selectedSlot = null;
        swappingSlot = null;
        // Most states start false
        isInventoryPanelAnimated = false;
        isSelectingSlotOptions = false;
        isSwappingSlots = false;
        isSelling = false;
        isBuying = false;
        isThisOpen = false;
    }
    void Start()
    {
        SpawnSlots();
        // isOpen is set up on start to make sure InputActions
        // has finished running Awake
        InputActions.Instance.isInventoryOpen = false;
        rectTransform.anchoredPosition = hiddenPosition;
        selectingCol = 0;
        selectingRow = 0;
        swappingRow = 0;
        swappingCol = 0;
        if (isPlayerInventory)
        {
            if (playerController == null)
            {
                Debug.Log("InventoryUI reference for playerController is null");
            }
            if (optionsBubble == null)
            {
                Debug.Log("InventoryUI reference for OptionsBubble is null");
            }
            bubbleCountRectTransform.anchoredPosition = new Vector2(570f,350f);
            bubbleCountText.text = PlayerStats.Instance.bubbles.ToString();
        }
    }
    void Update()
    {
        if (isThisOpen && !isCoolingAction)
        {
            // Picking
            if (!isPlayerInventory && !isViewingItem && InputActions.Instance.pickInput != 0)
            {
                isCoolingAction = true;
                if (currentCooldown != null) StopCoroutine(currentCooldown);
                currentCooldown = StartCoroutine(Cooldown());
                OpenInventory();
            }
            // Moving through Inventory Slots
            if(InputActions.Instance.moveInventoryInput.y != 0 || InputActions.Instance.moveInventoryInput.x != 0)
            {
                if (isPlayerInventory)
                {
                    if (!isSelectingSlotOptions)
                        HoverSlot();
                }
                else if (!isViewingItem)
                    HoverSlot();
            }
            // Selecting Inventory Slot
            if (InputActions.Instance.inventorySelectInput != 0 && !isSelectingSlotOptions)
            {
                // como un inventario necesito una forma para saber si el que esta abierto soy yo

                if (isPlayerInventory)
                {
                    if (isSelling)
                    {
                        if (selectedSlot != null && selectedSlot.itemPrefab != null)
                        {
                            if (SellSlot())
                            {
                                OpenInventory();
                                isInventoryPanelAnimated = true;
                            }
                        }
                    }
                    else
                    {
                        if (selectedSlot != null && selectedSlot.itemPrefab != null)
                        {
                            isSelectingSlotOptions = true;
                            ShowOptionsFor(selectedSlot.itemData);
                        }
                        isCoolingAction = true;
                        if (currentCooldown != null) StopCoroutine(currentCooldown);
                        currentCooldown = StartCoroutine(Cooldown());
                    }
                }
                // here
                else if (!isPlayerInventory && !isViewingItem)
                {
                    if (selectedSlot != null && selectedSlot.itemData != null)
                    {
                        HideGridForItem();
                        regularDialogueBubble.ShowItemOptions(selectedSlot.itemData, this);
                    }
                    isCoolingAction = true;
                    if (currentCooldown != null) StopCoroutine(currentCooldown);
                    currentCooldown = StartCoroutine(Cooldown());
                }
            }
            if (isPlayerInventory)
            {
                // Swap
                if(isSelectingSlotOptions && isSwappingSlots)
                {
                    // Move
                    if(InputActions.Instance.moveInventoryInput.y != 0 || InputActions.Instance.moveInventoryInput.x != 0)
                    {
                        HoverSlot();
                    }
                    // Select swap slot
                    if (InputActions.Instance.inventorySelectInput != 0)
                    {
                        if (swappingSlot != null)
                        {
                            ItemData temp = null;
                            selectedSlot.StopGlow();
                            if (swappingSlot.itemData != null)
                            {
                                temp = swappingSlot.itemData;
                            }
                            swappingSlot.Discard();
                            swappingSlot.SetContainer(selectedSlot.itemData);
                            selectedSlot.Discard();
                            if (temp != null) selectedSlot.SetContainer(temp);
                            isSelectingSlotOptions = false;
                            isSwappingSlots = false;
                            selectingCol = swappingCol;
                            selectingRow = swappingRow;
                            selectedSlot = swappingSlot;
                            swappingSlot = null;
                            swappingCol = 0;
                            swappingRow = 0;
                            selectedSlot.Hover();
                        }
                        isCoolingAction = true;
                        if (currentCooldown != null) StopCoroutine(currentCooldown);
                        currentCooldown = StartCoroutine(Cooldown());
                    }
                } 
            }
        }
        if (!isInventoryPanelAnimated)
        {
            if (((!playerController.isInteracting && !InputActions.Instance.isInventoryOpen) || InputActions.Instance.isInventoryOpen) && isPlayerInventory && !isSelectingSlotOptions)
            {
                if (isSelling)
                {
                    if (InputActions.Instance.pickInput != 0 && InputActions.Instance.isInventoryOpen)
                        OpenInventory();
                }
                else
                {
                    if (InputActions.Instance.inventoryInput != 0)
                        OpenInventory();
                }
            }
        }
    }
    public void ShowOptionsFor(ItemData data)
    {
        List<DialogueOption> resolvedOptions = new List<DialogueOption>();

        foreach (string optionText in data.options)
        {
            System.Action action = optionText switch
            {
                "Use" => Use,
                "Eat" => Eat,
                "Give" => Give,
                _ => null
            };

            resolvedOptions.Add(new DialogueOption { text = optionText, onOptionSelected = action});
        }
        resolvedOptions.Add(new DialogueOption { text = "Swap", onOptionSelected = Swap});
        resolvedOptions.Add(new DialogueOption { text = "Drop", onOptionSelected = Drop});

        optionsBubble.SetupOptions(resolvedOptions, selectedSlot.GetComponent<RectTransform>().anchoredPosition);
    }
    public bool SetSlot(ItemData item)
    {
        int nextAvailableSlot = FindAvailableSlot();
        if (nextAvailableSlot != -1)
        {
            slots[nextAvailableSlot].SetContainer(item);
            return true;
        }
        else
        {
            Debug.Log("the inventory is full");
            return false;
        }
    }    
    private void HoverSlot()
    {
        if (isSwappingSlots)
        {
            if (swappingSlot != null)
            {
                swappingSlot.QuitHover();
            }
            isCoolingAction = true;
            swappingCol += (int)InputActions.Instance.moveInventoryInput.x;
            swappingCol = (swappingCol < 0) ? 4 : swappingCol % 5;
            swappingRow += (int)InputActions.Instance.moveInventoryInput.y;
            swappingRow = (swappingRow < 0) ? 1 : swappingRow % 2;
            swappingSlot = slots[(5 * swappingRow) + swappingCol];
            swappingSlot.Hover();
        }
        else
        {
            if (selectedSlot != null)
            {
                selectedSlot.QuitHover();
            }
            isCoolingAction = true;
            selectingCol += (int)InputActions.Instance.moveInventoryInput.x;
            selectingCol = (selectingCol < 0) ? 4 : selectingCol % 5;
            selectingRow -= (int)InputActions.Instance.moveInventoryInput.y;
            if (isPlayerInventory)
                selectingRow = (selectingRow < 0) ? 1 : selectingRow % 2;
            else
                selectingRow = (selectingRow < 0) ? 2 : selectingRow % 3;
            selectedSlot = slots[(5 * selectingRow) + selectingCol];
            selectedSlot.Hover();
        }
        if (currentCooldown != null) StopCoroutine(currentCooldown);
        currentCooldown = StartCoroutine(Cooldown());
    }
    private int FindAvailableSlot()
    {
        if (slots[(5*selectingRow) + selectingCol].icon == null) return (5*selectingRow) + selectingCol; 
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].icon == null) return i;
        }
        return -1;
    }
    public void OpenInventory()
    {
        if (selectedSlot == null)
        {
            selectedSlot = slots[0];
        }
        isInventoryPanelAnimated = true;
        if (!InputActions.Instance.isInventoryOpen && isPlayerInventory)
            bubbleCountText.text = PlayerStats.Instance.bubbles.ToString();
        Vector2 target = InputActions.Instance.isInventoryOpen ? hiddenPosition : shownPosition;
        InputActions.Instance.isInventoryOpen = !InputActions.Instance.isInventoryOpen;
        isThisOpen = !isThisOpen;
        selectedSlot.Hover();
        if (!isSelling && !isBuying)
            playerController.isInteracting = InputActions.Instance.isInventoryOpen;
            Debug.Log("inventoryUIisinteracting "+playerController.isInteracting);
        if (!InputActions.Instance.isInventoryOpen)
        {
            if (selectedSlot.itemPrefab != null) selectedSlot.QuitHover();
        }
        else InputActions.Instance.inventorySelectInput = 0f;
        if (currentAnimatePanel != null) StopCoroutine(currentAnimatePanel);
        currentAnimatePanel = StartCoroutine(AnimatePanel(target));
    }
    private System.Collections.IEnumerator AnimatePanel(Vector2 target)
    {
        while (currentAnimation != null)
            yield return null;
        Vector2 start = rectTransform.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float tiempo = elapsed / 0.3f;
            rectTransform.anchoredPosition = Vector2.Lerp(start, target, tiempo);
            yield return null;
        }
        rectTransform.anchoredPosition = target;
        
        isInventoryPanelAnimated = false;
        if (!InputActions.Instance.isInventoryOpen)
        {
            if (regularDialogueBubble.isChoosing)
            {
                if (regularDialogueBubble.currentAnimation != null ) StopCoroutine(regularDialogueBubble.currentAnimation);
                regularDialogueBubble.currentAnimation = StartCoroutine(regularDialogueBubble.AnimatePanel(regularDialogueBubble.shownPosition));
            }
            isSelling = false;
            if (!isViewingItem)
                isBuying = false;
        }
    }
    private System.Collections.IEnumerator AnimateGridOnly(Vector2 target)
    {
        while ( currentAnimation != null) yield return null;
        Vector2 start = rectTransform.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(start, target, elapsed / 0.3f);
            yield return null;
        }
        rectTransform.anchoredPosition = target;
    }
    public void HideGridForItem()
    {
        isViewingItem = true;
        if (currentAnimatePanel != null) StopCoroutine(currentAnimatePanel);
        currentAnimatePanel = StartCoroutine(AnimateGridOnly(hiddenPosition));
    }
    public void ShowGridBack()
    {
        isViewingItem = false;
        InputActions.Instance.inventorySelectInput = 0f;
        if (currentAnimatePanel != null) StopCoroutine(currentAnimatePanel);
        currentAnimatePanel = StartCoroutine(AnimateGridOnly(shownPosition));
        if (selectedSlot != null) selectedSlot.Hover();
    }
    public void RemoveSelectedItem()
    {
        if (selectedSlot != null) selectedSlot.Discard();
    }
    private System.Collections.IEnumerator Cooldown()
    {
        float elaps = 0f;
        while (elaps < 0.3f)
        {
            elaps += Time.deltaTime;
            yield return null;
        }
        isCoolingAction = false;
    }

    private bool SellSlot()
    {
        if (currentCooldown != null) StopCoroutine(currentCooldown);
        currentCooldown = StartCoroutine(Cooldown());
        if (!selectedSlot.itemData.isSaleable) return false;
        int startAmount = PlayerStats.Instance.bubbles;
        if (PlayerStats.Instance.ChangeBubbles(selectedSlot.itemData.price))
        {
            selectedSlot.Discard();
            if (currentAnimation != null) StopCoroutine(currentAnimation);
            currentAnimation = StartCoroutine(BubblesAnimation(startAmount));
            return true;   
        }
        else
            return false;
    }
    public void Use()
    {
        selectedSlot.Use();
    }
    public void Give()
    {
        selectedSlot.Give();
    }
    public void Eat()
    {
        if (currentCooldown != null) StopCoroutine(currentCooldown);
        currentCooldown = StartCoroutine(Cooldown());
        selectedSlot.Discard();
        isSelectingSlotOptions = false;
    }
    public void Buy(List<ItemData> items)
    {
        if (isPlayerInventory) return;
        foreach (ItemData item in items)
        {
            SetSlot(item);
        }
        if (InputActions.Instance.isInventoryOpen || isInventoryPanelAnimated || isSelectingSlotOptions) 
        {
            return;
        }
        isBuying = true;
        regularDialogueBubble.HideTemp();       
        OpenInventory();
    }
    public void Sell()
    {
        isSelling = true;
        if (InputActions.Instance.isInventoryOpen || isInventoryPanelAnimated || isSelectingSlotOptions) 
        {
            isSelling = false;
            return;
        }
        if (regularDialogueBubble.currentAnimation != null) StopCoroutine(regularDialogueBubble.currentAnimation);
        regularDialogueBubble.currentAnimation = StartCoroutine(regularDialogueBubble.AnimatePanel(regularDialogueBubble.hiddenPosition));
        OpenInventory();
    }
    public void Swap()
    {
        isSwappingSlots = true;
        selectedSlot.StartGlow();
        isCoolingAction = true;
        swappingCol = selectingCol;
        swappingRow = selectingRow;
        if(currentCooldown != null) StopCoroutine(currentCooldown);
        currentCooldown = StartCoroutine(Cooldown());
    }
    public void Drop()
    {
        selectedSlot.Drop();
        isSelectingSlotOptions = false;
    }
    private System.Collections.IEnumerator BubblesAnimation(int amount)
    {
        int startAmount = amount;
    
        while (startAmount <= PlayerStats.Instance.bubbles)
        {
            bubbleCountText.text = startAmount.ToString();
            startAmount += 10;
            yield return null;
        }
        bubbleCountText.text = PlayerStats.Instance.bubbles.ToString();
        currentAnimation = null;
    }
}