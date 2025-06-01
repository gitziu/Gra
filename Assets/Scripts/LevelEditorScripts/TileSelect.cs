using UnityEngine;
using UnityEngine.UI;

public class TileSelect : MonoBehaviour
{
    public Button btn;
    public string selectabletile;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        btn.onClick.AddListener(updatetile);
    }

    void updatetile(){
        transform.parent.GetComponent<TilePanel>().TogglePanel();
        LevelEditorManager.Instance.ChangeSelectedTile(selectabletile);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
