using TMPro;
using UnityEngine;

public class StartScreenManager : MonoBehaviour
{

    void Awake()
    {
        transform.Find("StartScreen").gameObject.SetActive(false);
        transform.Find("Login").gameObject.SetActive(true);
    }
    public void ToggleLogin(string username)
    {
        transform.Find("Login").gameObject.SetActive(!transform.Find("Login").gameObject.activeSelf);
        transform.Find("StartScreen").gameObject.SetActive(!transform.Find("StartScreen").gameObject.activeSelf);
        transform.Find("StartScreen").Find("UserWelcomeText").GetComponent<TMP_Text>().text = "Welcome " + username;
    }

}
