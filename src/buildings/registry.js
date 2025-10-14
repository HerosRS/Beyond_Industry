import { House } from './house.js';
import { Farm } from './farm.js';
import { LumberCamp } from './lumberCamp.js';
import { Road } from './road.js';

export const BuildingRegistry = {
  map: new Map([
    ['house', House],
    ['farm', Farm],
    ['lumber', LumberCamp],
    ['road', Road],
  ]),
  resolve(id) { return this.map.get(id); }
};
