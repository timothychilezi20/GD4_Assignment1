using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("Player Assignment")]
    public int playerNumber = 1;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] TextMeshProUGUI totalPointsText;
    [SerializeField] private TextMeshProUGUI roundPointsText;
    [SerializeField] private TextMeshProUGUI heldVotesText;
    [SerializeField] private Slider roundProgressSlider;
    [SerializeField] private RawImage playerColorImage;
    [SerializeField] private TextMeshProUGUI multiplierText;

    [Header("Colours")]
    [SerializeField] private Color player1Color = Color.red;
    [SerializeField] private Color player2Color = Color.blue;

    [Header("Position")]
    [SerializeField] private bool isLeftSide = true;

    private PlayerPoints playerPoints;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (playerColorImage != null)
        {
            playerColorImage.color = playerNumber == 1 ? player1Color : player2Color;
        }

        if (playerNameText != null)
        {
            playerNameText.text = $"PLAYER {playerNumber}";
            playerNameText.color = playerNumber == 1 ? player1Color : player2Color;
        }

        //PositionUI();
    }

    void PositionUI()
    {
        if (rectTransform == null) return;

        if (playerNumber == 1)
        {
            // Player 1 UI on left side
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(20, -20);
        }
        else
        {
            // Player 2 UI on right side
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);
            rectTransform.anchoredPosition = new Vector2(-20, -20);
        }
    }

    public void RegisterPlayer(PlayerPoints points)
    {
        playerPoints = points;

        playerPoints.OnTotalPointsChanged += UpdateTotalPoints;
        playerPoints.OnRoundPointsChanged += UpdateRoundPoints;
        playerPoints.OnHeldVotesChanged += UpdateHeldVotes;

        UpdateTotalPoints(playerNumber, points.GetTotalPoints());
        UpdateRoundPoints(playerNumber, points.GetRoundPoints());
        UpdateHeldVotes(playerNumber, points.GetHeldVotes());
    }

    void UpdateTotalPoints(int playerNum, int total)
    {
        if (playerNum != playerNumber)
            return;

        if (totalPointsText != null)
        {
            totalPointsText.text = $"TOTAL: {total}";
        }
    }

    void UpdateRoundPoints(int playerNum, int round)
    {
        if (playerNum != playerNumber)
            return;

        if (roundPointsText != null)
        {
            roundPointsText.text = $"ROUND: {round}";
        }
    }

    void UpdateHeldVotes(int playerNum, int votes)
    {
        if (playerNum != playerNumber)
            return;

        if (heldVotesText != null)
        {
            heldVotesText.text = $"VOTES: {votes}";
        }

        if (roundProgressSlider != null)
        {
            roundProgressSlider.value = (float)votes / 50f;
        }
    }

    public void UpdateMultiplier(float multiplier)
    {
        if (multiplierText != null)
        {
            multiplierText.text = $"{multiplier:F1}x";
            multiplierText.gameObject.SetActive(multiplier > 1f);
        }
    }

    private void OnDestroy()
    {
        if (playerPoints != null)
        {
            playerPoints.OnTotalPointsChanged -= UpdateTotalPoints;
            playerPoints.OnRoundPointsChanged -= UpdateRoundPoints;
            playerPoints.OnHeldVotesChanged -= UpdateHeldVotes;
        }
    }
}
