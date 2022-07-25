using BLG.GTC.Lingguo;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demo : MonoBehaviour
{
    [Language("lingguo")]
    public string name;
    void Start()
    {
        LanguageManager.AddLanguageScript(this);
    }
}
