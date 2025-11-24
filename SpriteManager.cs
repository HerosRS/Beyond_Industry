using Raylib_cs;
using System.Collections.Generic;

namespace BeyondIndustry.Utils
{
    public class SpriteManager
    {
        private static Dictionary<string, Texture2D> sprites = new Dictionary<string, Texture2D>();
        
        // Lade ein Sprite
        public static void LoadSprite(string name, string path)
        {
            Texture2D texture = Raylib.LoadTexture(path);
            sprites[name] = texture;
        }
        
        // Hole ein Sprite
        public static Texture2D GetSprite(string name)
        {
            if (sprites.ContainsKey(name))
            {
                return sprites[name];
            }
            
            // Fallback: Leere Textur
            return default(Texture2D);
        }
        
        // Prüfe ob Sprite existiert
        public static bool HasSprite(string name)
        {
            return sprites.ContainsKey(name);
        }
        
        // Entlade alle Sprites (beim Beenden)
        public static void UnloadAll()
        {
            foreach (var sprite in sprites.Values)
            {
                Raylib.UnloadTexture(sprite);
            }
            sprites.Clear();
        }
    }
}