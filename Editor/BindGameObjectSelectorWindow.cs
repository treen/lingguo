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
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

namespace BLG.GTC.Lingguo
{
    public class BindGameObjectSelectorWindow : EditorWindow
    {
        public class ItemInfo
        {
            public int instanceId;
            public string label;
        }
        static ObjectSelectorSearchContext s_Context = new ObjectSelectorSearchContext()
        {
            requiredTypes = new Type[] { typeof(GameObject) },
            requiredTypeNames = new string[] { typeof(GameObject).Name },
            visibleObjects = VisibleObjects.Scene
        };
        static Action<GameObject> s_OnSelectionChanged;
        static Action<GameObject, bool> s_OnSelectorClosed;
        static Func<GameObject, bool> s_FilterConstraint;
        public static BindGameObjectSelectorWindow instance { get; private set; }

        List<ItemInfo> m_FilteredItems;
        ToolbarSearchField m_Searchbox;
        ListView m_ListView;
        string m_SearchText;
        ItemInfo m_CurrentItem;
        bool m_Canceled = true;
        static GameObject startGameObject;
        public bool initialized { get; private set; } = false;
        internal static int objectSelectorID = 0;
        public string searchText
        {
            get => m_SearchText;
            set
            {
                m_SearchText = value;
                FilterItems();
            }
        }

        public List<ItemInfo> allItems { get; private set; }

        public static void Show(GameObject rootGameObject, Action<GameObject> onSelectionChanged, Action<GameObject, bool> onSelectorClosed, Func<GameObject, bool> pickerConstraint = null)
        {
            startGameObject = rootGameObject;
            s_OnSelectionChanged = onSelectionChanged;
            s_OnSelectorClosed = onSelectorClosed;
            s_FilterConstraint = pickerConstraint ?? (o => true);

            // Create a window with CreateInstance, and show it with ShowAuxWindow.
            var window = CreateInstance<BindGameObjectSelectorWindow>();
            instance = window;
            window.ShowAuxWindow();
        }

        void Init()
        {
            m_SearchText = "";
            //allItems = new List<ItemInfo>();
            m_FilteredItems = new List<ItemInfo>();

            EnumGameObject(startGameObject, m_FilteredItems, startGameObject.name);
            //if ((s_Context.visibleObjects & VisibleObjects.Assets) == VisibleObjects.Assets)
            //    allItems.AddRange(FetchAllAssets());
            //if ((s_Context.visibleObjects & VisibleObjects.Scene) == VisibleObjects.Scene)
            //    allItems.AddRange(FetchAllGameObjects(s_Context));

            //allItems.Sort((item, other) => item.label.CompareTo(other.label));


            //m_FilteredItems.AddRange(allItems);
        }
        
        void OnEnable()
        {
            Init();

            m_Searchbox = new ToolbarSearchField();
            m_Searchbox.RegisterValueChangedCallback(SearchFilterChanged);
            m_Searchbox.style.flexGrow = 1;
            m_Searchbox.style.maxHeight = 16;
            m_Searchbox.style.width = Length.Percent(100f);
            m_Searchbox.style.marginRight = 4;
            rootVisualElement.Add(m_Searchbox);

            m_ListView = new ListView(m_FilteredItems, 16, MakeItem, BindItem);
            m_ListView.onSelectionChange += ItemSelectionChanged;
            m_ListView.onItemsChosen += ItemsChosen;
            m_ListView.style.flexGrow = 1;
            rootVisualElement.Add(m_ListView);

            // Initialize selection
            if (s_Context.currentObject != null)
            {
                var currentSelectedId = s_Context.currentObject.GetInstanceID();
                var selectedIndex = m_FilteredItems.FindIndex(item => item.instanceId == currentSelectedId);
                if (selectedIndex >= 0)
                    m_ListView.selectedIndex = selectedIndex;
            }

            FinishInit();
        }

        void FinishInit()
        {
            EditorApplication.delayCall += () =>
            {
                m_ListView.Focus();
                initialized = true;
            };
        }

