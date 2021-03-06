﻿/* 
QuickMute
Copyright 2017 Malah

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>. 
*/

using System.Collections;
using UnityEngine;

namespace QuickMute {
	public partial class QGUI {
		
		internal static QGUI Instance;

		[KSPField (isPersistant = true)] internal static QBlizzyToolbar BlizzyToolbar;

		internal bool WindowSettings = false;

		internal bool Draw = false;

		Coroutine wait;

		string Icon_TexturePathSound = MOD + "/Textures/Icon_sound";
		string Icon_TexturePathMute = MOD + "/Textures/Icon_mute";

		Texture2D Icon_Texture {
			get {
				return GameDatabase.Instance.GetTexture ((QSettings.Instance.Muted ? Icon_TexturePathMute : Icon_TexturePathSound), false);
			}
		}

		Rect rectSettings = new Rect();
		Rect RectSettings {
			get {
				rectSettings.x = (Screen.width - rectSettings.width) / 2;
				rectSettings.y = (Screen.height - rectSettings.height) / 2;
				return rectSettings;
			}
			set {
				rectSettings = value;
			}
		}

		Rect rectSetKey = new Rect ();
		Rect RectSetKey {
			get {
				rectSetKey.x = (Screen.width - rectSetKey.width) / 2;
				rectSetKey.y = (Screen.height - rectSetKey.height) / 2;
				return rectSetKey;
			}
			set {
				rectSetKey = value;
			}
		}

		IEnumerator Wait(int seconds) {
			yield return new WaitForSeconds (seconds);
			QGUI.Instance.Draw = false;
			wait = null;
			Log ("Wait", "QGUI");
		}

		protected override void Awake() {
			Instance = this;
			if (BlizzyToolbar == null) BlizzyToolbar = new QBlizzyToolbar ();
			GameEvents.onVesselGoOffRails.Add (OnVesselGoOffRails);
			Log ("Awake", "QGUI");
		}

		protected override void Start() {
			BlizzyToolbar.Start ();
			if (QSettings.Instance.Muted) {
				Mute (true);
			}
			Log ("Start", "QGUI");
		}

		protected override void OnDestroy() {
			BlizzyToolbar.OnDestroy ();
			GameEvents.onVesselGoOffRails.Remove (OnVesselGoOffRails);
			Log ("OnDestroy", "QGUI");
		}

		void OnVesselGoOffRails(Vessel vessel) {
			QMute.Verify ();
			Log ("OnVesselGoOffRails", "QGUI");
		}

		public void Mute() {
			Mute (!QSettings.Instance.Muted);
		}

		void Mute(bool mute) {
			QSettings.Instance.Muted = mute;
			if (BlizzyToolbar != null) {
				BlizzyToolbar.Refresh ();
			}
			if (QStockToolbar.Instance != null) {
				QStockToolbar.Instance.Refresh ();
			}
			QMute.refresh (mute);
			Draw = true;
			if (wait != null) {
				StopCoroutine (wait);
			}
			wait = StartCoroutine (Wait (5));
			QSettings.Instance.Save ();
			Log ("Mute()", "QGUI");
		}

		void OnApplicationQuit() {
			Mute (false);
			GameSettings.SaveSettings ();
			Log ("OnApplicationQuit", "QGUI");
		}

		void Lock(bool activate, ControlTypes Ctrl) {
			if (HighLogic.LoadedSceneIsFlight) {
				FlightDriver.SetPause (activate);
				if (activate) {
					InputLockManager.SetControlLock (ControlTypes.CAMERACONTROLS | ControlTypes.MAP, "Lock" + MOD);
					return;
				}
			}
			else if (HighLogic.LoadedSceneIsEditor) {
				if (activate) {
					EditorLogic.fetch.Lock (true, true, true, "EditorLock" + MOD);
					return;
				}
				else {
					EditorLogic.fetch.Unlock ("EditorLock" + MOD);
				}
			}
			if (activate) {
				InputLockManager.SetControlLock (Ctrl, "Lock" + MOD);
				return;
			}
			else {
				InputLockManager.RemoveControlLock ("Lock" + MOD);
			}
			if (InputLockManager.GetControlLock ("Lock" + MOD) != ControlTypes.None) {
				InputLockManager.RemoveControlLock ("Lock" + MOD);
			}
			if (InputLockManager.GetControlLock ("EditorLock" + MOD) != ControlTypes.None) {
				InputLockManager.RemoveControlLock ("EditorLock" + MOD);
			}
			Log ("Lock " + activate, "QGUI");
		}

