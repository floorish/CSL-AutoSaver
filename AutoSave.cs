// AutoSave by @floorish
//------------------------------------
// Don't lose your precious citizens!
// This mod saves your city every 5 minutes (configurable)
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
using ColossalFramework;
using ColossalFramework.Packaging;

namespace AutoSave {

	public class AutoSaveMod : IUserMod {

		public string Name {
			get { 
				return "Auto Save";
			}
		}

		public string Description {
			get { return "Automatically save your city every couple of minutes."; }
		}


	}
		
	// Official mod API autosaver
	public class AutoSaver : SerializableDataExtensionBase {

		private static Timer t = new Timer();


		// run when new game is loaded
		public override void OnLoadData() {

			int minutes = Config.GetInterval();

			if (minutes <= 0) {
				return;
			}

			t.AutoReset = false;
			t.Elapsed += new ElapsedEventHandler((sender, e) => SaveCity(sender, e, this.serializableDataManager));
			t.Interval = minutes * 60 * 1000; // minutes * 60 seconds * 1000 milliseconds
			t.Start();

		}

		// executed after every interval
		static void SaveCity(object sender, System.Timers.ElapsedEventArgs e, ISerializableData serializableData) {

			string cityName = Singleton<SimulationManager>.instance.m_metaData.m_CityName;

			if (cityName == null) {
				cityName = " City";
			} else {
				cityName = " " + cityName;
			}

			serializableData.SaveGame("_Autosave" + cityName);

			t.Start();
		}
	}


	// read config file
	public static class Config {

		public static int GetInterval() {

			const string filename = "AutoSaveConf.txt";
			int minutes = 5; // default 5 minutes

			// try to read config file
			if (File.Exists(filename)) {

				using (StreamReader file = new StreamReader (filename)) {

					string line = file.ReadLine ();
					bool parsed = int.TryParse (line, out minutes);

					if (!parsed || minutes <= 0) {

						// don't use illegal numbers
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