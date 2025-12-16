using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Header("Parallax")]
    // How much this layer moves relative to camera (0 = no movement, 1 = moves with camera)
    public float parallaxFactor = 0.5f;
    
    private Vector3 startPosition;
    private Vector3 lastCameraPosition;
    
    void Start()
    {
        lastCameraPosition = Camera.main.transform.position;
        startPosition = transform.position;
    }
    
    void LateUpdate()
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 cameraMovement = cameraPosition - lastCameraPosition;
        
        // Move this background at a fraction of the camera's movement
        transform.position += new Vector3(
            cameraMovement.x * parallaxFactor,
            cameraMovement.y * parallaxFactor,
            0
        );
        
        lastCameraPosition = cameraPosition;
    }
}