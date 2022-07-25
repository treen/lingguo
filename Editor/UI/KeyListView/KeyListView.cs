using BLG.GTC.Lingguo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System;

namespace BLG.GTC.Lingguo
{

    public class KeyListView
    {
        ListView keyListView;

        List<(string, object, string)> keyList = new List<(string, object, string)>();
        public List<(string, object, string)> KeyList { get => keyList; }

        public event Action<IEnumerable<object>> onSelectionChange;
        public event Action<IEnumerable<object>> onItemsChosen;
        public KeyListView(VisualElement listView)
        {
            keyListView = listView.Q<ListView>("KeyList");
            keyListView.onSelectionChange += KeyListView_onSelectionChange;
            keyListView.onItemsChosen += KeyListView_onItemsChosen; ;
            var searchBar = listView.Q<ToolbarSearchField>("SearchBar");
            searchBar.RegisterValueChangedCallback(OnSearchKeyChange);
            
        }

        private void KeyListView_onItemsChosen(IEnumerable<object> obj)
        {
            onItemsChosen?.Invoke(obj);
        }

        private void KeyListView_onSelectionChange(IEnumerable<object> obj)
        {
            onSelectionChange?.Invoke(obj);
        }

        void OnSearchKeyChange(ChangeEvent<string> evt)
        {
            var result = (from data in keyList.AsParallel()
                          where data.Item1.Contains(evt.newValue) || data.Item2.ToString().Contains(evt.newValue) || data.Item3?.Contains(evt.newValue) ==true
                          select data).ToList();
            keyListView.itemsSource = result;
            keyListView.Rebuild();
        }

        public void SetSource(List<(string, object, string)> list)
        {
            keyList = list;
            keyListView.itemsSource = keyList;
            keyListView.makeItem = () =>
            {
                var item = new VisualElement();
                item.style.alignItems = Align.Center;
                item.style.flexDirection = FlexDirection.Row;
                var key = new Label();
                var value = new Label();
                var comment = new Label();

                key.AddToClassList("list-item-key");
                value.AddToClassList("list-item-value");
                comment.AddToClassList("list-item-comment");

                item.Add(key);
                item.Add(value);
                item.Add(comment);
                return item;
            };
            keyListView.bindItem = BindKeyItem;
        }

        void BindKeyItem(VisualElement visualElement, int index)
        {
            var keyList = (List<(string, object, string)>)keyListView.itemsSource;
            (visualElement.ElementAt(0) as Label).text = keyList[index].Item1;
            (visualElement.ElementAt(1) as Label).text = keyList[index].Item2.ToString();
            (visualElement.ElementAt(2) as Label).text = keyList[index].Item3;


        }
        public void Rebuild()
        {
            keyListView.Rebuild();
        }
    }
}
