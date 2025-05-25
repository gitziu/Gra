using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public LoginManager Instance;
    private TMP_InputField Username, Password;
    private Transform Submit;
    public bool NewUser = false;

    void Awake()
    {
        Debug.Log("start");
        Instance = this;
        Debug.Log("instance set");
        Username = transform.Find("UsernameInput").GetComponent<TMP_InputField>();
        Debug.Log("input 1 set");
        Password = transform.Find("PasswordInput").GetComponent<TMP_InputField>();
        Debug.Log("input 2 set");
        Submit = transform.Find("LoginButton");
        Debug.Log("submit button found");
        Submit.GetComponent<Button>().interactable = false;
        Submit.GetComponent<Button>().onClick.AddListener(BeginLoginSubmission);
        Debug.Log("Submit not interactible");
    }

    public void SwitchType()
    {
        Debug.Log("Switch type on login manager");
        NewUser = !NewUser;
        if (NewUser)
        {
            Submit.GetChild(0).GetComponent<TMP_Text>().SetText("Register");
        }
        else
        {
            Submit.GetChild(0).GetComponent<TMP_Text>().SetText("Login");
        }

    }

    public void CheckInputFields()
    {
        bool usernameFilled = !string.IsNullOrEmpty(Username.text);
        bool passwordFilled = !string.IsNullOrEmpty(Password.text);
        Submit.GetComponent<Button>().interactable = usernameFilled && passwordFilled;
        if (Submit.GetComponent<Button>().interactable)
        {
            Debug.Log("Submit interactible : true");
        }
    }

    public void BeginLoginSubmission()
    {
        var usernameCopy = Username.text;
        try
        {
            if (!NewUser)
            {
                DatabaseManager.Instance.Login(Username.text, Password.text);
                Debug.Log("login succesful");
                Username.text = "";
                Password.text = "";
                transform.parent.parent.GetComponent<StartScreenManager>().ToggleLogin(usernameCopy);
            }
            else
            {
                DatabaseManager.Instance.RegisterUser(Username.text, Password.text);
                Debug.Log("registration succesful");
                Username.text = "";
                Password.text = "";
                transform.parent.parent.GetComponent<StartScreenManager>().ToggleLogin(usernameCopy);
            }
        }
        catch (Exception e)
        {
            transform.parent.Find("ErrorText").GetComponent<TMP_Text>().text = e.Message;
        }
    }

}
