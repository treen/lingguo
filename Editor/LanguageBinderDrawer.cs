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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using BLG.GTC.Lingguo;
using UnityEditorInternal;


namespace BLG.GTC.Lingguo
{
    [CustomPropertyDrawer(typeof(LanguageBinder))]
    public class LanguageBinderDrawer : PropertyDrawer
    {

        float lineHeight = EditorGUIUtility.singleLineHeight + 2;
        //float defaultHeight = 0;
        Dictionary<string, float> totalHeights = new Dictionary<string, float>();
        Dictionary<string, GameObject> selectedGameObjects = new Dictionary<string, GameObject>();
        Dictionary<string, BindableMember> bindMembers = new Dictionary<string, BindableMember>();

        ReorderableList reorderableKeyList;
        
        string MakeBindMemberName(GameObject gameObject, SerializedProperty property, UnityEngine.Object component, string bindName)
        {
            if (component == null)
                return "none";
            //if (component.GetType() == typeof(GameObject))
            //    return "GameObject";
            var language = property.serializedObject.targetObject as Language;
            var objects = gameObject.GetComponents(component.GetType());
            var index = Array.IndexOf(objects, component);
            return index switch
            {
                int i when i < 0 => "none",
                int i when i >= 1 => string.Format("{0}.{1}({2})", component.GetType().Name, bindName, index),
                _ => string.Format("{0}.{1}", component.GetType().Name, bindName)
            };
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //defaultHeight = lineHeight * 2;
            totalHeights[property.propertyPath] = lineHeight*2;
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();

            position = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.indentLevel = 1;
            var bindTypeProperty = property.FindPropertyRelative("keyType");
            EditorGUI.PropertyField(position, bindTypeProperty);

            position = new Rect(position.x, position.y + lineHeight, position.width, EditorGUIUtility.singleLineHeight);

            SerializedProperty keysProperty;
            var keyType = bindTypeProperty.enumDisplayNames[bindTypeProperty.enumValueIndex] switch
            {
                "Static" => LanguageBinder.KeyType.Static,
                "Template" => LanguageBinder.KeyType.Template
            };
            

            float keyListHeight = DrawKeys(position, property, keyType, new GUIContent("Key"));

            
            totalHeights[property.propertyPath] += keyListHeight;


            position = new Rect(position.x, position.y + keyListHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

            DrawBindMember(position, property, label);
            
            position = new Rect(position.x, position.y + position.height, position.width, EditorGUIUtility.singleLineHeight);

            
            //position = CheckBind(position, property, stringKeyProperty.stringValue,memberProperty.stringValue);
            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }
        public float DrawKeys(Rect position,SerializedProperty property, LanguageBinder.KeyType keyType, GUIContent lable)
        {          
            var keysProperty = keyType switch
            {
                LanguageBinder.KeyType.Static => property.FindPropertyRelative("keys"),
                LanguageBinder.KeyType.Template => property.FindPropertyRelative("templates"),
                _ => null,
            };
            
            float keyListHeight = 0;           
            bool isList = bindMembers.TryGetValue(property.propertyPath, out var bindMember) ? bindMember.isList : false;

            //if (bindType == LanguageBinder.BindType.Template && isList)
            //{
            //    EditorGUI.HelpBox(position, "template can not bind to list", MessageType.Error);
            //    return EditorGUIUtility.singleLineHeight * 2;
            //}

            if (isList)
            {
                keysProperty.arraySize = bindMember.arraySize;
                if (keyType == LanguageBinder.KeyType.Static)
                {
                    reorderableKeyList = new ReorderableList(property.serializedObject, keysProperty, true, true, false, false);
                    reorderableKeyList.elementHeight = EditorGUIUtility.singleLineHeight;
                    reorderableKeyList.drawElementCallback = DrawStaticKey;
                    reorderableKeyList.drawHeaderCallback = DrawKeyHeader;
                    
                    reorderableKeyList.DoList(position);
                    
                    keyListHeight = reorderableKeyList.GetHeight();
                }
                else if(keyType == LanguageBinder.KeyType.Template)
                {
                    EditorGUI.PropertyField(position, keysProperty, new GUIContent("Keys"));
                    keyListHeight = EditorGUI.GetPropertyHeight(keysProperty, new GUIContent("Keys"));
                }
                
            }
            else
            {
                keysProperty.arraySize = 1;
                if (keyType == LanguageBinder.KeyType.Static)
                {

                    GuiHelper.KeyField(position, keysProperty.GetArrayElementAtIndex(0),null,LanguageBinder.KeyType.Static);
                    //EditorGUI.PropertyField(position, keysProperty.GetArrayElementAtIndex(0), new GUIContent("Key"));
                }
                else if (keyType == LanguageBinder.KeyType.Template)
                {
                    EditorGUI.ObjectField(position, keysProperty.GetArrayElementAtIndex(0), new GUIContent("Key"));
                }
                keyListHeight = EditorGUIUtility.singleLineHeight;
            }

            return keyListHeight;
        }

        private void DrawKeyHeader(Rect rect)
        {
            EditorGUI.LabelField(rect,new GUIContent("Keys"));
            //EditorGUI.Foldout(rect,)
        }

        public void DrawStaticKey(Rect rect, int index, bool isActive, bool isFocused)
        {
            var  stringField = reorderableKeyList.serializedProperty.GetArrayElementAtIndex(index);
            GuiHelper.KeyField(rect, stringField,null,LanguageBinder.KeyType.Static);
        }

        public void DrawTemplateKey(Rect rect, int index, bool isActive, bool isFocused)
        {
            var stringField = reorderableKeyList.serializedProperty.GetArrayElementAtIndex(index);
            GuiHelper.KeyField(rect, stringField, null, LanguageBinder.KeyType.Template);
        }

        public float DrawBindMember(Rect position, SerializedProperty property, GUIContent label)
        {
            var serializedObject = property.serializedObject;

            var language = serializedObject.targetObject as Language;

            var memberProperty = property.FindPropertyRelative("bindMember");
            var componentProperty = property.FindPropertyRelative("component");
            var component = componentProperty.objectReferenceValue as Component;
            GameObject bindGameObject = component ? component.gameObject : language.gameObject;

            //position = new Rect(position.x, position.y + position.height, position.width, EditorGUIUtility.singleLineHeight);

            //EditorGUI.ObjectField(position, gameObjectProperty);
            selectedGameObjects.TryGetValue(property.propertyPath, out GameObject selectedGameObject);
            selectedGameObject ??= bindGameObject;
            var gameObjectPosition = new Rect(position.x, position.y, 100, EditorGUIUtility.singleLineHeight);
            int controlID = GUIUtility.GetControlID("s_PPtrHash".GetHashCode(), FocusType.Keyboard, position);
            GuiHelper.DoBindGameObjectField(gameObjectPosition, gameObjectPosition, controlID, selectedGameObject,
                language.gameObject, (obj) =>
                {
                    selectedGameObjects[property.propertyPath] = obj;
                    memberProperty.stringValue = "none";
                    GUI.changed = true;
                    property.serializedObject.ApplyModifiedProperties();
                },
                typeof(GameObject), null, EditorStyles.objectField);
            //var objectList = new List<GameObject>();
            //var objectNameList = new List<string>();



            //EnumGameObject(instance.gameObject, objectList, objectNameList, "");
            //int objectSelected = objectList.IndexOf(instanceProperty.objectReferenceValue)
            //EditorGUI.Popup(position, objectSelected, objectNameList.ToArray());
            //EditorGUI.LabelField(labelPosition, "Member");

            var memberList = FindAllBindable(selectedGameObject);
            if (memberList.Length > 1)
            {
                var bindObjectPosition = new Rect(position.x + 100, position.y , position.width - 100, EditorGUIUtility.singleLineHeight);

                var memberName = MakeBindMemberName(bindGameObject, property, componentProperty.objectReferenceValue, memberProperty.stringValue);

                var objectSelected = Array.FindIndex(memberList, 1, (member) => string.Compare(member.fullMemberName, 0, memberName, 0, member.fullMemberName.IndexOf('(')) == 0);
                if (objectSelected < 0)
                {
                    objectSelected = 0;
                }
                var popupString = (from member in memberList
                                   select member.fullMemberName)
                                  .ToArray();

                objectSelected = EditorGUI.Popup(bindObjectPosition, objectSelected, popupString);
                bindMembers[property.propertyPath] = memberList[objectSelected];
                memberProperty.stringValue = memberList[objectSelected].memberName;
                //if (objectSelected == 0)
                //{
                //    componentProperty.objectReferenceValue = memberList.Item1[1];
                //}
                //else
                //{
                componentProperty.objectReferenceValue = memberList[objectSelected].obj;
                //}
            }
            return EditorGUIUtility.singleLineHeight;
        }

        public static void ForEachBindableMember(GameObject gameObject, Action<UnityEngine.Object, MemberInfo, int> action)
        {
            List<string> typeNames = new List<string>();
            var objList = gameObject.GetComponents<Component>();
            foreach (var obj in objList)
            {
                var type = obj.GetType();
                typeNames.Add(type.Name);
                LanguageHelper.ForEachBindableMember(type, (memberInfo) =>
                {
                    action(obj, memberInfo, typeNames.Count((name) => name == type.Name) - 1);
                });
            }
        }
        struct BindableMember {
            public UnityEngine.Object obj;
            public string memberName;
            public string fullMemberName;
            public bool isList;
            public int arraySize;
        }
        BindableMember[] FindAllBindable(GameObject instance)
        {
            var result = new List<BindableMember>();
            result.Add(new BindableMember() { obj = null, memberName = "none", fullMemberName = "none", isList = false });


            ForEachBindableMember(instance, (obj, memberInfo, typeIndex) =>
            {
                var langAttr = memberInfo.GetCustomAttribute<LanguageAttribute>();
                if (langAttr == null)
                {
                    BindableMember bindableMember;
                    bindableMember.obj = obj;
                    bindableMember.memberName = memberInfo.Name;
                    string typeName = "";
                    bindableMember.isList = false;
                    bindableMember.arraySize = 1;
                    if (memberInfo.MemberType == MemberTypes.Field)
                    {
                        var fi = memberInfo as FieldInfo;
                        if (typeof(IList).IsAssignableFrom(fi.FieldType))
                        {
                            bindableMember.isList = true;
                            bindableMember.arraySize = (fi.GetValue(obj) as IList).Count;
                        }
                        typeName = fi.FieldType.Name;
                    }
                    else if (memberInfo.MemberType == MemberTypes.Property)
                    {
                        var pi = memberInfo as PropertyInfo;
                        if (typeof(IList).IsAssignableFrom(pi.PropertyType))
                        {
                            bindableMember.isList = true;
                            bindableMember.arraySize = (pi.GetValue(obj) as IList).Count;
                        }
                        typeName = pi.PropertyType.Name;
                    }
                    bindableMember.fullMemberName = string.Format("{0}.{1}{3}({2})", obj.GetType().Name, memberInfo.Name, typeName, typeIndex > 0 ? string.Format("({0})", typeIndex) : "");
                    result.Add(bindableMember);
                }
            });
            return result.ToArray();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (totalHeights.TryGetValue(property.propertyPath, out float height))
                return height;
            return EditorGUIUtility.singleLineHeight*3;

        }



    }


}