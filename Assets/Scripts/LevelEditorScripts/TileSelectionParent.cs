using UnityEngine;
using UnityEngine.UI;

public class TileSelectionParent : MonoBehaviour
{
    public TilePanel tp;
    public Button self;
    void Start()
    {
        self.onClick.AddListener(EnableMenu);
    }

    public void EnableMenu(){
        tp.TogglePanel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
