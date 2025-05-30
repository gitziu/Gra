using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class TilemapEditor : MonoBehaviour
{
    public Tilemap tmap;
    private int minTile = 0, maxTile = 999;
    private Vector3 mouse_pos;


    void Update()
    {
        if (LevelEditorManager.Instance.cameraDrag) return;

        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            mouse_pos = Input.mousePosition;
            Vector3 world_pos = Camera.main.ScreenToWorldPoint(mouse_pos);
            world_pos.z = 0;
            Vector3Int tilemap_pos = tmap.WorldToCell(world_pos);

            if (!(tilemap_pos.x >= minTile && tilemap_pos.x <= maxTile && tilemap_pos.y >= minTile && tilemap_pos.y <= maxTile)) return;
            if (!IsPointInCameraView(world_pos)) return;
            if (tmap.GetTile(tilemap_pos) == TileRegistry.GetTile(LevelEditorManager.Instance.selectedTile)) return;
            
            Debug.Log("Attempting to add new tile to pos : " + tilemap_pos + " tile name : " + LevelEditorManager.Instance.selectedTile);
            tmap.SetTile(tilemap_pos, TileRegistry.GetTile(LevelEditorManager.Instance.selectedTile));
            LevelEditorManager.Instance.AddTile(LevelEditorManager.Instance.selectedTile, tilemap_pos);
        }
        else if (Input.GetMouseButton(1))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            mouse_pos = Input.mousePosition;
            Vector3 world_pos = Camera.main.ScreenToWorldPoint(mouse_pos);
            world_pos.z = 0;
            Vector3Int tilemap_pos = tmap.WorldToCell(world_pos);

            if (!(tilemap_pos.x >= minTile && tilemap_pos.x <= maxTile && tilemap_pos.y >= minTile && tilemap_pos.y <= maxTile)) return;
            if (!IsPointInCameraView(world_pos)) return;
            if (tmap.GetTile(tilemap_pos) == null) return;

            Debug.Log("removing tile on pos : " + tilemap_pos);
            tmap.SetTile(tilemap_pos, null);
            LevelEditorManager.Instance.RemoveTIle(tilemap_pos);
        }
    }
    
    bool IsPointInCameraView(Vector3 worldPoint)
    {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(worldPoint);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
               viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
               viewportPoint.z >= 0;
    }
}