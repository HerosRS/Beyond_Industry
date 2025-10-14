export class Resources {
  constructor(initial) {
    this.values = { ...initial }; // wood, stone, food, pop, heat
  }
  canAfford(cost = {}) {
    for (const k of Object.keys(cost)) {
      if ((this.values[k] ?? 0) < cost[k]) return false;
    }
    return true;
  }
  pay(cost = {}) {
    for (const k of Object.keys(cost)) {
      this.values[k] = (this.values[k] ?? 0) - cost[k];
    }
  }
  add(delta = {}) {
    for (const k of Object.keys(delta)) {
      this.values[k] = (this.values[k] ?? 0) + delta[k];
    }
  }
}
