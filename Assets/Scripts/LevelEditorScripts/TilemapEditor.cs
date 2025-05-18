using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class TilemapEditor : MonoBehaviour
{
    public LevelEditorManager man;
    public Tilemap tmap;
    private Vector3 mouse_pos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(man.Instance.cameraDrag) return;
        if(Input.GetMouseButton(0)){
            if(EventSystem.current.IsPointerOverGameObject()) return;
            mouse_pos = Input.mousePosition;
            Vector3 world_pos = Camera.main.ScreenToWorldPoint(mouse_pos);
            Vector3Int tilemap_pos = tmap.WorldToCell(world_pos);
            Vector3Int min_tile = tmap.WorldToCell(Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)));
            if(tilemap_pos.x < min_tile.x) return;
            if(tmap.GetTile(tilemap_pos) == TileRegistry.GetTile(man.Instance.selectedTile)) return;
            Debug.Log("adding new tile to pos : " + tilemap_pos + " tile name : " + man.Instance.selectedTile);
            tmap.SetTile(tilemap_pos, TileRegistry.GetTile(man.Instance.selectedTile));
            man.Instance.AddTile(man.Instance.selectedTile, tilemap_pos);
        }
        if(Input.GetMouseButton(1)){
            if(EventSystem.current.IsPointerOverGameObject()) return;
            mouse_pos = Input.mousePosition;
            Vector3 world_pos = Camera.main.ScreenToWorldPoint(mouse_pos);
            Vector3Int tilemap_pos = tmap.WorldToCell(world_pos);
            if(tmap.GetTile(tilemap_pos) == null) return;
            Debug.Log("removing tile on pos : " + tilemap_pos);
            tmap.SetTile(tilemap_pos, null);
            man.Instance.RemoveTIle(tilemap_pos);
        }
    }
}
