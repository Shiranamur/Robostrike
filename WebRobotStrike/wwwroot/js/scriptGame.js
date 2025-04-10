const TILE_SIZE = 48;
let mapData;
let playerPos = { x: 0, y: 0 };
let actionQueue = [];

//map json
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

  //map
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

  //ajouter joueur
  const player = document.createElement('div');
  player.id = 'player';
  mapContainer.appendChild(player);
  updatePlayerPosition();
}
//position joueur
function updatePlayerPosition() {
  const player = document.getElementById('player');
  player.style.left = `${playerPos.x * TILE_SIZE}px`;
  player.style.top = `${playerPos.y * TILE_SIZE}px`;
}
//check la tuile
function getTileType(x, y) {
  return mapData.tiles.find(t => t.x === x && t.y === y)?.Type || "normal";
}
//mouvement joueur
function handleMovement(e) {
  let newX = playerPos.x;
  let newY = playerPos.y;
  if (e.key === "ArrowUp" )
     newY--;
  if (e.key === "ArrowDown" ) 
    newY++;
  if (e.key === "ArrowLeft" )
     newX--;
  if (e.key === "ArrowRight" ) 
     newX++;
  //collision
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
//liste action
function queueAction(direction) {
  if (actionQueue.length >= 6) return;
  actionQueue.push(direction);
  updateActionList();
}
function updateActionList() {
  const list = document.getElementById('actionList');
  list.innerHTML = '';
  actionQueue.forEach((action, index) => {
    const li = document.createElement('li');
    li.textContent = `${index + 1}. ${action}`;
    list.appendChild(li);
  });
}
function executeActions() {
  if (actionQueue.length !== 6) {
    alert("Ajoute 6 actions");
    return;
  }
  let index = 0;
  const interval = setInterval(() => {
    const action = actionQueue[index];
    doMovement(action);
    index++;
    if (index >= actionQueue.length) {
      clearInterval(interval);
      actionQueue = []; 
      updateActionList();
    }
  }, 1000);
}
function doMovement(direction) {
  let keyMap = {
    haut: "ArrowUp",
    bas: "ArrowDown",
    gauche: "ArrowLeft",
    droite: "ArrowRight"
  };
  const event = new KeyboardEvent('keydown', { key: keyMap[direction] });
  handleMovement(event);
}
