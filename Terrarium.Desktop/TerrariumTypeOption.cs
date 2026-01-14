using System.Windows.Media;

namespace Terrarium.Desktop
{
    public class TerrariumTypeOption
    {
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public FontFamily TitleFont { get; set; } = new FontFamily("Segoe UI");
        public double TitleFontSize { get; set; } = 16;
        public double SubtitleFontSize { get; set; } = 12;
        public Brush ThemeColor { get; set; } = Brushes.Black;
        public string IconGeometry { get; set; } = ""; // Path Data
        public string Id { get; set; } = ""; // To map back to internal IDs
    }
}
