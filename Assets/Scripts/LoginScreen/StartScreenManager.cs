using TMPro;
using UnityEngine;

public class StartScreenManager : MonoBehaviour
{

    void Awake()
    {
        if (DatabaseManager.Instance.CurrentUser != null)
        {
            transform.Find("StartScreen").gameObject.SetActive(true);
            transform.Find("Login").gameObject.SetActive(false);
            transform.Find("StartScreen/UserWelcomeText").GetComponent<TMP_Text>().text = "Welcome " + DatabaseManager.Instance.CurrentUser.username;
        }
        else
        {
            transform.Find("StartScreen").gameObject.SetActive(false);
            transform.Find("Login").gameObject.SetActive(true);
        }
    }
    public void ToggleLogin(string username)
    {
        transform.Find("Login").gameObject.SetActive(!transform.Find("Login").gameObject.activeSelf);
        transform.Find("StartScreen").gameObject.SetActive(!transform.Find("StartScreen").gameObject.activeSelf);
        transform.Find("StartScreen").Find("UserWelcomeText").GetComponent<TMP_Text>().text = "Welcome " + username;
    }

}
