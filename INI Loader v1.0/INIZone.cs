using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;

namespace A2
{
    /// <summary>
    /// Signed integer return delegate to be called in S+
    /// </summary>
    /// <param name="isValid">Return 1 if data is valid, 0 of not</param>
    /// <param name="data">Signed return integer</param>
    public delegate void signedIntDelegate(dataValidity isValid, short data);
    /// <summary>
    /// Unsigned integer return delegate to be called in S+
    /// </summary>
    /// <param name="isValid">Return 1 if data is valid, 0 of not</param>
    /// <param name="data">Unsigned return integer</param>
    public delegate void unsignedIntDelegate(dataValidity isValid, ushort data);
    /// <summary>
    /// Boolean return delegate to be called in S+
    /// </summary>
    /// <param name="isValid">Return 1 if data is valid, 0 of not</param>
    /// <param name="data">Bool return value</param>
    public delegate void boolDelegate(dataValidity isValid, ushort data);
    /// <summary>
    /// String return delegate to be called in S+
    /// </summary>
    /// <param name="isValid">Return 1 if data is valid, 0 of not</param>
    /// <param name="data">String return value</param>
    public delegate void stringDelegate(dataValidity isValid, SimplSharpString data);

    /// <summary>
    /// Enumeration for valid data
    /// </summary>
    public enum dataValidity : ushort { invalidData = 0, validData };

    public class INIZone
    {
        /// <summary>
        /// The section of the INI file this zone is looking for
        /// </summary>
        string section { get; set; }

        /// <summary>
        /// Actual delegates for this zone
        /// </summary>
        public signedIntDelegate signedIntD { get; set; }
        /// <summary>
        /// Actual delegates for this zone
        /// </summary>
        public unsignedIntDelegate unsignedIntD { get; set; }
        /// <summary>
        /// Actual delegates for this zone
        /// </summary>
        public boolDelegate boolD { get; set; }
        /// <summary>
        /// Actual delegates for this zone
        /// </summary>
        public stringDelegate stringD { get; set; }

        /// <summary>
        /// Event to populate outputs in S+ when file loaded
        /// </summary>
        public event EventHandler populateOutputs = null;

        /// <summary>
        /// Default constructor which subscribes to the fileLoaded event
        /// </summary>
        public INIZone()
        {
            //subscribe to the fileLoadedEvent in the INIFile class;  When fired, the INI file is done being parsed
            A2.INIFile.fileLoadedEvent += new A2.INIFile.FileLoadedHandler(initialize);
            //default section
            section = "none";
        }
        /// <summary>
        /// This is the method that fires when the fileLoadedEvent() is fired in the INIFile class
        /// </summary>
        /// <param name="e">Arguments, not used currently</param>
        private void initialize(EventArgs e)
        {
            if (populateOutputs != null)
                populateOutputs(this, new EventArgs());
        }
        /// <summary>
        /// Sets the section of the INI file that this zone listens to
        /// </summary>
        /// <param name="section">The section name</param>
        public void setSection(string _section)
        {
            if (section != "")
                section = _section;
        }
        
