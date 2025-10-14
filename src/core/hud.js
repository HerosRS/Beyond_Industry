export class HUD {
  constructor(game) {
    this.game = game;
    this.el = {
      wood: document.getElementById('wood'),
      stone: document.getElementById('stone'),
      food: document.getElementById('food'),
      pop: document.getElementById('pop'),
      heat: document.getElementById('heat'),
      day: document.getElementById('day'),
      season: document.getElementById('season'),
      buttons: [...document.querySelectorAll('.btn')]
    };
    this.el.buttons.forEach(b => {
      b.addEventListener('click', () => this.activate(b.dataset.tool));
    });
    this.update();
  }

  bindTools(onSelect) {
    this.onSelect = onSelect;
  }

  activate(tool) {
    this.el.buttons.forEach(b => b.classList.toggle('active', b.dataset.tool === tool));
    if (this.onSelect) this.onSelect(tool);
  }

  update() {
    const r = this.game.resources.values;
    this.el.wood.textContent = Math.floor(r.wood);
    this.el.stone.textContent = Math.floor(r.stone);
    this.el.food.textContent = Math.floor(r.food);
    this.el.pop.textContent  = r.pop;
    this.el.heat.textContent = r.heat;
    this.el.day.textContent = this.game.time.day;
    this.el.season.textContent = this.game.time.season();
  }
}
