using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class RegularDialogueBubble : DialogueBubble
{
    [SerializeField] InventoryUI inventoryUI, otherInventoryUI;
    [SerializeField] protected TextMeshProUGUI textMeshPro;
    [SerializeField] private TextMeshProUGUI nameTextMeshPro;
    [SerializeField] protected OptionsBubble optionsBubble;
    [SerializeField] private PlayerInteractionBox interactionBox;
    [SerializeField] private CameraController cameraController;
    private List<string> dialogues;
    protected bool isReady, isFirstLine;
    protected List<DialogueOption> options;
    public Vector2 optionsPosition, shownPosition, hiddenPosition;
    public bool isChoosing;
    private NPC npc;    
    protected Coroutine currentCooldown, typewriterCoroutine, descriptionCoroutine, textCoroutine;
    private ItemData pendingItem;
    private InventoryUI pendingShop;
    private List<DialogueOption> itemOptions;
    protected override void Awake()
    {
        base.Awake();
        shownPosition = new Vector2(0f,-270f);
        hiddenPosition = new Vector2(0f, -3500f);
        currentCooldown = null;
        typewriterCoroutine = null;
        if (interactionBox == null) Debug.Log("no interaction box reference from dialogueBubble");
    }
    public void Buy()
    {
        otherInventoryUI.Buy(npc.items);
    }
    public void ShowItemOptions(ItemData item, InventoryUI shop)
    {
        pendingItem = item;
        pendingShop = shop;
        itemOptions = new List<DialogueOption>
        {
            new DialogueOption {text="Buy", onOptionSelected = BuyItem},
            new DialogueOption {text="Back", onOptionSelected = BackFromItem}
        };
        if (descriptionCoroutine != null) StopCoroutine(descriptionCoroutine);
        descriptionCoroutine = StartCoroutine(ShowDescriptionThenOptions(item.description));
    }
    private System.Collections.IEnumerator ShowDescriptionThenOptions(string description)
    {
        yield return StartCoroutine(AnimatePanel(shownPosition));
        descriptionCoroutine = StartTypewriter(description, false);
        yield return descriptionCoroutine;
        optionsBubble.SetupOptions(itemOptions, optionsPosition);
    }
    private Coroutine StartTypewriter(string phrase, bool signalsReady)
    {
        if (textCoroutine != null) StopCoroutine(textCoroutine);
        textCoroutine = StartCoroutine(TypewriterAnimation(phrase, signalsReady));
        return textCoroutine;
    }
    public void BackFromItem()
    {
        HideTemp();
        pendingShop.ShowGridBack();
        pendingItem = null;
        pendingShop = null;
    }
    public void BuyItem()
    {
        ItemData item = pendingItem;
        InventoryUI shop = pendingShop;
        pendingItem = null;
        pendingShop = null;
        if (PlayerStats.Instance.ChangeBubbles(-item.price))
        {
            inventoryUI.SetSlot(item);
            shop.RemoveSelectedItem();
            HideTemp();
            shop.ShowGridBack();   
        }
        else
            StartCoroutine(NotEnoughBubbles(shop));
    }
    private System.Collections.IEnumerator NotEnoughBubbles(InventoryUI shop)
    {
        yield return StartTypewriter("Not enough cash, Stranger!", false);
        yield return new WaitForSeconds(1f);
        HideTemp();
        shop.ShowGridBack();
    }
    public void HideTemp()
    {
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        isReady = true;
        currentAnimation = StartCoroutine(AnimatePanel(hiddenPosition));
    }
    public virtual void Back() {isChoosing = false;}
    public virtual void SetText(NPC npc)
    {
        InputActions.Instance.isTalking = true;
        if (npc.isDealer)
        {
            options = new List<DialogueOption>();
            foreach (string optionText in npc.dialogues[npc.dialogues.Count-1].lines)
            {
                System.Action action = optionText switch
                {
                    "Sell" => inventoryUI.Sell,
                    "Buy" => Buy,
                    "Back" => Back,
                    _ => null
                };
                options.Add(new DialogueOption { text = optionText, onOptionSelected = action});
            }
            dialogues = npc.dialogues[Random.Range(0,npc.dialogues.Count-2)].lines;
        }
        else
        {
            dialogues = npc.dialogues[Random.Range(0,npc.dialogues.Count)].lines;
        }
        this.npc = npc;
        cameraController.Zoom();

        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
        textMeshPro = texts[0];
        textMeshPro.text = "";
        nameTextMeshPro = texts[1];
        nameTextMeshPro.text = npc.npcName;

        PopIn();
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(Typewriter());
    }
    protected override void Start()
    {
        base.Start();
        isFirstLine = false;
        isChoosing = false;
        isReady = false;
    }
    private System.Collections.IEnumerator Typewriter()
    {
        while (isAnimated)
        {
            yield return null;
        }
        isReady = true;
        isFirstLine = true;
        foreach(string phrase in dialogues)
        {
            while ((!isFirstLine && InputActions.Instance.buttonInput == 0) || !isReady)
            {
                yield return null;
            }
            isFirstLine = false;
            isReady = false;
            if (currentCooldown != null) StopCoroutine(currentCooldown);
            currentCooldown = StartTypewriter(phrase, true);
        }
        isChoosing = true;
        if (npc.isDealer)
        {
            while (InputActions.Instance.buttonInput == 0 || !isReady)
            {
                yield return null;
            }
            InputActions.Instance.isRegularDialogue = true;
            optionsBubble.SetupOptions(options, optionsPosition);
            while(isChoosing)
            {
                yield return null;
            }
            string phrase = npc.dialogues[npc.dialogues.Count-2].lines[0];
            isReady = false;
            if (currentCooldown != null) StopCoroutine(currentCooldown);
            currentCooldown = StartTypewriter(phrase, true);
        }
        while (InputActions.Instance.buttonInput == 0 || !isReady)
        {
            yield return null;
        }
        PopOut();
        otherInventoryUI.isBuying = false;
        InputActions.Instance.isRegularDialogue = false;
        npc.stayStill = false;
    }
    public override void Hide()
    {
        if (interactionBox != null) interactionBox.interactingSubject = null;
        base.Hide();
        InputActions.Instance.isTalking = false;
        if (cameraController != null) cameraController.QuitZoom();
        if (interactionBox != null) interactionBox.parentScript.SetInteracting(InputActions.Instance.isInventoryOpen);
    }
    public System.Collections.IEnumerator AnimatePanel(Vector2 target)
    {
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
        textMeshPro.text = "";
        if (target == shownPosition && !otherInventoryUI.isBuying)
        {
            isChoosing = false;
        }
        inventoryUI.currentAnimation = null;
    }
    public System.Collections.IEnumerator TypewriterAnimation(string phrase, bool signalsReady = true)
    {
        textMeshPro.text = "";
        foreach(char letter in phrase)
        {
            textMeshPro.text += letter;
            float elapsed = 0f;
            while (elapsed < 0.035f)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        if (signalsReady)
        {
            isReady = true;
            InputActions.Instance.buttonInput = 0;
        }
    }
}