namespace VaganteBot
{
    public class Format
    {
        public static string FormatText(string text, string style = "")
        {
            text = text.Replace("*", "");
            text = text.Replace("~", "");
            text = text.Replace("_", "");
            return style + text + style;
        }
    }
}
