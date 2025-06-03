using UnityEngine;

public class FitScrollArea : MonoBehaviour
{
    private RectTransform searchArea;
    private RectTransform myTransform;
    private RectTransform searchCanvas;
    void Awake()
    {
        searchArea = GameObject.Find("Canvas/SearchPanel").transform.GetComponent<RectTransform>();
        searchCanvas = GameObject.Find("ScrollAreaCanvas").transform.GetComponent<RectTransform>();
        myTransform = transform.GetComponent<RectTransform>();
    }

    void Update()
    {
        myTransform.sizeDelta = new Vector2 { x = myTransform.sizeDelta.x , y = searchCanvas.rect.height + searchArea.anchoredPosition.y};
    }

}
