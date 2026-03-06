using UnityEngine;
using TMPro;
using System.Collections;

public class DroppedVotes : MonoBehaviour
{
    private int voteAmount;
    private float lifetime = 8f;
    private bool isCollected = false;

    [SerializeField] private TextMeshPro valueText;
    [SerializeField] private MeshRenderer voteRenderer;
    [SerializeField] private Light voteLight;

    private Vector3 startPosition;
    private float bobOffset;

    void Start()
    {
        startPosition = transform.position;
        bobOffset = Random.Range(0f, Mathf.PI * 2f);

        if (valueText != null)
            valueText.text = voteAmount.ToString();

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (isCollected) return;

        Vector3 newPosition = startPosition;
        newPosition.y += Mathf.Sin((Time.time * 2f) + bobOffset) * 0.2f;
        transform.position = newPosition;

        transform.Rotate(Vector3.up, 50f * Time.deltaTime);
    }

    public void Initialize(int amount)
    {
        voteAmount = amount;
    }

    public int PickUp()
    {
        if (isCollected) return 0;

        isCollected = true;
        Destroy(gameObject);
        return voteAmount;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                int amount = PickUp();
                player.AddVotes(amount);
            }
        }
    }
}