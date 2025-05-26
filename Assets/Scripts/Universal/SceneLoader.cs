using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    static Stack<string> sceneStack = new Stack<string>();

    public static void LoadPreviousScene()
    {
        SceneManager.LoadScene(sceneStack.Pop());
    }

    public static void LoadScene(string sceneName)
    {
        if (DatabaseManager.Instance.CurrentLevel == null) DatabaseManager.Instance.CurrentLevel = new DatabaseManager.BasicLevelData() {name = "", id = -1, author = DatabaseManager.Instance.CurrentUser.username};
        sceneStack.Push(SceneManager.GetActiveScene().name);
        SceneManager.LoadScene(sceneName);
    }

}
