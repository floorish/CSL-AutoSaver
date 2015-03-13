// Autosaver by @floorish
// Don't lose your precious citizens!
// This mod saves your city every 5 minutes (configurable)
//
// Windows - C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\AutoSaveConf.txt
// Mac - /Users/<username>/Library/Application Support/Steam/steamapps/common/Cities_Skylines/AutoSaveConf.txt
// Linux - /home/<username>/.steam/steam/SteamApps/common/Cities_Skylines/AutoSaveConf.txt
//
// If AutoSaveConf.txt does not contain a valid number ( 1...maxInt ) AutoSave will be off

using System.IO;
using ICities;
using UnityEngine;

namespace AutoSave {
	public class AutoSaveMod : IUserMod {
	
		public string Name {
			get { return "Auto Save"; }
		}
		
		public string Description {
			get { return "Automatically save your city every 5 minutes"; }
        }
        
	}
	
	public class AutoSaver : SerializableDataExtensionBase {
        
        static System.Timers.Timer t;
        static ISerializableData serializedData;
        
        // store link to serializedData in order to get access to SaveGame method
        public override void OnCreated(ISerializableData _serializedData) {
            base.OnCreated(_serializedData);
            serializedData = _serializedData;
        }
        
        // run when new game is loaded
        public override void OnLoadData() {
            
            string filename = "AutoSaveConf.txt";
            int minutes = 5; // default 5 minutes
            
            // try to read config file
            if (File.Exists(filename)) {
                
                using (StreamReader file = new StreamReader(filename)) {
                
                    string line = file.ReadLine();
                    if ( int.TryParse(line, out minutes) ) {
                        if (minutes <= 0) {
                            // don't use illegal numbers
                            return;
                        }
                    } else {
                        // don't use illegal numbers
                        return;
                    }
                    
                }
                
            } else {
                
                // write to config if it does not exists
                using (StreamWriter sw = new StreamWriter(File.Create(filename))) {
                   sw.Write(minutes.ToString());
                }
                
            }
            
            t = new System.Timers.Timer();
            t.AutoReset = false;
            t.Elapsed += new System.Timers.ElapsedEventHandler(SaveCity);
            t.Interval = minutes * 60 * 1000; // minutes * 60 seconds * 1000 milliseconds
            t.Start();
        }

        // executed after interval
        static void SaveCity(object sender, System.Timers.ElapsedEventArgs e){
            serializedData.SaveGame("_Autosave");
            t.Start();
        }
        
        
	}
}