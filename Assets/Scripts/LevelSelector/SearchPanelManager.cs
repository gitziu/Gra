using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchPanelManager : MonoBehaviour
{
    public TMP_InputField author, levelName, minSuccesRatio, maxSuccesRatio, minRating, maxRating;
    public Toggle myLevels;
    public TMP_Dropdown order, sortColumn;
    public Button toggleSearch, searchButton;

    void Awake()
    {
        author = transform.Find("AuthorInput").GetComponent<TMP_InputField>();
        levelName = transform.Find("LevelNameInput").GetComponent<TMP_InputField>();
        minSuccesRatio = transform.Find("SuccesRatioParent").Find("MinAttemptRatio").GetComponent<TMP_InputField>();
        maxSuccesRatio = transform.Find("SuccesRatioParent").Find("MaxAttemptRatio").GetComponent<TMP_InputField>();
        myLevels = transform.Find("MyLevelsToggle").GetComponent<Toggle>();
        order = transform.Find("SortParent").Find("SortOrder").GetComponent<TMP_Dropdown>();
        sortColumn = transform.Find("SortParent").Find("OrderBy").GetComponent<TMP_Dropdown>();
        toggleSearch = transform.Find("ToggleSearchButton").GetComponent<Button>();
        searchButton = transform.Find("SearchButton").GetComponent<Button>();
        minRating = transform.Find("RatingParent/MinRating").GetComponent<TMP_InputField>();
        maxRating = transform.Find("RatingParent/MaxRating").GetComponent<TMP_InputField>();
        Debug.Log("author : " + author);
        Debug.Log("sortColumn : " + sortColumn);
        Debug.Log(transform.Find("SortParent/OrderBy").name);
        myLevels.isOn = false;
        toggleSearch.onClick.AddListener(TogglePanel);
        searchButton.onClick.AddListener(Search);
    }

    private int clamp(String value, int min, int max, int defaultValue) {
        int res;
        if (!int.TryParse(value, out res)) return defaultValue;
        if (res < min) return min;
        if (res > max) return max;
        return res;
    }

    public void Search()
    {
        var col = "";
        switch (sortColumn.value)
        {
            case 0: col = "update"; break;
            case 1: col = "created"; break;
            case 2: col = "succesRatio"; break;
            case 3: col = "rating"; break;
        }
        var levels = DatabaseManager.Instance.SearchLevels(author.text,
            levelName.text,
            myLevels.isOn,
            order.value != 2,
            col,
            clamp(minSuccesRatio.text, 0, 100, 0) / 100.0,
            clamp(maxSuccesRatio.text, 0, 100, 100) / 100.0,
            clamp(minRating.text, 0, 100, 0) / 100.0,
            clamp(maxRating.text, 0, 100, 100) / 100.0
        );
        Debug.Log(levels);
    }

    public void TogglePanel()
    {

    }

}