        void OnDisable()
        {
            s_OnSelectorClosed?.Invoke(GetCurrentObject(), m_Canceled);
            instance = null;
        }

        public void SetSearchFilter(string query)
        {
            m_Searchbox.value = query;
        }

        void SearchFilterChanged(ChangeEvent<string> evt)
        {
            searchText = evt.newValue;
        }

        void FilterItems()
        {
            m_FilteredItems.Clear();
            m_FilteredItems.AddRange(allItems.Where(item => string.IsNullOrEmpty(searchText) || item.label.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) >= 0));

            m_ListView.Rebuild();
        }

        void BindItem(VisualElement listItem, int index)
        {
            if (index < 0 || index >= m_FilteredItems.Count)
                return;

            var label = listItem as Label;
            if (label == null)
                return;
            label.text = m_FilteredItems[index].label;
        }

        static VisualElement MakeItem()
        {
            return new Label();
        }

        void ItemSelectionChanged(IEnumerable<object> selectedItems)
        {
            m_CurrentItem = selectedItems.FirstOrDefault() as ItemInfo;
            s_OnSelectionChanged?.Invoke(GetCurrentObject());
            
        }

        void ItemsChosen(IEnumerable<object> selectedItems)
        {
            m_CurrentItem = selectedItems.FirstOrDefault() as ItemInfo;
            m_Canceled = false;
            Close();
        }

        static IEnumerable<ItemInfo> FetchAllAssets()
        {
            var allPaths = UnityEditor.AssetDatabase.GetAllAssetPaths();
            if (allPaths == null)
                yield break;

            var requiredTypes = s_Context.requiredTypeNames != null ? s_Context.requiredTypeNames.ToList() : new List<string>();
            foreach (var path in allPaths)
            {
                var type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(path);
                var typeName = type.FullName ?? "";
                if (requiredTypes.Any(requiredType => typeName.Contains(requiredType)))
                {
                    var asset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(path);
                    var matchFilterConstraint = s_FilterConstraint?.Invoke(asset as GameObject);
                    if (matchFilterConstraint.HasValue && !matchFilterConstraint.Value)
                        continue;
                    var instanceId = asset?.GetInstanceID() ?? 0;
                    yield return new ItemInfo { instanceId = instanceId, label = path };
                }
            }
        }
        void EnumGameObject(GameObject node, List<ItemInfo> gameObjects, string nodename)
        {
            if (node != null)
            {
                gameObjects.Add(new ItemInfo { instanceId = node.GetInstanceID(), label = nodename });
                //objectNameList.Add(parentName);
                foreach (Transform child in node.transform)
                {
                    var newNodename = string.Format("{0}.{1}", nodename, child.name);

                    EnumGameObject(child.gameObject, gameObjects, newNodename);

                }
            }
        }

        static IEnumerable<ItemInfo> FetchAllGameObjects(ObjectSelectorSearchContext context)
        {
            var property = new HierarchyProperty(HierarchyType.GameObjects, false);

            var requiredTypes = s_Context.requiredTypeNames != null ? s_Context.requiredTypeNames.ToList() : new List<string>();
            while (property.Next(null))
            {
                var objectReferenced = property.pptrValue;
                if (objectReferenced == null)
                    continue;
                var matchFilterConstraint = s_FilterConstraint?.Invoke(objectReferenced as GameObject);
                if (matchFilterConstraint.HasValue && !matchFilterConstraint.Value)
                    continue;
                var typeName = objectReferenced.GetType().FullName ?? "";
                if (requiredTypes.Any(requiredType => typeName.Contains(requiredType)))
                    yield return new ItemInfo { instanceId = property.instanceID, label = property.name };
            }

        }

        public GameObject GetCurrentObject()
        {
            if (m_CurrentItem == null)
                return null;
            return EditorUtility.InstanceIDToObject(m_CurrentItem.instanceId) as GameObject;
        }
    }
}