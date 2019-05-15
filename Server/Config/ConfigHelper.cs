using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace Server.Config
{
    public class ConfigHelper<T>
    {
        private static readonly ConfigHelper<T> _instance = new ConfigHelper<T>();

        private ConfigHelper() { }

        public static ConfigHelper<T> Instance()
        {
            return _instance;
        }

        public T GetServerConfig()
        {
            var serverConfig = LoadFromXmlFile<T>(AppDomain.CurrentDomain.BaseDirectory + "Config\\ServerConfig.xml");
            if (serverConfig == null)
                throw new ArgumentNullException();

            return serverConfig;
        }

        #region XML

        private static bool SaveToXmlFile<U>(string filePath, U instance)
        {
            return SaveToFile(filePath, sw =>
            {
                new XmlSerializer(typeof(U)).Serialize(sw, instance);
            });
        }

        private static U LoadFromXmlFile<U>(string filePath)
        {
            return LoadFromFile(filePath, sr =>
            {
                return (U)(new XmlSerializer(typeof(U))).Deserialize(sr);
            });
        }

        #endregion

        private static bool SaveToFile(string filePath, Action<TextWriter> serialize)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    using (var sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        serialize(sw);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static U LoadFromFile<U>(string filePath, Func<TextReader, U> deserialize)
        {
            try
            {
                if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
                    return default(U);

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        return deserialize(sr);
                    }
                }
            }
            catch (Exception ex)
            {
                return default(U);
            }
        }
    }
}
