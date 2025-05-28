using System;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LevelEditorManager : MonoBehaviour
{
    public TilePanel tp;
    public static LevelEditorManager Instance;
    //public Tilemap minimap;
    public bool cameraDrag = true;
    public TMP_InputField levelName;
    public Button saveLevelButton;
    public string selectedTile;
    private LevelData level = new LevelData();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(this.gameObject);
        cameraDrag = true;
        selectedTile = "Grass";
        TileRegistry.LoadTilesFromResources();
        levelName.text = DatabaseManager.Instance.CurrentLevel.name;
        if (string.IsNullOrEmpty(levelName.text)) saveLevelButton.interactable = false;
        levelName.onEndEdit.AddListener(CheckLevelName);
    }

    public void CheckLevelName(string text)
    {
        if (string.IsNullOrEmpty(text)) saveLevelButton.interactable = false;
        else saveLevelButton.interactable = true;   
    }

    public void AddTile(string Tilename, Vector3Int pos)
    {
        level.tiles[pos] = Tilename;
        //minimap.SetTile(pos, TileRegistry.GetTile(TileRegistry.GetMinimapAlt(Tilename)));
    }

    public void RemoveTIle(Vector3Int pos)
    {
        level.tiles[pos] = "";
        //minimap.SetTile(pos, null);
    }

    public void ChangeSelectedTile(string new_tile)
    {
        Debug.Log("selected tile : " + new_tile);
        selectedTile = new_tile;
        tp.TogglePanel();
    }
    public void ChangeCamDrag(bool new_val)
    {
        cameraDrag = new_val;
        Debug.Log("Cam toggle, new cam val : " + cameraDrag);
    }

    public void SaveLevel()
    {
        Debug.Log("saving content");
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int y = 0; y < 1000; y++)
        {
            for (int x = 0; x < 1000; x++)
            {
                sb.Append(level.tiles[new Vector3Int(x, y, 0)]);
                sb.Append(",");
            }
        }
        string levelContent = sb.ToString();
        Debug.Log("LEVEL content");
        var zippedLevelContent = ZipFunctions.Zip(levelContent);
        Debug.Log("zipped");
        try
        {
            Debug.Log("calling function");
            DatabaseManager.Instance.SaveLevel(levelName.text, DatabaseManager.Instance.CurrentLevel.id, zippedLevelContent);
            Debug.Log("success");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

}
