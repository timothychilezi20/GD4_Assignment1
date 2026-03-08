using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{
    [Header("HUD Elements")]
    public TextMeshProUGUI votesText;
    public TextMeshProUGUI ballotsText;
    public Image itemImage;

    [Header("Temporary Message Elements")]
    public TextMeshProUGUI tempMessageText;
    public CanvasGroup tempMessageCanvasGroup;

    [Header("Item Sprites")]
    public Sprite noneSprite;  // Default empty item icon
    public Sprite protractorSprite;
    public Sprite basketballSprite;
    public Sprite paintbrushSprite;
    public Sprite appleSprite;
    public Sprite prankKitSprite;

    [Header("Temporary Message Settings")]
    public float tempMessageDuration = 1.5f;
    public Color defaultMessageColor = Color.white;

    private Coroutine tempMessageCoroutine;

    void Awake()
    {
        // Auto-find TMP text and CanvasGroup if not assigned
        if (tempMessageText == null)
        {
            tempMessageText = GetComponentInChildren<TextMeshProUGUI>();
            if (tempMessageText == null)
                Debug.LogWarning($"{name}: No TMP text found for temporary messages!");
        }

        if (tempMessageCanvasGroup == null)
        {
            tempMessageCanvasGroup = GetComponentInChildren<CanvasGroup>();
            if (tempMessageCanvasGroup == null)
                Debug.LogWarning($"{name}: No CanvasGroup found for temporary messages!");
        }

        // Hide temp message initially
        if (tempMessageCanvasGroup != null)
            tempMessageCanvasGroup.alpha = 0f;

        // Initialize item icon
        if (itemImage != null)
            itemImage.sprite = noneSprite;
    }

    // --- Update HUD ---
    public void UpdateVotes(int votes)
    {
        if (votesText != null)
            votesText.text = $"Votes: {votes}";
    }

    public void UpdateBallots(int ballots)
    {
        if (ballotsText != null)
            ballotsText.text = $"Ballots: {ballots}";
    }

    public void UpdateItem(ItemType item)
    {
        if (itemImage == null) return;

        switch (item)
        {
            case ItemType.None: itemImage.sprite = noneSprite; break;
            case ItemType.Protractor: itemImage.sprite = protractorSprite; break;
            case ItemType.Basketball: itemImage.sprite = basketballSprite; break;
            case ItemType.Paintbrush: itemImage.sprite = paintbrushSprite; break;
            case ItemType.Apple: itemImage.sprite = appleSprite; break;
            case ItemType.PrankKit: itemImage.sprite = prankKitSprite; break;
            default: itemImage.sprite = noneSprite; break;
        }
    }

    // --- Temporary messages ---
    public void ShowTemporaryMessage(string message, Color? color = null)
    {
        if (tempMessageText == null || tempMessageCanvasGroup == null)
        {
            Debug.LogWarning($"{name}: Cannot show temp message, references missing!");
            return;
        }

        // Set message and color
        tempMessageText.text = message;
        tempMessageText.color = color ?? defaultMessageColor;

        // Stop existing coroutine if running
        if (tempMessageCoroutine != null)
            StopCoroutine(tempMessageCoroutine);

        tempMessageCoroutine = StartCoroutine(ShowTempMessageCoroutine());
    }

    private IEnumerator ShowTempMessageCoroutine()
    {
        tempMessageCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(tempMessageDuration);

        float fadeDuration = 0.5f;
        float t = 0f;
        float startAlpha = tempMessageCanvasGroup.alpha;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            tempMessageCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
            yield return null;
        }

        tempMessageCanvasGroup.alpha = 0f;
        tempMessageCoroutine = null;
    }
}