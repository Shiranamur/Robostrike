

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

    players.forEach(player => {
        const playerEl = document.createElement("div");
        const rotationValue = directionToRotation[player.direction.toLowerCase()] || "0deg";
        console.log(player.direction);

        playerEl.classList.add("player");
        playerEl.style.position = "absolute";
        playerEl.style.width = "48px";
        playerEl.style.height = "48px";

        playerEl.style.left = (player.x * 48) + "px";
        playerEl.style.top = (player.y * 48) + "px";

        playerEl.style.backgroundImage = `url('/images/Sprites/player${player.inGameId}.png')`;
        playerEl.style.backgroundSize = "contain";
        playerEl.style.backgroundRepeat = "no-repeat";
        playerEl.style.backgroundPosition = "center";

        playerEl.dataset.playerId = player.id;
        playerEl.style.transform = `rotate(${rotationValue})`;

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
    }/**
 * Triggers a flash message to indicate the input limit has been reached.
 */

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

    if (!Array.isArray(turn.players)) return;
    
    turn.players.forEach(shooter => {
        if (shooter.input === "d") {
            // find the victim of this shot, if any
            const victim = turn.players.find(p => p.damage_Taken > 0);
            const target = victim || getCellAhead(shooter);
            animateShot(shooter, target);
        }
    });
    
    turn.players.forEach(player => {
        updatePlayerState(player);
    });
}
function getCellAhead(player) {
    const dir = player.direction.toLowerCase();
    const dx = dir === 'e' ? 1 : dir === 'w' ? -1 : 0;
    const dy = dir === 's' ? 1 : dir === 'n' ? -1 : 0;
    return { x: player.x + dx, y: player.y + dy };
}


function updatePlayerState(player) {
    let playerEl = document.querySelector(`.player[data-player-id="${player.id}"]`);
    if (!playerEl) {
        console.warn("Player element not found for ID:", player.id);
        return;
    }
    
    const newLeft = player.x * 48;
    const newTop  = player.y * 48;
    
    const rotationValue = directionToRotation[player.direction.toLowerCase()] || "0deg";
    
    playerEl.style.left = newLeft + "px";
    playerEl.style.top = newTop + "px";
    playerEl.style.transform = `rotate(${rotationValue})`;
}

/**
 * Animate a plasma shot from shooter to target cell.
 * Uses plasma1.png or plasma2.png as the sprite.
 */
function animateShot(shooter, target) {
    const mapDiv = document.getElementById("map");
    const shotEl = document.createElement("div");
    shotEl.classList.add("shot");
    
    //const sprite = Math.random() < 0.5 ? "plasma1.png" : "plasma2.png";
    shotEl.style.backgroundImage = `url('/images/Sprites/plasma1.png')`;
    
    const SHOT_SIZE = 40;
    shotEl.style.width  = `${SHOT_SIZE}px`;
    shotEl.style.height = `${SHOT_SIZE}px`;
    shotEl.style.transformOrigin = "center center";
    const angle = directionToRotation[shooter.direction.toLowerCase()] || "0deg";
    shotEl.style.transform = `rotate(${angle})`;
    
    const startX = shooter.x * 48 + (48 - SHOT_SIZE) / 2;
    const startY = shooter.y * 48 + (48 - SHOT_SIZE) / 2;
    shotEl.style.left = `${startX}px`;
    shotEl.style.top  = `${startY}px`;

    mapDiv.appendChild(shotEl);
    
    const endX = target.x * 48 + (48 - SHOT_SIZE) / 2;
    const endY = target.y * 48 + (48 - SHOT_SIZE) / 2;
    
    const travelTime = 500; // ms
    requestAnimationFrame(() => {
        shotEl.style.transition = `left ${travelTime}ms linear, top ${travelTime}ms linear`;
        shotEl.style.left = `${endX}px`;
        shotEl.style.top  = `${endY}px`;
    });
    
    setTimeout(() => {
        shotEl.remove();
        animateBoom(target);
    }, travelTime + 50);
}

/**
 * Flash an explosion on the cell of the player who got hit.
 */
function animateBoom(victim) {
    const boomEl = document.createElement("div");
    boomEl.classList.add("boom");
    boomEl.style.backgroundImage = "url('/images/Sprites/boom1.png')";
    boomEl.style.left = victim.x * 48 + "px";
    boomEl.style.top  = victim.y * 48 + "px";

    const mapDiv = document.getElementById("map");
    mapDiv.appendChild(boomEl);
    
    setTimeout(() => boomEl.remove(), 500);
}

const directionToRotation = {
    'n': '0deg',
    's': '180deg',
    'e': '90deg',
    'w': '270deg',
};

function onGameOver(){
    var isgameover = true;
}