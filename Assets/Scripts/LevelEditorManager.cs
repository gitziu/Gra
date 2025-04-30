using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelEditorManager : MonoBehaviour
{
    public TilePanel tp;
    public LevelEditorManager Instance;
    public Tilemap minimap;
    public bool cameraDrag = true;
    public string selectedTile;
    private LevelData level = new LevelData();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;   
        cameraDrag = true;
        selectedTile = "Grass";
        TileRegistry.LoadTilesFromResources();
    }

    public void AddTile(string Tilename, Vector3Int pos){
        level.tiles.Add(new TileData(pos, Tilename));
        minimap.SetTile(pos, TileRegistry.GetTile(TileRegistry.GetMinimapAlt(Tilename)));
    }

    public void RemoveTIle(Vector3Int pos){
        level.tiles.Remove(level.tiles.Find(TileData => TileData.position == pos));
        minimap.SetTile(pos, null);
    }

    public void ChangeSelectedTile(string new_tile){
        Debug.Log("selected tile : " + new_tile);
        selectedTile = new_tile;
        tp.TogglePanel();
    }
    public void ChangeCamDrag(bool new_val){
        cameraDrag = new_val;
        Debug.Log("Cam toggle, new cam val : " + cameraDrag);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
