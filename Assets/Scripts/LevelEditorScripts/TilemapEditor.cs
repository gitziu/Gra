using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems; // Make sure this is present if you use UI

public class TilemapEditor : MonoBehaviour
{
    public Tilemap tmap;
    private int minTile = 0, maxTile = 999;
    private Vector3 mouse_pos;

    // Use a variable to store the tile name *before* calling AddTile
    private string tileNameBeforeAdd;

    void Start()
    {
        // No custom start logic needed for now
    }

    void Update()
    {
        if (LevelEditorManager.Instance.cameraDrag) return;

        if (Input.GetMouseButtonDown(0)) // Changed to GetMouseButtonDown for single click action
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            mouse_pos = Input.mousePosition;
            Vector3 world_pos = Camera.main.ScreenToWorldPoint(mouse_pos);
            world_pos.z = 0;
            Vector3Int tilemap_pos = tmap.WorldToCell(world_pos);

            if (!(tilemap_pos.x >= minTile && tilemap_pos.x <= maxTile && tilemap_pos.y >= minTile && tilemap_pos.y <= maxTile)) return;
            if (!IsPointInCameraView(world_pos)) return;

            // Only place if the tile is different from the one currently there
            // This prevents continuous placement of the same tile type if holding mouse down
            if (tmap.GetTile(tilemap_pos) == TileRegistry.GetTile(LevelEditorManager.Instance.selectedTile)) return;
            
            Debug.Log("Attempting to add new tile to pos : " + tilemap_pos + " tile name : " + LevelEditorManager.Instance.selectedTile);
            
            // Store the tile name *before* calling AddTile, as AddTile might change LevelEditorManager.Instance.selectedTile
            tileNameBeforeAdd = LevelEditorManager.Instance.selectedTile;

            // First, update the visual Tilemap
            tmap.SetTile(tilemap_pos, TileRegistry.GetTile(tileNameBeforeAdd));
            
            // Then, inform LevelEditorManager about the placement.
            // This call might change LevelEditorManager.Instance.selectedTile if it's Player/Exit.
            LevelEditorManager.Instance.AddTile(tileNameBeforeAdd, tilemap_pos);
        }
        else if (Input.GetMouseButtonDown(1)) // Changed to GetMouseButtonDown for single click action
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            mouse_pos = Input.mousePosition;
            Vector3 world_pos = Camera.main.ScreenToWorldPoint(mouse_pos);
            world_pos.z = 0;
            Vector3Int tilemap_pos = tmap.WorldToCell(world_pos);

            if (!(tilemap_pos.x >= minTile && tilemap_pos.x <= maxTile && tilemap_pos.y >= minTile && tilemap_pos.y <= maxTile)) return;
            if (!IsPointInCameraView(world_pos)) return;
            if (tmap.GetTile(tilemap_pos) == null) return; // Only remove if there's a tile

            Debug.Log("removing tile on pos : " + tilemap_pos);
            
            // Clear the visual tilemap first
            tmap.SetTile(tilemap_pos, null);
            
            // Then, inform LevelEditorManager about the removal.
            LevelEditorManager.Instance.RemoveTIle(tilemap_pos);
        }
    }
    
    // (Your IsPointInCameraView function, no changes)
    bool IsPointInCameraView(Vector3 worldPoint)
    {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(worldPoint);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
               viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
               viewportPoint.z >= 0;
    }
}