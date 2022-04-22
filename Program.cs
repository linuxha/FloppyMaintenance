using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Xml;
using System.IO;

using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Reflection;

namespace FloppyMaintenance
{
    static class Program
    {
        static public string programKeyName = @"SOFTWARE\EvensonConsultingServices\FloppyMaintenance";
        static public RegistryKey programKey;

        static public string preferencesKeyName = string.Format(@"{0}\{1}", programKeyName, "Preferences");
        static public RegistryKey preferencesKey;

        public static string configFileName = "configuration.xml";
        public static string dataDir = ".\\";
        public static string commonAppDir = ".\\";
        public static string userAppDir = ".\\";

        public static string[] args;

        private static FileStream[] _floppyDriveStream = new FileStream[4];
        public static FileStream[] FloppyDriveStream
        {
            get { return Program._floppyDriveStream; }
            set { Program._floppyDriveStream = value; }
        }

        private static string[] driveImagePaths = new string[4];
        public static string[] DriveImagePaths
        {
            get { return Program.driveImagePaths; }
            set { Program.driveImagePaths = value; }
        }

        private static OSPlatform _platform;
        public static OSPlatform Platform { get => _platform; set => _platform = value; }

        public static Version version = new Version();

        public static void GetOSPlatform()
        {
            OSPlatform osPlatform = OSPlatform.Create("Other Platform");
            // Check if it's windows 
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            osPlatform = isWindows ? OSPlatform.Windows : osPlatform;
            // Check if it's osx 
            bool isOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            osPlatform = isOSX ? OSPlatform.OSX : osPlatform;
            // Check if it's Linux 
            bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            osPlatform = isLinux ? OSPlatform.Linux : osPlatform;
            Platform = osPlatform;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] _args)
        {
            args = _args;

            GetOSPlatform();

            configFileName = "configuration.xml";    // set default in case user has not made a preference on the command line

            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/EvensonConsultingServices/SWTPCmemulator/";
            //Directory.CreateDirectory(appDataFolder);
            //string defaultConfigFilename = appDataFolder + "defaultConfiguration.txt";
            //if (File.Exists(defaultConfigFilename))
            //{
            //    configFileName = File.ReadAllText(defaultConfigFilename).TrimEnd();
            //}

            commonAppDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).Replace("\\", "/") + "/EvensonConsultingServices/SWTPCmemulator/";
            userAppDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace("\\", "/") + "/EvensonConsultingServices/SWTPCmemulator/";

            System.Diagnostics.Debug.WriteLine(string.Format("Common App Directory: {0}", commonAppDir));
            System.Diagnostics.Debug.WriteLine(string.Format("User App Directory: {0}", userAppDir));

            // this logic gives precedence to the User AppDir over the Common AppDir. If neither exist - uses execution directory as dataDir.

            if (Directory.Exists(userAppDir))
            {
                dataDir = userAppDir;
            }
            //else if (Directory.Exists(commonAppDir))
            //{
            //    dataDir = commonAppDir;
            //}
            else
            {
                dataDir = appDataFolder;
            }

            System.Diagnostics.Debug.WriteLine(string.Format("Data Directory: {0}", dataDir));

