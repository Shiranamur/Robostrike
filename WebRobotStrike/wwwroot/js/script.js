window.localStore = {
  set: function(key, value) {
    localStorage.setItem(key, value);
  },
  get: function(key) {
    return localStorage.getItem(key);
  },
  remove: function(key) {
    localStorage.removeItem(key);
  }
};


const TILE_SIZE = 48;
let mapData;
let playerPos = { x: 0, y: 0 };

//Charger map json
fetch('map_test.json')
  .then(res => res.json())
  .then(data => {
    mapData = data;
    initMap();
  });

function initMap() {
  const mapContainer = document.getElementById('map');
  mapContainer.style.gridTemplateColumns = `repeat(${mapData.map_width}, ${TILE_SIZE}px)`;
  mapContainer.style.gridTemplateRows = `repeat(${mapData.map_height}, ${TILE_SIZE}px)`;

  //Générer grille
  for (const tile of mapData.tiles) {
    const div = document.createElement('div');
    div.classList.add('cell');

    if (tile.Type === 'wall') {
      div.style.backgroundImage = "url('assets/wall.png')";
    } else {
      div.style.backgroundImage = "url('assets/normal.png')";
    }

    div.dataset.x = tile.x;
    div.dataset.y = tile.y;

    mapContainer.appendChild(div);

    if (tile.Type === 'spawn') {
      playerPos = { x: tile.x, y: tile.y };
      div.style.backgroundImage = "url('assets/spawn.png')";
    }
  }

  //Ajouter joueur
  const player = document.createElement('div');
  player.id = 'player';
  mapContainer.appendChild(player);
  updatePlayerPosition();

  document.addEventListener('keydown', handleMovement);
}

function updatePlayerPosition() {
  const player = document.getElementById('player');
  player.style.left = `${playerPos.x * TILE_SIZE}px`;
  player.style.top = `${playerPos.y * TILE_SIZE}px`;
}

function getTileType(x, y) {
  return mapData.tiles.find(t => t.x === x && t.y === y)?.Type || "normal";
}

function handleMovement(e) {
  let newX = playerPos.x;
  let newY = playerPos.y;

  if (e.key === "ArrowUp" || e.key === "z") newY--;
  if (e.key === "ArrowDown" || e.key === "s") newY++;
  if (e.key === "ArrowLeft" || e.key === "q") newX--;
  if (e.key === "ArrowRight" || e.key === "d") newX++;

  //Collision
  if (
    newX >= 0 && newX < mapData.map_width &&
    newY >= 0 && newY < mapData.map_height &&
    getTileType(newX, newY) !== "wall"
  ) {
    playerPos.x = newX;
    playerPos.y = newY;
    updatePlayerPosition();
  }
}

// script.js

function initializeGameCanvas(containerId) {
  console.log("Initializing game canvas for container:", containerId);
  var container = document.getElementById(containerId);
  if (!container) return;

  // Pour l'exemple, on crée un canvas dans le conteneur
  var canvas = document.createElement("canvas");
  canvas.width = container.clientWidth;
  canvas.height = container.clientHeight;
  container.appendChild(canvas);

  var ctx = canvas.getContext("2d");
  // Exemple d'animation simple : dessiner un carré animé
  var posX = 0;
  function animate() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = "blue";
    ctx.fillRect(posX, 50, 50, 50);
    posX += 2;
    if (posX > canvas.width) posX = 0;
    requestAnimationFrame(animate);
  }
  animate();
}


