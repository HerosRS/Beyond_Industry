import * as THREE from 'three';

export class Trees {
  constructor(scene, gridSize, grid) {
    this.scene = scene;
    this.gridSize = gridSize;
    this.grid = grid;
    this.treeMeshes = new Map(); // key: `${x},${z}` -> [trunk, top]

    this.spawn(0.2);
  }

  spawn(density = 0.18) {
    const trunkMat = new THREE.MeshStandardMaterial({ color: 0x6b4a2f, roughness: .9 });
    const topMat   = new THREE.MeshStandardMaterial({ color: 0x2f6b3a, roughness: .7 });
    const trunkGeo = new THREE.CylinderGeometry(0.08, 0.12, 0.6, 7);
    const topGeo   = new THREE.ConeGeometry(0.45, 0.9, 8);
    const half = this.gridSize * 0.5;

    for (const t of this.grid.iter()) {
      if (t.reserved) continue;
      if (Math.random() < density) {
        const tx = (t.x + .5) - half;
        const tz = (t.z + .5) - half;

        const trunk = new THREE.Mesh(trunkGeo, trunkMat);
        trunk.position.set(tx, 0.3, tz); trunk.castShadow = true; trunk.receiveShadow = true;
        const top = new THREE.Mesh(topGeo, topMat);
        top.position.set(tx, 1.0, tz); top.castShadow = true; top.receiveShadow = true;

        this.scene.add(trunk, top);
        this.treeMeshes.set(`${t.x},${t.z}`, [trunk, top]);
        t.hasTree = true;
      }
    }
  }

  adjacentTreeCount(x, z) {
    let c = 0;
    for (let dx = -1; dx <= 1; dx++) {
      for (let dz = -1; dz <= 1; dz++) {
        if (dx === 0 && dz === 0) continue;
        const nx = x + dx, nz = z + dz;
        const nt = this.grid.get(nx, nz);
        if (nt?.hasTree) c++;
      }
    }
    return c;
  }

  removeTreeAt(x, z) {
    const key = `${x},${z}`;
    const pair = this.treeMeshes.get(key);
    if (pair) {
      for (const m of pair) this.scene.remove(m);
      this.treeMeshes.delete(key);
    }
    const t = this.grid.get(x, z);
    if (t) t.hasTree = false;
  }

  removeRandomAdjacentTo(x, z) {
    const candidates = [];
    for (let dx = -1; dx <= 1; dx++) {
      for (let dz = -1; dz <= 1; dz++) {
        if (dx === 0 && dz === 0) continue;
        const nx = x + dx, nz = z + dz;
        const nt = this.grid.get(nx, nz);
        if (nt?.hasTree) candidates.push({ x: nx, z: nz });
      }
    }
    if (candidates.length) {
      const c = candidates[Math.floor(Math.random() * candidates.length)];
      this.removeTreeAt(c.x, c.z);
    }
  }
}
