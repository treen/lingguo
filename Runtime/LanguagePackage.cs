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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BLG.GTC.Language
{

    [Serializable]
    [CreateAssetMenu(menuName = "BLG/GTC/Lingguo/CreateLanguagePackage")]
    public class LanguagePackage : ScriptableObject,IEnumerable
    {
        [SerializeField]
        internal string language;
        [SerializeField]
        internal AssetDatabase assetDatabase;

        [SerializeField]
        internal StringDatabase stringDatabase;

        public string Language { get => language; }
        public AssetDatabase AssetDatabase { get => assetDatabase; }
        public StringDatabase StringDatabase { get => stringDatabase; }

        public static LanguagePackage CreateInstance(string language,StringDatabase stringDatabase,AssetDatabase assetDatabase)
        {
            var instance = ScriptableObject.CreateInstance<LanguagePackage>();
            instance.language = language;
            instance.stringDatabase = stringDatabase;
            instance.assetDatabase = assetDatabase;
            return instance;
        }
        public object GetValue(string key)
        {
            string strValue = GetString(key);
            if(strValue != null)
                return strValue;

            AssetReference assetReference = null;
            assetDatabase?.GetData(key, out assetReference);

            return assetReference;
        }

        public AssetReference GetAssetReference(string key)
        {
            AssetReference assetReference = null;
            assetDatabase?.GetData(key, out assetReference);
            if(assetReference != null)
            {
                return assetReference;
            } 
            Debug.LogError($"key:{key} is not int AssetDatabase");      
            return null;
        }
        public string GetString(string key)
        {
            string strValue = null;
            stringDatabase?.GetData(key, out strValue);
            strValue = strValue != null ? LanguageHelper.Translate(strValue, this) : null;
            return strValue;
        }
        public bool ContainsKey(string key)
        {
            var result = stringDatabase?.ContainsKey(key);
            if (result == true)
                return true;

            return assetDatabase?.ContainsKey(key) == true;
        }
        private void OnDestroy()
        {
            
        }

        private void OnValidate()
        {
        }

        public IEnumerator GetEnumerator()
        {
            if(assetDatabase!= null)
            {
                foreach(var asset in assetDatabase.Dictionary)
                {
                    yield return new KeyValuePair<string,object>( asset.Key, asset.Value );
                }
            }
            if( stringDatabase != null)
            {
                foreach(var str in stringDatabase.Dictionary)
                {
                    yield return new KeyValuePair<string, object>(str.Key, str.Value);
                }
            }
        }
        public IEnumerator GetStringEnumerator()
        {
            if (stringDatabase != null)
            {
                foreach (var str in stringDatabase.Dictionary)
                {
                    yield return new KeyValuePair<string, string>(str.Key, str.Value);
                }
            }
        }
    }
}
