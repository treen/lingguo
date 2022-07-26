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
using UnityEngine;
using UnityEditor;
using BLG.GTC.Lingguo;

[CustomEditor(typeof(LanguageTemplate))]
public class LanguageTemplateEditor : Editor
{
    // Start is called before the first frame update
    public override void OnInspectorGUI()
    {
        var languageTemplate = target as LanguageTemplate;
        serializedObject.Update();
        var templateKeyProperty = serializedObject.FindProperty("templateKey");
        var rect = EditorGUILayout.GetControlRect();
        GuiHelper.KeyField(rect, templateKeyProperty,new GUIContent("Key"),LanguageBinder.KeyType.Template);

        EditorGUILayout.LabelField("template");
        var template = LanguageManager.CurrentLanguagePackage.GetString(templateKeyProperty.stringValue);
        EditorGUILayout.TextArea(template, GUILayout.Height(100));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("args"), new GUIContent("Params"));
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.LabelField("result");
        EditorGUILayout.TextArea(languageTemplate.Value, GUILayout.Height(200));
    }
}
