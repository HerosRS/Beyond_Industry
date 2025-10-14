import * as THREE from 'three';
import { Grid } from '../world/grid.js';
import { Trees } from '../world/trees.js';

export class World {
  constructor(scene, { gridSize = 20, onTileClick } = {}) {
    this.scene = scene;
    this.gridSize = gridSize;
    this.onTileClick = onTileClick;

    // Ground
    const ground = new THREE.Mesh(
      new THREE.PlaneGeometry(gridSize, gridSize, gridSize, gridSize),
      new THREE.MeshStandardMaterial({ color: 0x1b2a1f, roughness: .95 })
    );
    ground.rotation.x = -Math.PI / 2;
    ground.receiveShadow = true;
    scene.add(ground);
    this.ground = ground;

    const gridHelper = new THREE.GridHelper(gridSize, gridSize, 0x2f3946, 0x232b38);
    gridHelper.position.y = 0.001;
    scene.add(gridHelper);

    this.grid = new Grid(gridSize);
    this.trees = new Trees(scene, gridSize, this.grid);

    // Highlight
    this.highlight = new THREE.Mesh(
      new THREE.PlaneGeometry(1, 1),
      new THREE.MeshBasicMaterial({ color: 0x6da6ff, transparent: true, opacity: .22, side: THREE.DoubleSide })
    );
    this.highlight.rotation.x = -Math.PI / 2;
    this.highlight.visible = false;
    scene.add(this.highlight);

    this.raycaster = new THREE.Raycaster();
    this.mouse = new THREE.Vector2();

    window.addEventListener('mousemove', (e) => this.onMouseMove(e));
    window.addEventListener('mousedown',  (e) => this.onMouseDown(e));
  }

  screenToTile(ev) {
    const rect = document.body.querySelector('canvas').getBoundingClientRect();
    this.mouse.x = ((ev.clientX - rect.left) / rect.width) * 2 - 1;
    this.mouse.y = -((ev.clientY - rect.top) / rect.height) * 2 + 1;
    // get camera from global THREE scope? We'll fetch from renderer via scene? Instead use the first camera found on window:
    const camera = window.Game?.camera || (window.camera);
    if (!camera) return null;

    this.raycaster.setFromCamera(this.mouse, camera);
    const hit = this.raycaster.intersectObject(this.ground, true)[0];
    if (!hit) return null;
    const p = hit.point;
    const half = this.gridSize * 0.5;
    const gx = Math.floor(p.x + half);
    const gz = Math.floor(p.z + half);
    if (gx < 0 || gz < 0 || gx >= this.gridSize || gz >= this.gridSize) return null;
    return this.grid.get(gx, gz);
  }

  onMouseMove(ev) {
    const tile = this.screenToTile(ev);
    if (tile) {
      const half = this.gridSize * 0.5;
      this.highlight.visible = true;
      this.highlight.position.set((tile.x + .5) - half, 0.003, (tile.z + .5) - half);
    } else {
      this.highlight.visible = false;
    }
  }

  onMouseDown(ev) {
    const tile = this.screenToTile(ev);
    if (tile && this.onTileClick) this.onTileClick(tile);
  }

  iterTiles() { return this.grid.iter(); }

  getTileBuild(x, z) {
    const t = this.grid.get(x, z);
    return t?.buildInstance || null;
  }

  placeBuilding(x, z, buildingInstance) {
    const t = this.grid.get(x, z);
    if (!t) return false;

    // Road: darf auf leeres Feld oder Road
    if (buildingInstance.kind === 'road') {
      // remove prior mesh if any
      if (t.buildInstance && t.buildInstance.kind !== 'road') return false;
      if (t.mesh) this.scene.remove(t.mesh);
      const m = buildingInstance.createMesh();
      const half = this.gridSize * 0.5;
      m.position.set((x + .5) - half, 0.002, (z + .5) - half);
      m.rotation.x = -Math.PI / 2;
      m.receiveShadow = true;
      this.scene.add(m);
      t.mesh = m;
      t.buildInstance = buildingInstance;
      return true;
    }

    // Andere Gebäude: nur auf leer
    if (t.buildInstance) return false;

    // Baum weg? -> Kleiner Holzbonus wird in Building selbst abgewickelt (onPlaced) oder hier:
    if (t.hasTree) {
      this.trees.removeTreeAt(x, z);
      t.hasTree = false;
      // Bonus wird über onPlaced im Building geregelt (z.B. +2 Holz).
    }

    const m = buildingInstance.createMesh();
    const half = this.gridSize * 0.5;
    m.position.set((x + .5) - half, 0.3, (z + .5) - half);
    m.castShadow = true; m.receiveShadow = true;
    this.scene.add(m);

    t.mesh = m;
    t.buildInstance = buildingInstance;
    return true;
  }
}
