using UnityEngine;
using TMPro;

public class ReputationPopup : MonoBehaviour
{
    public TextMeshProUGUI popupText;

    float floatSpeed = 1.5f;
    float lifeTime = 1.2f;

    public void SetPopup(float changeAmount, NPCMovement.NPCGroup group, int playerNumber)
    {
        if (popupText == null)
        {
            Debug.LogError("PopupText reference is missing!");
            return;
        }

        // Add + sign for positive numbers
        string sign = changeAmount > 0 ? "+" : "";

        // Set popup text
        popupText.text = $"{sign}{changeAmount} ({group})";

        // Change color depending on reputation gain/loss
        if (changeAmount > 0)
        {
            popupText.color = Color.green;
        }
        else
        {
            popupText.color = Color.red;
        }
    }

    void Update()
    {
        // Float upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Countdown until destroy
        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }
}