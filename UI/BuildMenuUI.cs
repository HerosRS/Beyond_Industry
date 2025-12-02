using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;
using BeyondIndustry.Factory;

namespace BeyondIndustry.UI
{
    public class BuildMenuItem
    {
        public MachineDefinition Definition { get; set; }
        public Rectangle ButtonRect { get; set; }
        public RenderTexture2D PreviewTexture { get; set; }
        public bool IsHovered { get; set; }
        public int Index { get; set; }
        
        public BuildMenuItem(MachineDefinition def, int index)
        {
            Definition = def;
            Index = index;
            IsHovered = false;
        }
    }
    
    public class BuildMenuUI
    {
        private List<BuildMenuItem> menuItems;
        private PlacementSystem placementSystem;
        private Camera3D previewCamera;
        
        // UI-Layout
        private int menuHeight = 120;
        private int buttonSize = 100;
        private int buttonSpacing = 10;
        private int startX = 20;
        
        // Render Textures für 3D-Vorschauen
        private Dictionary<int, RenderTexture2D> previewTextures;
        
        public BuildMenuUI(List<MachineDefinition> definitions, PlacementSystem placement)
        {
            menuItems = new List<BuildMenuItem>();
            placementSystem = placement;
            previewTextures = new Dictionary<int, RenderTexture2D>();
            
            // Setup Preview Camera
            previewCamera = new Camera3D
            {
                Position = new Vector3(2, 2, 2),
                Target = new Vector3(0, 0.5f, 0),
                Up = new Vector3(0, 1, 0),
                FovY = 45.0f,
                Projection = CameraProjection.Perspective
            };
            
            // Erstelle Menu Items
            for (int i = 0; i < definitions.Count; i++)
            {
                var item = new BuildMenuItem(definitions[i], i);
                menuItems.Add(item);
                
                // Erstelle Render Texture für 3D-Preview
                var renderTex = Raylib.LoadRenderTexture(buttonSize, buttonSize);
                previewTextures[i] = renderTex;
            }
            
            UpdateLayout(Data.GlobalData.SCREEN_WIDTH, Data.GlobalData.SCREEN_HEIGHT);
        }
        
        // ===== LAYOUT AKTUALISIEREN BEI FENSTER-RESIZE =====
        public void UpdateLayout(int screenWidth, int screenHeight)
        {
            int startY = screenHeight - menuHeight;
            
            for (int i = 0; i < menuItems.Count; i++)
            {
                int x = startX + i * (buttonSize + buttonSpacing);
                int y = startY + 10;
                
                menuItems[i].ButtonRect = new Rectangle(x, y, buttonSize, buttonSize);
            }
        }
        
        // ===== RENDER 3D PREVIEWS =====
        private void RenderPreviews()
        {
            foreach (var item in menuItems)
            {
                if (!previewTextures.ContainsKey(item.Index)) continue;
                
                var renderTex = previewTextures[item.Index];
                
                Raylib.BeginTextureMode(renderTex);
                Raylib.ClearBackground(new Color(50, 50, 50, 255));
                
                Raylib.BeginMode3D(previewCamera);
                
                // Zeichne das 3D-Model
                Raylib.DrawModelEx(
                    item.Definition.Model,
                    Vector3.Zero,
                    new Vector3(0, 1, 0),
                    0f,
                    Vector3.One,
                    Color.White
                );
                
                // Beleuchtung simulieren
                Raylib.DrawSphere(new Vector3(2, 2, 2), 0.1f, Color.Yellow);
                
                Raylib.EndMode3D();
                Raylib.EndTextureMode();
            }
        }
        
        // ===== UPDATE =====
        public void Update()
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            
            // Hover-Detection
            foreach (var item in menuItems)
            {
                item.IsHovered = Raylib.CheckCollisionPointRec(mousePos, item.ButtonRect);
            }
            
