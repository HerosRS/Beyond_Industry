import * as THREE from 'three';
import { Building } from './building.js';

export class House extends Building {
  constructor() {
    super();
    this.kind = 'house';
    this.cost = { wood: 10 };
  }

  createMesh() {
    const geo = new THREE.BoxGeometry(1, 0.6, 1);
    const mat = new THREE.MeshStandardMaterial({ color: 0x937a5c, roughness: .9 });
    return new THREE.Mesh(geo, mat);
  }

  onPlaced(game) {
    // Beim Haus ziehen Leute ein
    game.resources.values.pop += 2;
    // Wenn am Platz ein Baum war, hat World ihn bereits entfernt – optional Bonus:
    game.resources.values.wood += 0; // hier kein Bonus
  }
}
