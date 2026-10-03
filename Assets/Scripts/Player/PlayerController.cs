using UnityEngine;
public class PlayerController : MonoBehaviour
{
    public bool isInteracting;
    private Coroutine currentCooldown, currentAnimation;
    [SerializeField] InventoryUI inventoryScript;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Bubbles"))
        {
            if (InputActions.Instance.pickInput != 0 && !isInteracting)
            {
                Bubbles bubbles = other.gameObject.GetComponent<Bubbles>();
                bubbles.Pick();
            }
        }
        if (other.CompareTag("Items"))
        {
            if (InputActions.Instance.pickInput != 0 && !isInteracting)
            {
                Item item = other.gameObject.GetComponent<Item>();
                if (inventoryScript.SetSlot(item.data)) item.Pick();
            }
        }
    }
    public void SetInteracting(bool value)
    {
        if (currentCooldown != null) StopCoroutine(currentCooldown);
        currentCooldown = StartCoroutine(Cooldown(value));
    }
    private System.Collections.IEnumerator Cooldown(bool value)
    {
        float elaps = 0f;
        while (elaps < 0.5f)
        {
            elaps += Time.deltaTime;
            yield return null;
        }
        isInteracting = value;
    }
    void Start()
    {
        isInteracting = false;
        currentCooldown = null;
        if (inventoryScript == null) Debug.LogWarning("PlayerController Script does not have a reference to Inventory script");
    }
    void Update()
    {
        if (!isInteracting)
        {
            transform.position += new Vector3(InputActions.Instance.moveInput.x * 5f * Time.deltaTime, 0.0f, InputActions.Instance.moveInput.y * 5f * Time.deltaTime);

            if (InputActions.Instance.isMoving)
            {
                Quaternion targetRotation = Quaternion.Euler(new Vector3(0, Mathf.Atan2(InputActions.Instance.rotateInput.y * -1f,InputActions.Instance.rotateInput.x) * Mathf.Rad2Deg, 0));

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    1080f * Time.deltaTime);   
            }
        }
    }
    public virtual void RotateTowardsTarget(Vector3 target)
    {
        if(currentAnimation != null) return;
        currentAnimation = StartCoroutine(Rotate(target));
    }
    private System.Collections.IEnumerator Rotate(Vector3 target)
    {
        float deltay = target.z - transform.position.z;
        float deltax = target.x - transform.position.x;
        Quaternion targetRotation = Quaternion.Euler(
            new Vector3(0,
            Mathf.Atan2(deltay * -1, deltax) * Mathf.Rad2Deg,
            0));
        float elaps = 0f;
        while (transform.rotation != targetRotation)
        {
            elaps += Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                360f * Time.deltaTime);  
            yield return null;
        }
    }
}
