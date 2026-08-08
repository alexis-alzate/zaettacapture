namespace ZaettaCaptureNative
{
    internal static class AppInfo
    {
        public const string Name = "Zaetta Capture";
        public const string Version = "1.0.27";
        public const string Publisher = "Victor Alexis Alzate Cortes";

        public static string AboutText
        {
            get
            {
                return Name + "\n\nVersion " + Version + "\n\nDesarrollador:\n" + Publisher;
            }
        }

        public static string AboutTitle
        {
            get { return "Acerca de " + Name; }
        }
    }
}
