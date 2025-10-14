export class Grid {
  constructor(size) {
    this.size = size;
    this.tiles = Array.from({ length: size }, (_, x) =>
      Array.from({ length: size }, (_, z) => ({
        x, z, buildInstance: null, mesh: null, hasTree: false, reserved: (x < 3 && z < 3)
      }))
    );
  }

  get(x, z) {
    if (x < 0 || z < 0 || x >= this.size || z >= this.size) return null;
    return this.tiles[x][z];
  }

  *iter() {
    for (let x = 0; x < this.size; x++) {
      for (let z = 0; z < this.size; z++) {
        yield this.tiles[x][z];
      }
    }
  }
}
