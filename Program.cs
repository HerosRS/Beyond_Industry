
using BeyondIndustry.Core;

// Erstelle und starte das Spiel
var game = new Game(
    gridWidth: 20,
    gridHeight: 15,
    cellSize: 32
);

game.Initialize();
game.Run();