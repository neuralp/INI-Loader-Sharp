using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Crestron.SimplSharp;      // For Basic SIMPL# Classes
using Crestron.SimplSharp.CrestronIO;

namespace A2
{
    /// <summary>
    /// Handles the loading and saving and parsing of the INI file from the file system
    /// </summary>
    public static class INILoader
    {
        static string _fileName;    // The filename of the configuration file
        static string _saveFile;    // The temporary file used to save the configuration file
        /// <summary>
        /// SIMPL+ can only execute the default constructor. If you have variables that require initialization, please
        /// use an Initialize method
        /// </summary>
        static INILoader()
        {
        }

        /// <summary>
        /// Used the set the filename that is loaded.
        /// </summary>
        /// <param name="fileName">Contains the full path to the file for loading.</param>
        public static void setFilename(string fileName)
        {
            _fileName = "\\NVRAM\\" + Crestron.SimplSharp.InitialParametersClass.ProgramIDTag + "\\" + fileName;
            _saveFile = "\\NVRAM\\" + Crestron.SimplSharp.InitialParametersClass.ProgramIDTag + "\\" + fileName + ".tmp";
//            _saveFile = "\\NVRAM\\" + Crestron.SimplSharp.InitialParametersClass.ProgramIDTag + "\\" + "out.conf";
        }

        /// <summary>
        /// execute to read the file, a filename must be set using setFilename(name) first
        /// </summary>
        public static void read()
        {
            A2.INIFile.clear();

            CrestronConsole.PrintLine("A2 : Configuration file read at {0}..", _fileName);

            if (File.Exists(_fileName))
            {
                try
                {
                    using (StreamReader read = File.OpenText(_fileName))
                    {
                        string s = "";  //read string from the file
                        Match match;    //regex match
                        string sectionTitle = "none";
                        Dictionary<string, string> section = new Dictionary<string,string>(); //dictionary of the current section

                        // The regex matches for INI headers and key value pairs
                        Regex sectionHeader = new Regex(@"^\[([0-9_ a-zA-Z]+)\]");
                        Regex keyValue = new Regex(@"\s*(.+?)\s*=\s*([^;\n]*)");

                        while ((s = read.ReadLine()) != null)
                        {
                            // run the key / value first, as statistically there is a lot more of thes in a file
                            match = keyValue.Match(s);
                            if (match.Success)
                            {
                                section.Add(match.Groups[1].Value, match.Groups[2].Value.Trim());
                                continue;
                            }
                            match = sectionHeader.Match(s);
                            if (match.Success)
                            {
                                A2.INIFile.addSection(sectionTitle, section);
                                section = new Dictionary<string,string>();
                                sectionTitle = match.Groups[1].Value.Trim();
                            }
                        }
                        // add the last section
                        A2.INIFile.addSection(sectionTitle, section);
                        read.Close();
                        read.Dispose();
                        CrestronConsole.PrintLine("A2 : Configuration file successfully loaded..");

                        /*foreach(var key in A2.INIFile.getINI().Keys)
                        {
                            CrestronConsole.PrintLine("A2 : Showing section : {0}", key);
                            foreach (KeyValuePair<string, string> item in A2.INIFile.getINI()[key])
                            {
                                CrestronConsole.PrintLine("A2 : {0} : {1}->{2}", key, item.Key, item.Value);
                            }
                        }*/

                        // call the fileLoaded event from the storage class
                        A2.INIFile.fileLoaded();
                    }
                }
                catch (Exception e)
                {
                    CrestronConsole.PrintLine("A2 : Exception opening configuration file : {0}", e);
                }
            }
            else
            {
                CrestronConsole.PrintLine("A2 : Configuration file does not exist..");
            }
        }

        /// <summary>
        /// Saves the changed configuration data back into the file.  This will ONLY overwrie a key value
        /// that already exists.  You cannot use this to CREATE a file at this time.
        /// </summary>
        /// <returns>Success (1) or failure (0)</returns>
        public static ushort save()
        {

            CrestronConsole.PrintLine("A2 : Saving modified configuration at {0}..", _fileName);

            try
            {
                using (StreamReader read = File.OpenText(_fileName))
                {
                    string s = "";  //read string from the file
                    Match match;    //regex match
                    string sectionTitle = "none";
                    StreamWriter write = new StreamWriter(_saveFile, false);

                    // The regex matches for INI headers and key value pairs and replacement
                    Regex sectionHeader = new Regex(@"^\[([0-9_ a-zA-Z]+)\]");
                    Regex keyValue = new Regex(@"\s*(.+?)\s*=\s*([^;\n]*)");
                    Regex valueReplace = new Regex(@"(= *)([^;\n]*)");

                    // Read from the file and fall through (using continue) based on the following:
                    //   New section:  Take note of the section and write it to the output
                    //   Key not changed:  Write this line directly to the output
                    //   Changed key:  Replace ONLY they keys value section, keep spacing and comments in tact as much as possible
                    //   Other lines:  Just write them to the output
                    while ((s = read.ReadLine()) != null)
                    {
                        match = sectionHeader.Match(s);
                        if (match.Success)
                        {
                            sectionTitle = match.Groups[1].Value;
                            write.WriteLine(s);
                            continue;
                        }
                        if (!A2.INIFile.getChanges().ContainsKey(sectionTitle))
                        {
                            write.WriteLine(s);
                            continue;
                        }
                        match = keyValue.Match(s);
                        if (match.Success)
                        {
                            string currentKey = match.Groups[1].Value;
                            string currentValue = match.Groups[2].Value;
                            string newValue = A2.INIFile.getChange(sectionTitle, currentKey);
                            if (newValue != null)
                            {
                                match = valueReplace.Match(s);
                                /*foreach (Group g in match.Groups)
                                {
                                    CrestronConsole.PrintLine("Group {0} found: '{1}'", g.Index, g.Value);
                                }
                                CrestronConsole.PrintLine("Current length: {0}", currentValue.Length);*/
                                string newLine = valueReplace.Replace(s, match.Groups[1] + newValue.PadRight(currentValue.Length, ' '));
                                write.WriteLine(newLine);
                            }
                            else
                                write.WriteLine(s);
                            continue;
                        }
                        write.WriteLine(s);
                    }
                    read.Close();
                    read.Dispose();
                    write.Close();
                    write.Dispose();
                    // Delete the old configuration file and move the new one into its place
                    File.Delete(_fileName);
                    File.Move(_saveFile, _fileName);
                    CrestronConsole.PrintLine("A2 : Configuration file written..");
                    return 1;
                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("A2 : Exception saving configuration file : {0}", e.ToString());
                return 0;
            }
        }
    }
}