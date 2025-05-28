using UnityEngine;
using UnityEngine.UI;

public class SaveLevelButton : MonoBehaviour
{
    private Button btn;

    void Awake()
    {
        btn = transform.GetComponent<Button>();
        btn.onClick.AddListener(SaveLevel);
    }

    void SaveLevel()
    {
        Debug.Log("button clicked");
        LevelEditorManager.Instance.SaveLevel();
    }

}
