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
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

namespace BLG.GTC.Lingguo
{

    public class LingguoConfig : MonoBehaviour
    {
        //class LanguagePackWrap
        [Serializable]
        public class LanguagePackWrap
        {
            [SerializeField]
            internal string language;
            [SerializeField]
            internal AssetReferenceT<LanguagePackage> reference;

        }
        [InspectorName("Current")]
        [SerializeField]
        private uint currentLanguageIndex;
        // Start is called before the first frame update
        //[SerializeField]
        //StringDatabase stringDatabase;
        //[InspectorName("CurrentLanguage")]

        LanguagePackage currentLanguage;
        [SerializeField]
        [InspectorName("SupportLanguages")]
        
        LanguagePackWrap[] languagePackageList;
        AssetReferenceT<LanguagePackage> currentLanguagePackRef;

        public uint CurrentLanguageIndex { get => currentLanguageIndex; 
            set
            { 
                currentLanguageIndex = value;
                Reload();
            }
        }

        public LanguagePackWrap[] LanguagePackageList { get => languagePackageList; }

        private void Awake()
        {


        }
        private void OnEnable()
        {
            
            Reload();
        }
        void Start()
        {
        }


        private void OnValidate()
        {
#if UNITY_EDITOR
            foreach (var warp in languagePackageList)
            {
                if (warp.reference.RuntimeKeyIsValid())
                    warp.language = warp.reference.editorAsset.language;
            }
#endif
            Reload();
        }
        void Reload()
        {
            if (languagePackageList == null || languagePackageList.Length <= 0)
            {
                return;
            }
            currentLanguageIndex = Math.Min(currentLanguageIndex, (uint)languagePackageList.Length - 1);
            currentLanguagePackRef = languagePackageList[currentLanguageIndex].reference;
            if (currentLanguagePackRef.IsValid())
                currentLanguagePackRef.ReleaseAsset();
            var opHandle = currentLanguagePackRef.LoadAssetAsync();
            opHandle.WaitForCompletion();
            if( opHandle.IsDone)
            {
                Handle_Completed(opHandle);
            }
            else
            {
                opHandle.Completed -= Handle_Completed;
                opHandle.Completed += Handle_Completed;
            }
        }

        private void Handle_Completed(AsyncOperationHandle<LanguagePackage> obj)
        {
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                currentLanguage = obj.Result;
                LanguageManager.SetLanguagePackage(currentLanguage);
            }
            else
            {
                Debug.LogError($"{languagePackageList[CurrentLanguageIndex].language} package load failed");
            }
        }

        private void OnDestroy()
        {
            

        }
        public void SwitchLanguage(string language)
        {
            if( language == currentLanguage.language)
            {
                return ;
            }
            var index = Array.FindIndex(languagePackageList, (v) =>
            {
                return v.language == language;
            });
            if (index > 0)
                CurrentLanguageIndex = (uint)index;
            else
            {
                UnityEngine.Debug.LogError($"{language} package can not found");
                return;
            }
            Reload();
        }

    }
}