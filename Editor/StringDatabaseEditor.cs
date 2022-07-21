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

using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using TinyCsvParser;
using TinyCsvParser.Mapping;
using TinyCsvParser.Tokenizer;
using BLG.GTC.Language;

[CustomEditor(typeof(StringDatabase))]
public class StringDatabaseEditor : Editor
{
    ToolbarPopupSearchField searchField;
    KeyListView keyListView;
    class StringData
    {
        public string Key { set; get; }
        public string Value { set; get; }
        public string Comment { set; get; }
    }
    private class CsvStringDataMapping : CsvMapping<StringData>
    {
        public CsvStringDataMapping()
            : base()
        {
            MapProperty(0, x => x.Key);
            MapProperty(1, x => x.Value);
            MapProperty(2, x => x.Comment);
        }
    }
    private class StringSplitTokenizer : ITokenizer
    {
        QuotedStringTokenizer quoted;
        public StringSplitTokenizer(char columnDelimiter) 
        {
            quoted = new QuotedStringTokenizer(columnDelimiter);
        }
        public string[] Tokenize(string input)
        {
            var result = quoted.Tokenize(input);
            if( result.Length <3)
                Array.Resize(ref result, 3);
            return result;
        }

    }
    public TextAsset TextAsset;
    StringDatabase stringDatabase;
    public override VisualElement CreateInspectorGUI()
    {
        stringDatabase = target as StringDatabase;
        //var customInspector = new VisualElement();
        var visualTree = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.blg.gtc.lingguo/Editor/UI/KeyListView/KeyListView.uxml");
        //var styleSheet = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.blg.gtc.language/Editor/UI/StringDatabaseEditor.uss");
        VisualElement customInspector = new VisualElement();
        visualTree.CloneTree(customInspector);
        keyListView = new KeyListView(customInspector);

        //customInspector.styleSheets.Add(styleSheet);
        var language = new TextField("Language");
        customInspector.Insert(0,language);
        language.BindProperty(serializedObject.FindProperty("language"));

        var importDictionarayButton = new Button(OnImportDictionaray);
        importDictionarayButton.text = "ImportDictionaray";
        customInspector.Insert(1,importDictionarayButton);
        //var keys = serializedObject.FindProperty("keys");
        //var values = serializedObject.FindProperty("values");
        //listView.BindProperty(keys);


        //IMGUIContainer iMGUIContainer = customInspector.Q<IMGUIContainer>();
        //iMGUIContainer.onGUIHandler += OnGUI;
        UpdateKeyListView();
        return customInspector;
    }
    void UpdateKeyListView()
    {
        List<(string, object, string)> keyList = new List<(string, object, string)>();
        var commentManager = CommentManager.Single();
        foreach (var data in stringDatabase.Datas)
        {
            var comment = commentManager.GetComment(stringDatabase, data.key);
            keyList.Add((data.key, data.asset, comment));
        }
        keyListView.SetSource(keyList);
    }

    void OnImportDictionaray()
    {
        string path = EditorUtility.OpenFilePanel("import dictionaray", "", "csv");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        StringSplitTokenizer quotedStringTokenizer = new StringSplitTokenizer(',');
        CsvParserOptions csvParserOptions = new CsvParserOptions(false, quotedStringTokenizer);
        CsvStringDataMapping csvMapper = new CsvStringDataMapping();
        CsvParser<StringData> csvParser = new CsvParser<StringData>(csvParserOptions, csvMapper);

        var result = csvParser
            .ReadFromFile(path, Encoding.UTF8)
            .ToList();
        var database = target as StringDatabase;

        var datas = serializedObject.FindProperty("datas");
        var commentData = CommentManager.Single().GetOrCreateCommentAsset(database);


        foreach (var data in result)
        {
            database.AddData(data.Result.Key, data.Result.Value);
            if(!string.IsNullOrEmpty( data.Result.Comment))
                commentData.AddData(data.Result.Key, data.Result.Comment);
        }
        CommentManager.Single().Save();
        serializedObject.Update();
        //serializedObject.SetIsDifferentCacheDirty();
        ////int endPos = keys.arraySize;
        datas.arraySize = datas.arraySize + 1;
        datas.arraySize = datas.arraySize - 1;
        //GUI.changed = true;
        serializedObject.ApplyModifiedProperties();
        UpdateKeyListView();
        keyListView.Rebuild();
    }

    //[CustomPropertyDrawer(typeof(AssetReferenceStringDatabase))]
    public class StringDatabaseDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //var temp = property.FindPropertyRelative("editorAsset");
            EditorGUI.BeginProperty(position, label, property);
            position = new Rect(position.x, position.y , 50, EditorGUIUtility.singleLineHeight);
            //EditorGUI.LabelField(position, property.objectReferenceValue.name);
            position = new Rect(position.x + 50, position.y , 50, EditorGUIUtility.singleLineHeight);
            //EditorGUI.ObjectField(position,);
            EditorGUI.EndProperty();

        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;

        }
    }
}
