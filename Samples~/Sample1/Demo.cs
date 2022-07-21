using BLG.GTC.Language;
public class Demo 
{
    [Language("name")]
    string name;
    public Demo()
    {
        LanguageManager.AddLanguageScript(this);
    }
}
