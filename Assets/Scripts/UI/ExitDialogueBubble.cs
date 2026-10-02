using System.Collections.Generic;
using UnityEngine;

public class ExitDialogueBubble : RegularDialogueBubble
{
    // write a way to save data
    public bool wasCalled;
    private Coroutine currentExit;
    private PlayerController player;
    protected override void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        shownPosition = new Vector2(0f,-270f);
        hiddenPosition = new Vector2(0f, -3500f);
        wasCalled = false;
        currentCooldown = null;
        typewriterCoroutine = null;
    }
    protected override void Start()
    {
        base.Start();
        player = PlayerStats.Instance.gameObject.GetComponent<PlayerController>();
    }
    public void Exit()
    {
        if (currentExit != null) StopCoroutine(currentExit);
        currentExit = StartCoroutine(ExitAnimation());
    }
    private System.Collections.IEnumerator ExitAnimation()
    {
        PopOut();
        JsonLoader.Instance.WriteSaveData();
        while (isAnimated || JsonLoader.Instance.isSaving)
        {
            yield return null;
        }
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    public void Call()
    {
        if (wasCalled || InputActions.Instance.isInventoryOpen || InputActions.Instance.isTalking) return;
        wasCalled = true;
        InputActions.Instance.isRegularDialogue = true;
        player.isInteracting = true;
        PopIn();
        options = new List<DialogueOption>();
        options.Add(new DialogueOption{ text = "Quit", onOptionSelected = Exit});
        options.Add(new DialogueOption{ text = "Back", onOptionSelected = Back});
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(Typewriter());
    }
    private System.Collections.IEnumerator Typewriter()
    {
        while (isAnimated)
        {
            yield return null;
        }
        isReady = false;
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(TypewriterAnimation("Would you like to quit now?"));
        while (InputActions.Instance.buttonInput == 0 || !isReady)
        {
            yield return null;
        }
        optionsBubble.SetupOptions(options, optionsPosition);
        isChoosing = true;
        while(isChoosing)
        {
            yield return null;
        }
        PopOut();
    }
    protected override void PopOut()
    {
        base.PopOut();
        textMeshPro.text = "";
        player.isInteracting = false;
        InputActions.Instance.isRegularDialogue = false;
        wasCalled = false;
    }
}
