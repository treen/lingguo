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
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
namespace BLG.GTC.Language
{



    [Serializable]
    public class LanguageBinder : ISerializationCallbackReceiver ,IEquatable<LanguageBinder>
    {
        public enum KeyType
        {
            Static = 0,
            Template = 1
        };
        static BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        public static Type[] bindableTypes = new Type[] {typeof(Sprite),  typeof(Texture2D), typeof(string), typeof(AudioClip),typeof(Mesh),typeof(Material),typeof(ScriptableObject)};
        [SerializeField]
        KeyType keyType;

        [Delayed]
        [SerializeField]
        internal string[] keys;

        [SerializeField]
        internal LanguageTemplate[] templates;

        public LanguageTemplate[] Templates { get => templates; }
        public string[] Keys { get => keys; }

        [SerializeField]
        UnityEngine.Object component;
        internal WeakReference weakReference;
        [SerializeField]
        internal string bindMember;


        public static bool IsBindable(Type type)
        {
            return typeof(IList).IsAssignableFrom(type) switch
            {
                false => bindableTypes.Any(t => t.IsAssignableFrom(type)),
                true => bindableTypes.Any(t => t.IsAssignableFrom(type.GetElementType())),
            };
        }
        internal MemberInfo GetBindMember()
        {
            var obj = weakReference.Target as UnityEngine.Object;
            if (obj != null)
            {
                var type = obj.GetType();
                var member = type.GetMember(bindMember, MemberTypes.Field | MemberTypes.Property, bindingFlags) switch
                    {
                        MemberInfo[] infos when infos.Length>0=> infos[0],
                        _=> null,
                    };
                return member;
            }
            return null;
        }
        public Type GetBindType() => GetBindMember() switch {
                FieldInfo info => info.FieldType,
                PropertyInfo info=> info.PropertyType,
                _=> null,
            };
        public LanguageBinder(string key,UnityEngine.Object obj,string memberName)
        {
            component = null;
            this.keys = new string[] { key};
            templates = new LanguageTemplate[1];
            weakReference = new WeakReference(obj);
            bindMember = memberName;
            keyType = KeyType.Static;
        }
        public LanguageBinder(IEnumerable<string> keys, UnityEngine.Object obj, string memberName)
        {
            component = null;
            this.keys = keys.ToArray();
            weakReference = new WeakReference(obj);
            bindMember = memberName;
            keyType = KeyType.Static;

        }

        public bool Equals(LanguageBinder other)
        {
            //Check whether the compared object is null.
            if (System.Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (!System.Object.ReferenceEquals(weakReference.Target, other.weakReference.Target)) return false;
            //if (weakReference != other.weakReference) return false;

            //Check whether the products' properties are equal.
            return bindMember.Equals(other.bindMember);
        }

        public override int GetHashCode()
        {
            int hashInstance = 0;
            if( weakReference != null && weakReference.Target != null)
            {
                hashInstance = weakReference.Target.GetHashCode();
            }
            return bindMember.GetHashCode() ^ hashInstance;
        }
        public void OnAfterDeserialize()
        {
            weakReference = new WeakReference(component);
#if UNITY_EDITOR
            component = null;
#endif
        }

        public void OnBeforeSerialize()
        {
            component = weakReference.Target as UnityEngine.Object;
        }
        public bool SetValues(object[] values)
        {
            var obj = weakReference.Target as UnityEngine.Object;
            if (obj != null && values != null)
            {
                var type = obj.GetType();
                var member = type.GetMember(bindMember, MemberTypes.Field | MemberTypes.Property, bindingFlags)[0];
                var memberType = member.MemberType;
                if (memberType == MemberTypes.Field)
                {
                    var field = member as FieldInfo;
                    field.SetValue(obj, values);
                }
                else if (memberType == MemberTypes.Property)
                {
                    var property = member as PropertyInfo;
                    property.SetValue(obj, values);
                }
            }

            return false;
        }
        public bool SetValue(object value)
        {
            var obj = weakReference.Target as UnityEngine.Object;
            if (obj != null && value != null)
            {
                var type = obj.GetType();
                var member = type.GetMember(bindMember, MemberTypes.Field | MemberTypes.Property, bindingFlags)[0];
                SetValue(member,obj,value);
            }

            return false;
        }
        void SetValue(MemberInfo member,object obj,object value)
        {
            var memberType = member.MemberType;
            if (memberType == MemberTypes.Field)
            {
                var field = member as FieldInfo;
                field.SetValue(obj, value);
            }
            else if (memberType == MemberTypes.Property)
            {
                var property = member as PropertyInfo;
                property.SetValue(obj, value);
            }
        }
        void SetValueAtIndex(IList list, int index,object value)
        {
            list[index] = value;
        }
        bool SetValue(Type type, LanguagePackage languagePack)
        {
            void OnLoadAsset( object obj)
            {
                SetValue(obj);
            }
            var value = keyType switch
            {
                KeyType.Static => languagePack.GetValue(keys[0]),
                KeyType.Template => templates[0].Value,
                _=>null,
            };            
            return value switch
            {
               string v=> SetValue(v),
               AssetReference v =>LoadAssetAsync(type, v, OnLoadAsset),
               _=> throw new Exception("not support value type"),
            };
        }


        void SetValues(IList list,Type type, LanguagePackage languagePack)
        {
            if( keyType == KeyType.Static )
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    void OnLoadAsset(object obj)
                    {
                        list[i] = obj;
                    }
                    var value = languagePack.GetValue(keys[i]);
                    if (value is string)
                    {
                        list[i] = value;
                    }
                    else
                    {
                        LoadAssetAsync(type, value as AssetReference, OnLoadAsset);
                    }
                }
            }
            else if( keyType == KeyType.Template )
            {
                int count = Math.Min(templates.Length, list.Count);
                for (int i = 0; i < count; ++i)
                {
                    list[i] = templates[i]?.Value??"";
                }
            }
            
        }

