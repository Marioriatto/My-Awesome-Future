using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    private int _id;
    public int id { get{return _id;} set {_id = value;}}
    private GameObject _playerController;
    public GameObject playerController {get {return _playerController;} set {_playerController = value;}}
    private RectTransform rectTransform, iconRectTransform;
    private Vector2 normalSize, iconNormalSize;
    private float hoverScaling;
    private GameObject _icon;
    public GameObject icon {get{return _icon;} set { _icon = value;}}
    public GameObject itemPrefab;
    public ItemData itemData;
    
    public bool isAnimatingGlow;
    private Image image;
    private Coroutine currentAnimation;
    void Awake()
    {
        icon = null;
        iconRectTransform = null;
        itemData = null;
        itemPrefab = null;
        currentAnimation = null;
    }
    void Start()
    {
        isAnimatingGlow = false;
        image = GetComponent<Image>();
        normalSize = new Vector2(250f,250f);
        hoverScaling = 1.2f;
        rectTransform = gameObject.GetComponent<RectTransform>();
    }
    public void SetContainer(ItemData data)
    {
        if (playerController == null) Debug.LogWarning("slot "+id+" does not have access to PlayerController");
        if (data == null) 
        {
            Debug.LogWarning("item is null");
            return;
        }
        itemData = data;
        // Prefab setup
        if (data.prefab != null) itemPrefab = data.prefab;
        else Debug.LogWarning("item " + data.itemName + " doesnt have a prefab");
        // Icon setup
        icon = Instantiate(data.icon, transform);
        iconRectTransform = icon.GetComponent<RectTransform>();
        iconNormalSize = iconRectTransform.sizeDelta;
    }
    public void Drop()
    {
        if (itemPrefab == null) {Debug.Log("no item"); return;}
        if (playerController == null) {Debug.Log("No player"); return;}

        GameObject droppedItem = Instantiate(itemPrefab);
        Vector3 playerPos = playerController.transform.position;
        droppedItem.transform.position = new Vector3(playerPos.x, 0f, playerPos.z);
        Discard();
    }
    public void Use()
    {
        if (itemPrefab == null) {Debug.Log("no item"); return;}
        if (playerController == null) {Debug.Log("No player"); return;}
    
        //TODO
        
        Discard();
    }
    public void Give()
    {
        if (itemPrefab == null) {Debug.Log("no item"); return;}
        if (playerController == null) {Debug.Log("No player"); return;}
    
        //TODO
        
        Discard();
    }
    public void Discard()
    {
        if (itemPrefab == null) {Debug.Log("no item"); return;}
        if (playerController == null) {Debug.Log("No player"); return;}
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(Dissapear());
        itemPrefab = null;
        itemData = null;
        QuitHover();
    }
    public void Hover()
    {
        Debug.Log("icon=");
        Debug.Log(icon);
        rectTransform.sizeDelta = new Vector2(normalSize.x * hoverScaling, normalSize.y * hoverScaling);
        if (icon != null) 
        {
            iconRectTransform.sizeDelta = new Vector2(iconNormalSize.x * hoverScaling, iconNormalSize.y * hoverScaling);
        }
    }
    public void QuitHover()
    {
        rectTransform.sizeDelta = normalSize;
        if (icon != null) 
        {
            iconRectTransform.sizeDelta = new Vector2(iconNormalSize.x, iconNormalSize.y);
        }
    }
    protected System.Collections.IEnumerator Dissapear()
    {
        if (iconRectTransform != null)
        {
            float elapsed = 0f;
            while (elapsed < 0.1f)
            {
                elapsed += Time.deltaTime;
                float tiempo = elapsed / 0.1f;
                iconRectTransform.localScale = Vector2.Lerp(Vector3.one, Vector3.zero, tiempo);
                yield return null;
            }
            iconRectTransform.localScale = Vector3.zero;
            Destroy(icon);
            icon = null;
            iconRectTransform = null;
        }
    }
    public void StartGlow()
    {
        isAnimatingGlow = true;
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(AnimateGlow());
    }
    public void StopGlow()
    {
        isAnimatingGlow = false;
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        image.color = Color.white;
    }
    private System.Collections.IEnumerator AnimateGlow()
    {
        float elapsed = 0f;

        while(isAnimatingGlow)
        {
            elapsed += Time.deltaTime;
            float tiempo = (elapsed % 1f) / 1f;
            float angle = tiempo * Mathf.PI * 2f;
            float colorValue = (Mathf.Sin(angle) + 1f) * (255f / 2f); 
            image.color = new Color32(255, (byte)colorValue, (byte)colorValue, 255);
            yield return null;
        }
    }
}
