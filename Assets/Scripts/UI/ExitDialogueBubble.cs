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
        currentExit = null;
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
        player.SetInteracting(true);
        wasCalled = true;
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
        player.SetInteracting(true);
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
        isAnimated = true;
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(ExitAnimateScale(Vector3.one, Vector3.zero));
        textMeshPro.text = "";
        Debug.Log("PoppingOut");
        player.SetInteracting(false);
        InputActions.Instance.isRegularDialogue = false;
        wasCalled = false;
    }
    protected System.Collections.IEnumerator ExitAnimateScale(Vector3 start, Vector3 target)
    {
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float tiempo = elapsed / 0.3f;
            rectTransform.localScale = Vector2.Lerp(start, target, tiempo);
            yield return null;
        }
        rectTransform.localScale = target;
        isAnimated = false;

        if (target == Vector3.zero && currentExit == null) Hide();
    }
}
