using UnityEngine;
using UnityEngine.Tilemaps;

public class Door : MonoBehaviour
{
    private Tilemap tilemap;

    void Start()
    {
        // get Tilemap 
        tilemap = GetComponent<Tilemap>();
    }

    
    public void OpenDoor()
    {
        // delete all door  Tile
        tilemap.ClearAllTiles();
        Debug.Log("Door opened!");
    }
}
