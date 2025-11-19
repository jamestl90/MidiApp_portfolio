using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Org.Json;
using TestApp.Midi;

namespace TestApp
{
    public static class GlobalSettings
    {
        public static ISharedPreferences SharedPrefs = null;
        private static Activity m_activity;

        public static bool IsFirstRun { get; private set; }

        public static void DoOnFirstRun(Action action)
        {            
            if (SharedPrefs.GetBoolean("first_run", true))
            {
                m_activity.RunOnUiThread(() =>
                {
                    action();

                    SharedPrefs.Edit().PutBoolean("first_run", false).Apply();
                    IsFirstRun = false;
                });                
            }
        }

        public static void Init(Activity act)
        {
            m_activity = act;
            SharedPrefs = m_activity.GetSharedPreferences("BEAT_REMOTE_SHARED_PREFS", FileCreationMode.Private);
        }

        public static bool ShowFirstTimeTutorial()
        {
            return SharedPrefs.GetBoolean("show_tutorial", true);
        }

        public static void ShowFirstTimeTutorials(bool val)
        {
            SharedPrefs.Edit().PutBoolean("show_tutorial", val).Apply();
        }

        //public static List<string> Tutorials = new List<string>
        //{
        //    "PlayMode",
        //    "PianoMode",
        //    "ChordMode",
        //    "ControlMode",
        //    "PlayScreenExpl"
        //    "ChordScreenExpl"
        //    "PianoScreenExpl"
        //    "ControlScreenExpl"
        //};

        public static bool ShowTutorial(string t)
        {
            return SharedPrefs.GetBoolean(t, true);
        }

        public static void ShowTutorials(string t, bool val)
        {
            SharedPrefs.Edit().PutBoolean(t, val).Apply();
        }

        public static List<MidiChordSerialize> GetDefaultChords(Activity act)
        {
            var savedString = SharedPrefs.GetString("DefaultChordArray", null);
            if (savedString != null)
            {
                return JsonConvert.DeserializeObject<List<MidiChordSerialize>>(savedString);
            }
            // return an empty list
            return new List<MidiChordSerialize>();
        }

        public static void ClearDefaultChords()
        {
            SharedPrefs.Edit().Remove("DefaultChordArray").Apply();
        }

        public static void SetDefaultChords(List<MidiChordSerialize> chords)
        {
            // remove existing
            SharedPrefs.Edit().Remove("DefaultChordArray").Apply();

            string output = JsonConvert.SerializeObject(chords, Formatting.Indented);

            var editor = SharedPrefs.Edit();
            editor.PutString("DefaultChordArray", output);
            editor.Apply();
        }

        public static List<string> GetSavedChords()
        {
            string arrayName = "SavedChordsArray";

            var items = SharedPrefs.GetString(arrayName, "[]");
            if (string.IsNullOrEmpty(items))
                return new List<string>();

            JSONArray jsonArray = new JSONArray(items);
            var returnItems = new List<string>();
            for (int i = 0; i < jsonArray.Length(); ++i)
            {
                returnItems.Add(jsonArray.GetString(i).Replace("SavedChordLayout_", ""));
            }

            return returnItems;
        }

        public static void RemoveFromSavedChords(string layoutToRemove)
        {
            string arrayName = "SavedChordsArray";

            var items = GetSavedChords();
            items.Remove(layoutToRemove);

            var jsonArr = ListToJSONArray(items);
            var editor = SharedPrefs.Edit();
            editor.PutString(arrayName, jsonArr.ToString());
            editor.Apply();
        }

        private static List<string> StringToList(string items)
        {
            JSONArray jsonArray = new JSONArray(items);
            var returnItems = new List<string>();
            for (int i = 0; i < jsonArray.Length(); ++i)
            {
                returnItems.Add(jsonArray.GetString(i));
            }
            return returnItems;;
        }

        private static JSONArray ListToJSONArray(List<string> items)
        {
            var jsonArr = new JSONArray();
            foreach (var item in items)
            {
                jsonArr.Put(item);
            }
            return jsonArr;
        }

        public static void AddToSavedChords(string newLayoutName)
        {
            string arrayName = "SavedChordsArray";
            var savedChords = GetSavedChords();

            //SharedPrefs.Edit().Remove("SavedChordLayout_" + newLayoutName).Apply();

            savedChords.Remove(newLayoutName.Replace("SavedChordLayout_", ""));
            savedChords.Add(newLayoutName);

            JSONArray chordLayoutArr = new JSONArray();
            foreach (var item in savedChords)
            {
                chordLayoutArr.Put(item);
            }
            var editor = SharedPrefs.Edit();
            editor.PutString(arrayName, chordLayoutArr.ToString());
            editor.Apply();
        }

