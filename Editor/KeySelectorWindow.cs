/*
MIT License

Copyright (c) 2022 ZhengQun

treen@163.com

https://github.com/treen/lingguo

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using BLG.GTC.Language;
using UnityEditor.UIElements;
using UnityEngine.AddressableAssets;

public class KeySelectorWindow : EditorWindow
{
    static KeySelectorWindow instance;
    
    List<(string, object,string)> keyList = new List<(string, object,string)> ();
    VisualElement bottomElement;
    SerializedProperty keyProperty;
    ToolbarSearchField searchBar;
    LanguageBinder.KeyType keyType;
    Image previewImage;
    KeyListView keyListView;

    public static void Show(SerializedProperty property, LanguageBinder.KeyType keyType)
    {
        instance = GetWindow<KeySelectorWindow>(true, "Select Key", true);
        instance.keyProperty = property;
        instance.keyType = keyType;
        instance.ShowKeyListView();

    }
    private void OnEnable()
    {
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        var splitView = new TwoPaneSplitView(0, 600, TwoPaneSplitViewOrientation.Vertical);
        // Import UXML
        var visualTree = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.blg.gtc.lingguo/Editor/UI/KeyListView/KeyListView.uxml");
        VisualElement topElement = visualTree.Instantiate();
        keyListView = new KeyListView(topElement);
        keyListView.onSelectionChange += KeyListView_onSelectionChange;
        keyListView.onItemsChosen += KeyListView_onItemsChosen;
        searchBar = topElement.Q<ToolbarSearchField>();
        searchBar.RegisterValueChangedCallback(OnSearchKeyChange);
        root.Add(splitView);
        splitView.Add(topElement);
        bottomElement = new VisualElement();
        previewImage = new Image();
        bottomElement.Add(previewImage);
        bottomElement.style.width = Length.Percent(100);
        bottomElement.style.height = Length.Percent(100);
        bottomElement.style.justifyContent = Justify.Center;
        //var temp = new VisualElement();
        //temp.Add(previewImage);
        bottomElement.Add(previewImage);
        //previewImage.style.width = Length.Percent(100);
        //previewImage.style.height = Length.Percent(100);
        splitView.Add(bottomElement);

        //var keyItem = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.blg.gtc.language/Editor/UI/KeySelector/KeyItem.uxml");
        //keyListView.makeItem = keyItem.CloneTree;


    }

    void OnSearchKeyChange(ChangeEvent<string> evt)
    {
        var result = (from data in keyList
                      where data.Item1.Contains(evt.newValue) || data.Item2.ToString().Contains(evt.newValue) || data.Item3.Contains(evt.newValue)
                      select data).ToList();
        keyListView.SetSource(result);
        keyListView.Rebuild();
    }
    void ShowKeyListView()
    {
        if (LanguageManager.CurrentLanguagePackage == null)
        {
            bottomElement.Add(new HelpBox("no language package", HelpBoxMessageType.Error));
            return;
        }



        var itemSource = keyType == LanguageBinder.KeyType.Static ? LanguageManager.CurrentLanguagePackage.GetEnumerator() : LanguageManager.CurrentLanguagePackage.GetStringEnumerator();
        keyList.Clear();
        var commentManager = CommentManager.Single();
        while (itemSource.MoveNext())
        {
            var result = (KeyValuePair<string, object>)itemSource.Current;
            var comment = commentManager.GetComment(LanguageManager.CurrentLanguagePackage, result.Key);
            keyList.Add((result.Key, result.Value, comment)) ;
        }
        keyListView.SetSource( keyList);
        
    }

    void OnSelectItem(IEnumerable<object> obj)
    {
        var itr = obj.GetEnumerator();
        itr.MoveNext();
        var item = ((string, object,string))itr.Current;
        keyProperty.stringValue = item.Item1;
        GUI.changed = true;
        keyProperty.serializedObject.ApplyModifiedProperties();
        if( item.Item2 is AssetReference asset )
        {
            DrawPreview(asset);
        }        
        return;

        
    }

    void DrawPreview( AssetReference asset)
    {
        void KeySelectorWindow_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<Sprite> obj)
        {
            if (obj.Result != null)
            {
                Sprite sprite = obj.Result as Sprite;
                previewImage.sprite = sprite;
            }
            Addressables.Release(obj);
        }
        var rect = bottomElement.layout;
        Addressables.LoadAssetAsync<Sprite>(asset).Completed += KeySelectorWindow_Completed;
        
        Texture2D texture = asset.editorAsset as Texture2D;
        previewImage.image = texture;
    }

    

    private void KeyListView_onItemsChosen(IEnumerable<object> obj)
    {
        OnSelectItem(obj);

        Close();
    }


    private void KeyListView_onSelectionChange(IEnumerable<object> obj)
    {
        OnSelectItem(obj);
    }

    

    private void OnLostFocus()
    {
        Close();
    }
}
