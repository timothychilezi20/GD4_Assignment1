using UnityEngine;
using TMPro;

public class CampaignObjectiveBar : MonoBehaviour
{
    public static CampaignObjectiveBar Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI subText;

    [Header("Default Messages")]
    [SerializeField] private string defaultObjective = "COLLECT BALLOTS → DUMP THEM → WIN";
    [SerializeField] private string defaultSubText = "Watch your reputation and stop your opponent.";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ResetObjective();
    }

    public void SetObjective(string mainText, string secondaryText = "")
    {
        if (objectiveText != null)
            objectiveText.text = mainText;

        if (subText != null)
            subText.text = secondaryText;
    }

    public void ResetObjective()
    {
        if (objectiveText != null)
            objectiveText.text = defaultObjective;

        if (subText != null)
            subText.text = defaultSubText;
    }
}