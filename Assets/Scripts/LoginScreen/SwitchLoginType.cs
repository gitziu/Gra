using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SwitchLoginType : MonoBehaviour
{

    public LoginManager lm;
    private Button btn;
    private bool SwitchToNewUser = true;

    void Awake()
    {
        btn = transform.GetComponent<Button>();
        btn.onClick.AddListener(SwitchType);
    }

    void SwitchType()
    {
        SwitchToNewUser = !SwitchToNewUser;
        if (SwitchToNewUser)
        {
            transform.GetChild(0).GetComponent<TMP_Text>().SetText("New User");
        }
        else transform.GetChild(0).GetComponent<TMP_Text>().SetText("Returning User");
        lm.Instance.SwitchType();
    }

}
