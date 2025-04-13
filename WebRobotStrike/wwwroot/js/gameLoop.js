function startGameLoop(gameData) {
    console.log("Game loop started with:", gameData);
    if (!gameData || !gameData.map) {
        console.error("startGameLoop: gameData is missing or doesn't have map");
        return;
    }
    renderMap(gameData.map);
    renderPlayers(gameData.players);
    setupHud(gameData.matchId);
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
    
    mapDiv.innerHTML = "";

    // Configure the grid based on the map dimensions.
    mapDiv.style.display = "grid";
    mapDiv.style.gridTemplateColumns = `repeat(${mapData.map_width}, 48px)`;
    mapDiv.style.gridTemplateRows = `repeat(${mapData.map_height}, 48px)`;

    // Create and add each tile element.
    mapData.tiles.forEach(tile => {

        const tileDiv = document.createElement("div");
        tileDiv.classList.add("cell");
        tileDiv.style.width = "48px";
        tileDiv.style.height = "48px";
        tileDiv.style.border = "1px solid #ddd";
        
        const tileType = tile.type;
        tileDiv.style.backgroundImage = `url('/images/Tiles/${tileType}.png')`;
        tileDiv.style.backgroundSize = "cover";
        tileDiv.style.backgroundPosition = "center";

        mapDiv.appendChild(tileDiv);
    });
}


function renderPlayers(players) {
    const mapDiv = document.getElementById("map");
    if (!mapDiv) return;

    // Remove any existing player elements.
    //const existingPlayers = mapDiv.querySelectorAll(".player");
    //existingPlayers.forEach(el => el.remove());

    players.forEach(player => {
        const playerEl = document.createElement("div");
        playerEl.classList.add("player");
        playerEl.style.position = "absolute";
        playerEl.style.left = (player.x * 48) + "px";
        playerEl.style.top = (player.y * 48) + "px";
        playerEl.style.width = "48px";
        playerEl.style.height = "48px";

        playerEl.style.backgroundImage = `url('/images/Sprites/player${player.id}.png')`;
        playerEl.style.backgroundSize = "contain";
        playerEl.style.backgroundRepeat = "no-repeat";
        playerEl.style.backgroundPosition = "center";
        // playerEl.innerText = player.id;

        mapDiv.appendChild(playerEl);
    });
}


function setupHud() {
    const hudTextArea = document.getElementById("hudTextArea");
    if (!hudTextArea) return;

    // Enforce max length
    hudTextArea.maxLength = 6;

    // Helper: Append a character if there's room, else trigger flash message.
    function addChar(char) {
        if (hudTextArea.value.length < 6) {
            hudTextArea.value += char;
        } else {
            triggerFlashMessage();
        }
    }

    // Map buttons to their corresponding characters.
    const buttonMappings = [
        { id: "rotateLeftBtn", char: "a" },
        { id: "rotateRightBtn", char: "e" },
        { id: "forwardBtn", char: "z" },
        { id: "reverseBtn", char: "s" },
        { id: "shootBtn", char: "d" }
    ];

    buttonMappings.forEach(mapping => {
        const btn = document.getElementById(mapping.id);
        if (btn) {
            btn.addEventListener("click", () => {
                addChar(mapping.char);
            });
        }
    });

    // Delete button: Remove the last character.
    const deleteBtn = document.getElementById("deleteBtn");
    if (deleteBtn) {
        deleteBtn.addEventListener("click", () => {
            hudTextArea.value = hudTextArea.value.slice(0, -1);
        });
    }

    // Remove send button handling from JS.
    // The send button's click event is now managed from the Blazor component.
}

/**
 * Triggers a flash message to indicate the input limit has been reached.
 */
function triggerFlashMessage() {
    const flashDiv = document.getElementById("flashMessage");
    if (!flashDiv) {
        console.error("Flash message element not found.");
        return;
    }

    flashDiv.classList.add("show");

    // After a brief period, remove the 'show' class so that the message fades away.
    setTimeout(() => {
        flashDiv.classList.remove("show");
    }, 500);
}
