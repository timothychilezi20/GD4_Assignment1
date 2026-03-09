using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public string ItemName;
    public ItemType ItemType;

    [HideInInspector] public ItemSpawnPoint spawnPoint;
    [HideInInspector] public ItemSpawner spawner;

    public void TryPickUp(PlayerController player)
    {
        player.PickUpItem(this);

        // Notify spawn point and spawner
        if (spawnPoint != null)
            spawnPoint.ClearItem();

        if (spawner != null)
            spawner.Respawn(spawnPoint);

        // Disable object (optional)
        gameObject.SetActive(false);
    }

    public void DropItem(Vector3 position)
    {
        transform.SetParent(null);
        transform.position = position;
        gameObject.SetActive(true);
    }
}