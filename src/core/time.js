export class TimeSim {
  constructor({ onDay, secondsPerDay = 1.25 } = {}) {
    this.onDay = onDay;
    this.secondsPerDay = secondsPerDay;
    this.acc = 0;
    this.day = 1;
    this._seasonIndex = 0;
    this._last = performance.now();
    this._seasons = ["Frühling", "Sommer", "Herbst", "Winter"];
  }

  update(now, onTickDay) {
    const dt = (now - this._last) / 1000;
    this._last = now;
    this.acc += dt;
    if (this.acc >= this.secondsPerDay) {
      this.acc = 0;
      if (this.onDay) this.onDay();
      else if (onTickDay) onTickDay();
    }
  }

  advance() {
    this.day += 1;
    if (this.day % 8 === 1) {
      this._seasonIndex = (this._seasonIndex + 1) % 4;
    }
  }

  season() { return this._seasons[this._seasonIndex]; }
}
