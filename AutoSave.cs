// AutoSave by @floorish
//------------------------------------
// Don't lose your precious citizens!
// This mod saves your city every 5 minutes (configurable)
// 
// In order to keep achievements, disable this mod (and all other mods) in the Content Manager panel
// The AutoSave feature will continue to work
//
//
// CONFIG FILE
//------------------------------------
//
// Set the number of minutes for each interval in the following file:
//
// Windows - C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\AutoSaveConf.txt
// Mac - /Users/<username>/Library/Application Support/Steam/steamapps/common/Cities_Skylines/AutoSaveConf.txt
// Linux - /home/<username>/.steam/steam/SteamApps/common/Cities_Skylines/AutoSaveConf.txt
//
// If AutoSaveConf.txt does not contain a valid number ( 1...maxInt ) AutoSave will be off
//
//
// LICENSE
//------------------------------------
// Attribution-NonCommercial-ShareAlike 4.0 International
// http://creativecommons.org/licenses/by-nc-sa/4.0/

using System.IO;
using System.Timers;
using ICities;
using UnityEngine;
using ColossalFramework;

namespace AutoSave {

	public class AutoSaveMod : IUserMod {

		public string Name {
			get { 
				// add the AutoSaver behaviour (mod does not need to be enabled)
				// http://www.reddit.com/r/CitiesSkylinesModding/comments/2yvmlg/guide_how_to_make_your_mod_not_disable/
				GameObject go = new GameObject("AutoSaver");
				go.AddComponent<AutoSaverBehaviour>();
				return "Auto Save";
			}
		}

		public string Description {
			get { return "Automatically save your city every couple of minutes. (Disable to keep achievements)"; }
		}


	}
		
	// Custom behaviour that 
	public class AutoSaverBehaviour : MonoBehaviour {

		static Timer t = new Timer();

		void OnLevelWasLoaded(int level) {
		

			// City loaded, start the timer
			if (level == 6) {

				int minutes = Config.GetInterval();

				if (minutes <= 0) {
					return;
				}

				Log.Message ("AutoSave every " + minutes.ToString() + " minutes");

				t.AutoReset = false;
				t.Elapsed += new ElapsedEventHandler(SaveCity);
				t.Interval = minutes * 60 * 1000; // minutes * 60 seconds * 1000 milliseconds
				t.Start();

			}

		}

		// executed after each interval
		void SaveCity(object sender, System.Timers.ElapsedEventArgs e){

			// access to save panel
			// http://www.reddit.com/r/CitiesSkylinesModding/comments/2ys5l8/le_source_code_for_custom_chirp/
			if (!Singleton<SavePanel>.exists) {
				Log.Message ("Savepanel does not exist");
			}

			SavePanel panel = Singleton<SavePanel>.instance;

			panel.SaveGame ("_Autosave");

			t.Start();

		}
			
		void Awake() {
			DontDestroyOnLoad(this);
		}
	}

	public static class Config {

		public static int GetInterval() {

			string filename = "AutoSaveConf.txt";
			int minutes = 5; // default 5 minutes

			// try to read config file
			if (File.Exists(filename)) {

				using (StreamReader file = new StreamReader (filename)) {

					string line = file.ReadLine ();
					bool parsed = int.TryParse (line, out minutes);

					if (!parsed || minutes <= 0) {

						// don't use illegal numbers
						Log.Message ("Invalid config!");
						return 0;

					}

				}

			} else {

				// write to config if it does not exists
				using (StreamWriter sw = new StreamWriter(File.Create(filename))) {
					sw.Write(minutes.ToString());
				}

			}

			return minutes;

		}

	}

	// Simple debug panel log wrapper
	// https://gist.github.com/AlexanderDzhoganov/1dc4911976ae14ff4602
	public static class Log {
		public static void Message(string s)
		{
			DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, s);
		}

		public static void Error(string s)
		{
			DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, s);
		}

		public static void Warning(string s)
		{
			DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Warning, s);
		}
	}

}