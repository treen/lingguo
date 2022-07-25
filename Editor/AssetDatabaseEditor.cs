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
using UnityEditor;
using UnityEngine;

namespace BLG.GTC.Lingguo
{
    [CustomEditor(typeof(AssetDatabase))]
    public class AssetDatabaseEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("language"));
            var datas = serializedObject.FindProperty("datas");
            var oldSize = datas.arraySize;
            
            EditorGUILayout.PropertyField(datas, true);
            if (oldSize < datas.arraySize)
            {
                var newData = datas.GetArrayElementAtIndex(datas.arraySize - 1);
                newData.isExpanded = true;
                var key = newData.FindPropertyRelative("key");
                key.stringValue = "new key";
                
            }
            serializedObject.ApplyModifiedProperties();
        }

    }

}