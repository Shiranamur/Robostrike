

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
    
    // const existingPlayers = mapDiv.querySelectorAll(".player");
    // existingPlayers.forEach(el => el.remove());

    players.forEach(player => {
        const playerEl = document.createElement("div");
        playerEl.classList.add("player");
        playerEl.style.position = "absolute";
        playerEl.style.width = "48px";
        playerEl.style.height = "48px";
        
        playerEl.style.left = (player.x * 48) + "px";
        playerEl.style.top = (player.y * 48) + "px";
        
        playerEl.style.backgroundImage = `url('/images/Sprites/player${player.id}.png')`;
        playerEl.style.backgroundSize = "contain";
        playerEl.style.backgroundRepeat = "no-repeat";
        playerEl.style.backgroundPosition = "center";
        
        playerEl.dataset.playerId = player.id;
        
        playerEl.style.transition = "left 0.8s ease, top 0.8s ease";

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


function animateRound(roundJson) {
    const roundData = JSON.parse(roundJson);
    console.log(roundData);
    console.log("Animating round:", roundData.roundNumber);
    if (!roundData || !Array.isArray(roundData.turns)) {
        console.error("Invalid round data for animation.");
        return;
    }

    // Define a delay between turns (in milliseconds)
    let turnDelay = 0;
    const delayIncrement = 1000; // 1 second per turn; adjust as needed

    // Process each turn sequentially using setTimeout.
    roundData.turns.forEach(turn => {
        setTimeout(() => {
            animateTurn(turn);
        }, turnDelay);
        turnDelay += delayIncrement;
    });
}

/**
 * Animates a single turn.
 * @param {object} turn - A turn state object with properties such as turnNumber and players.
 */
function animateTurn(turn) {
    console.log("Animating turn:", turn.turnNumber);

    // Process each player in the turn:
    if (Array.isArray(turn.players)) {
        turn.players.forEach(playerState => {
            // Find the DOM element corresponding to this player.
            // Assuming players have been rendered with a data attribute "data-player-id"
            let playerEl = document.querySelector(`.player[data-player-id="${playerState.id}"]`);
            if (playerEl) {
                // Calculate new positions. Assume each cell is 48px.
                let newLeft = playerState.x * 48;
                let newTop = playerState.y * 48;

                // Animate movement using a CSS transition, for example:
                playerEl.style.transition = "left 0.8s ease, top 0.8s ease";
                playerEl.style.left = newLeft + "px";
                playerEl.style.top = newTop + "px";

                // Optionally, update other properties (like rotation/direction) or trigger CSS classes for shots.
                // For example, if a shot occurred, you might add a class:
                // if (playerState.ShotHitPlayer && Object.keys(playerState.ShotHitPlayer).length > 0) {
                //     playerEl.classList.add("shot-fired");
                //     setTimeout(() => { playerEl.classList.remove("shot-fired"); }, 800);
                // }
            } else {
                console.warn("Player element not found for ID:", playerState.id);
            }
        });
    }
}


function animateShot(shotData) {
    // Example:
    const shotEl = document.createElement("div");
    shotEl.classList.add("shot");
    document.body.appendChild(shotEl);

    // Set the shot's starting position based on shotData
    shotEl.style.left = shotData.startX + "px";
    shotEl.style.top = shotData.startY + "px";

    // Animate the shot to its destination over 800ms.
    setTimeout(() => {
        shotEl.style.left = shotData.endX + "px";
        shotEl.style.top = shotData.endY + "px";
    }, 100);

    // Remove the shot element after the animation completes.
    setTimeout(() => {
        shotEl.remove();
    }, 1000);
}