using UnityEngine;

public class FitScrollArea : MonoBehaviour
{
    private RectTransform searchArea;
    private RectTransform myTransform;
    void Awake()
    {
        searchArea = GameObject.Find("Canvas/SearchPanel").transform.GetComponent<RectTransform>();
        myTransform = transform.GetComponent<RectTransform>();
    }

    void Update()
    {
        myTransform.sizeDelta = new Vector2 { x = myTransform.sizeDelta.x , y = 1440 + searchArea.anchoredPosition.y};
    }

}