        public static bool SaveChordLayout(string saveName, List<MidiChordSerialize> chords)
        {
            string finalName = "SavedChordLayout_" + saveName;

            // remove it if it exists
            SharedPrefs.Edit().Remove(finalName).Apply();

            string output = JsonConvert.SerializeObject(chords, Formatting.Indented);

            var editor = SharedPrefs.Edit();
            editor.PutString(finalName, output);
            editor.Apply();

            AddToSavedChords(finalName);

            return true;
        }

        public static List<MidiChordSerialize> GetChordLayout(string fileName)
        {
            string finalName = "SavedChordLayout_" + fileName;

            var savedString = SharedPrefs.GetString(finalName, null);
            if (savedString != null)
            {
                return JsonConvert.DeserializeObject<List<MidiChordSerialize>>(savedString);
            }
            return new List<MidiChordSerialize>();
        }

        public static void SetDefaultLaunch(List<LaunchButtonData> launchData)
        {
            SharedPrefs.Edit().Remove("DefaultLaunchArray").Apply();

            string output = JsonConvert.SerializeObject(launchData, Formatting.Indented);

            var editor = SharedPrefs.Edit();
            editor.PutString("DefaultLaunchArray", output);
            editor.Apply();
        }

        public static List<LaunchButtonData> GetDefaultLaunch()
        {
            var savedString = SharedPrefs.GetString("DefaultLaunchArray", null);
            if (savedString != null)
            {
                return JsonConvert.DeserializeObject<List<LaunchButtonData>>(savedString);
            }
            // return an empty list
            return new List<LaunchButtonData>();
        }

        public static List<string> GetSavedLaunch()
        {
            string arrayName = "SavedLaunchArray";

            var items = SharedPrefs.GetString(arrayName, "[]");
            if (string.IsNullOrEmpty(items))
                return new List<string>();

            JSONArray jsonArray = new JSONArray(items);
            var returnItems = new List<string>();
            for (int i = 0; i < jsonArray.Length(); ++i)
            {
                returnItems.Add(jsonArray.GetString(i).Replace("SavedLaunchLayout_", ""));
            }

            return returnItems;
        }

        public static List<LaunchButtonData> GetLaunchLayout(string fileName)
        {
            string finalName = "SavedLaunchLayout_" + fileName;

            var savedString = SharedPrefs.GetString(finalName, null);
            if (savedString != null)
            {
                return JsonConvert.DeserializeObject<List<LaunchButtonData>>(savedString);
            }
            return new List<LaunchButtonData>();
        }

        public static void AddToSavedLaunch(string saveName)
        {
            string arrayName = "SavedLaunchArray";
            var savedLaunchLayouts = GetSavedLaunch();

            //SharedPrefs.Edit().Remove("SavedChordLayout_" + newLayoutName).Apply();

            savedLaunchLayouts.Remove(saveName.Replace("SavedLaunchLayout_", ""));
            savedLaunchLayouts.Add(saveName);

            JSONArray launchLayoutArr = new JSONArray();
            foreach (var item in savedLaunchLayouts)
            {
                launchLayoutArr.Put(item);
            }
            var editor = SharedPrefs.Edit();
            editor.PutString(arrayName, launchLayoutArr.ToString());
            editor.Apply();
        }

        public static bool SaveLaunchLayout(string saveName, List<LaunchButtonData> launchData)
        {
            string finalName = "SavedLaunchLayout_" + saveName;

            // remove it if it exists
            SharedPrefs.Edit().Remove(finalName).Apply();

            string output = JsonConvert.SerializeObject(launchData, Formatting.Indented);

            var editor = SharedPrefs.Edit();
            editor.PutString(finalName, output);
            editor.Apply();

            AddToSavedLaunch(finalName);

            return true;
        }

        private static byte[] GetScaleFromPrefs(string key)
        {
            JSONArray jsonArray = new JSONArray(SharedPrefs.GetString(key, "[]"));

            byte[] scale = new byte[jsonArray.Length()];

            for (int i = 0; i < jsonArray.Length(); ++i)
            {
                scale[i] = (byte)jsonArray.GetInt(i);
            }
            return scale;
        }

        private static void SetScaleToPrefs(string key, byte[] scale)
        {
            SharedPrefs.Edit().Remove(key).Apply();

            var editor = SharedPrefs.Edit();

            JSONArray jsonScale = new JSONArray();
            foreach (var note in scale)
            {
                jsonScale.Put((int)note);
            }

            editor.PutString(key, jsonScale.ToString());
            editor.Apply();
        }