            // Click-Detection
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                foreach (var item in menuItems)
                {
                    if (item.IsHovered)
                    {
                        placementSystem.SelectIndex(item.Index);
                        Console.WriteLine($"[BuildMenu] Selected: {item.Definition.Name}");
                        break;
                    }
                }
            }
        }
        
        // ===== DRAW =====
        public void Draw(int screenWidth, int screenHeight)
        {
            // Render 3D Previews (nur einmal pro Frame)
            RenderPreviews();
            
            int startY = screenHeight - menuHeight;
            
            // Hintergrund-Panel
            Raylib.DrawRectangle(0, startY, screenWidth, menuHeight, new Color(30, 30, 30, 230));
            Raylib.DrawLine(0, startY, screenWidth, startY, new Color(100, 100, 100, 255));
            
            // Menu Items zeichnen
            foreach (var item in menuItems)
            {
                DrawMenuItem(item);
            }
            
            // Info-Text
            var selectedItem = menuItems[placementSystem.SelectedMachineIndex];
            string infoText = $"Selected: {selectedItem.Definition.Name}";
            
            if (selectedItem.Definition.MachineType == "ConveyorBelt")
            {
                infoText += $" | Press R to rotate [{placementSystem.BeltDirectionNames[placementSystem.BeltRotation]}]";
            }
            
            int textX = startX;
            int textY = startY + menuHeight - 25;
            //Raylib.DrawText(infoText, textX, textY, 16, Color.White);
        }
        
        // ===== EINZELNES MENU ITEM ZEICHNEN =====
        private void DrawMenuItem(BuildMenuItem item)
        {
            Rectangle rect = item.ButtonRect;
            
            // Button-Hintergrund
            Color bgColor = item.IsHovered ? 
                new Color(70, 70, 70, 255) : 
                new Color(50, 50, 50, 255);
            
            // Highlight wenn ausgewählt
            if (item.Index == placementSystem.SelectedMachineIndex)
            {
                bgColor = new Color(80, 120, 200, 255);
            }
            
            Raylib.DrawRectangleRec(rect, bgColor);
            
            // 3D-Preview Texture
            if (previewTextures.ContainsKey(item.Index))
            {
                var renderTex = previewTextures[item.Index];
                
                Rectangle sourceRect = new Rectangle(
                    0, 0, 
                    renderTex.Texture.Width, 
                    -renderTex.Texture.Height  // Flip vertikal
                );
                
                Rectangle destRect = new Rectangle(
                    rect.X + 2, 
                    rect.Y + 2, 
                    rect.Width - 4, 
                    rect.Height - 25
                );
                
                Raylib.DrawTexturePro(
                    renderTex.Texture,
                    sourceRect,
                    destRect,
                    Vector2.Zero,
                    0f,
                    Color.White
                );
            }
            
            // Name-Label
            string label = GetShortName(item.Definition.Name);
            int labelWidth = Raylib.MeasureText(label, 10);
            int labelX = (int)(rect.X + rect.Width / 2 - labelWidth / 2);
            int labelY = (int)(rect.Y + rect.Height - 18);
            
            Raylib.DrawText(label, labelX, labelY, 10, Color.White);
            
            // Rahmen
            Color borderColor = item.IsHovered ? Color.Yellow : new Color(100, 100, 100, 255);
            if (item.Index == placementSystem.SelectedMachineIndex)
            {
                borderColor = Color.White;
            }
            
            Raylib.DrawRectangleLinesEx(rect, 2f, borderColor);
            
            // Hotkey-Anzeige (1-9)
            if (item.Index < 9)
            {
                string hotkey = (item.Index + 1).ToString();
                Raylib.DrawText(hotkey, (int)rect.X + 5, (int)rect.Y + 5, 12, new Color(200, 200, 200, 150));
            }
            
            // Hover-Tooltip
            if (item.IsHovered)
            {
                DrawTooltip(item);
            }
        }
        
        // ===== TOOLTIP =====
        private void DrawTooltip(BuildMenuItem item)
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            
            string[] lines = new string[]
            {
                item.Definition.Name,
                $"Type: {item.Definition.MachineType}",
                $"Power: {item.Definition.PowerConsumption}W",
            };
            
            int tooltipWidth = 250;
            int lineHeight = 20;
            int tooltipHeight = lines.Length * lineHeight + 10;
            
            int tooltipX = (int)mousePos.X + 15;
            int tooltipY = (int)mousePos.Y - tooltipHeight - 10;
            
            // Verhindere dass Tooltip aus dem Bildschirm geht
            if (tooltipY < 0) tooltipY = (int)mousePos.Y + 15;
            
            // Hintergrund
            Raylib.DrawRectangle(tooltipX, tooltipY, tooltipWidth, tooltipHeight, new Color(20, 20, 20, 240));
            Raylib.DrawRectangleLines(tooltipX, tooltipY, tooltipWidth, tooltipHeight, Color.White);
            
            // Text
            int y = tooltipY + 5;
            foreach (string line in lines)
            {
                Raylib.DrawText(line, tooltipX + 5, y, 14, Color.White);
                y += lineHeight;
            }
        }
        
        // ===== HELPER: KÜRZE NAMEN =====
        private string GetShortName(string name)
        {
            // Entferne Klammern-Inhalt für kompakte Anzeige
            int parenIndex = name.IndexOf('(');
            if (parenIndex > 0)
            {
                string main = name.Substring(0, parenIndex).Trim();
                string sub = name.Substring(parenIndex).Trim();
                
                // Kürze wenn zu lang
                if (main.Length > 12)
                    return main.Substring(0, 9) + "...";
                
                return main;
            }
            
            if (name.Length > 12)
                return name.Substring(0, 9) + "...";
            
            return name;
        }
        
        // ===== CLEANUP =====
        public void Unload()
        {
            foreach (var kvp in previewTextures)
            {
                Raylib.UnloadRenderTexture(kvp.Value);
            }
            previewTextures.Clear();
        }
        
        // ===== PRÜFE OB MAUS ÜBER UI IST =====
        public bool IsMouseOverUI()
        {
            foreach (var item in menuItems)
            {
                if (item.IsHovered)
                    return true;
            }
            return false;
        }
    }
}