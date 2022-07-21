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
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Events;

namespace BLG.GTC.Language
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public sealed class LanguageGameObject :MonoBehaviour,ILanguage
    {
        public string currentLanguage;
        //public bool copyUnityEvent = true;
        [Serializable]
        public struct GameobjectBinder
        {
            public string language;
            public AssetReference gameObject;
        }
        public List<GameobjectBinder> gameobjectBinders;
        AsyncOperationHandle<GameObject> gameObjectHandle;
        public UnityEvent OnSwitchOut;
        public UnityEvent OnSwitchIn;
        private void OnEnable()
        {
            LanguageManager.AddLanguageScript(this);
        }
        private void OnDisable()
        {
            LanguageManager.RemoveLanguageScript(this);
        }

        private void OnDestroy()
        {
            if (gameObjectHandle.IsValid())
            {
                gameObjectHandle.Completed -= Handler_Completed;
                Addressables.Release(gameObjectHandle);
            }
        }

        public void OnLanguageSwitch(LanguagePackage languagePack)
        {
            if(!Application.isPlaying)
            {
                return;
            }
            if (currentLanguage == languagePack.language)
            {
                return;
            }
            currentLanguage = languagePack.language;
            if (gameObjectHandle.IsValid())
                Addressables.Release(gameObjectHandle);
            var binder = gameobjectBinders.Find((binder) => binder.language == languagePack.language);
            gameObjectHandle = Addressables.LoadAssetAsync<GameObject>(binder.gameObject);
            gameObjectHandle.Completed -= Handler_Completed;
            gameObjectHandle.Completed += Handler_Completed;
        }

        private void Handler_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> obj)
        {            
            if(obj.Status == AsyncOperationStatus.Succeeded )
            {
                GameObject newObject = obj.Result;
                //gameObject.transform.SetPositionAndRotation(transform.localPosition,transform.localRotation);
                //gameObject.transform.localScale = transform.localScale;
                if(!newObject.TryGetComponent<LanguageGameObject>(out LanguageGameObject languageGameObject))
                {
                    languageGameObject = newObject.AddComponent<LanguageGameObject>();
                }
                languageGameObject.gameobjectBinders = gameobjectBinders;
                languageGameObject.currentLanguage = currentLanguage;
                languageGameObject.enabled = enabled;


                newObject = Instantiate(newObject, transform?.parent, false);
                newObject.transform.name = gameObject.transform.name;
                languageGameObject.OnSwitchIn?.Invoke();

                OnSwitchOut?.Invoke();
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }
    }
}