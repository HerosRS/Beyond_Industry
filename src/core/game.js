import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { World } from './world.js';
import { Resources } from './resources.js';
import { TimeSim } from './Time.js';
import { BuildingRegistry } from '../buildings/registry.js';

export class Game {
  constructor() {
    this.renderer = new THREE.WebGLRenderer({ antialias: true });
    this.renderer.setSize(innerWidth, innerHeight);
    this.renderer.setPixelRatio(devicePixelRatio);
    document.body.appendChild(this.renderer.domElement);

    this.scene = new THREE.Scene();
    this.scene.background = new THREE.Color(0x0f1115);

    this.camera = new THREE.PerspectiveCamera(55, innerWidth / innerHeight, 0.1, 2000);
    this.camera.position.set(20, 28, 20);

    this.controls = new OrbitControls(this.camera, this.renderer.domElement);
    this.controls.enableDamping = true;
    this.controls.dampingFactor = .06;
    this.controls.enablePan = true;
    this.controls.panSpeed = .7;
    this.controls.maxPolarAngle = Math.PI * 0.495;
    this.controls.target.set(10, 0, 10);

    const hemi = new THREE.HemisphereLight(0xbcd1ff, 0x40424a, 0.8);
    this.scene.add(hemi);
    const dir = new THREE.DirectionalLight(0xffffff, 0.8);
    dir.position.set(10, 20, 8);
    dir.castShadow = true;
    dir.shadow.mapSize.set(1024, 1024);
    this.scene.add(dir);

    this.resources = new Resources({
      wood: 30, stone: 15, food: 25, pop: 5, heat: 0
    });

    this.world = new World(this.scene, {
      gridSize: 20,
      onTileClick: (tile) => this.placeOnTile(tile),
    });

    this.time = new TimeSim({
      onDay: () => this.simulateDay(),
      secondsPerDay: 1.25
    });

    this.currentTool = null; // 'house' | 'farm' | 'lumber' | 'road'
    this.onResourcesChanged = null;
    this.onTimeChanged = null;

    window.addEventListener('resize', () => this.onResize());
    this.animate();
  }

  onResize() {
    this.camera.aspect = innerWidth / innerHeight;
    this.camera.updateProjectionMatrix();
    this.renderer.setSize(innerWidth, innerHeight);
  }

  selectTool(toolId) {
    this.currentTool = (toolId === 'cancel') ? null : toolId;
  }

  placeOnTile(tile) {
    if (!tile) return;
    if (!this.currentTool) return;

    const BuildingClass = BuildingRegistry.resolve(this.currentTool);
    if (!BuildingClass) return;

    // Straßen können über existierende Straße/tile gelegt werden, andere nur auf freien Feldern
    const already = this.world.getTileBuild(tile.x, tile.z);
    if (this.currentTool !== 'road' && already) return;

    // Kosten prüfen
    const tmp = new BuildingClass();
    if (!this.resources.canAfford(tmp.cost)) return;

    // Platzieren
    const built = this.world.placeBuilding(tile.x, tile.z, new BuildingClass());
    if (!built) return;

    // Kosten abziehen + evtl. Sofort-Effekte
    this.resources.pay(tmp.cost);
    if (tmp.onPlaced) tmp.onPlaced(this);

    if (this.onResourcesChanged) this.onResourcesChanged();
  }

  simulateDay() {
    // Produktions- und Verbrauchsrunde
    const season = this.time.season();
    let producedFood = 0;
    let producedWood = 0;

    for (const tile of this.world.iterTiles()) {
      const inst = tile.buildInstance;
      if (!inst) continue;
      if (inst.producePerDay) {
        const prod = inst.producePerDay({ game: this, tile, season });
        if (prod?.food) producedFood += prod.food;
        if (prod?.wood) producedWood += prod.wood;
      }
    }

    this.resources.add({ food: producedFood, wood: producedWood });

    // Konsum
    const hunger = this.resources.values.pop * 1;
    this.resources.add({ food: -hunger });

    // Winter-Heizbedarf
    if (season === 'Winter') {
      const heatNeed = Math.ceil(this.resources.values.pop * 0.5);
      const burn = Math.min(this.resources.values.wood, heatNeed);
      this.resources.add({ wood: -burn });
      this.resources.values.heat = burn;
    } else {
      this.resources.values.heat = 0;
    }

    // Verhungern / Wachstum
    if (this.resources.values.food < 0) {
      this.resources.values.food = 0;
      this.resources.values.pop = Math.max(0, this.resources.values.pop - 1);
    } else if ((producedFood - hunger) >= 2 && Math.random() < 0.3) {
      this.resources.values.pop += 1;
    }

    this.time.advance(); // Tag ++ / ggf. Jahreszeit wechseln
    if (this.onResourcesChanged) this.onResourcesChanged();
    if (this.onTimeChanged) this.onTimeChanged();
  }

  animate() {
    const loop = (now) => {
      this.controls.update();
      this.time.update(now, () => this.simulateDay());
      this.renderer.render(this.scene, this.camera);
      requestAnimationFrame(loop);
    };
    requestAnimationFrame(loop);
  }
}
