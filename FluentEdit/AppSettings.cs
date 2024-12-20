using FluentEdit.Helper;
using Windows.Storage;

namespace TextControlBox_DemoApp
{
    public class AppSettings
    {
        public static void SaveSettings(string value, object data)
        {
            if (data == null)
                return;

            //cancel if data is a type
            if (data.ToString() == data.GetType().Name)
                return;

            ApplicationData.Current.LocalSettings.Values[value] = data.ToString();
        }
        public static string GetSettings(string value, string defaultValue = "")
        {
            return ApplicationData.Current.LocalSettings.Values[value] as string ?? defaultValue;
        }
        public static int GetSettingsAsInt(string value, int defaultvalue = 0)
        {
            return ApplicationData.Current.LocalSettings.Values[value] is string data
                ? ConvertHelper.ToInt(data, defaultvalue) : defaultvalue;
        }
    }
}
