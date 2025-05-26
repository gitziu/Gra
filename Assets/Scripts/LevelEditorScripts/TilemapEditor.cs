using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class TilemapEditor : MonoBehaviour
{
    public Tilemap tmap;
    private int minTile = 0, maxTile = 999;
    private Vector3 mouse_pos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
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
            //Vector3Int min_tile = tmap.WorldToCell(Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)));
            //if (tilemap_pos.x < min_tile.x) return;
            if (tmap.GetTile(tilemap_pos) == TileRegistry.GetTile(LevelEditorManager.Instance.selectedTile)) return;
            Debug.Log("adding new tile to pos : " + tilemap_pos + " tile name : " + LevelEditorManager.Instance.selectedTile);
            tmap.SetTile(tilemap_pos, TileRegistry.GetTile(LevelEditorManager.Instance.selectedTile));
            LevelEditorManager.Instance.AddTile(LevelEditorManager.Instance.selectedTile, tilemap_pos);
        }
        if (Input.GetMouseButton(1))
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
        // Konwertujemy punkt świata na współrzędne Viewport (0-1)
        // (0,0) to lewy dół, (1,1) to prawy góra.
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(worldPoint);

        // Sprawdzamy, czy punkt jest w zakresie [0,1] dla X i Y
        // Z >= 0 oznacza, że punkt nie jest za kamerą (ważne dla kamer perspektywicznych, ale bezpieczne dla ortograficznych)
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
               viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
               viewportPoint.z >= 0;
    }

}
