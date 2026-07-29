using System;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace ZaettaCaptureNative
{
    internal static class LastSelectionStore
    {
        private static readonly string FilePath = Path.Combine(Paths.BaseDir, "last-selection.txt");

        public static bool TryLoad(out Rectangle selection)
        {
            selection = Rectangle.Empty;
            try
            {
                if (!File.Exists(FilePath))
                    return false;

                string[] parts = File.ReadAllText(FilePath).Split(new[] { ',' }, StringSplitOptions.None);
                if (parts.Length != 4)
                    return false;

                int x = int.Parse(parts[0], CultureInfo.InvariantCulture);
                int y = int.Parse(parts[1], CultureInfo.InvariantCulture);
                int width = int.Parse(parts[2], CultureInfo.InvariantCulture);
                int height = int.Parse(parts[3], CultureInfo.InvariantCulture);
                if (width < 10 || height < 10)
                    return false;

                selection = new Rectangle(x, y, width, height);
                return true;
            }
            catch
            {
                selection = Rectangle.Empty;
                return false;
            }
        }

        public static void Save(Rectangle selection)
        {
            if (selection.Width < 10 || selection.Height < 10)
                return;

            Directory.CreateDirectory(Paths.BaseDir);
            string value = string.Join(
                ",",
                selection.X.ToString(CultureInfo.InvariantCulture),
                selection.Y.ToString(CultureInfo.InvariantCulture),
                selection.Width.ToString(CultureInfo.InvariantCulture),
                selection.Height.ToString(CultureInfo.InvariantCulture)
            );
            File.WriteAllText(FilePath, value);
        }
    }
}