            configFileName = Path.Combine(dataDir, "CONFIGFILES", configFileName).Replace(@"\", "/");

            if (!File.Exists(configFileName))
            {
                DirectoryInfo di = Directory.CreateDirectory(Path.Combine(dataDir, "CONFIGFILES"));

                string defaults = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <Global>
    <FileMaintenance EditorPath=""subl"" UseExternalEditor=""Y"" LogOS9FloppyWrites=""N"" os9FloppyWritesFile="""">
    <FloppyCreate Format=""FLEX"" Processor=""6809"" IncludeBootSector=""1"" Tracks35=""0"" Tracks40=""0"" Tracks80=""1"" UserDefined=""0"" DoubleDensity=""1"" SingleDensity=""0"" QuadDensity=""0"" DoubleSided=""0"" UserDefinedCylinders=""80"" UserDefinedSectors=""18"" ControllerType=""0"" />
      <FileExport>
        <ExpandTabs enabled=""1"" />
        <AddLinefeed enabled=""1"" />
      </FileExport>
      <BinaryFile>
        <CompactBinary enabled=""0"" />
      </BinaryFile>
      <FileImport>
        <StripLinefeed enabled=""1"" />
        <CompressSpaces enabled=""1"" />
        <ConvertLfOnly enabled=""1"" />
        <ConvertLfOnlyToCrLf enabled=""0"" />
        <ConvertLfOnlyToCr enabled=""1"" />
      </FileImport>
    </FileMaintenance>
  </Global>
</configuration>";

                // create a default config file if one does not already exist.

                using (StreamWriter cf = new StreamWriter(File.Open(configFileName, FileMode.Create, FileAccess.ReadWrite)))
                {
                    cf.WriteLine(defaults);
                }
            }

            version = Assembly.GetEntryAssembly().GetName().Version;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmFloppymaintDialog());
        }

        public static void SaveConfigurationAttribute(string xpath, string attribute, string value)
        {
            XmlReader reader = null;
            FileStream xmlDocStream = null;

            try
            {
                xmlDocStream = File.OpenRead(Program.configFileName);
                reader = XmlReader.Create(xmlDocStream);
            }
            catch
            {

            }

            XmlDocument newDoc = null;

            if (reader != null)
            {
                XmlDocument doc = new XmlDocument();
                if (doc != null)
                {
                    doc.Load(reader);

                    Program.SaveConfigurationAttribute(doc, xpath, attribute, value);

                    newDoc = (XmlDocument)doc.Clone();
                }
                reader.Close();
                reader.Dispose();

                xmlDocStream.Close();

                newDoc.Save(Program.configFileName);
            }
        }

        public static void SaveConfigurationAttribute(XmlDocument doc, string xpath, string attribute, string value)
        {
            XmlNode configurationNode = doc.SelectSingleNode("/configuration");
            XmlNode node = configurationNode.SelectSingleNode(xpath);
            if (node != null)
            {
                XmlAttributeCollection coll = node.Attributes;
                if (coll != null)
                {
                    XmlNode valueNode = coll.GetNamedItem(attribute);

                    if (valueNode != null)
                    {
                        if (value != valueNode.Value)
                            valueNode.Value = value;
                    }
                    else
                    {
                        XmlAttribute attr = doc.CreateAttribute(attribute);
                        attr.Value = value;
                        node.Attributes.Append(attr);
                    }
                }
            }
            else
            {
                // need to add this xpath node to the keyboard map

                string[] uriParts = xpath.Split('/');
                string name = uriParts[uriParts.Length - 1];

                XmlNode finalNode = configurationNode;
                XmlNode previousNode = finalNode;
                for (int i = 0; i < uriParts.Length - 1; i++)
                {
                    finalNode = finalNode.SelectSingleNode(uriParts[i]);
                    if (finalNode == null)
                    {
                        XmlNode newNode = doc.CreateNode(XmlNodeType.Element, uriParts[i], "");
                        previousNode.AppendChild(newNode);

                        finalNode = previousNode.SelectSingleNode(uriParts[i]);
                    }
                    previousNode = finalNode;
                }

                if (finalNode != null)
                {
                    XmlNode newNode = doc.CreateNode(XmlNodeType.Element, name, "");
                    XmlAttribute attr = doc.CreateAttribute(attribute);
                    attr.Value = value;

                    newNode.Attributes.Append(attr);
                    finalNode.AppendChild(newNode);
                }
            }
        }

        public static string GetConfigurationAttribute(string xpath, string attribute, string defaultvalue)
        {
            string value = defaultvalue;

            try
            {
                FileStream xmlDocStream = File.OpenRead(configFileName);
                XmlReader reader = XmlReader.Create(xmlDocStream);

                if (reader != null)
                {
                    XmlDocument doc = new XmlDocument();
                    if (doc != null)
                    {
                        doc.Load(reader);

                        XmlNode configurationNode = doc.SelectSingleNode("/configuration");
                        XmlNode node = configurationNode.SelectSingleNode(xpath);
                        if (node != null)
                        {
                            XmlAttributeCollection coll = node.Attributes;
                            if (coll != null)
                            {
                                XmlNode valueNode = coll.GetNamedItem(attribute);

                                if (valueNode != null)
                                    value = valueNode.Value;
                            }
                        }
                    }
                    reader.Close();
                }
                xmlDocStream.Close();
            }
            catch
            {

            }
            return value;
        }

        // Modified to allow numbers to be specified as eothe decimal or hex if preceeded with "0x" or "0X"
        public static int GetConfigurationAttribute(string xpath, string attribute, int defaultvalue)
        {
            int value = defaultvalue;

            FileStream xmlDocStream = File.OpenRead(configFileName);
            XmlReader reader = XmlReader.Create(xmlDocStream);

            if (reader != null)
            {
                XmlDocument doc = new XmlDocument();
                if (doc != null)
                {
                    doc.Load(reader);

                    XmlNode configurationNode = doc.SelectSingleNode("/configuration");
                    XmlNode node = configurationNode.SelectSingleNode(xpath);
                    if (node != null)
                    {
                        XmlAttributeCollection coll = node.Attributes;
                        if (coll != null)
                        {
                            XmlNode valueNode = coll.GetNamedItem(attribute);
                            if (valueNode != null)
                            {
                                string strvalue = valueNode.Value;
                                if (strvalue.StartsWith("0x") || strvalue.StartsWith("0X"))
                                {
                                    value = Convert.ToInt32(strvalue, 16);
                                }
                                else
                                    Int32.TryParse(strvalue, out value);
                            }
                        }
                    }
                }
                reader.Close();
            }
            xmlDocStream.Close();
            return value;
        }

        public static string GetConfigurationAttribute(string xpath, string attribute, string ordinal, string defaultvalue)
        {
            string value = defaultvalue;
            bool foundOrdinal = false;

            FileStream xmlDocStream = File.OpenRead(configFileName);
            XmlReader reader = XmlReader.Create(xmlDocStream);

            if (reader != null)
            {
                XmlDocument doc = new XmlDocument();
                if (doc != null)
                {
                    doc.Load(reader);

                    XmlNode configurationNode = doc.SelectSingleNode("/configuration");
                    XmlNode node = configurationNode.SelectSingleNode(xpath);
                    while (!foundOrdinal && node != null)
                    {
                        if (node != null)
                        {
                            XmlAttributeCollection coll = node.Attributes;
                            if (coll != null)
                            {
                                foreach (XmlAttribute a in coll)
                                {
                                    if (a.Name == "ID")
                                    {
                                        string index = a.Value;
                                        if (index == ordinal)
                                        {
                                            XmlNode valueNode = coll.GetNamedItem(attribute);

                                            if (valueNode != null)
                                            {
                                                value = valueNode.Value;
                                                foundOrdinal = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (!foundOrdinal)
                                node = node.NextSibling;
                        }
                    }
                }
                reader.Close();
            }
            xmlDocStream.Close();
            return value;
        }
        public static int GetConfigurationAttribute(string xpath, string attribute, string ordinal, int defaultvalue)
        {
            int value = defaultvalue;
            bool foundOrdinal = false;

            FileStream xmlDocStream = File.OpenRead(configFileName);
            XmlReader reader = XmlReader.Create(xmlDocStream);

            if (reader != null)
            {
                XmlDocument doc = new XmlDocument();
                if (doc != null)
                {
                    doc.Load(reader);

                    XmlNode configurationNode = doc.SelectSingleNode("/configuration");
                    XmlNode node = configurationNode.SelectSingleNode(xpath);
                    while (!foundOrdinal && node != null)
                    {
                        XmlAttributeCollection coll = node.Attributes;
                        if (coll != null)
                        {
                            foreach (XmlAttribute a in coll)
                            {
                                if (a.Name == "ID")
                                {
                                    string index = a.Value;
                                    if (index == ordinal)
                                    {
                                        XmlNode valueNode = coll.GetNamedItem(attribute);

                                        if (valueNode != null)
                                        {
                                            string strvalue = valueNode.Value;
                                            if (strvalue.StartsWith("0x") || strvalue.StartsWith("0X"))
                                            {
                                                value = Convert.ToInt32(strvalue, 16);
                                            }
                                            else
                                                Int32.TryParse(strvalue, out value);
                                            foundOrdinal = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (!foundOrdinal)
                            node = node.NextSibling;
                    }
                }
                reader.Close();
            }
            xmlDocStream.Close();
            return value;
        }
        public static int GetConfigurationAttributeHex(string xpath, string attribute, string ordinal, int defaultValue)
        {
            int value = defaultValue;

            try
            {
                string strValue = GetConfigurationAttribute(xpath, attribute, ordinal, defaultValue.ToString("X4"));
                value = Convert.ToUInt16(strValue, 16);
            }
            catch
            {
            }

            return value;
        }
    }
}
