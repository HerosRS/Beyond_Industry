import { Game } from './core/game.js';
import { HUD } from './core/hud.js';
import { BuildingRegistry } from './buildings/registry.js';

const game = new Game();
const hud = new HUD(game);

game.onResourcesChanged = () => hud.update();
game.onTimeChanged = () => hud.update();

hud.bindTools((toolId) => {
  game.selectTool(toolId); // 'house' | 'farm' | 'lumber' | 'road' | 'cancel'
});

// Keyboard-Shortcuts
window.addEventListener('keydown', (e) => {
  if (e.repeat) return;
  if (e.key === '1') hud.activate('house');
  if (e.key === '2') hud.activate('farm');
  if (e.key === '3') hud.activate('lumber');
  if (e.key.toLowerCase() === 'r') hud.activate('road');
  if (e.key === 'Escape') hud.activate('cancel');
});

// Optional: Registry im Fenster (Debug)
window.Game = game;
window.BuildingRegistry = BuildingRegistry;
