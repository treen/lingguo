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
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;

namespace BLG.GTC.Language
{
    
    internal static class GuiHelper
    {
        internal static EditorStyles s_Current;
        private static GUIContent s_SceneMismatch = EditorGUIUtility.TrTextContent("Scene mismatch (cross scene references not supported)");
        private static GUIContent s_TypeMismatch = EditorGUIUtility.TrTextContent("Type mismatch");
        private static Color s_MixedValueContentColorTemp = Color.white;
        private static readonly Color s_MixedValueContentColor = new Color(1f, 1f, 1f, 0.5f);
        internal static GUIStyle ms_Error;
        internal static GUIStyle objectFieldButton => GetStyle("ObjectFieldButton");
        public static bool showMixedValue { get; set; }
        [Flags]
        internal enum ObjectFieldValidatorOptions
        {
            None = 0x0,
            ExactObjectTypeValidation = 0x1
        }

        internal delegate UnityEngine.Object ObjectFieldValidator(UnityEngine.Object[] references, Type objType, SerializedProperty property, ObjectFieldValidatorOptions options);
        internal enum ObjectFieldVisualType
        {
            IconAndText,
            LargePreview,
            MiniPreview
        }


        private static bool ValidDroppedObject(UnityEngine.Object[] references, Type objType, GameObject startObject, out string errorString)
        {
            errorString = "";
            if (references == null || references.Length == 0)
            {
                return true;
            }

            UnityEngine.Object @object = references[0];
            var object2 = EditorUtility.InstanceIDToObject(@object.GetInstanceID()) as GameObject;
            if (object2 == null)
                return false;
            GameObject parent = object2.transform.parent?.gameObject;
            while(parent != null)
            {
                if (startObject == parent)
                    return true;
                parent = parent.transform.parent?.gameObject;
            }
            errorString = "object must child node";
            return false;
        }
        private static Rect GetButtonRect(ObjectFieldVisualType visualType, Rect position)
        {
            return visualType switch
            {
                ObjectFieldVisualType.IconAndText => new Rect(position.xMax - 19f, position.y, 19f, position.height),
                ObjectFieldVisualType.MiniPreview => new Rect(position.xMax - 14f, position.y, 14f, position.height),
                ObjectFieldVisualType.LargePreview => new Rect(position.xMax - 36f, position.yMax - 14f, 36f, 14f),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
        internal static GameObject DoBindGameObjectField(Rect position, Rect dropRect, int id,GameObject obj, GameObject startObject,Action<GameObject> onObjectSelectedChange,
            Type objType, SerializedProperty property, GUIStyle style, ObjectFieldValidator validator=null)
        {
            if (validator == null)
            {
                validator = ValidateObjectFieldAssignment;
            }

            if (property != null)
            {
                obj = property.objectReferenceValue as GameObject ?? obj;
            }

            Event current = Event.current;
            EventType eventType = current.type;
            if (!GUI.enabled && Event.current.rawType == EventType.MouseDown)
            {
                eventType = Event.current.rawType;
            }

            bool flag = EditorGUIUtility.HasObjectThumbnail(objType);
            ObjectFieldVisualType objectFieldVisualType = ObjectFieldVisualType.IconAndText;
            //if (flag && position.height <= 18f && position.width <= 32f)
            //{
            //    objectFieldVisualType = ObjectFieldVisualType.MiniPreview;
            //}
            //else if (flag && position.height > 18f)
            //{
            //    objectFieldVisualType = ObjectFieldVisualType.LargePreview;
            //}

            Vector2 iconSize = EditorGUIUtility.GetIconSize();
            switch (objectFieldVisualType)
            {
                case ObjectFieldVisualType.IconAndText:
                    EditorGUIUtility.SetIconSize(new Vector2(12f, 12f));
                    break;
                case ObjectFieldVisualType.LargePreview:
                    EditorGUIUtility.SetIconSize(new Vector2(64f, 64f));
                    break;
            }

            switch (eventType)
            {
                case EventType.DragExited:
                    if (GUI.enabled)
                    {
                        HandleUtility.Repaint();
                    }

                    break;
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    {
                        if (eventType == EventType.DragPerform && !ValidDroppedObject(DragAndDrop.objectReferences, objType, startObject, out var errorString))
                        {
                            UnityEngine.Object object2 = DragAndDrop.objectReferences[0];
                            EditorUtility.DisplayDialog("Can't assign script", errorString, "OK");
                            Event.current.Use();
                        }
                        else
                        {
                            if (!dropRect.Contains(Event.current.mousePosition) || !GUI.enabled)
                            {
                                break;
                            }

                            UnityEngine.Object[] objectReferences = DragAndDrop.objectReferences;
                            UnityEngine.Object object3 = validator(objectReferences, objType, property, ObjectFieldValidatorOptions.None);
                            //if (object3 != null && /*!allowSceneObjects &&*/ !EditorUtility.IsPersistent(object3))
                            //{
                            //    object3 = null;
                            //}

                            if (!(object3 != null))
                            {
                                break;
                            }

                            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                            if (eventType == EventType.DragPerform)
                            {
                                obj = object3 as GameObject;
                                if (property != null)
                                {
                                    property.objectReferenceValue = object3;
                                }
                                onObjectSelectedChange(obj);
                                GUI.changed = true;
                                DragAndDrop.AcceptDrag();
                                DragAndDrop.activeControlID = 0;
                            }
                            else
                            {
                                DragAndDrop.activeControlID = id;
                            }
                            Event.current.Use();

                        }

                        break;
                    }
                case EventType.MouseDown:
                    {
                        if (!position.Contains(Event.current.mousePosition))
                        {
                            break;
                        }

                        if (Event.current.button != 0)
                        {
                            break;
                        }

                        Rect buttonRect = GetButtonRect(objectFieldVisualType, position);
                        EditorGUIUtility.editingTextField = false;
                        if (buttonRect.Contains(Event.current.mousePosition))
                        {
                            if (GUI.enabled)
                            {
                                GUIUtility.keyboardControl = id;
                                if (property != null)
                                {
                                    BindGameObjectSelectorWindow.Show(startObject, onObjectSelectedChange, null);
                                }
                                else
                                {
                                    BindGameObjectSelectorWindow.Show(startObject, onObjectSelectedChange, null);
                                }

                                BindGameObjectSelectorWindow.objectSelectorID = id;
                                current.Use();
                                GUIUtility.ExitGUI();
                            }

                            break;
                        }

                        


                        if (Event.current.clickCount == 1)
                        {
                            GUIUtility.keyboardControl = id;
                            PingObjectOrShowPreviewOnClick(obj, position);
                            current.Use();
                        }
                        else if (Event.current.clickCount == 2)
                        {
                            if ((bool)obj)
                            {
                                UnityEditor.AssetDatabase.OpenAsset(obj);
                                GUIUtility.ExitGUI();
                            }

                            current.Use();
                        }

                        break;
                    }
                case EventType.ExecuteCommand:
                    {
                        string commandName = current.commandName;
                        if (commandName == "ObjectSelectorUpdated" && BindGameObjectSelectorWindow.objectSelectorID == id &&
                            GUIUtility.keyboardControl == id && (property == null || !(property.type == "PPtr<MonoScript>")))
                        {
                            return AssignSelectedObject(property, validator, objType, current);
                        }

                        if (commandName == "ObjectSelectorClosed" && BindGameObjectSelectorWindow.objectSelectorID == id &&
                            GUIUtility.keyboardControl == id && property != null && (property.type == "PPtr<MonoScript>"))
                        {
                            if (BindGameObjectSelectorWindow.instance.GetInstanceID() != 0)
                            {
                                return AssignSelectedObject(property, validator, objType, current);
                            }

                            current.Use();
                        }

                        break;
                    }
                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl != id)
                    {
                        break;
                    }

                    if (current.keyCode == KeyCode.Backspace || (current.keyCode == KeyCode.Delete && (current.modifiers & EventModifiers.Shift) == 0))
                    {
                        if (property != null)
                        {
                            if (property.propertyPath.EndsWith("]"))
                            {

                                property.objectReferenceValue = null;
                            }
                            else
                            {
                                property.objectReferenceValue = null;
                            }
                        }
                        else
                        {
                            obj = null;
                        }

                        GUI.changed = true;
                        current.Use();
                    }

                    if (MainActionKeyForControl(current, id))
                    {
                        if (property != null)
                        {
                            BindGameObjectSelectorWindow.Show(startObject, onObjectSelectedChange, null);
                        }
                        else
                        {
                            BindGameObjectSelectorWindow.Show(startObject, onObjectSelectedChange,null);
                        }

                        BindGameObjectSelectorWindow.objectSelectorID = id;
                        current.Use();
                        GUIUtility.ExitGUI();
                    }

                    break;
                case EventType.Repaint:
                    {
                        GUIContent gUIContent;

                        {
                            gUIContent = ((obj == null && (object)objType == null && property != null) ?
                                EditorGUIUtility.TrTempContent(property.objectReferenceValue.ToString()) :
                                ((property == null) ?
                                EditorGUIUtility.ObjectContent(obj, objType) :
                                EditorGUIUtility.ObjectContent(obj, objType)));
                            if (property != null && obj != null)
                            {
                                UnityEngine.Object[] references = new UnityEngine.Object[1] { obj };
                                if (EditorSceneManager.preventCrossSceneReferences && CheckForCrossSceneReferencing(obj, property.serializedObject.targetObject))
                                {
                                    if (!EditorApplication.isPlaying)
                                    {
                                        gUIContent = s_SceneMismatch;
                                    }
                                    else
                                    {
                                        gUIContent.text += $" ({GetGameObjectFromObject(obj).scene.name})";
                                    }
                                }
                                else if (validator(references, objType, property, ObjectFieldValidatorOptions.ExactObjectTypeValidation) == null)
                                {
                                    gUIContent = s_TypeMismatch;
                                }
                            }
                        }

                        switch (objectFieldVisualType)
                        {
                            case ObjectFieldVisualType.IconAndText:
                                {
                                    BeginHandleMixedValueContentColor();
                                    style.Draw(position, gUIContent, id, DragAndDrop.activeControlID == id, position.Contains(Event.current.mousePosition));
                                    Rect position2 = objectFieldButton.margin.Remove(GetButtonRect(objectFieldVisualType, position));
                                    objectFieldButton.Draw(position2, GUIContent.none, id, DragAndDrop.activeControlID == id, position2.Contains(Event.current.mousePosition));
                                    EndHandleMixedValueContentColor();
                                    break;
                                }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    }
            }

            EditorGUIUtility.SetIconSize(iconSize);
            return obj;
        }
        internal static void BeginHandleMixedValueContentColor()
        {
            s_MixedValueContentColorTemp = GUI.contentColor;
            GUI.contentColor = (showMixedValue ? (GUI.contentColor * s_MixedValueContentColor) : GUI.contentColor);
        }

        internal static void EndHandleMixedValueContentColor()
        {
            GUI.contentColor = s_MixedValueContentColorTemp;
        }
        internal static GameObject GetGameObjectFromObject(UnityEngine.Object obj)
        {
            GameObject gameObject = obj as GameObject;
            if (gameObject == null && obj is Component)
            {
                gameObject = ((Component)obj).gameObject;
            }

            return gameObject;
        }
        internal static bool CheckForCrossSceneReferencing(UnityEngine.Object obj1, UnityEngine.Object obj2)
        {
            GameObject gameObjectFromObject = GetGameObjectFromObject(obj1);
            if (gameObjectFromObject == null)
            {
                return false;
            }

            GameObject gameObjectFromObject2 = GetGameObjectFromObject(obj2);
            if (gameObjectFromObject2 == null)
            {
                return false;
            }

            if (EditorUtility.IsPersistent(gameObjectFromObject) || EditorUtility.IsPersistent(gameObjectFromObject2))
            {
                return false;
            }

            if (!gameObjectFromObject.scene.IsValid() || !gameObjectFromObject2.scene.IsValid())
            {
                return false;
            }

            return gameObjectFromObject.scene != gameObjectFromObject2.scene;
        }

        internal static UnityEngine.Object ValidateObjectFieldAssignment(UnityEngine.Object[] references, Type objType, SerializedProperty property, ObjectFieldValidatorOptions options)
        {
            if (references.Length != 0)
            {
                bool flag = DragAndDrop.objectReferences.Length != 0;
                bool flag2 = references[0] != null && references[0] is Texture2D;


                //if (property != null)
                {
                    if (references[0] != null)
                        return references[0];
                }
            }

            return null;
        }

        private static GameObject AssignSelectedObject(SerializedProperty property, ObjectFieldValidator validator, Type objectType, Event evt)
        {
            UnityEngine.Object[] references = new UnityEngine.Object[1] { BindGameObjectSelectorWindow.instance.GetCurrentObject() };
            GameObject @object = validator(references, objectType, property, ObjectFieldValidatorOptions.None) as GameObject;
            if (property != null)
            {
                property.objectReferenceValue = @object;
            }

            GUI.changed = true;
            evt.Use();
            return @object;
        }
        internal static bool MainActionKeyForControl(this Event evt, int controlId)
        {
            if (GUIUtility.keyboardControl != controlId)
            {
                return false;
            }

            bool flag = evt.alt || evt.shift || evt.command || evt.control;
            return evt.type == EventType.KeyDown && (evt.keyCode == KeyCode.Space || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter) && !flag;
        }
        internal static void PingObjectOrShowPreviewOnClick(UnityEngine.Object targetObject, Rect position)
        {
            if (!(targetObject == null))
            {
                Event current = Event.current;
                if (!current.shift && !current.control)
                {
                    EditorGUIUtility.PingObject(targetObject);
                }
                else if (targetObject is Texture)
                {
                    //PopupWindowWithoutFocus.Show(new RectOffset(6, 3, 0, 3).Add(position), new ObjectPreviewPopup(targetObject), new PopupLocation[3]
                    //{
                    //    PopupLocation.Left,
                    //    PopupLocation.Below,
                    //    PopupLocation.Right
                    //});
                }
            }
        }
        internal static GUIStyle error
        {
            get
            {
                if (ms_Error == null)
                {
                    ms_Error = new GUIStyle();
                    ms_Error.name = "StyleNotFoundError";
                }

                return ms_Error;
            }
        }
        internal static GUIStyle GetStyle(string styleName)
        {
            GUIStyle gUIStyle = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
            if (gUIStyle == null)
            {
                Debug.LogError("Missing built-in guistyle " + styleName);
                gUIStyle = error;
            }

            return gUIStyle;
        }

        public static void KeyField(Rect rect, SerializedProperty property,GUIContent label,LanguageBinder.KeyType keyType)
        {
            float searchButtonSize = EditorGUIUtility.singleLineHeight - 2;
            //var stringField = reorderableKeyList.serializedProperty.GetArrayElementAtIndex(index);
            var fieldRect = new Rect(rect.x, rect.y, rect.width - searchButtonSize, rect.height);
            
            EditorGUI.PropertyField(fieldRect, property,  label);

            var buttonRect = new Rect(rect.x + rect.width - searchButtonSize, rect.y + 1, searchButtonSize, searchButtonSize);
            if (GUI.Button(buttonRect, "", GuiHelper.objectFieldButton))
            {
                KeySelectorWindow.Show(property, keyType);
            }

        }

        public static string CreateFolder(string path)
        {
            path = System.IO.Path.GetDirectoryName(path);
            char[] delimiterChars = { '/', '\\' };
            var folders = path.Split(delimiterChars);
            int index = folders?[0] == "Assets" ? 1 : 0;
            var parentFolder = "Assets";
            for(; index < folders.Length; index++)
            {
                var newFolder = $"{parentFolder}/{folders[index]}";
                if (!UnityEditor.AssetDatabase.IsValidFolder(newFolder))
                    UnityEditor.AssetDatabase.CreateFolder(parentFolder, folders[index]);
                parentFolder = newFolder;
            }
            return parentFolder;
        }
    }
}
