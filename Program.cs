using Raylib_cs;

// Fenster erstellen
Raylib.InitWindow(800, 600, "Beyond Industry");
Raylib.SetTargetFPS(60);

// Game Loop - läuft 60x pro Sekunde
while (!Raylib.WindowShouldClose())  // Läuft bis X gedrückt wird
{
    // ===== UPDATE (Logik) =====
    
    // ===== DRAW (Zeichnen) =====
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.DarkGreen);  // Grüner Hintergrund
    Raylib.DrawText("Hello Factory!", 100, 100, 40, Color.White);
    Raylib.EndDrawing();
}

// Aufräumen
Raylib.CloseWindow();