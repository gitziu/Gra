using System;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LevelGameplayManager : MonoBehaviour
{
    public static LevelGameplayManager Instance;
    private Tilemap Platforms, Border, Obstacles;
    [SerializeField] GameObject CollectiblePrefab, EnemyPrefab, ExitPrefab, PlayerPrefab;
    private Button ExitLevel;
    public Vector3 PlayerSpawnPoint;
    public int collectedCollectibles = 0;
    public int attempts = 1, successful = 0;
    private int totalCollectibles = 0;
    private Transform LevelEndPopUp, RatePanel, ErrorPanel;
    public bool levelEnd = false;


    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        TileRegistry.LoadTilesFromResources();
        ExitLevel = GameObject.Find("Canvas/BackButton").transform.GetComponent<Button>();
        LevelEndPopUp = GameObject.Find("Canvas/LevelEndPopUp").transform;
        ErrorPanel = GameObject.Find("Canvas/ErrorPanel").transform;
        RatePanel = GameObject.Find("Canvas/RatePanel").transform;
        GameObject.Find("Canvas/LevelEndPopUp/CloseButton").transform.GetComponent<Button>().onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            levelEnd = false;
            SceneLoader.LoadPreviousScene();
        });
        RatePanel.Find("RatingInput").GetComponent<TMP_InputField>().onEndEdit.AddListener(checkIfSubmit);
        RatePanel.Find("ExitRating").GetComponent<Button>().onClick.AddListener(switchToRate);
        RatePanel.Find("SubmitButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            try
            {
                Debug.Log(int.Parse(RatePanel.Find("RatingInput").GetComponent<TMP_InputField>().text) / 100.0);
                DatabaseManager.Instance.UpdateRatings(int.Parse(RatePanel.Find("RatingInput").GetComponent<TMP_InputField>().text) / 100.0);
            }
            catch (Exception e)
            {
                DisplayError(e);
                return;
            }
            Time.timeScale = 1f;
            levelEnd = false;
            SceneLoader.LoadPreviousScene();
        });
        LevelEndPopUp.Find("RateButton").GetComponent<Button>().onClick.AddListener(switchToRate);
        ErrorPanel.Find("CloseError").GetComponent<Button>().onClick.AddListener(toggleErrorPanel);
        ErrorPanel.gameObject.SetActive(false);
        RatePanel.gameObject.SetActive(false);
        LevelEndPopUp.gameObject.SetActive(false);
        ExitLevel.onClick.AddListener(() => triggerLevelEnd(false));
        Time.timeScale = 0f;
        Obstacles = GameObject.Find("Grid/Obstacles").transform.GetComponent<Tilemap>();
        Border = GameObject.Find("Grid/Border").transform.GetComponent<Tilemap>();
        Platforms = GameObject.Find("Grid/Ground").transform.GetComponent<Tilemap>();
        SetBorder();
        LoadLevel();
        Time.timeScale = 1f;
    }

    void checkIfSubmit(string s)
    {
        if (string.IsNullOrEmpty(s)) RatePanel.Find("SubmitButton").GetComponent<Button>().interactable = false;
        else
        {
            int entered_value = int.Parse(s);
            if (entered_value < 0) RatePanel.Find("RatingInput").GetComponent<TMP_InputField>().text = "0";
            else if (entered_value > 100) RatePanel.Find("RatingInput").GetComponent<TMP_InputField>().text = "100";
            RatePanel.Find("SubmitButton").GetComponent<Button>().interactable = true;
        }
    }

    void SetBorder()
    {
        for (int i = -1; i <= 1000; i++)
        {
            Border.SetTile(new Vector3Int(-1, i, 0), TileRegistry.GetTile("Border"));
            Border.SetTile(new Vector3Int(i, -1, 0), TileRegistry.GetTile("Border"));
            Border.SetTile(new Vector3Int(1000, -i, 0), TileRegistry.GetTile("Border"));
            Border.SetTile(new Vector3Int(i, 1000, 0), TileRegistry.GetTile("Border"));

        }
    }

    void LoadLevel()
    {
        string[] levelContent = DatabaseManager.Instance.CurrentLevel.content.Split(",");
        Vector3 cellSize = Platforms.cellSize;
        Vector3 centerOffset = new Vector3(cellSize.x / 2f, cellSize.y / 2f, 0f);
        for (int y = 0; y < 1000; y++)
        {
            for (int x = 0; x < 1000; x++)
            {
                string s = levelContent[y * 1000 + x];
                if (string.IsNullOrEmpty(s)) continue;
                Debug.Log("tile : " + s);
                Vector3 cellBottomLeftWorldPos = Platforms.CellToWorld(new Vector3Int(x, y, 0));
                Vector3 spawnPosition = cellBottomLeftWorldPos + centerOffset;
                if (s == "Player") PlayerSpawnPoint = spawnPosition;
                if (s == "Collectible") totalCollectibles++;
                if (s == "Collectible") Instantiate(CollectiblePrefab, spawnPosition, Quaternion.identity);
                else if (s == "Enemy") Instantiate(EnemyPrefab, spawnPosition, Quaternion.identity);
                else if (s == "Exit") Instantiate(ExitPrefab, spawnPosition, Quaternion.identity);
                else if (s == "Player") Instantiate(PlayerPrefab, spawnPosition, Quaternion.identity);
                else
                {
                    if (s == "Platform") Platforms.SetTile(new Vector3Int(x, y, 0), TileRegistry.GetTile(s));
                    else Obstacles.SetTile(new Vector3Int(x, y, 0), TileRegistry.GetTile(s));
                }
            }
        }
    }

    public void triggerLevelEnd(bool levelFinished)
    {
        Time.timeScale = 0f;
        levelEnd = true;
        LevelEndPopUp.gameObject.SetActive(true);
        LevelEndPopUp.Find("CompletionText").GetComponent<TMP_Text>().text = levelFinished ? "Level Complete!" : "Level Failed";
        if (!levelFinished) LevelEndPopUp.Find("RateButton").GetComponent<Button>().interactable = false;
        else LevelEndPopUp.Find("RateButton").GetComponent<Button>().interactable = true;
        LevelEndPopUp.Find("Deaths").GetComponent<TMP_Text>().text = "Deaths : " + (attempts - 1);
        LevelEndPopUp.Find("Collectibles").GetComponent<TMP_Text>().text = "Collectibles : " + collectedCollectibles + " / " + totalCollectibles;
        try
        {
            DatabaseManager.Instance.UpdateAttempts(attempts, successful);
        }
        catch (Exception e)
        {
            DisplayError(e);
        }
    }

    public void switchToRate()
    {
        LevelEndPopUp.gameObject.SetActive(!LevelEndPopUp.gameObject.activeSelf);
        RatePanel.gameObject.SetActive(!RatePanel.gameObject.activeSelf);
        RatePanel.Find("RatingInput").GetComponent<TMP_InputField>().text = "";
        RatePanel.Find("SubmitButton").GetComponent<Button>().interactable = false;
    }

    void toggleErrorPanel()
    {
        ErrorPanel.gameObject.SetActive(!ErrorPanel.gameObject.activeSelf);
    }

    public void DisplayError(Exception e)
    {
        toggleErrorPanel();
        ErrorPanel.Find("ErrorMessage").GetComponent<TMP_Text>().text = e.Message;
    }

}
