import * as THREE from 'three';
import { Building } from './building.js';

export class Road extends Building {
  constructor() {
    super();
    this.kind = 'road';
    this.cost = { stone: 1 };
  }

  createMesh() {
    const geo = new THREE.PlaneGeometry(1, 1);
    const mat = new THREE.MeshStandardMaterial({ color: 0x404850, roughness: .95 });
    const m = new THREE.Mesh(geo, mat);
    // Rotation/Position wird in World gesetzt (liegt plan auf Boden)
    return m;
  }
}
