using TMPro;
using UnityEngine;

public class LoginInputField : MonoBehaviour
{
    public LoginManager lm;
    private TMP_InputField input;
    void Awake()
    {
        input = transform.GetComponent<TMP_InputField>();
        input.onEndEdit.AddListener(InputSubmitted);
    }

    public void InputSubmitted(string a)
    {
        Debug.Log("submitted input in : " + transform.tag);
        lm.CheckInputFields();
    }
}
