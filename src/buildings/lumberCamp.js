import * as THREE from 'three';
import { Building } from './building.js';

/**
 * Holzfällerhütte
 * - Produziert täglich Holz abhängig von angrenzenden Bäumen
 * - Lichtet umliegende Bäume langsam (20% Chance/Tag einen Baum im Umfeld zu entfernen)
 * - Beim Platzieren auf einer Lichtung nach Baumfällung gibt’s einen kleinen Holzbonus
 */
export class LumberCamp extends Building {
  constructor() {
    super();
    this.kind = 'lumber';
    this.cost = { wood: 8 };
  }

  createMesh() {
    // Placeholder: einfacher Klotz; später gern durch Low-Poly-Hütte ersetzen
    const geo = new THREE.BoxGeometry(1, 0.6, 1);
    const mat = new THREE.MeshStandardMaterial({ color: 0x5c6a7a, roughness: 0.9 });
    const mesh = new THREE.Mesh(geo, mat);
    return mesh;
  }

  /**
   * Tägliche Produktion
   * @param {{ game:any, tile:{x:number,z:number}, season:string }} ctx
   * @returns {{ wood?: number }}
   */
  producePerDay({ game, tile /*, season */ }) {
    // Mehr angrenzende Bäume -> mehr Holz, bis max. 3/Tag
    const around = game.world.trees.adjacentTreeCount(tile.x, tile.z);
    let wood = 0;
    if (around > 0) {
      wood = Math.min(3, around);

      // 20% Chance, einen angrenzenden Baum zu entfernen (schleichende Abholzung)
      if (Math.random() < 0.2) {
        game.world.trees.removeRandomAdjacentTo(tile.x, tile.z);
      }
    }
    return { wood };
  }

  /**
   * Soforteffekt beim Platzieren
   * @param {any} game
   */
  onPlaced(game) {
    // Kleiner Holzbonus beim Bau (z. B. aus gefällten Stämmen vor Ort)
    game.resources.values.wood += 2;
  }
}

export default LumberCamp;
