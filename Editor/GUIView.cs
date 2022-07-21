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

namespace BLG.GTC.Language {

	public static class GUIView {
		
		private static Type guiViewType;
		private static MethodInfo sendEvent;
		private static MethodInfo repaintInfo;
		private static PropertyInfo currentInfo;
		private static MethodInfo repaintImmediatelyInfo;
		private static object[] sendEventParams;

		public static object Current {

			get { return currentInfo.GetValue(null, null); }
		}


		static GUIView() {

			guiViewType = Type.GetType("UnityEditor.GUIView, UnityEditor");

			sendEvent = guiViewType.GetMethod("SendEvent", BindingFlags.NonPublic | BindingFlags.Instance);
			currentInfo = guiViewType.GetProperty("current", BindingFlags.Public | BindingFlags.Static);
			repaintInfo = guiViewType.GetMethod("Repaint", BindingFlags.Public | BindingFlags.Instance);
			repaintImmediatelyInfo = guiViewType.GetMethod("RepaintImmediately", BindingFlags.Public | BindingFlags.Instance);
			sendEventParams = new object[1];
		}

		public static bool SendEvent(object view, Event evt) {

			sendEventParams[0] = evt;

			return (bool)sendEvent.Invoke(view, sendEventParams);
		}

		public static void RepaintCurrent() {

			repaintInfo.Invoke(Current, null);
		}

		public static void RepaintCurrentImmediately() {

			repaintImmediatelyInfo.Invoke(Current, null);
		}
	}
}
