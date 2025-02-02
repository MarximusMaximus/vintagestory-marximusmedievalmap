
# MarximusMedievalMap

Simplifies the world map, with customizable colors. Similar to the default map, but looks a bit nicer on the map for building a settlement. Also provides a happy medium between the default map and the full color map as it helps a bit with the visibility on the map of generated Points of Interest like ruins and traders.

This is a rebuild and update of the mod MedievalMap by  Rangelost, who is on indefinite break (as of some point in 2023 and currently as of 2025-02-02).

 
## WARNING: Read carefully before considering installing this mod.

This mod replaces the way world maps are drawn. It does not stylize your existing world maps entirely. Only chunks that are loaded while this mod is active will be drawn in the new style. Inversely, if you disable this mod, the chunks will not be reverted to their original version until they are loaded again.

Before using this mod, it is highly recommended that you create a backup of your maps (`.../VintagestoryData/Maps/`).

 
Upon running the mod, a `medievalmap.json` config file will be created or updated in the game's `ModConfig` directory. The available options are:

- `ink_colour`: hex colour code for most non-natural blocks and outlines.
- `land_colour`: hex colour code for land.
- `desert_colour`: hex colour code for sand and gravel.
- `forest_colour`: hex colour code for forest floor.
- `road_colour`: hex colour code for roads and farmland.
- `plant_colour`: hex colour code for most plants.
- `water_colour`: hex colour code for water.
- `ice_colour`: hex colour code for permanent ice, such as glaciers.
- `lava_colour`: hex colour code for lava (currently unused).
- `high_colour`: hex colour code to denote high altitude, such as mountains.
- `low_colour`: hex colour code to denote low altitude, such as holes.
- `water_edge_colour`: hex colour code for the water edges.
- `generic_shadow_colour`: hex colour code for most shading.
- `plant_shadow_colour`: hex colour code for plant shading.
- `generic_grid_colour`: hex colour code for the grid.
- `water_grid_colour`: hex colour code for the grid over water.
- `generic_grid_opacity`: opacity of the grid (between 0 and 1).
- `ocean_grid_opacity`: opacity of the grid over oceans (between 0 and 1).
- `ocean_textured`: true to enable ocean texture, otherwise false.

 
 
## Changes I'd like to make:

- configurable via config lib GUI
- ability to customize what blocks are in a category
- ability to customize categories (e.g. add new categories and set colors for each)
