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
using System.Reflection;

namespace BLG.GTC.Lingguo
{
    class HashWeakReference : WeakReference, IEquatable<HashWeakReference>
    {
        public HashWeakReference(object target) : base(target)
        {
        }

        public bool Equals(HashWeakReference other)
        {
            //Check whether the compared object is null.
            if (System.Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (!System.Object.ReferenceEquals(Target, other.Target)) return false;
            //if (weakReference != other.weakReference) return false;

            //Check whether the products' properties are equal.
            return true;
        }

        public override int GetHashCode()
        {
            if (IsAlive)
            {
                return Target.GetHashCode();
            }
            return 0;
        }
    }
    public class LanguageManager
    {
        static HashSet<HashWeakReference> languageScript = new HashSet<HashWeakReference>();
        //static HashSet<HashWeakReference> languageScripts = new HashSet<HashWeakReference>();
        //static HashSet< WeakReference> languageGameObjects = new HashSet<WeakReference>();
        // Start is called before the first frame update
        static LanguageManager instance;
        static LanguagePackage currentLanguagePack;
        public static LanguagePackage CurrentLanguagePackage { get => currentLanguagePack; }

        //public static  LanguageManager Single
        //{
        //    get
        //    {
        //        //提高分支预测命中率
        //        if (instance != null)
        //            return instance;

        //        instance = new LanguageManager();
        //        return instance; ;
        //    }
        //}

        //this is a WeakReference,so there is no need to remove

        public static void AddLanguageScript(System.Object script)
        {
            languageScript.Add(new HashWeakReference(script));
            if (currentLanguagePack != null)
            {
                try
                {
                    UpdateLanguageObject(script);
                    
                    (script as ILanguage)?.OnLanguageSwitch(currentLanguagePack);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
        public static void RemoveLanguageScript(System.Object script)
        {
            languageScript.Remove(new HashWeakReference(script));
        }
        
        //public static void AddLanguageScript(System.Object obj)
        //{
        //    languageScripts.Add(new HashWeakReference( obj));
        //    UpdateLanguageObject(obj);
        //}


        public static void SetLanguagePackage(LanguagePackage languagePack)
        {
            if (languagePack == null)
            {
                UnityEngine.Debug.LogError("StringDatabase is Null");
                return;
            }

            if( languagePack != LanguageManager.currentLanguagePack)
            {
                LanguageManager.currentLanguagePack = languagePack;

            }
            OnLanguageSwitch();


        }
        internal static  void UpdateLanguageObject(object obj)
        {
            var type = obj.GetType();
            LanguageHelper.ForEachStringMember(type, (memberInfo) =>
            {
                var languageAttribute = memberInfo.GetCustomAttribute<LanguageAttribute>();
                if (languageAttribute != null)
                {
                    string value = currentLanguagePack.GetString(languageAttribute.key);
                    var memberType = memberInfo.MemberType;
                    if (memberType == MemberTypes.Field)
                    {
                        var field = memberInfo as FieldInfo;
                        field.SetValue(obj, value);
                    }
                    else if (memberType == MemberTypes.Property)
                    {
                        var property = memberInfo as PropertyInfo;
                        property.SetValue(obj, value);
                    }
                }
            });

            (obj as LanguageTemplate)?.OnLanguageSwitch(currentLanguagePack);
        }
        static void OnLanguageSwitch()
        {
            languageScript.RemoveWhere((obj)=>!obj.IsAlive);
             

            List<WeakReference> newObjectList = new List<WeakReference>();
            foreach (var weakObj in languageScript)
            {
                if (weakObj.IsAlive)
                {
                    try
                    {
                        var obj = weakObj.Target as System.Object;
                        UpdateLanguageObject(obj);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }

            foreach (var obj in languageScript)
            {
                try
                {
                    (obj.Target as ILanguage)?.OnLanguageSwitch(currentLanguagePack);
                }catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            
        }

        public static string GetString(string key)
        {
            if( currentLanguagePack != null )
                return currentLanguagePack.GetString(key);
            return "There is no StringDatabase";
        }

    }


    
}
