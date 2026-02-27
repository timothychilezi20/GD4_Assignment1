using UnityEngine;

public class SharedCameraSystem : MonoBehaviour
{
    public Transform player1;
    public Transform player2;
    public float smoothSpeed = 5f; 
    public float minZoom = 10f;
    public float maxZoom = 20f;
    public float zoomLimiter = 50f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        Move();
        Zoom();
    }

    void Move()
    {
        Vector3 midpoint = (player1.position + player2.position) / 2f;
        Vector3 newPos = new Vector3(midpoint.x, transform.position.y, midpoint.z);

        transform.position = Vector3.Lerp(transform.position, newPos, smoothSpeed * Time.deltaTime);
    }

    void Zoom()
    {
        float distance = Vector3.Distance(player1.position, player2.position);
        float newZoom = Mathf.Lerp(maxZoom, minZoom, distance / zoomLimiter);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newZoom, Time.deltaTime);
    }   
}

