// Tetris Game
let tetrisGame = null;
let tetrisEngine = null;
let tetrisInput = null;
let tetrisHighScore = null;

const TETROMINOS = {
    'I': [[1, 1, 1, 1]],
    'O': [[1, 1], [1, 1]],
    'T': [[0, 1, 0], [1, 1, 1]],
    'S': [[0, 1, 1], [1, 1, 0]],
    'Z': [[1, 1, 0], [0, 1, 1]],
    'J': [[1, 0, 0], [1, 1, 1]],
    'L': [[0, 0, 1], [1, 1, 1]]
};

const COLORS = {
    'I': '#00f0f0',
    'O': '#f0f000',
    'T': '#a000f0',
    'S': '#00f000',
    'Z': '#f00000',
    'J': '#0000f0',
    'L': '#f0a000'
};

function initTetris() {
    const canvas = document.getElementById('tetrisCanvas');
    if (!canvas) return;

    canvas.width = 300;
    canvas.height = 600;

    tetrisEngine = new GameEngine('tetrisCanvas', 60);
    tetrisInput = new InputHandler();
    tetrisHighScore = new HighScoreManager('tetris');

    tetrisGame = {
        gridWidth: 10,
        gridHeight: 20,
        cellSize: 30,
        grid: Array(20).fill(null).map(() => Array(10).fill(0)),
        currentPiece: null,
        currentX: 0,
        currentY: 0,
        currentType: '',
        nextPiece: '',
        score: 0,
        level: 1,
        lines: 0,
        gameOver: false,
        dropTimer: 0,
        dropInterval: 1.0,
        lockDelay: 0,
        lastMoveTime: 0
    };

    spawnNewPiece();
    tetrisEngine.start(updateTetris, renderTetris);
}

function stopTetris() {
    if (tetrisEngine) {
        tetrisEngine.stop();
    }
}

function getRandomPiece() {
    const pieces = Object.keys(TETROMINOS);
    return pieces[Math.floor(Math.random() * pieces.length)];
}

function spawnNewPiece() {
    if (!tetrisGame.nextPiece) {
        tetrisGame.nextPiece = getRandomPiece();
    }

    tetrisGame.currentType = tetrisGame.nextPiece;
    tetrisGame.currentPiece = JSON.parse(JSON.stringify(TETROMINOS[tetrisGame.currentType]));
    tetrisGame.nextPiece = getRandomPiece();
    tetrisGame.currentX = Math.floor(tetrisGame.gridWidth / 2) - Math.floor(tetrisGame.currentPiece[0].length / 2);
    tetrisGame.currentY = 0;
    tetrisGame.lockDelay = 0;

    if (checkCollision()) {
        gameOverTetris();
    }
}

function rotatePiece() {
    const rotated = tetrisGame.currentPiece[0].map((_, i) =>
        tetrisGame.currentPiece.map(row => row[i]).reverse()
    );

    const oldPiece = tetrisGame.currentPiece;
    tetrisGame.currentPiece = rotated;

    if (checkCollision()) {
        tetrisGame.currentPiece = oldPiece;
    }
}

function checkCollision() {
    for (let y = 0; y < tetrisGame.currentPiece.length; y++) {
        for (let x = 0; x < tetrisGame.currentPiece[y].length; x++) {
            if (tetrisGame.currentPiece[y][x]) {
                const newX = tetrisGame.currentX + x;
                const newY = tetrisGame.currentY + y;

                if (newX < 0 || newX >= tetrisGame.gridWidth ||
                    newY >= tetrisGame.gridHeight ||
                    (newY >= 0 && tetrisGame.grid[newY][newX])) {
                    return true;
                }
            }
        }
    }
    return false;
}

function mergePiece() {
    for (let y = 0; y < tetrisGame.currentPiece.length; y++) {
        for (let x = 0; x < tetrisGame.currentPiece[y].length; x++) {
            if (tetrisGame.currentPiece[y][x]) {
                const gridY = tetrisGame.currentY + y;
                const gridX = tetrisGame.currentX + x;
                if (gridY >= 0) {
                    tetrisGame.grid[gridY][gridX] = tetrisGame.currentType;
                }
            }
        }
    }
}

function clearLines() {
    let linesCleared = 0;

    for (let y = tetrisGame.gridHeight - 1; y >= 0; y--) {
        if (tetrisGame.grid[y].every(cell => cell !== 0)) {
            tetrisGame.grid.splice(y, 1);
            tetrisGame.grid.unshift(Array(tetrisGame.gridWidth).fill(0));
            linesCleared++;
            y++; // Check this line again
        }
    }

    if (linesCleared > 0) {
        const points = [0, 40, 100, 300, 1200];
        tetrisGame.score += points[linesCleared] * tetrisGame.level;
        tetrisGame.lines += linesCleared;
        tetrisGame.level = Math.floor(tetrisGame.lines / 10) + 1;
        tetrisGame.dropInterval = Math.max(0.1, 1.0 - (tetrisGame.level - 1) * 0.1);
        document.getElementById('tetrisScore').textContent = tetrisGame.score;
    }
}

