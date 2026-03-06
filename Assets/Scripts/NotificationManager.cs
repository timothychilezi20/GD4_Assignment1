using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("Notification Settings")]
    [SerializeField] private GameObject notificationPrefab; // prefab with TextMeshProUGUI
    [SerializeField] private Transform notificationParent; // UI canvas parent
    [SerializeField] private float floatDuration = 1.5f;
    [SerializeField] private Vector3 floatOffset = new Vector3(0, 2f, 0);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Spawn a notification in world space
    public void SpawnNotification(string text, Color color, Vector3 worldPosition)
    {
        if (notificationPrefab == null || notificationParent == null) return;

        GameObject notif = Instantiate(notificationPrefab, notificationParent);
        notif.transform.position = worldPosition + floatOffset;

        TextMeshProUGUI tmp = notif.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
        }

        StartCoroutine(FloatAndFade(notif, floatDuration));
    }

    private IEnumerator FloatAndFade(GameObject obj, float duration)
    {
        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        Vector3 startPos = obj.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            obj.transform.position = startPos + Vector3.up * (t * 2f);

            if (tmp != null)
                tmp.alpha = 1f - t;

            yield return null;
        }

        Destroy(obj);
    }
}