using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
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
