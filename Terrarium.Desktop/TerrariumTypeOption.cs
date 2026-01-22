namespace Terrarium.Desktop
{
    using System.Windows.Media;

    public class TerrariumTypeOption
    {
        public string Title { get; set; } = string.Empty;

        public string Subtitle { get; set; } = string.Empty;

        public FontFamily TitleFont { get; set; } = new FontFamily("Segoe UI");

        public double TitleFontSize { get; set; } = 16;

        public double SubtitleFontSize { get; set; } = 12;

        public Brush ThemeColor { get; set; } = Brushes.Black;

        public string IconGeometry { get; set; } = string.Empty; // Path Data

        public string Id { get; set; } = string.Empty; // To map back to internal IDs
    }
}
