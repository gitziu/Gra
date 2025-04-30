using UnityEngine;

public class TilePanel : MonoBehaviour
{
    public GameObject menuPanel;

    void Start()
    {
        menuPanel.SetActive(false);
    }

    public void TogglePanel(){
        menuPanel.SetActive(!menuPanel.activeSelf);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
