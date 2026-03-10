using UnityEngine;
using TMPro;
using System.Collections;

public class BallotRiskWarningUI : MonoBehaviour
{
    public static BallotRiskWarningUI Instance { get; private set; }

    [System.Serializable]
    public class PlayerWarningUI
    {
        public GameObject panel;
        public TextMeshProUGUI text;
        public CanvasGroup canvasGroup;
    }

    [Header("Player UI")]
    [SerializeField] private PlayerWarningUI player1UI;
    [SerializeField] private PlayerWarningUI player2UI;

    [Header("Timing")]
    [SerializeField] private float fadeSpeed = 6f;

    private bool player1Visible = false;
    private bool player2Visible = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeUI(player1UI);
        InitializeUI(player2UI);
    }

    private void InitializeUI(PlayerWarningUI ui)
    {
        if (ui.panel != null)
            ui.panel.SetActive(false);

        if (ui.canvasGroup != null)
            ui.canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        UpdateUI(player1UI, player1Visible);
        UpdateUI(player2UI, player2Visible);
    }

    private void UpdateUI(PlayerWarningUI ui, bool shouldBeVisible)
    {
        if (ui.panel == null || ui.canvasGroup == null)
            return;

        if (shouldBeVisible && !ui.panel.activeSelf)
            ui.panel.SetActive(true);

        float targetAlpha = shouldBeVisible ? 1f : 0f;
        ui.canvasGroup.alpha = Mathf.MoveTowards(
            ui.canvasGroup.alpha,
            targetAlpha,
            fadeSpeed * Time.unscaledDeltaTime
        );

        if (!shouldBeVisible && ui.canvasGroup.alpha <= 0f && ui.panel.activeSelf)
            ui.panel.SetActive(false);
    }

    public void ShowWarning(int playerNumber, string message)
    {
        if (playerNumber == 1)
        {
            if (player1UI.text != null)
                player1UI.text.text = message;

            player1Visible = true;
        }
        else if (playerNumber == 2)
        {
            if (player2UI.text != null)
                player2UI.text.text = message;

            player2Visible = true;
        }
    }

    public void HideWarning(int playerNumber)
    {
        if (playerNumber == 1)
            player1Visible = false;
        else if (playerNumber == 2)
            player2Visible = false;
    }

    public void HideAllWarnings()
    {
        player1Visible = false;
        player2Visible = false;
    }
}