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
using System.Reflection;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;

namespace BLG.GTC.Language
{
    public class LanguageHelper
    {
        static readonly BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        public static void ForEachBindableMember(Type type,Action<MemberInfo> action)
        {
            if (type != typeof(Language))
            {
                var fields = type.GetFields(bindingFlags);
                foreach (var field in fields)
                {
                    if ( LanguageBinder.IsBindable(field.FieldType))
                    {
                        action(field);
                    }
                }
                var properties = type.GetProperties(bindingFlags);
                foreach (var property in properties)
                {
                    if (property.Name == "tag")
                        continue;
                    if (!property.CanWrite)
                        continue;
                    if (LanguageBinder.IsBindable(property.PropertyType))
                    {
                        action(property);
                    }
                }
            }
        }
        public static void ForEachMember<T>(Type type,Action<MemberInfo> action)
        {
            if (type != typeof(Language))
            {
                var fields = type.GetFields(bindingFlags);
                foreach (var field in fields)
                {
                    if (field.FieldType.IsSubclassOf(typeof(T)))
                    {
                        action(field);
                    }
                }
                var properties = type.GetProperties(bindingFlags);
                foreach (var property in properties)
                {
                    if (property.PropertyType.IsSubclassOf(typeof(T)))
                    {
                        action(property);
                    }
                }
            }
        }
        public static void ForEachStringMember(Type type, Action<MemberInfo> action)
        {
            //if (type != typeof(Language))
            {
                var fields = type.GetFields(bindingFlags);
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(string))
                    {
                        action(field);
                    }
                }
                var properties = type.GetProperties(bindingFlags);
                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(string))
                    {
                        action(property);
                    }
                }
            }
        }


        public static bool HasLanguageAttribute(Type type)
        {
            var fields = type.GetFields(bindingFlags);
            foreach (var field in fields)
            {
                if (field.IsDefined(typeof(LanguageAttribute)))
                    return true;
            }
            var properties = type.GetProperties(bindingFlags);
            foreach (var property in properties)
            {
                if (property.IsDefined(typeof(LanguageAttribute)))
                    return true;
            }
            return false;
        }

        public static bool CheckBinder(in LanguageBinder bind, out string errorMessage)
        {
            errorMessage = null;
            
            if (!bind.weakReference.IsAlive)
            {
                errorMessage = "the binded object was nolong alive";
                return false;
            }
            if( string.IsNullOrEmpty(bind.bindMember))
            {
                errorMessage = "there was some binded none";
                return false;
            }
            var language = LanguageManager.CurrentLanguagePackage;
            if (language != null)
            {
                foreach( var key in bind.keys)
                {
                    if (!language.ContainsKey(key))
                    {
                        errorMessage = $"this key:{key} is not in the current language package";
                        return false;
                    }
                    return true;
                };
            }
            return true;
        }

        public static bool ChangeStringMemberKey(string key, System.Object obj, string memberName)
        {
            var memberInfos = obj.GetType().GetMember(memberName, MemberTypes.Field | MemberTypes.Property, bindingFlags);
            if (memberInfos.Length <= 0)
            {
                Debug.LogError($"there is no member named \"{memberName}\"");
                return false;
            }
            var memberInfo = memberInfos[0];
            if (memberInfo != null)
            {
                var attribute = memberInfo.GetCustomAttribute<LanguageAttribute>();
                if (attribute != null)
                {
                    attribute.key = key;
                    string value = LanguageManager.GetString(key);
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
                    return true;
                }
                else
                {
                    Debug.LogError($"\"{memberName}\" dose't have LanguageAttribute");
                    return false;
                }

            }

            return false;
        }
        const string pattern = "(?<!#)#{1}([^#]+)#";
        static public string Translate(string source,LanguagePackage languagePackage)
        {
            var results = Regex.Matches(source, pattern);
            if (results.Count == 0)
                return source;
            StringBuilder sb = new StringBuilder();
            int pos = 0;
            foreach(Match match in results )
            {
                sb.Append(source, pos, match.Groups[0].Index);
                sb.Append(languagePackage.GetString(match.Groups[1].Value));
                pos = match.Groups[0].Index + match.Groups[0].Length;
            }
            return sb.ToString();
        }

    }
}