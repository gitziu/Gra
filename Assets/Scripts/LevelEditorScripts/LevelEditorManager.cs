using System;
using System.Linq;
using System.Collections.Generic; // Still needed for nullable Vector3Int
using TMPro;
using Unity.VisualScripting;
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
    
    // LevelData, where 'tiles' is your custom Board class
    private LevelData level = new LevelData();

    // Store the current positions of the Player and Exit tiles.
    // These are now the SOLE source of truth for where Player/Exit are.
    private Vector3Int? currentPlayerPos = null; 
    private Vector3Int? currentExitPos = null;   

    // References to the player and exit buttons to enable/disable them
    private Button playerButton;
    private Button exitButton;

    // Reference to the main Tilemap for visual updates when a tile is removed/moved
    public Tilemap mainTilemap; // IMPORTANT: Assign this in the Inspector!

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(this.gameObject);

        cameraDrag = true;
        selectedTile = "Platform";
        TileRegistry.LoadTilesFromResources();

        levelName.text = DatabaseManager.Instance.CurrentLevel.name;
        levelName.onEndEdit.AddListener(CheckLevelName);

        // Get references to buttons
        playerButton = GameObject.Find("Canvas/ObjectSelection/ObjectPanel/Player").transform.GetComponent<Button>();
        exitButton = GameObject.Find("Canvas/ObjectSelection/ObjectPanel/Exit").transform.GetComponent<Button>();

        // When loading the scene, if there's an existing level from the DatabaseManager,
        // it needs to be loaded into 'level.tiles' BEFORE InitializeLevelEditorState is called.
        // This is usually done by a separate loading mechanism (e.g., in a LevelLoader script
        // or a method called by DatabaseManager right after setting CurrentLevel).
        // For this code, I'll assume 'level.tiles' is already populated if a level is being edited.

        InitializeLevelEditorState(); 
        checkIfSavable(); 
    }

    // ---
    /// <summary>
    /// Initializes the editor state based on the current tiles in the LevelData.
    /// This is crucial when loading an existing level.
    /// </summary>
    private void InitializeLevelEditorState()
    {
        currentPlayerPos = null;
        currentExitPos = null;
        playerButton.interactable = true;
        exitButton.interactable = true;
        for (int y = 0; y < 1000; y++)
        {
            for (int x = 0; x < 1000; x++)
            {
                Vector3Int currentPos = new Vector3Int(x, y, 0);
                string tileName = level.tiles[currentPos];

                if (tileName == "Player" && !currentPlayerPos.HasValue)
                {
                    currentPlayerPos = currentPos;
                    playerButton.interactable = false;
                }
                else if (tileName == "Exit" && !currentExitPos.HasValue)
                {
                    currentExitPos = currentPos;
                    exitButton.interactable = false;
                }
                if (currentPlayerPos.HasValue && currentExitPos.HasValue)
                {
                    return; 
                }
            }
        }
    }

    // ---
    public void CheckLevelName(string text)
    {
        checkIfSavable();
    }

    // ---
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

    // ---
    public void AddTile(string Tilename, Vector3Int pos)
{
    // 1. Get the tile that *currently* exists at this position.
    string existingTileAtPos = level.tiles[pos]; 

    // 2. Handle if we're replacing an existing Player or Exit tile.
    // This re-enables their buttons and clears our internal tracking if *this specific*
    // player/exit tile is being replaced.
    if (existingTileAtPos == "Player" && currentPlayerPos.HasValue && currentPlayerPos.Value == pos)
    {
        currentPlayerPos = null;
        playerButton.interactable = true;
    }
    else if (existingTileAtPos == "Exit" && currentExitPos.HasValue && currentExitPos.Value == pos)
    {
        currentExitPos = null;
        exitButton.interactable = true;
    }

    // 3. Now, handle the *new* tile placement, especially for Player/Exit.
    if (Tilename == "Player")
    {
        // If a player already exists somewhere else, clear that old position first.
        if (currentPlayerPos.HasValue && currentPlayerPos.Value != pos)
        {
            // Clear the old player tile from the level data using your Board's indexer with an empty string.
            level.tiles[currentPlayerPos.Value] = ""; 
            // Also visually clear it on the main Tilemap.
            if (mainTilemap != null)
            {
                mainTilemap.SetTile(currentPlayerPos.Value, null);
            }
        }
        currentPlayerPos = pos;
        playerButton.interactable = false;
        selectedTile = "Platform"; // <-- ADDED THIS LINE HERE!
    }
    else if (Tilename == "Exit")
    {
        // Similar for Exit.
        if (currentExitPos.HasValue && currentExitPos.Value != pos)
        {
            // Clear the old exit tile from the level data using your Board's indexer with an empty string.
            level.tiles[currentExitPos.Value] = ""; 
            // Also visually clear it on the main Tilemap.
            if (mainTilemap != null)
            {
                mainTilemap.SetTile(currentExitPos.Value, null);
            }
        }
        currentExitPos = pos;
        exitButton.interactable = false;
        selectedTile = "Platform"; // <-- ADDED THIS LINE HERE!
    }

    // 4. Finally, place the new tile in the level data.
    level.tiles[pos] = Tilename; 
    
    checkIfSavable(); // Always re-check save button status after a tile change.
}

    // ---
    public void RemoveTIle(Vector3Int pos)
    {
        // Get the tile that *currently* exists at this position from your Board.
        string tileBeingRemoved = level.tiles[pos];

        if (tileBeingRemoved == "Player")
        {
            // Only update our internal state if the removed tile was the one we were tracking.
            if (currentPlayerPos.HasValue && currentPlayerPos.Value == pos)
            {
                currentPlayerPos = null; 
                playerButton.interactable = true; 
                selectedTile = "Platform"; // Reset selected tile
            }
        }
        else if (tileBeingRemoved == "Exit")
        {
            // Only update our internal state if the removed tile was the one we were tracking.
            if (currentExitPos.HasValue && currentExitPos.Value == pos)
            {
                currentExitPos = null; 
                exitButton.interactable = true; 
                selectedTile = "Platform"; // Reset selected tile
            }
        }
        
        // Clear the tile from your custom Board using the indexer with an empty string.
        // This will trigger the 'set' in your Board class to remove the entry.
        level.tiles[pos] = ""; 
        
        checkIfSavable(); // Always re-check save button status after a tile change.
    }

    // ---
    public void ChangeSelectedTile(string new_tile)
    {
        Debug.Log("selected tile : " + new_tile);
        selectedTile = new_tile;
    }

    // ---
    public void ChangeCamDrag(bool new_val)
    {
        cameraDrag = new_val;
        Debug.Log("Cam toggle, new cam val : " + cameraDrag);
    }

    // ---
    public void SaveLevel()
    {
        Debug.Log("saving content");
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Iterate through all 1000x1000 positions.
        // Your Board's indexer will correctly return "" for empty spots due to TryGetValue.
        for (int y = 0; y < 1000; y++)
        {
            for (int x = 0; x < 1000; x++)
            {
                Vector3Int currentPos = new Vector3Int(x, y, 0);
                sb.Append(level.tiles[currentPos]); // Uses your Board's indexer's get
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
            Debug.LogError("Error saving level: " + e.Message); // Use Debug.LogError for errors
        }
    }
}