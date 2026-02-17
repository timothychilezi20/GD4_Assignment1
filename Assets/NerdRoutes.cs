using UnityEngine;

public class NerdRoutes : MonoBehaviour
{
    public Transform[] OneWaypoints;
    public Transform[] TwoWaypoints;
    private NerdMovement nerdMovement;
    void Start()
    {
        nerdMovement = GetComponent<NerdMovement>();

        if (CompareTag("NerdOne"))
        {
            nerdMovement.waypoints = OneWaypoints;
        }
        else if (CompareTag("NerdTwo"))
        {
            nerdMovement.waypoints = TwoWaypoints;
        }
        
    }
    
}
