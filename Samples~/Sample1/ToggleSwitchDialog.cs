using BLG.GTC.Lingguo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSwitchDialog : MonoBehaviour
{
    // Start is called before the first frame update
    public void OnToggleSwitchDialog(bool enable)
    {
        GameObject dialog = GameObject.Find("Canvas/LoginDialog");
        var languageGameObject = dialog.GetComponent<LanguageGameObject>();
        languageGameObject.enabled = enable;
    }
}
