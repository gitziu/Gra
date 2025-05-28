using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSceneButton : MonoBehaviour
{
    public string sceneName;

    void Awake()
    {
        transform.GetComponent<Button>().onClick.AddListener(ChangeScene);
    }

    void ChangeScene()
    {
        if (sceneName == "Level_Editor") DatabaseManager.Instance.CurrentLevel = new DatabaseManager.BasicLevelData() {id = -1, author = DatabaseManager.Instance.CurrentUser.username, name = "", content = ""};
        if (sceneName == "back") SceneLoader.LoadPreviousScene();
        else SceneLoader.LoadScene(sceneName);
    }

}
