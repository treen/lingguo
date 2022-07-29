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
using UnityEngine;
namespace BLG.GTC.Lingguo
{

    [Serializable]
    public class Database<T> : ScriptableObject, ISerializationCallbackReceiver where T:class
    {
        [Serializable]
        public class Data
        {
            public string key;
            public T asset;
        }
        public string language;
        [SerializeField]
        List<Data> datas = new List<Data>();
#if !UNITY_EDITOR
        SortedDictionary<string, T> dictionary = new SortedDictionary<string, T>();
        public SortedDictionary<string, T> Dictionary { get => dictionary; }
#else
        Dictionary<string, T> dictionary = new Dictionary<string, T>();
        public Dictionary<string, T> Dictionary { get => dictionary; }
#endif

        public List<Data> Datas { get => datas; }
        virtual public void OnAfterDeserialize()
        {
            lock (this)
            {
                dictionary.Clear();
                for (int i = 0; i < datas.Count; i++)
                {
                    dictionary.Add(datas[i].key, datas[i].asset);
                }
#if !UNITY_EDITOR
                datas.Clear();
#endif
            }


        }

        virtual public void OnBeforeSerialize()
        {
            lock (this)
            {
                datas.Clear();
                foreach (var kvp in dictionary)
                {
                    datas.Add(new Data() { key=kvp.Key,asset=kvp.Value});
                }
            }

        }

        public void AddData(string key, T value)
        {
            dictionary[key] = value;
        }
        
        public bool GetData(string key,out T value) 
        {
            value = null;
            return dictionary?.TryGetValue(key, out value) ?? false;
            
        }
        public bool ContainsKey(string key)
        {
            return dictionary?.ContainsKey(key) ?? false ;
        }
        public void RemoveKey(string key)
        {
            dictionary?.Remove(key);
        }
        public void Clear()
        {
            dictionary?.Clear();
        }
    }
}