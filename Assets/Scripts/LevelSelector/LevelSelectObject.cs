using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectObject : MonoBehaviour
{
    private int level_id;
    private int author_id;

    public void Initialize(DatabaseManager.Level levelData)
    {
        level_id = levelData.id;
        author_id = levelData.Uid;
        transform.Find("Name").GetComponent<TMP_Text>().text = levelData.name;
        transform.Find("Author").GetComponent<TMP_Text>().text = levelData.author;
        transform.Find("UpdateDate").GetComponent<TMP_Text>().text = levelData.updated.ToString();
        transform.Find("CreateDate").GetComponent<TMP_Text>().text = levelData.created.ToString();
        transform.Find("Rating").GetComponent<TMP_Text>().text = "Rating : " + Convert.ToString((int)(levelData.rating * 100));
        transform.Find("SuccessRatio").GetComponent<TMP_Text>().text = "Success Ratio : " + Convert.ToString((int)(levelData.succesRatio * 100));
        if (DatabaseManager.Instance.CurrentUser.uid != author_id)
        {
            transform.Find("Edit").gameObject.SetActive(false);
            transform.Find("DeleteLevel").gameObject.SetActive(false);
        }
        transform.Find("Edit").GetComponent<Button>().onClick.AddListener(() => LoadLevel(true));
        transform.Find("Play").GetComponent<Button>().onClick.AddListener(() => LoadLevel(false));
        transform.Find("DeleteLevel").GetComponent<Button>().onClick.AddListener(deleteLevel);
    }

    void deleteLevel()
    {
        try
        {
            DatabaseManager.Instance.DeleteLevel(level_id);
            Destroy(gameObject);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            GameObject.Find("Canvas/SearchPanel").transform.GetComponent<SearchPanelManager>().displayError(e);
        }
    }

    void LoadLevel(bool editMode)
    {
        DatabaseManager.Instance.CurrentLevel = new DatabaseManager.BasicLevelData() { id = level_id, author = transform.Find("Author").GetComponent<TMP_Text>().text, name = transform.Find("Name").GetComponent<TMP_Text>().text, content = ZipFunctions.Unzip(DatabaseManager.Instance.LevelContent(level_id)) };
        SceneLoader.LoadScene(editMode ? "Level_Editor" : "Level_Play");
    }

}
