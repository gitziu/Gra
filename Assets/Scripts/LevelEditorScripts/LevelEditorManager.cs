using System;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LevelEditorManager : MonoBehaviour
{
    public TilePanel tp;
    public static LevelEditorManager Instance;
    public bool cameraDrag = true;
    public TMP_InputField levelName;
    public Button saveLevelButton;
    public string selectedTile;

    private LevelData level = new LevelData();

    private Vector3Int? currentPlayerPos = null;
    private Vector3Int? currentExitPos = null;

    private Button playerButton;
    private Button exitButton;

    private int platform_num = -1;

    public Tilemap tilemap;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(this.gameObject);
        cameraDrag = true;
        selectedTile = "platform0";
        TileRegistry.LoadTilesFromResources();
        levelName.text = DatabaseManager.Instance.CurrentLevel.name;
        levelName.onEndEdit.AddListener(CheckLevelName);
        playerButton = GameObject.Find("Canvas/ObjectSelection/ObjectPanel/Player").transform.GetComponent<Button>();
        exitButton = GameObject.Find("Canvas/ObjectSelection/ObjectPanel/Exit").transform.GetComponent<Button>();
        checkIfSavable();
        InitializeLevelEditorState();
    }

    private void InitializeLevelEditorState()
    {
        currentPlayerPos = null;
        currentExitPos = null;
        LoadLevel();
        AddBorder();
        for (int y = 0; y < 1000; y++)
        {
            for (int x = 0; x < 1000; x++)
            {
                Vector3Int currentPos = new Vector3Int(x, y, 0);
                string tileName = level.tiles[currentPos];

                if (tileName == "Player" && !currentPlayerPos.HasValue)
                {
                    currentPlayerPos = currentPos;
                }
                else if (tileName == "Exit" && !currentExitPos.HasValue)
                {
                    currentExitPos = currentPos;
                }
                if (currentPlayerPos.HasValue && currentExitPos.HasValue)
                {
                    return;
                }
            }
        }
    }

    private void AddBorder()
    {
        for (int i = -1; i <= 1000; i++)
        {
            tilemap.SetTile(new Vector3Int(i, -1, 0), TileRegistry.GetTile("Border"));
            tilemap.SetTile(new Vector3Int(i, 1000, 0), TileRegistry.GetTile("Border"));
            tilemap.SetTile(new Vector3Int(-1, i, 0), TileRegistry.GetTile("Border"));
            tilemap.SetTile(new Vector3Int(1000, i, 0),TileRegistry.GetTile("Border"));
        }
    }

    private void LoadLevel()
    {
        if (string.IsNullOrEmpty(DatabaseManager.Instance.CurrentLevel.content)) return;
        string[] tiles = DatabaseManager.Instance.CurrentLevel.content.Split(",");
        for (int y = 0; y < 1000; y++)
        {
            for (int x = 0; x < 1000; x++)
            {
                string tile = tiles[y * 1000 + x];
                if (string.IsNullOrEmpty(tile)) continue;
                level.tiles[new Vector3Int(x, y, 0)] = tile;
                tilemap.SetTile(new Vector3Int(x, y, 0), TileRegistry.GetTile(tile));
                if (tile == "Player")
                {
                    currentPlayerPos = new Vector3Int(x, y, 0);
                }
                if (tile == "Exit")
                {
                    currentExitPos = new Vector3Int(x, y, 0);
                }
            }
        }
    }

    public void CheckLevelName(string text)
    {
        checkIfSavable();
    }

    public void checkIfSavable()
    {
        bool isPlayerPlaced = currentPlayerPos.HasValue;
        bool isExitPlaced = currentExitPos.HasValue;

        if (isPlayerPlaced && isExitPlaced && !string.IsNullOrEmpty(levelName.text))
        {
            saveLevelButton.interactable = true;
        }
        else
        {
            saveLevelButton.interactable = false;
        }
    }

    public void AddTile(string Tilename, Vector3Int pos)
    {
        string existingTileAtPos = level.tiles[pos];
        if (existingTileAtPos == "Player" && Tilename != "Player")
        {
            currentPlayerPos = null;
        }
        else if (existingTileAtPos == "Exit" && Tilename != "Exit")
        {
            currentExitPos = null;
        }
        if (Tilename == "Player")
        {
            if (currentPlayerPos.HasValue && currentPlayerPos.Value != pos)
            {
                level.tiles[currentPlayerPos.Value] = "";
                tilemap.SetTile(pos, null);
                tilemap.SetTile(currentPlayerPos.Value, null);
            }
            currentPlayerPos = pos;
        }
        else if (Tilename == "Exit")
        {
            if (currentExitPos.HasValue && currentExitPos.Value != pos)
            {
                level.tiles[currentExitPos.Value] = "";
                tilemap.SetTile(pos, null);
                tilemap.SetTile(currentExitPos.Value, null);
            }
            currentExitPos = pos;
        }
        level.tiles[pos] = Tilename;
        checkIfSavable();
    }


    public void RemoveTIle(Vector3Int pos)
    {
        string tileBeingRemoved = level.tiles[pos];
        if (tileBeingRemoved == "Player")
        {
            currentPlayerPos = null;
        }
        else if (tileBeingRemoved == "Exit")
        {
            currentExitPos = null;
        }
        level.tiles[pos] = "";
        checkIfSavable();
    }

    public void ChangeSelectedTile(string new_tile)
    {
        Debug.Log("selected tile : " + new_tile);
        if (new_tile == "platform")
        {
            platform_num++;
            platform_num %= 4;
            new_tile += platform_num.ToString();
        }
        selectedTile = new_tile;
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
                Vector3Int currentPos = new Vector3Int(x, y, 0);
                sb.Append(level.tiles[currentPos]);
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
            Debug.LogError("Error saving level: " + e.Message);
        }
    }
}