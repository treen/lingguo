using System.Collections.Generic;
using UnityEngine;
using BLG.GTC.Language;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;


[ExecuteAlways]
public class Test :MonoBehaviour,ILanguage
{
    public string[] keys;
    public string[] stringList;
    public LanguageTemplate template;
    //public Sprite;
    public AssetReferenceT<Sprite> assetReference;
    public GameObject testObject;
    public Dictionary<string, string> testDictionary;
    public string test = "test";
    public Text text;
    public Texture2D texture;
    [Language("end")]
    public string end = "end";
    //[Language("update")]
    public string update;
    public LanguageConfig languageConfig;
    private void Start()
    {
        template = ScriptableObject.Instantiate(template);
        template.Bind(this, "test");

        Language language = new Language();
        language.BindArrayMember(keys, this, "stringList");
        string test = LanguageHelper.Translate("我有#item_knife0001#", LanguageManager.CurrentLanguagePackage);

    }

    private void OnValidate()
    {


        return;
    }
    private void OnEnable()
    {
        LanguageManager.AddLanguageScript(this);
    }
    string heroName = "步惊云";
    string targetName = "雄霸";
    string weapon = "#item_knife0001#";
    public void OnLanguageSwitch(LanguagePackage languagePack)
    {
    }
    public void SwtichLanguage()
    {
        if (languageConfig.CurrentLanguageIndex < languageConfig.LanguagePackageList.Length - 1)
            languageConfig.CurrentLanguageIndex++;
        else
            languageConfig.CurrentLanguageIndex = 0;
    }
}
