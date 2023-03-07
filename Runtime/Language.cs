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
using System.Linq;
using UnityEngine.Events;

namespace BLG.GTC.Lingguo
{
    public interface ILanguage
    {
        public void OnLanguageSwitch(LanguagePackage languagePack);
    }

    


    [ExecuteAlways]
    [AddComponentMenu("Lingguo/Language")]
    public class Language : MonoBehaviour,ILanguage
    {
        //public List<StringBind> stringBinds = new List<StringBind>();


        [SerializeField]
#if UNITY_EDITOR
        public
#endif
        List<LanguageBinder> binders;

        [SerializeField]


        public UnityEvent onLanguageChange;

        public string errorMessage = null;
        private void OnEnable()
        {
            LanguageManager.AddLanguageScript(this);
        }

        private void OnDisable()
        {
            LanguageManager.RemoveLanguageScript(this);
        }

        public void BindMember(string key,UnityEngine.Object obj, string memberName)
        {
            var binder = new LanguageBinder(key, obj, memberName);
            AddBinder(binder);

        }

        public void BindArrayMember(string[] keys, UnityEngine.Object obj,string memberName)
        {

        }

        public bool AddBinder( LanguageBinder binder)
        {
            var index = binders.IndexOf(binder);
            if (index >= 0)
                binders[index] = binder;
            else
                binders.Add(binder);
            //binder.SetValue(LanguageManager.GetString(key));
            return binder.UpdateBinder(LanguageManager.CurrentLanguagePackage);
        }


        private void OnValidate()
        {
            if (binders == null)
                return;

            foreach(var bind in binders)
            {
                bind?.UpdateTemplateBind();
            }

            if( Check())
                OnLanguageSwitch(LanguageManager.CurrentLanguagePackage);

            

        }
        public bool Check()
        {
            do
            {
                if (!CheckBinder())
                {
                    break;
                }

                if (binders.Count != binders.Distinct().Count())
                {
                    errorMessage = "重复的字符串绑定目标";
                    break;
                }

                errorMessage = null;

                return true;
            } while (false);

            //Debug.LogError(errorMessage, gameObject);
            return false;
        }
        public bool CheckBinder()
        {
            foreach(var bind in binders)
            {
                if (!LanguageHelper.CheckBinder(bind, out errorMessage))
                    return false;

            }
            return true;
        }

        

        

        

        public void OnLanguageSwitch(LanguagePackage languagePack)
        {
            if(languagePack!=null)
            {
                binders = (from bind in binders
                               where bind.weakReference.IsAlive
                               select bind).ToList();

                foreach (var binder in binders)
                {
                    try
                    {
                        binder.UpdateBinder(languagePack);
                    }
                    catch(Exception e)
                    {
                        errorMessage = e.Message;
                        Debug.LogException(e);
                    }
                }
                onLanguageChange.Invoke();
            }
            Check();
        }

    }
}