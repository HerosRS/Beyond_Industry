using Raylib_cs;
using System.Numerics;

namespace BeyondIndustry.Data
{
    public class GlobalData
    {
        public static int SCREEN_WIDTH = 1600;  // Größer für bessere Sicht
        public static int SCREEN_HEIGHT = 900;
        public static int CELL_SIZE = 2;  // Für 3D: kleinere Zellen = mehr Details

        public static Camera3D camera = new Camera3D();
    }
    
    public class GlobalColor
    {
        public static Color BACKGROUND_COLOR = new Color(40, 40, 40, 255);
        public static Color FORGROUND_COLOR = new Color(56, 55, 52, 255);
        public static Color DEBUG_GREEN_COLOR = new Color(0, 255, 0, 255);
        public static Color TEXT_COLOR = new Color(211, 196, 165, 255);
    }
}