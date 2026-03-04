using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ReputationUI : MonoBehaviour
{
    [Header("Player Assignment")]
    [SerializeField] private int playerNumber = 1;

    [Header("UI References")]
    [SerializeField] private GameObject reputationPanel;
    [SerializeField] private TextMeshProUGUI teacherRepText;
    [SerializeField] private TextMeshProUGUI athleteRepText;
    [SerializeField] private TextMeshProUGUI artistRepText;
    [SerializeField] private TextMeshProUGUI nerdRepText;
    [SerializeField] private TextMeshProUGUI grade8RepText;

    [Header("Progress Bars")]
    [SerializeField] private Slider teacherSlider;
    [SerializeField] private Slider athleteSlider;
    [SerializeField] private Slider artistSlider;
    [SerializeField] private Slider nerdSlider;
    [SerializeField] private Slider grade8Slider;

    [Header("Colors")]
    [SerializeField] private Color highRepColor = Color.green;
    [SerializeField] private Color mediumRepColor = Color.white;
    [SerializeField] private Color lowRepColor = Color.red;

    private Dictionary<NPCMovement.NPCGroup, TextMeshProUGUI> repTexts;
    private Dictionary<NPCMovement.NPCGroup, Slider> repSliders;

    void Start()
    {
        // Initialize dictionaries
        repTexts = new Dictionary<NPCMovement.NPCGroup, TextMeshProUGUI>
        {
            { NPCMovement.NPCGroup.Teacher, teacherRepText },
            { NPCMovement.NPCGroup.Athlete, athleteRepText },
            { NPCMovement.NPCGroup.Artist, artistRepText },
            { NPCMovement.NPCGroup.Nerd, nerdRepText },
            { NPCMovement.NPCGroup.Grade8, grade8RepText }
        };

        repSliders = new Dictionary<NPCMovement.NPCGroup, Slider>
        {
            { NPCMovement.NPCGroup.Teacher, teacherSlider },
            { NPCMovement.NPCGroup.Athlete, athleteSlider },
            { NPCMovement.NPCGroup.Artist, artistSlider },
            { NPCMovement.NPCGroup.Nerd, nerdSlider },
            { NPCMovement.NPCGroup.Grade8, grade8Slider }
        };

        // Subscribe to reputation changes
        if (ReputationManager.Instance != null)
        {
            ReputationManager.Instance.OnReputationChanged += UpdateReputationDisplay;

            // Initial update for all groups
            foreach (NPCMovement.NPCGroup group in System.Enum.GetValues(typeof(NPCMovement.NPCGroup)))
            {
                float rep = ReputationManager.Instance.GetReputation(playerNumber, group);
                UpdateReputationDisplay(playerNumber, group, rep);
            }
        }
    }

    void UpdateReputationDisplay(int playerNum, NPCMovement.NPCGroup group, float rep)
    {
        if (playerNum != playerNumber) return;

        // Update text
        if (repTexts.ContainsKey(group) && repTexts[group] != null)
        {
            string level = ReputationManager.Instance.GetReputationLevel(rep);
            repTexts[group].text = $"{group}: {level} ({rep:F2})";
            repTexts[group].color = ReputationManager.Instance.GetReputationColor(rep);
        }

        // Update slider
        if (repSliders.ContainsKey(group) && repSliders[group] != null)
        {
            float min = ReputationManager.Instance.minReputation;
            float max = ReputationManager.Instance.maxReputation;
            float sliderValue = Mathf.InverseLerp(min, max, rep);
            repSliders[group].value = sliderValue;

            // Color the slider fill
            Image fillImage = repSliders[group].fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = ReputationManager.Instance.GetReputationColor(rep);
            }
        }
    }

    void OnDestroy()
    {
        if (ReputationManager.Instance != null)
        {
            ReputationManager.Instance.OnReputationChanged -= UpdateReputationDisplay;
        }
    }

    // Toggle reputation panel (optional)
    public void ToggleReputationPanel()
    {
        if (reputationPanel != null)
            reputationPanel.SetActive(!reputationPanel.activeSelf);
    }
}