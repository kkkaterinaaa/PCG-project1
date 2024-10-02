using UnityEngine;

public class CameraController : MonoBehaviour
{
    public int mapWidth = 15; 
    public int mapHeight = 15;
    public float tileSize = 1f;

    void Start()
    {
        CenterCamera();
    }

    void CenterCamera()
    {
        // Place the camera in the center of map
        float centerX = (mapWidth * tileSize) / 2f;
        float centerY = (mapHeight * tileSize) / 2f;
        
        Camera.main.transform.position = new Vector3(centerX, centerY, Camera.main.transform.position.z);
    }
}