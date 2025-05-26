using UnityEngine;
using UnityEngine.UI;

public class BackToLogin : MonoBehaviour
{
    private Button btn;
    void Awake()
    {
        btn = transform.GetComponent<Button>();
        btn.onClick.AddListener(SwitchToLogin);
    }

    private void SwitchToLogin()
    {
        Debug.Log(transform.parent.parent.name);
        transform.parent.parent.GetComponent<StartScreenManager>().ToggleLogin("");
    }

}
