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

using System;
using System.IO;
using System.Timers;
using System.Collections.Generic;

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
			get {
				return "Automatically save your city every couple of minutes.";
			}
		}
	
	}

	public class AutoSaver : SerializableDataExtensionBase
	{

		private static string suffix = "_autosave-";
		private static int maxSaves = 3;

		private bool m_saving = false;
		private static Timer m_timer = null;
		private Package.Asset m_lastSave = null;

		/*
		 * Start the autosaver
		 */
		public override void OnLoadData() {
			int minutes = Config.GetInterval();

			if (minutes <= 0) {
				return;
			}

			m_timer = new Timer ();
			m_timer.AutoReset = false;
			m_timer.Elapsed += new ElapsedEventHandler((sender, e) => SaveCity(managers.serializableData));
			m_timer.Interval = minutes * 60 * 1000; // minutes * 60 secons * 1000 milliseconds
			m_timer.Start();
		}


		/*
		 * Called on close, makes sure latest autosave is visible
		 */
		public override void OnReleased() {

			if (m_timer != null)
			{
				m_timer.Stop();
				m_timer = null;
			}

			// make sure last save is enabled
			getNewSaveName ();
			if (m_lastSave != null) {
				m_lastSave.isEnabled = true;
				cleanLegacySave ();
			}

		}


		/*
		 * Removes old autosave package "_Autosave CityName"
		 */
		private void cleanLegacySave() {

			string cityName = Singleton<SimulationManager>.instance.m_metaData.m_CityName;

			IEnumerable<Package.Asset> assets = PackageManager.FilterAssets (new Package.AssetType[] {
				UserAssetType.SaveGameMetaData
			});

			Package p = null;

			foreach (Package.Asset asset in assets) {

				if (asset != null) {

					if ( asset.package.packageName == "_Autosave " + cityName) {

						p = asset.package;
						break;
					}

				}
			}

			if (p != null) {
				PackageManager.Remove (p);
			}

		}

		/*
		 * Creates a new save name
		 * stays within maxSave slots
		 */
		private string getNewSaveName() {

			string cityName = Singleton<SimulationManager>.instance.m_metaData.m_CityName;

			if (cityName == null) {
				cityName = "City";
			}

			string saveName = cityName + suffix;

			int saveNum = getLastSaveNum (saveName);

			saveNum = saveNum % maxSaves + 1;

			return saveName + saveNum;

		}

		/*
		 * Search all savegames for autosave packages
		 */
		private int getLastSaveNum( string saveName ) {

			int lastSaveNum = 0;
			DateTime lastSaveDate = new DateTime (0);
			Package.Asset lastSave = null;

			IEnumerable<Package.Asset> assets = PackageManager.FilterAssets (new Package.AssetType[] {
				UserAssetType.SaveGameMetaData
			});

			foreach (Package.Asset asset in assets) {

				if (asset != null) {

					string baseName = asset.package.packageName.Substring (0, asset.package.packageName.Length - 1);
					string counterName = asset.package.packageName.Substring (asset.package.packageName.Length - 1);

					int c = 0;
					bool parsed = int.TryParse (counterName, out c);

					// check if current city
					if ( parsed && baseName == saveName ) {

						// hide it
						asset.isEnabled = false;

						SaveGameMetaData saveGameMetaData = asset.Instantiate<SaveGameMetaData> ();

						// check if later than latest found
						if (saveGameMetaData.timeStamp.CompareTo (lastSaveDate) >= 0) {
							lastSaveNum = c;
							lastSaveDate = saveGameMetaData.timeStamp;
							lastSave = asset;
						}

					}


				}
			}

			m_lastSave = lastSave;

			return lastSaveNum;

		}

		/*
		 * Saves the city, executed after every interval
		 */
		public void SaveCity(ISerializableData serializableData) {

			if (m_saving) {
				Log.Message ("skipping, already saving");
				return;
			}

			m_saving = true;

			string saveName = getNewSaveName ();

			serializableData.SaveGame(saveName);

			m_timer.Start();
			m_saving = false;
		}

	}



	/*
	 * Read simple config file
	 */
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