function updateTetris(deltaTime) {
    if (tetrisGame.gameOver) return;

    const now = performance.now();

    // Rotation
    if (tetrisInput.isKeyDown('ArrowUp') && now - tetrisGame.lastMoveTime > 200) {
        rotatePiece();
        tetrisGame.lastMoveTime = now;
    }

    // Left/Right movement
    if (tetrisInput.isKeyDown('ArrowLeft') && now - tetrisGame.lastMoveTime > 100) {
        tetrisGame.currentX--;
        if (checkCollision()) tetrisGame.currentX++;
        tetrisGame.lastMoveTime = now;
    }

    if (tetrisInput.isKeyDown('ArrowRight') && now - tetrisGame.lastMoveTime > 100) {
        tetrisGame.currentX++;
        if (checkCollision()) tetrisGame.currentX--;
        tetrisGame.lastMoveTime = now;
    }

    // Fast drop
    const dropSpeed = tetrisInput.isKeyDown('ArrowDown') ? 0.05 : tetrisGame.dropInterval;

    // Auto drop
    tetrisGame.dropTimer += deltaTime;
    if (tetrisGame.dropTimer >= dropSpeed) {
        tetrisGame.dropTimer = 0;
        tetrisGame.currentY++;

        if (checkCollision()) {
            tetrisGame.currentY--;
            tetrisGame.lockDelay += deltaTime;

            if (tetrisGame.lockDelay >= 0.5) {
                mergePiece();
                clearLines();
                spawnNewPiece();
            }
        } else {
            tetrisGame.lockDelay = 0;
        }
    }

    // Hard drop (Space)
    if (tetrisInput.isKeyDown(' ') && now - tetrisGame.lastMoveTime > 200) {
        while (!checkCollision()) {
            tetrisGame.currentY++;
        }
        tetrisGame.currentY--;
        mergePiece();
        clearLines();
        spawnNewPiece();
        tetrisGame.lastMoveTime = now;
    }
}

function renderTetris(ctx) {
    // Clear canvas
    ctx.fillStyle = '#1a202c';
    ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height);

    // Draw grid
    ctx.strokeStyle = 'rgba(255,255,255,0.1)';
    ctx.lineWidth = 1;
    for (let x = 0; x <= tetrisGame.gridWidth; x++) {
        ctx.beginPath();
        ctx.moveTo(x * tetrisGame.cellSize, 0);
        ctx.lineTo(x * tetrisGame.cellSize, ctx.canvas.height);
        ctx.stroke();
    }
    for (let y = 0; y <= tetrisGame.gridHeight; y++) {
        ctx.beginPath();
        ctx.moveTo(0, y * tetrisGame.cellSize);
        ctx.lineTo(ctx.canvas.width, y * tetrisGame.cellSize);
        ctx.stroke();
    }

    // Draw placed pieces
    for (let y = 0; y < tetrisGame.gridHeight; y++) {
        for (let x = 0; x < tetrisGame.gridWidth; x++) {
            if (tetrisGame.grid[y][x]) {
                ctx.fillStyle = COLORS[tetrisGame.grid[y][x]];
                ctx.fillRect(
                    x * tetrisGame.cellSize + 1,
                    y * tetrisGame.cellSize + 1,
                    tetrisGame.cellSize - 2,
                    tetrisGame.cellSize - 2
                );
            }
        }
    }

    // Draw current piece
    if (tetrisGame.currentPiece) {
        ctx.fillStyle = COLORS[tetrisGame.currentType];
        for (let y = 0; y < tetrisGame.currentPiece.length; y++) {
            for (let x = 0; x < tetrisGame.currentPiece[y].length; x++) {
                if (tetrisGame.currentPiece[y][x]) {
                    ctx.fillRect(
                        (tetrisGame.currentX + x) * tetrisGame.cellSize + 1,
                        (tetrisGame.currentY + y) * tetrisGame.cellSize + 1,
                        tetrisGame.cellSize - 2,
                        tetrisGame.cellSize - 2
                    );
                }
            }
        }
    }

    // Draw game over
    if (tetrisGame.gameOver) {
        ctx.fillStyle = 'rgba(0,0,0,0.7)';
        ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height);

        ctx.fillStyle = 'white';
        ctx.font = 'bold 36px Arial';
        ctx.textAlign = 'center';
        ctx.fillText('GAME OVER', ctx.canvas.width / 2, ctx.canvas.height / 2 - 40);

        ctx.font = '20px Arial';
        ctx.fillText(`Score: ${tetrisGame.score}`, ctx.canvas.width / 2, ctx.canvas.height / 2 + 10);
        ctx.fillText(`Lines: ${tetrisGame.lines}`, ctx.canvas.width / 2, ctx.canvas.height / 2 + 40);
        ctx.fillText('Press R to Restart', ctx.canvas.width / 2, ctx.canvas.height / 2 + 80);

        const highScore = tetrisHighScore.get();
        if (highScore > 0) {
            ctx.fillText(`High Score: ${highScore}`, ctx.canvas.width / 2, ctx.canvas.height / 2 + 110);
        }
    }
}

function gameOverTetris() {
    tetrisGame.gameOver = true;
    tetrisHighScore.set(tetrisGame.score);

    const restartHandler = (e) => {
        if (e.key === 'r' || e.key === 'R') {
            window.removeEventListener('keydown', restartHandler);
            initTetris();
        }
    };
    window.addEventListener('keydown', restartHandler);
}
