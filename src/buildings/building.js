// Abstrakte Basisklasse
// Implementiere in Subklassen: kind, cost, createMesh(), optional onPlaced(game), producePerDay(ctx)
export class Building {
  constructor() {
    this.kind = 'generic';
    this.cost = {}; // z.B. { wood: 8, stone: 0 }
  }
  createMesh() { throw new Error('createMesh() not implemented'); }
  onPlaced(game) { /* optional */ }
  producePerDay(ctx) { return null; } // { food, wood, stone, ... } oder null
}
