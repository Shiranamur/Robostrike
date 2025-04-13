function startGameLoop(gameData) {
    console.log("Game loop started with:", gameData);
    if (!gameData || !gameData.map) {
        console.error("startGameLoop: gameData is missing or doesn't have map");
        return;
    }
    renderMap(gameData.map);
    renderPlayers(gameData.players);
    setupHud();
}

function renderMap(mapData) {
    const mapDiv = document.getElementById("map");
    if (!mapDiv) {
        console.error("renderMap: mapDiv not found");
        return;
    }

    if (!mapData) {
        console.error("renderMap: mapData is undefined");
        return;
    }

    if (!mapData.tiles || !Array.isArray(mapData.tiles)) {
        console.error("renderMap: mapData.tiles is not available or not an array", mapData);
        return;
    }

    // Clear existing content.
    mapDiv.innerHTML = "";

    // Configure the grid based on the map dimensions.
    mapDiv.style.display = "grid";
    mapDiv.style.gridTemplateColumns = `repeat(${mapData.map_width}, 48px)`;
    mapDiv.style.gridTemplateRows = `repeat(${mapData.map_height}, 48px)`;

    // Create and add each tile element.
    mapData.tiles.forEach(tile => {
        console.log("Tile object:", tile);

        const tileDiv = document.createElement("div");
        tileDiv.classList.add("cell");
        tileDiv.style.width = "48px";
        tileDiv.style.height = "48px";
        tileDiv.style.border = "1px solid #ddd";

        // Use the correct property name ("Type")
        const tileType = tile.type;
        console.log("Tile type:", tileType); // Should output "normal" (or whatever type)
        tileDiv.style.backgroundImage = `url('/images/Tiles/${tileType}.png')`;
        tileDiv.style.backgroundSize = "cover";
        tileDiv.style.backgroundPosition = "center";

        mapDiv.appendChild(tileDiv);
    });
}


function renderPlayers(players) {
    const mapDiv = document.getElementById("map");
    if (!mapDiv) return;
    
    const existingPlayers = mapDiv.querySelectorAll('.player');
    existingPlayers.forEach(el => el.remove());

    players.forEach(player => {
        const playerEl = document.createElement("div");
        playerEl.classList.add("player");
        // needs modification, like hover on player or smth
        playerEl.innerText = player.id;
        playerEl.style.position = "absolute";
        playerEl.style.left = (player.x * 48) + "px";
        playerEl.style.top = (player.y * 48) + "px";
        playerEl.style.width = "48px";
        playerEl.style.height = "48px";
        playerEl.style.background = "rgba(0, 0, 0, 0.5)";
        playerEl.style.color = "#fff";
        playerEl.style.display = "flex";
        playerEl.style.alignItems = "center";
        playerEl.style.justifyContent = "center";
        mapDiv.appendChild(playerEl);
    });
}

function setupHud(){
    const hudTextArea = document.getElementById("hudTextArea");
    if (!hudTextArea) return;
    
    function addChar(char){
        hudTextArea.value += char;
    }
    
    const buttonMappings = [
        {id: "rotateLeftBtn", char: "L"},
        {id: "rotateRightBtn", char: "R"},
        {id: "forwardBtn", char: "F"},
        {id: "dashBtn", char: "D"},
        {id: "shootBtn", char: "S"}
    ];
    
    buttonMappings.forEach(mapping => {
        const btn = document.getElementById(mapping.id);
        if (btn) {
            btn.addEventListener("click", ()=>{
                addChar(mapping.char);
            });
        }
    });
    
    const deleteBtn = document.getElementById("deleteBtn");
    if (deleteBtn) {
        deleteBtn.addEventListener("click", ()=>{
            hudTextArea.value = hudTextArea.value.slice(0, -1);
        });
    }
    
    const sendBtn = document.getElementById("sendBtn");
    if (sendBtn) {
        sendBtn.addEventListener("click", ()=>{
            const commands =hudTextArea.value;
            console.log("sent value to api:", commands);
            hudTextArea.value = "";
        });
    }
}