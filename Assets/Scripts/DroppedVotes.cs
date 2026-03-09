using UnityEngine;
using TMPro;

public class DroppedVotes : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifetime = 8f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float scatterForce = 3f;

    [Header("Visuals")]
    [SerializeField] private TextMeshPro valueText;
    [SerializeField] private MeshRenderer voteRenderer;
    [SerializeField] private Light voteLight;

    private int voteAmount;
    private bool isCollected = false;

    private Vector3 basePosition;
    private float bobOffset;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        bobOffset = Random.Range(0f, Mathf.PI * 2f);

        // destroy after a while
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (isCollected) return;

        // floating/bobbing
        Vector3 newPosition = basePosition;
        newPosition.y += Mathf.Sin((Time.time * bobSpeed) + bobOffset) * bobHeight;
        transform.position = newPosition;

        // rotation
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    public void Initialize(int amount, Vector3 direction)
    {
        voteAmount = amount;

        // update display after amount is assigned
        if (valueText != null)
            valueText.text = voteAmount.ToString();

        // set visuals based on value if you want
        if (voteLight != null)
        {
            voteLight.intensity = Mathf.Clamp(1f + (voteAmount * 0.1f), 1f, 3f);
        }

        // scatter outward a bit
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 launchDir = (direction.normalized + Vector3.up * 0.5f).normalized;
            rb.AddForce(launchDir * scatterForce, ForceMode.Impulse);
        }

        // wait one frame before locking floating position, so physics can settle a bit
        Invoke(nameof(SetBasePosition), 0.15f);
    }

    private void SetBasePosition()
    {
        if (isCollected) return;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        basePosition = transform.position;
    }

    public int PickUp()
    {
        if (isCollected) return 0;

        isCollected = true;
        int amount = voteAmount;
        Destroy(gameObject);
        return amount;
    }
}