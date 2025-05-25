using UnityEngine;
using UnityEngine.UI;

public class LoginSubmission : MonoBehaviour
{
    public LoginManager lm;
    private Button loginbtn;

    void Awake()
    {
        loginbtn = transform.GetComponent<Button>();
        loginbtn.onClick.AddListener(SubmitLoginData);
    }

    public void SubmitLoginData()
    {
        lm.SubmitLoginData();
    }

}
