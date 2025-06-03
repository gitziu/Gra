using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour
{

    private Transform levelExitPopUp;

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
        levelExitPopUp = GameObject.Find("Canvas/ExitGamePopUp").transform;
        levelExitPopUp.gameObject.SetActive(false);
        GameObject.Find("Canvas/ExitGameButton").transform.GetComponent<Button>().onClick.AddListener(() => levelExitPopUp.gameObject.SetActive(true));
        levelExitPopUp.Find("ExitGame").GetComponent<Button>().onClick.AddListener(() =>
        {
            DatabaseManager.Instance.CloseConnection();
            Application.Quit();
        });
        levelExitPopUp.Find("ContinueGame").GetComponent<Button>().onClick.AddListener(() => levelExitPopUp.gameObject.SetActive(false));
    }
    public void ToggleLogin(string username)
    {
        transform.Find("Login").gameObject.SetActive(!transform.Find("Login").gameObject.activeSelf);
        transform.Find("StartScreen").gameObject.SetActive(!transform.Find("StartScreen").gameObject.activeSelf);
        transform.Find("StartScreen").Find("UserWelcomeText").GetComponent<TMP_Text>().text = "Welcome " + username;
    }

}