        private static void SetString(string key, string value)
        {
            SharedPrefs.Edit().Remove(key).Apply();

            var editor = SharedPrefs.Edit();
            editor.PutString(key, value);
            editor.Apply();
        }

        private static string GetString(string key)
        {
            return SharedPrefs.GetString(key, null);
        }

        private static void SetInt(string key, int value)
        {
            SharedPrefs.Edit().Remove(key).Apply();

            var editor = SharedPrefs.Edit();
            editor.PutInt(key, value);
            editor.Apply();
        }

        private static int GetInt(string key)
        {
            return SharedPrefs.GetInt(key, 0);
        }

        public static int GetDefaultPort()
        {
            return GetInt("DefaultPort");            
        }

        public static void SetDefaultPort(int port)
        {
            SetInt("DefaultPort", port);
        }

        public static string GetDefaultBonjourName()
        {
            return GetString("DefaultBonjourName");
        }

        public static void SetDefaultBonjourName(string bonjourName)
        {
            SetString("DefaultBonjourName", bonjourName);
        }

        public static string GetDefaultPadScreenScaleName()
        {
            return GetString("DefaultPadScreenScaleName");
        }

        public static void SetDefaultPadScreenScaleName(string name)
        {
            SetString("DefaultPadScreenScaleName", name);
        }

        public static string GetDefaultChordScreenScaleName()
        {
            return GetString("DefaultChordScreenScaleName");
        }

        public static void SetDefaultChordScreenScaleName(string name)
        {
            SetString("DefaultChordScreenScaleName", name);
        }

        public static byte[] GetDefaultPadScreenScale()
        {
            return GetScaleFromPrefs("DefaultPadScreenScale");
        }

        public static void SetDefaultPadScreenScale(byte[] scale)
        {
            SetScaleToPrefs("DefaultPadScreenScale", scale);
        }        

        public static void SetDefaultChordScreenScale(byte[] scale)
        {
            SetScaleToPrefs("DefaultChordScreenScale", scale);
        }

        public static byte[] GetDefaultChordScreenScale()
        {
            return GetScaleFromPrefs("DefaultChordScreenScale");
        }

        public static void SetDefaultKey(string key)
        {
            SetString("DefaultKey", key);
        }

        public static string GetDefaultKey()
        {
            return GetString("DefaultKey");
        }

        public static void SetDefaultVelocity(int velocity)
        {
            SetInt("DefaultVelocity", velocity);
        }

        public static int GetDefaultVelocity()
        {
            return GetInt("DefaultVelocity");
        }

        public static void SetDefaultOctave(int octave)
        {
            SetInt("DefaultOctave", octave);
        }

        public static int GetDefaultOctave()
        {
            return GetInt("DefaultOctave");
        }

        public static void SetDefaultChannel(int channel)
        {
            SetInt("DefaultChannel", channel);
        }

        public static int GetDefaultChannel()
        {
            return GetInt("DefaultChannel");
        }

        public static int GetDefaultShiftAmount()
        {
            return GetInt("DefaultShiftAmount");
        }

        public static void SetDefaultShiftAmount(int shiftAmount)
        {
            SetInt("DefaultShiftAmount", shiftAmount);
        }

        public static void ClearAllSettings()
        {
            throw new NotImplementedException();
            SharedPrefs.Edit().Remove("DefaultBonjourName").Apply();
            SharedPrefs.Edit().Remove("DefaultPort").Apply();
        }

        public static int GetDefaultPadScreenSize()
        {
            return GetInt("DefaultPadScreenSize");
        }

        public static void SetDefaultPadScreenSize(int size)
        {
            SetInt("DefaultPadScreenSize", size);
        }

        public static int GetDefaultLaunchScreenSize()
        {
            return GetInt("DefaultLaunchScreenSize");
        }

        public static void SetDefaultLaunchScreenSize(int size)
        {
            SetInt("DefaultLaunchScreenSize", size);
        }

        public static int GetDefaultChordScreenSize()
        {
            return GetInt("DefaultChordScreenSize");
        }

        public static void SetDefaultChordScreenSize(int size)
        {
            SetInt("DefaultChordScreenSize", size);
        }

        public static void SetShowNoteNames(bool val)
        {
            SetInt("ShowNoteNames", val ? 1 : 0);
        }

        public static bool GetShowNoteNames()
        {
            int val = GetInt("ShowNoteNames");
            return val > 0 ? true : false;
        }
    }
}