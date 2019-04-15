using System;
using System.Configuration;

namespace RedisDemo
{
    public class Util
    {
        public static string ReadConnectionString(string keyName, string defaultValue)
        {
            string value = String.Empty;
            try
            {
                value = ConfigurationManager.ConnectionStrings[keyName].ConnectionString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(ReadConnectionString)} Exception : {ex.Message}");
                value = String.Empty;
            }
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        public static string ReadAppSetting(string keyName, string defaultValue)
        {
            string value = String.Empty;
            try
            {
                value = ConfigurationManager.AppSettings[keyName];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(ReadAppSetting)} Exception : {ex.Message}");
                value = String.Empty;
            }
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        public static int ReadAppSetting(string keyName, int defaultValue)
        {
            int value = defaultValue;
            try
            {
                if (!Int32.TryParse(ConfigurationManager.AppSettings[keyName], out value))
                {
                    value = defaultValue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(ReadAppSetting)} Exception : {ex.Message}");
                value = defaultValue;
            }
            return value;
        }
    }
}
