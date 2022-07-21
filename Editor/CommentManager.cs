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
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BLG.GTC.Language
{


    public class CommentManager :ScriptableObject, ISerializationCallbackReceiver
    {
        internal Dictionary<ScriptableObject, CommentData> commentDictionary = new Dictionary<ScriptableObject, CommentData>();
        [SerializeField]
        List<ScriptableObject> keyList = new List<ScriptableObject>();
        [SerializeField]
        List<CommentData> commentList = new List<CommentData>();

        static CommentManager instance = null;
        const string assetPath = "Assets/Language/Editor/CommentManager.asset";
        const string commentFolder = "Assets/Language/Editor/Comment/";
        public static CommentManager Single()
        {
            if (instance)
                return instance;
            instance = UnityEditor.AssetDatabase.LoadAssetAtPath<CommentManager>(assetPath);
            if (instance == null)
            {
                instance = CommentManager.CreateInstance<CommentManager>();
                //UnityEditor.AssetDatabase.CreateFolder("Assets", "Language");
                //UnityEditor.AssetDatabase.CreateFolder("Assets/Language", "Editor");
                //UnityEditor.AssetDatabase.CreateFolder("Assets/Language/Editor", "Comment");
                GuiHelper.CreateFolder(assetPath);
                GuiHelper.CreateFolder(commentFolder);
                UnityEditor.AssetDatabase.CreateAsset(instance, assetPath);
            }

            return instance;
        }
        public StringDatabase GetOrCreateCommentAsset(ScriptableObject database)
        {
            if (commentDictionary.TryGetValue(database, out CommentData comment))
            {
                if( comment)
                    return comment;
            }
            GuiHelper.CreateFolder(commentFolder);
            var commentPath = commentFolder + $"{database.name}Comment.asset";
            comment = StringDatabase.CreateInstance<CommentData>();
            UnityEditor.AssetDatabase.CreateAsset(comment, commentPath);
            commentDictionary[database] = comment;
            return comment;
        }
        public void Save()
        {
            EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            foreach(var comment in commentDictionary.Values)
            {
                EditorUtility.SetDirty(comment);
                UnityEditor.AssetDatabase.SaveAssetIfDirty(comment);
            }
        }
        public void OnAfterDeserialize()
        {
            commentDictionary.Clear();
            for(int i = 0; i < keyList.Count; ++ i)
            {
                if( keyList[i] != null)
                    commentDictionary[keyList[i]] = commentList[i];
            }

        }

        public void OnBeforeSerialize()
        {
            keyList.Clear();
            commentList.Clear();
            foreach(var (k,v)in commentDictionary)
            {
                if (k != null)
                {
                    keyList.Add(k);
                    commentList.Add(v);
                }
            }

        }

        public string GetComment(LanguagePackage languagePackage,string key)
        {
            string comment = null;
            commentDictionary?.GetValueOrDefault(languagePackage.StringDatabase)?.GetData(key,out comment);
            if(comment == null)
                commentDictionary?.GetValueOrDefault(languagePackage.AssetDatabase)?.GetData(key, out comment);
            return comment;            
        }
        public string GetComment(ScriptableObject database, string key)
        {
            string comment = null;
            commentDictionary?.GetValueOrDefault(database)?.GetData(key, out comment);
            return comment;
        }
    }
}