using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGameplayManager : MonoBehaviour
{
    public static LevelGameplayManager Instance;
    private Tilemap Platforms, Border, Obstacles;
    [SerializeField] GameObject CollectiblePrefab, EnemyPrefab, ExitPrefab, PlayerPrefab;
    public Vector3 PlayerSpawnPoint;

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
        Time.timeScale = 0f;
        Obstacles = GameObject.Find("Grid/Obstacles").transform.GetComponent<Tilemap>();
        Border = GameObject.Find("Grid/Border").transform.GetComponent<Tilemap>();
        Platforms = GameObject.Find("Grid/Ground").transform.GetComponent<Tilemap>();
        SetBorder();
        LoadLevel();
        Time.timeScale = 1f;
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

}
