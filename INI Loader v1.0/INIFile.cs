using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace A2
{

    /// <summary>
    /// Used for exception handling while parsing
    /// </summary>
    public class INIKeyException : Exception
    {
        public INIKeyException(string message): base(message) { }
    }

    /// <summary>
    /// The storage class for the INI file information
    /// </summary>
    public static class INIFile
    {
        public static FileLoadedHandler fileLoadedEvent;
        public delegate void FileLoadedHandler(EventArgs _e);
        public static DirtyDelegate dirtyEvent { get; set; }
        public delegate void DirtyDelegate();

        // A dictionary of dictionaries is used for storage.  <section, <key, value> >
        // ALL VALUES ARE STORED AS STRINGS
        private static Dictionary<string, Dictionary<string, string>> INI = new Dictionary<string,Dictionary<string,string>>();
        // Holds only the changes made to any of the settings (via the module _fb signals) to be written to disk
        private static Dictionary<string, Dictionary<string, string>> changesToINI = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// SIMPL+ Can only run the default constructor and it must exist
        /// </summary>
        static INIFile()
        {
        }

        /// <summary>
        /// Adds a new section to the INI store
        /// </summary>
        /// <param name="section">Name of the section</param>
        /// <param name="_data">The data for the section as a string, string dictionary</param>
        public static void addSection(string section, Dictionary<string, string> _data)
        {
            INI.Add(section, _data);
        }

        /// <summary>
        /// Adds a change to the changed dictionary, replaces if exists.  Flags dirty if set
        /// </summary>
        /// <param name="section">The section of the change</param>
        /// <param name="key">The key of the change</param>
        /// <param name="data">The data changed</param>
        public static void addChage(string section, string key, string data)
        {
            if (changesToINI.ContainsKey(section))
            {
                changesToINI[section][key] = data;
            }
            else
            {
                Dictionary<string, string> newSection = new Dictionary<string,string>();
                newSection[key] = data;
                changesToINI[section] = newSection;
            }
            if (dirtyEvent != null)
            {
                dirtyEvent();
            }
        }

        /// <summary>
        /// Returns the data from the given section, key.  Throws INIKeyException with
        /// incorrect key or section
        /// </summary>
        /// <param name="_section">The section of the change</param>
        /// <param name="_key">The key of the change</param>
        /// <returns>String of the change</returns>
        public static string getKey(string _section, string _key)
        {
            string val;

            if (_section.Length == 0)
            {
                throw new INIKeyException("Section length is zero");
            }
            if (INI.ContainsKey(_section))
            {
                if (INI[_section].TryGetValue(_key, out val))
                    return val;
                else
                    throw new INIKeyException("Key does not exist : " + _key);
            }
            else
                throw new INIKeyException("Section does not exist : " + _section);
        }

        /// <summary>
        /// Returns the change (from _fb signals) if it exists, otherwise null.
        /// </summary>
        /// <param name="_section">The section of the change</param>
        /// <param name="_key">The key of the change</param>
        /// <returns>String of the change</returns>
        public static string getChange(string _section, string _key)
        {
            if (changesToINI.ContainsKey(_section))
            {
                if (changesToINI[_section].ContainsKey(_key))
                {
                    return changesToINI[_section][_key];
                }
            }
            return null;
        }

        /// <summary>
        /// Clears the data
        /// </summary>
        public static void clear()
        {
            INI = new Dictionary<string, Dictionary<string, string>>();
            changesToINI = new Dictionary<string, Dictionary<string, string>>();
        }

        /// <summary>
        /// Call the fileLoadedEvent if it has been set
        /// </summary>
        public static void fileLoaded()
        {
            if (fileLoadedEvent != null)
                fileLoadedEvent(null);
        }

        /// <summary>
        /// Getter for the INI data
        /// </summary>
        /// <returns>The INI storage data</returns>
        public static Dictionary<string, Dictionary<string, string>> getINI()
        {
            return INI;
        }

        /// <summary>
        /// Getter for the CHANGE data
        /// </summary>
        /// <returns>The change storage data</returns>
        public static Dictionary<string, Dictionary<string, string>> getChanges()
        {
            return changesToINI;
        }
    }
}