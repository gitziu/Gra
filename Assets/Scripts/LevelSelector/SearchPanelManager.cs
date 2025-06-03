using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class SearchPanelManager : MonoBehaviour
{
    public TMP_InputField author, levelName, minSuccesRatio, maxSuccesRatio, minRating, maxRating;
    public Toggle myLevels;
    public TMP_Dropdown order, sortColumn;
    public Button toggleSearch, searchButton;
    private Vector3 velocity;
    public float smoothTime = 0.3f;
    private Transform ErrorScreen;
    [SerializeField] private LevelSelectObject ObjectPrefab;

    void Awake()
    {
        ErrorScreen = GameObject.Find("Canvas/ErrorMessage").transform;
        ErrorScreen.Find("CloseError").GetComponent<Button>().onClick.AddListener(toggleError);
        ErrorScreen.gameObject.SetActive(false);
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
        transform.Find("ReturnButton").GetComponent<Button>().onClick.AddListener(SceneLoader.LoadPreviousScene);
        Debug.Log("author : " + author);
        Debug.Log("sortColumn : " + sortColumn);
        Debug.Log(transform.Find("SortParent/OrderBy").name);
        myLevels.isOn = false;
        toggleSearch.onClick.AddListener(TogglePanel);
        searchButton.onClick.AddListener(Search);
        minSuccesRatio.onEndEdit.AddListener(ClampMinMax(minSuccesRatio, maxSuccesRatio));
        maxSuccesRatio.onEndEdit.AddListener(ClampMinMax(minSuccesRatio, maxSuccesRatio));
        maxRating.onEndEdit.AddListener(ClampMinMax(minRating, maxRating));
        minRating.onEndEdit.AddListener(ClampMinMax(minRating, maxRating));
        Search();
    }

    private UnityAction<string> ClampMinMax(TMP_InputField minField, TMP_InputField maxField)
    {
        return (string s) =>
        {
            int min, max;
            var minparse = int.TryParse(minField.text, out min);
            var maxparse = int.TryParse(maxField.text, out max);
            if (!minparse) min = 0;
            if (!maxparse) max = 100;
            if (min < 0) min = 0;
            if (min > 100) min = 100;
            if (max < 0) max = 0;
            if (max > 100) max = 100;
            if (min > max) min = max;
            minField.text = min.ToString();
            maxField.text = max.ToString();
        };
    }

    private int clamp(String value, int min, int max, int defaultValue)
    {
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
            case 1: col = "update"; break;
            case 2: col = "created"; break;
            case 3: col = "successRatio"; break;
            case 4: col = "rating"; break;
        }
        try
        {
            var levels = DatabaseManager.Instance.SearchLevels(author.text,
                levelName.text,
                myLevels.isOn,
                order.value != 1,
                col,
                clamp(minSuccesRatio.text, 0, 100, 0) / 100.0,
                clamp(maxSuccesRatio.text, 0, 100, 100) / 100.0,
                clamp(minRating.text, 0, 100, 0) / 100.0,
                clamp(maxRating.text, 0, 100, 100) / 100.0
            );
            Debug.Log(levels.Count);
            foreach (Transform child in GameObject.Find("ScrollAreaCanvas/ScrollArea/Viewport/Content").transform)
            {
                Destroy(child.gameObject);
            }
            for (int i = 0; i < levels.Count; i++)
            {
                Debug.Log("id : " + levels[i].id + " , name : " + levels[i].name);
                LevelSelectObject newLevelSelect = Instantiate(ObjectPrefab, GameObject.Find("ScrollAreaCanvas/ScrollArea/Viewport/Content").transform);
                newLevelSelect.Initialize(levels[i]);
            }
        }
        catch (Exception e)
        {
            displayError(e);
        }
    }

    public void TogglePanel()
    {
        transform.GetComponent<RectTransform>().DOAnchorPosY(transform.GetComponent<RectTransform>().anchoredPosition.y == 0f ? -transform.GetComponent<RectTransform>().rect.height : 0f, smoothTime);
    }

    public void displayError(Exception e)
    {
        transform.parent.Find("ErrorMessage").gameObject.SetActive(true);
        GameObject.Find("/Canvas/ErrorMessage/Error").transform.GetComponent<TMP_Text>().text = e.Message;
    }

    public void toggleError()
    {
        GameObject.Find("Canvas/ErrorMessage").SetActive(false);
    }

}
