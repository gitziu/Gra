using UnityEngine;
using UnityEngine.UI;

public class OnclickCameraDrag : MonoBehaviour
{
    public Button button;
    private bool camDrag = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button.onClick.AddListener(ChangeCamDrag);
    }

    void ChangeCamDrag(){
        camDrag = !camDrag;
        LevelEditorManager.Instance.ChangeCamDrag(camDrag);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
