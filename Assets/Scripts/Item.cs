using UnityEngine;

public class Item : MonoBehaviour
{
  public GroupType groupType;

    public void OnPickedUp(Transform holdPoint)
    {
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void OnDropped()
    {
        transform.SetParent(null);
    }
}