        /// <summary>
        /// Checks for a signed integer
        /// </summary>
        /// <param name="_key">The key to check</param>
        /// <returns>true if signed</returns>
        public bool isSigned(string _key)
        {
            short returnNum;
            string keyData;
            
            try
            {
                keyData = A2.INIFile.getKey(section, _key);
                returnNum = Int16.Parse(keyData);
                if (returnNum != Math.Abs(returnNum))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;            
        }

        /// <summary>
        /// Chooses which function to call if an integer is sgined or unsigned
        /// </summary>
        /// <param name="_key">The key to send</param>
        public void sendInt(string _key)
        {
            if (isSigned(_key))
                sendSignedInt(_key);
            else
                sendUnsignedInt(_key);
        }

        /// <summary>
        /// Send a signed int to S+
        /// </summary>
        /// <param name="_key">The key to send</param>
        private void sendSignedInt(string _key)
        {
            short returnNum;
            string keyData = null;

            try
            {
                keyData = A2.INIFile.getKey(section, _key);
                returnNum = Int16.Parse(keyData);
                if (signedIntD != null)
                    signedIntD(dataValidity.validData, returnNum);
            }
            catch (System.OverflowException)
            {
                CrestronConsole.PrintLine("A2 : INIZone : sendSignedInt -> Overflow exception : {0}->{1} : {2}", section, _key, keyData);
            }
            catch (System.FormatException)
            {
                CrestronConsole.PrintLine("A2 : INIZone : sendSignedInt -> Format exception : {0}->{1} : {2}", section, _key, keyData);
            }
            catch (System.ArgumentNullException)
            {
                CrestronConsole.PrintLine("A2 : INIZone : sendSignedInt -> Argument null exception : {0}->{1} : <null>", section, _key);
            }
            catch (A2.INIKeyException)
            {
            }
        }

        /// <summary>
        /// Send an unsigned int to S+
        /// </summary>
        /// <param name="_key">The key to send</param>
        private void sendUnsignedInt(string _key)
        {
            ushort returnNum;
            string keyData = null;

            try
            {
                keyData = A2.INIFile.getKey(section, _key);

                //matching time, example 30s for 30 seconds (at the beginning of the line)
                Match match = Regex.Match(keyData, @"^(\d+)s$");
                if (match.Success)
                {
                    returnNum = UInt16.Parse(match.Groups[1].Value);
                    returnNum *= 100;
                }
                else
                    returnNum = UInt16.Parse(keyData);
                if (unsignedIntD != null)
                    unsignedIntD(dataValidity.validData, returnNum);
            }
            catch (System.OverflowException)
            {
                CrestronConsole.PrintLine("A2 : INIZone : sendUnsignedInt -> Overflow exception : {0}->{1} : {2}", section, _key, keyData);
            }
            catch (System.FormatException)
            {
                CrestronConsole.PrintLine("A2 : INIZone : sendUnsignedInt -> Format exception : {0}->{1} : {2}", section, _key, keyData);
            }
            catch (System.ArgumentNullException)
            {
                CrestronConsole.PrintLine("A2 : INIZone : sendUnsignedInt -> Argument null exception : {0}->{1} : <null>", section, _key);
            }
            catch (A2.INIKeyException)
            {
            }
        }

        /// <summary>
        /// Send a digital to S+
        /// </summary>
        /// <param name="_key">The key to send</param>
        public void sendBool(string _key)
        {
            ushort returnVar;
            string keyData = null;

            try
            {
                keyData = A2.INIFile.getKey(section, _key);
                returnVar = UInt16.Parse(keyData);
                if (returnVar > 0 && boolD != null)
                    boolD(dataValidity.validData, 1);
                else if (boolD != null)
                    boolD(dataValidity.validData, 0);
            }
            catch (System.OverflowException)
            {
                CrestronConsole.PrintLine("A2 : INIZone : sendBool -> Overflow exception : {0}->{1} : {2}", section, _key, keyData);
            }
            catch (System.FormatException)
            {
                if (keyData.ToLower() == "true" && boolD != null)
                    boolD(dataValidity.validData, 1);
                else if (keyData.ToLower() == "false" && boolD != null)
                    boolD(dataValidity.validData, 0);
                else
                    CrestronConsole.PrintLine("A2 : INIZone : sendBool -> Format exception : {0}->{1} : {2}", section, _key, keyData);
            }
            catch (System.ArgumentNullException)
            {
                CrestronConsole.PrintLine("A2 : INIZone : sendBool -> Argument null exception : {0}->{1} : <null>", section, _key);
            }
            catch (A2.INIKeyException)
            {
            }
        }

        /// <summary>
        /// Send a string int to S+
        /// </summary>
        /// <param name="_key">The key to send</param>
        public void sendString(string _key)
        {
            try
            {
                string retVal = A2.INIFile.getKey(section, _key);
                if (stringD != null)
                    stringD(dataValidity.validData, retVal);
            }
            catch (A2.INIKeyException)
            {
            }            
        }

        /// <summary>
        /// A change has been made, add it to the changed dictionary
        /// </summary>
        /// <param name="_key">The key to store</param>
        /// <param name="_data">The data to store</param>
        public void addChangeString(string _key, string _data)
        {
            A2.INIFile.addChage(section, _key, _data);
        }

        /// <summary>
        /// A change has been made, add it to the changed dictionary
        /// </summary>
        /// <param name="_key">The key to store</param>
        /// <param name="_data">The data to store</param>
        public void addChangeInt(string _key, uint _data)
        {
            A2.INIFile.addChage(section, _key, _data.ToString());
            //CrestronConsole.PrintLine("..adding change '{0}->{1}' = {2}", section, _key, _data.ToString());
        }

    }
}