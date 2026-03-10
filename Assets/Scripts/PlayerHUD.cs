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
    public Sprite noneSprite;
    public Sprite protractorSprite;
    public Sprite basketballSprite;
    public Sprite paintbrushSprite;
    public Sprite bookSprite;
    public Sprite rubiksCubeSprite;
    public Sprite cricketBatSprite;
    public Sprite paintTinSprite;

    [Header("Temporary Message Settings")]
    public float tempMessageDuration = 1.5f;
    public Color defaultMessageColor = Color.white;

    private Coroutine tempMessageCoroutine;
    private PlayerController playerController;

    void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();

        if (tempMessageText == null)
            tempMessageText = GetComponentInChildren<TextMeshProUGUI>();

        if (tempMessageCanvasGroup == null)
            tempMessageCanvasGroup = GetComponentInChildren<CanvasGroup>();

        if (tempMessageCanvasGroup != null)
            tempMessageCanvasGroup.alpha = 0f;

        if (itemImage != null)
            itemImage.sprite = noneSprite;
    }

    void Update()
    {
        if (playerController == null) return;

        UpdateBallots(playerController.GetBallotCount());
    }

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
            case ItemType.MathSet: itemImage.sprite = protractorSprite; break;
            case ItemType.Football: itemImage.sprite = basketballSprite; break;
            case ItemType.PaintBrush: itemImage.sprite = paintbrushSprite; break;
            case ItemType.Book: itemImage.sprite = bookSprite; break;
            case ItemType.RubiksCube: itemImage.sprite = rubiksCubeSprite; break;
            case ItemType.CricketBat: itemImage.sprite = cricketBatSprite; break;
            case ItemType.PaintTin: itemImage.sprite = paintTinSprite; break;
            default: itemImage.sprite = noneSprite; break;
        }
    }

    public void ShowTemporaryMessage(string message, Color? color = null)
    {
        if (tempMessageText == null || tempMessageCanvasGroup == null)
            return;

        tempMessageText.text = message;
        tempMessageText.color = color ?? defaultMessageColor;

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