		public void Settings () {
			if (WindowSettings) {
				HideSettings ();
			}
			else {
				ShowSettings ();
			}
			Log ("Settings", "QGUI");
		}

		internal void ShowSettings () {
			WindowSettings = true;
			Switch (true);
			Log ("ShowSettings", "QGUI");
		}
			
		internal void HideSettings () {
			WindowSettings = false;
			Switch (false);
			Save ();
			Log ("HideSettings", "QGUI");
		}

		internal void Switch (bool set)	{
			QStockToolbar.Instance.Set (WindowSettings, false);
			Lock (WindowSettings, ControlTypes.KSC_ALL | ControlTypes.TRACKINGSTATION_UI | ControlTypes.CAMERACONTROLS | ControlTypes.MAP);
			Log ("Switch: " + set, "QGUI");
		}

		void Save () {
			QStockToolbar.Instance.Reset ();
			BlizzyToolbar.Reset ();
			QSettings.Instance.Save ();
			Log ("Save", "QGUI");
		}

		void Update() {
			if (Input.GetKeyDown (QSettings.Instance.KeyMute)) {
				Mute ();
			}
			if (QKey.SetKey == QKey.Key.None) {
				return;
			}
			if (Event.current.isKey) {
				KeyCode _key = Event.current.keyCode;
				if (_key != KeyCode.None) {
					QKey.SetCurrentKey (QKey.SetKey, _key);
					QKey.SetKey = QKey.Key.None;
				}
			}
		}

		void OnGUI () {
			GUI.skin = HighLogic.Skin;
			if (Draw) {
				GUILayout.BeginArea (new Rect ((Screen.width - 96) / 2, (Screen.height - 96) / 2, 96, 96), Icon_Texture);
				GUILayout.EndArea ();
			}
			if (!WindowSettings) {
				return;
			}
			if (QKey.SetKey != QKey.Key.None) {
				RectSetKey = GUILayout.Window (1545156, RectSetKey, QKey.DrawSetKey, string.Format ("{0} {1}", QLang.translate ("Set Key:"), QKey.GetText (QKey.SetKey)), GUILayout.ExpandHeight (true));
				return;
			}
			RectSettings = GUILayout.Window (1545175, RectSettings, DrawSettings, MOD + " " + VERSION);
		}

		void DrawSettings (int id) {
			GUILayout.BeginVertical ();
			GUILayout.BeginHorizontal ();
			GUILayout.Box (QLang.translate ("Toolbars"), GUILayout.Height (30));
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			QSettings.Instance.StockToolBar = GUILayout.Toggle (QSettings.Instance.StockToolBar, QLang.translate ("Use the Stock Toolbar"), GUILayout.Width (400));
			GUILayout.EndHorizontal ();
			if (QBlizzyToolbar.isAvailable) {
				GUILayout.BeginHorizontal ();
				QSettings.Instance.BlizzyToolBar = GUILayout.Toggle (QSettings.Instance.BlizzyToolBar, QLang.translate ("Use the Blizzy Toolbar"), GUILayout.Width (400));
				GUILayout.EndHorizontal ();
			}
			GUILayout.BeginHorizontal ();
			GUILayout.Box (QLang.translate ("Keyboard shortcuts"), GUILayout.Height (30));
			GUILayout.EndHorizontal ();
			QKey.DrawConfigKey (QKey.Key.Mute);
			QLang.DrawLang ();
			GUILayout.FlexibleSpace ();
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button (QLang.translate ("Close"), GUILayout.Height (30))) {
				HideSettings ();
			}
			GUILayout.EndHorizontal ();
			GUILayout.EndVertical ();
		}
	}
}