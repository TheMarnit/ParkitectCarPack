using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiniJSON;

namespace Custom_Scenery
{
    public class Main : IMod, IModSettings
    {
		private StreamWriter sw;
        private GameObject _go;
		private Dictionary<string, object> car_settings;
		private Dictionary<string, object> car_strings;
		private Dictionary<string, string> settings_string = new Dictionary<string, string>();
		public Dictionary<string, bool> settings_bool = new Dictionary<string, bool>();
		private Type type;
		private bool isenabled = false;
		private string output;
		private int i;
		private int result;
		private bool hotkey_rebind = false;

		public void Init() {
			car_settings = Json.Deserialize(File.ReadAllText(Path + @"/settings.json")) as Dictionary<string, object>;
			car_strings = Json.Deserialize(File.ReadAllText(Path + @"/dictionary.json")) as Dictionary<string, object>;
			foreach (KeyValuePair<string, object> S in car_settings) {
				type = S.Value.GetType();
				if(type==typeof(bool)) {
					settings_bool[S.Key] = bool.Parse(S.Value.ToString());
				} else {
					settings_string[S.Key] = S.Value.ToString();
				}
			}
		}
		
        public void onEnabled()
        {
			isenabled = true;
			
            _go = new GameObject();

            _go.AddComponent<SceneryLoader>();

            _go.GetComponent<SceneryLoader>().Path = Path;
			
            _go.GetComponent<SceneryLoader>().Settings = car_settings;

            _go.GetComponent<SceneryLoader>().Identifier = Identifier;

            _go.GetComponent<SceneryLoader>().LoadScenery();
        }

        public void onDisabled()
        {
			isenabled = false;
			
            _go.GetComponent<SceneryLoader>().UnloadScenery();

            UnityEngine.Object.Destroy(_go);
        }
		
		public void onDrawSettingsUI()
		{
			GUIStyle labelStyle = new GUIStyle (GUI.skin.label); 
			labelStyle.margin=new RectOffset(15,0,10,0);
			labelStyle.alignment = TextAnchor.MiddleLeft;
			GUIStyle toggleStyle = new GUIStyle (GUI.skin.toggle); 
			toggleStyle.margin=new RectOffset(0,10,19,24);
			toggleStyle.alignment = TextAnchor.MiddleLeft;
			GUIStyle textfieldStyle = new GUIStyle (GUI.skin.textField); 
			textfieldStyle.margin=new RectOffset(0,10,10,0);
			textfieldStyle.alignment = TextAnchor.MiddleCenter;
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			foreach (KeyValuePair<string, object> S in car_settings) {
				GUILayout.Label (car_strings.ContainsKey(S.Key)?car_strings[S.Key].ToString():S.Key, labelStyle, GUILayout.Height(30));
			}
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();
			foreach (KeyValuePair<string, object> S in car_settings) {
				type = S.Value.GetType();
				if(type==typeof(bool)) {
					settings_bool[S.Key] = GUILayout.Toggle (settings_bool[S.Key],"",toggleStyle, GUILayout.Width(16), GUILayout.Height(16));
				} else {
					settings_string[S.Key] = GUILayout.TextField (settings_string[S.Key], textfieldStyle, GUILayout.Width(130), GUILayout.Height(30));
				}
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
		
		public void onSettingsOpened()
		{

		}
		
		public void onSettingsClosed()
		{
			writeSettingsFile();
		}
		
		public void writeSettingsFile()
		{
			sw = File.CreateText(Path+@"/settings.json");
			sw.WriteLine("{");
			i = 0;
			foreach (KeyValuePair<string, object> S in car_settings) {
				type = S.Value.GetType();
				i++;
				output = "	\""+S.Key+"\": ";
				if(type==typeof(bool)) {
					output += settings_bool[S.Key].ToString().ToLower();
				} else if(type==typeof(double)) {
					output += settings_string[S.Key];
					if(int.TryParse(settings_string[S.Key],out result)) {
						output += ".0";
					}
				} else if(type==typeof(string)) {
					output += "\""+settings_string[S.Key]+"\"";
				}
				if(i!=car_settings.Count){
					output += ",";
				}
				sw.WriteLine(output);
			}
			sw.WriteLine("}");
			sw.Flush();
			sw.Close();	
			Init();
			if(isenabled==true) {
				onDisabled();
				onEnabled();
			}
		}

        public string Name { get { return "Cars 'R' Us [v1.1.1]"; } }
        public string Description { get { return "Decorative car props."; } }
        private string path;
		public string Path {
			get {
				return path;
			}
			set {
				path = value;
				Init();
			}
		}
		
        public string Identifier { get; set; }
		
		public void WriteToFile(string text) {
			sw = File.AppendText(Path+@"/mod.log");
			sw.WriteLine(text);
			sw.Flush();
			sw.Close();
		}
    }
}
