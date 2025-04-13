// wwwroot/js/gameUI.js
window.gameUI = (function(){

    let ctx = null;
    let gameData = null; // { matchId, map, players }

    function initCanvas(rawJson) {
        const canvas = document.getElementById("gameCanvas");
        if (!canvas) {
            console.warn("[gameUI] No canvas found");
            return;
        }
        ctx = canvas.getContext("2d");

        // parse the JSON
        gameData = JSON.parse(rawJson);

        // draw the map & players
        drawMap();
        drawPlayers();
    }

    function drawMap() {
        if (!ctx || !gameData?.map) return;
        const { width, height, tiles } = gameData.map;

        // fill background
        ctx.fillStyle = "#eee";
        ctx.fillRect(0, 0, 800, 600);

        if (tiles && Array.isArray(tiles)) {
            tiles.forEach(tile => {
                if (tile.Type === "wall") {
                    ctx.fillStyle = "black";
                    ctx.fillRect(tile.X, tile.Y, 30, 30);
                }
            });
        }
    }

    function drawPlayers() {
        if (!ctx || !gameData?.players) return;
        gameData.players.forEach(p => {
            ctx.fillStyle = "red";
            ctx.fillRect(p.x, p.y, 20, 20);
        });
    }

    function updateGameState(newData) {
        gameData = newData;
        ctx.clearRect(0, 0, 800, 600);
        drawMap();
        drawPlayers();
    }

    return {
        initCanvas,
        updateGameState
    };
})();
