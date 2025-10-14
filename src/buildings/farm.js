import * as THREE from 'three';
import { Building } from './building.js';

export class Farm extends Building {
  constructor() {
    super();
    this.kind = 'farm';
    this.cost = { wood: 8 };
  }

  createMesh() {
    const geo = new THREE.BoxGeometry(1, 0.6, 1);
    const mat = new THREE.MeshStandardMaterial({ color: 0x7a8f52, roughness: .95 });
    return new THREE.Mesh(geo, mat);
  }

  producePerDay({ season }) {
    const yieldPerSeason = (season === 'Sommer') ? 5 : (season === 'Frühling' || season === 'Herbst') ? 3 : 1;
    return { food: yieldPerSeason };
  }

  onPlaced(game) {
    // Baum entfernt? Kleiner Holzbonus global sinnvoll – hier bewusst keiner
  }
}
