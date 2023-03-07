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
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BLG.GTC.Lingguo
{
    public class LanguageWindow : EditorWindow
    {
        [MenuItem("Window/Lingguo")]
        public static void ShowExample()
        {
            LanguageWindow wnd = GetWindow<LanguageWindow>();
            wnd.titleContent = new GUIContent("LingguoWindow");
            wnd.FillLanguageList();
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.blg.gtc.lingguo/Editor/UI/LanguageWindow.uxml");
            VisualElement cloneVisualElement = visualTree.Instantiate();
            root.Add(cloneVisualElement);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.blg.gtc.lingguo/Editor/UI/LanguageWindow.uss");
            root.styleSheets.Add(styleSheet);
            minSize = new Vector2(500, 500);

            var createButton = cloneVisualElement.Q<Button>("Create");
            createButton.clicked += CreateButton_onClick;

        }

        void FillLanguageList()
        {
            var listview = rootVisualElement.Q<ListView>("LanguageList");
            listview.Clear();
            listview.makeItem = () => {
                return new ObjectField();
            };
            var guids = UnityEditor.AssetDatabase.FindAssets("t:LanguagePackage");
            listview.itemsSource = guids;
            listview.bindItem = (node, i) =>
            {
                var objectField = node as ObjectField;
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(listview.itemsSource[i] as string);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<LanguagePackage>(path);
                objectField.value = asset;
                objectField.label = asset.Language;
                objectField.SetEnabled(false);

            };
            listview.Refresh();
            
        }

        private void CreateButton_onClick()
        {
            var languageNameTextField = rootVisualElement.Q<TextField>("NewLanguageName");
            var newLanguageName = languageNameTextField.text;

            var parentFolder = $"Lingguo/{newLanguageName}/";
            parentFolder = GuiHelper.CreateFolder(parentFolder);

            var newAssetDatabase = CreateInstance<AssetDatabase>();
            newAssetDatabase.language = newLanguageName;
            
            var newAssetDatabasePath = $"{parentFolder}/{newLanguageName}AssetDatabase.asset";
            UnityEditor.AssetDatabase.CreateAsset(newAssetDatabase,newAssetDatabasePath);

            var newStringDatabase = CreateInstance<StringDatabase>();
            newStringDatabase.language = newLanguageName;

            var newStringDatabasePath = $"{parentFolder}/{newLanguageName}StringDatabase.asset";
            UnityEditor.AssetDatabase.CreateAsset(newStringDatabase, newStringDatabasePath);

            var newLanguagePack = LanguagePackage.CreateInstance(newLanguageName, newStringDatabase, newAssetDatabase);
            var newLanguagePackPath = $"{parentFolder}/{newLanguageName}Package.asset";
            UnityEditor.AssetDatabase.CreateAsset(newLanguagePack, newLanguagePackPath);

            FillLanguageList();

        }

        void OnEnable()
        {



        }

        private void OnValidate()
        {
            FillLanguageList();
        }





    }

    
}