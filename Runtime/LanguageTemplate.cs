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
using UnityEngine;
using System.Linq;
using System.Reflection;

namespace BLG.GTC.Lingguo
{
    [CreateAssetMenu(menuName = "Lingguo/Template")]
    [ExecuteAlways]
    public class LanguageTemplate : ScriptableObject
    {
        [SerializeField]
        string templateKey;
        private string value;
        [SerializeField]
        string[] args;

        WeakReference bindWeakreference;
        string memberName;
        int elementIndex = -1;
        Type bindType;
        MemberInfo memberInfo;

        public string Key { get { return templateKey; } }
        private void OnEnable()
        {
            LanguageManager.AddLanguageScript(this);
        }

        private void OnDisable()
        {
            LanguageManager.RemoveLanguageScript(this);
        }
        public string Value { get => value; }

        public void SetKey(string key)
        {
            templateKey = key;
            UpdateValue( LanguageManager.CurrentLanguagePackage);
        }
        public bool SetParams(params string[] args)
        {
            this.args = args;
            UpdateValue(LanguageManager.CurrentLanguagePackage);
            return true;
        }
        public void UpdateValue(LanguagePackage languagePack)
        {
            if (args == null)
            {
                Debug.LogError("args must not be null");
                return;
            }
            var tranlatedArgs = (from arg in args
                   select LanguageHelper.Translate(arg, languagePack))
                   .ToArray();
            value = String.Format(languagePack.GetString(templateKey), tranlatedArgs);

            if(bindWeakreference != null && bindWeakreference.IsAlive )
            {
                var bindObj = bindWeakreference.Target;
                if( elementIndex >= 0)
                {
                    var list = memberInfo switch
                    {
                        FieldInfo fi => fi.GetValue(bindObj) as IList,
                        PropertyInfo pi => pi.GetValue(bindObj) as IList,
                        _=>null
                    };
                    list[elementIndex] = value;
                }
                else
                {
                    if (memberInfo.MemberType == MemberTypes.Field)
                    {
                        (memberInfo as FieldInfo).SetValue(bindObj, value);
                    }
                    else if (memberInfo.MemberType == MemberTypes.Property)
                    {
                        ((PropertyInfo)memberInfo).SetValue(bindObj, value);
                    }
                }
                
            }
        }
        public bool SetParams(int index , string arg)
        {
            if( index >= args.Length )
                Array.Resize(ref args, index + 1);
            args[index] = arg;
            UpdateValue(LanguageManager.CurrentLanguagePackage);
            return true;
        }
        private void OnValidate()
        {
            if( LanguageManager.CurrentLanguagePackage != null )
            {
                OnLanguageSwitch(LanguageManager.CurrentLanguagePackage);
            }
        }

        public void OnLanguageSwitch(LanguagePackage languagePack)
        {
            UpdateValue(languagePack);
        }
        public void Bind(object bindTarget,string memberName)
        {
            Bind(bindTarget, memberName, -1);
        }

        //Bind list target
        public void Bind(object bindTarget, string listMemberName,int index)
        {
            elementIndex = index;
            this.bindWeakreference = new WeakReference(bindTarget);
            this.memberName = listMemberName;
            bindType = bindTarget.GetType();
            memberInfo = bindType.GetMember(memberName)?[0];
            if (LanguageManager.CurrentLanguagePackage)
                UpdateValue(LanguageManager.CurrentLanguagePackage);

        }
    }
}