        void SetDropdownValues(List<Dropdown.OptionData> list, LanguagePackage languagePack)
        {
            if (keyType == KeyType.Static)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    var value = languagePack.GetValue(keys[i]);
                    if (value is string)
                    {
                        list[i].text = languagePack.GetString(keys[i]);
                    }
                }
            }
            else if (keyType == KeyType.Template)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    var value = languagePack.GetValue(keys[i]);
                    if (value is string)
                    {
                        list[i].text = templates[i].Value;
                    }
                }
            }
        }

        internal bool UpdateBinder(LanguagePackage languagePack)
        {
            if( keys?.Length > 0 )
            {
                var obj = weakReference.Target as UnityEngine.Object;
                if (obj != null)
                {
                    var type = GetBindType();
                    var member = GetBindMember();
                    if (typeof(IList).IsAssignableFrom(type))
                    {
                        IList list = member switch
                        {
                            FieldInfo fi => fi.GetValue(obj) as IList,
                            PropertyInfo pi => pi.GetValue(obj) as IList,
                            _=>null
                        };
                        if (typeof(List<Dropdown.OptionData>).IsAssignableFrom(type))
                        {
                            SetDropdownValues(list as List<Dropdown.OptionData>, languagePack);
                        }
                        else
                        {
                            SetValues(list, type, languagePack);
                        }
                        return true;
                    }
                    if (type == null)
                        return false;
                    SetValue(type, languagePack);
                }
            }
            else
            {
                Debug.LogError("binder key is null or none");
            }

            return true;

        }

        bool LoadAssetAsync(Type type, AssetReference assetReference, Action<object> action)
        {
            if (assetReference != null)
            {
                if (type.IsAssignableFrom(typeof(Sprite)))
                {
                    return LoadAssetAsync<Sprite>(assetReference, action);
                }
                else if (type.IsAssignableFrom(typeof(Texture2D)))
                {
                    return LoadAssetAsync<Texture2D>(assetReference, action);
                }
                else if (type.IsAssignableFrom(typeof(AudioClip)))
                {
                    return LoadAssetAsync<AudioClip>(assetReference, action);
                }
                else if (type.IsAssignableFrom(typeof(Material)))
                {
                    return LoadAssetAsync<Material>(assetReference, action);
                }
                else if (type.IsAssignableFrom(typeof(Mesh)))
                {
                    return LoadAssetAsync<Mesh>(assetReference, action);
                }
                else
                {
                    return false;
                }

            }
            return false;

        }
        bool LoadAssetAsync<T>(AssetReference assetReference , Action<T> action) where T : UnityEngine.Object
        {
            void Handler_Completed(AsyncOperationHandle<T> obj)
            {
                //SetValue(obj.Result);
                action(obj.Result);
                obj.Completed -= Handler_Completed;
                Addressables.Release(obj);
            }
            if (assetReference != null)
            {
                AsyncOperationHandle<T> handler = Addressables.LoadAssetAsync<T>(assetReference);

                if (handler.Status == AsyncOperationStatus.Succeeded)
                {
                    SetValue(handler.Result);
                }
                else
                {
                    handler.Completed -= Handler_Completed;
                    handler.Completed += Handler_Completed;
                }
               
                return true;
            }
            return false;
        }

        internal void  UpdateTemplateBind()
        {
            if( keyType == KeyType.Template && weakReference.IsAlive)
            {
                var type = GetBindType();

                if (typeof(IList).IsAssignableFrom(type))
                {
                    for (int i = 0; i < templates.Length; i++)
                    {
                        templates[i]?.Bind(weakReference.Target, bindMember, i);
                    }
                } 
                else
                {
                    templates[0]?.Bind(weakReference.Target, bindMember);

                }

            }
        }

    }


}