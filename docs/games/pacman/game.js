// Pacman Game
let pacmanGame = null;
let pacmanEngine = null;
let pacmanInput = null;
let pacmanHighScore = null;

const MAZE_WIDTH = 28;
const MAZE_HEIGHT = 31;
const CELL_SIZE = 20;

// Simplified maze layout (1 = wall, 0 = empty, 2 = pellet, 3 = power pellet)
const MAZE_LAYOUT = [
    [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
    [1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1],
    [1, 2, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 2, 1],
    [1, 3, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 3, 1],
    [1, 2, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 2, 1],
    [1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1],
    [1, 2, 1, 1, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 2, 1],
    [1, 2, 1, 1, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 2, 1],
    [1, 2, 2, 2, 2, 2, 2, 1, 1, 2, 2, 2, 2, 1, 1, 2, 2, 2, 2, 1, 1, 2, 2, 2, 2, 2, 2, 1],
    [1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1],
    [1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1],
    [1, 1, 1, 1, 1, 1, 2, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2, 1, 1, 1, 1, 1, 1],
    [1, 1, 1, 1, 1, 1, 2, 1, 1, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 1, 1, 2, 1, 1, 1, 1, 1, 1],
    [1, 1, 1, 1, 1, 1, 2, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 2, 1, 1, 1, 1, 1, 1],
    [0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0],
    [1, 1, 1, 1, 1, 1, 2, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 2, 1, 1, 1, 1, 1, 1],
    [1, 1, 1, 1, 1, 1, 2, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 2, 1, 1, 1, 1, 1, 1],
    [1, 1, 1, 1, 1, 1, 2, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2, 1, 1, 1, 1, 1, 1],
    [1, 1, 1, 1, 1, 1, 2, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 2, 1, 1, 1, 1, 1, 1],
    [1, 1, 1, 1, 1, 1, 2, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 2, 1, 1, 1, 1, 1, 1],
    [1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1],
    [1, 2, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 2, 1],
    [1, 2, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 2, 1],
    [1, 3, 2, 2, 1, 1, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 1, 1, 2, 2, 3, 1],
    [1, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1, 1, 1],
    [1, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1, 1, 1],
    [1, 2, 2, 2, 2, 2, 2, 1, 1, 2, 2, 2, 2, 1, 1, 2, 2, 2, 2, 1, 1, 2, 2, 2, 2, 2, 2, 1],
    [1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1],
    [1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1],
    [1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1],
    [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1]
];

function initPacman() {
    const canvas = document.getElementById('pacmanCanvas');
    if (!canvas) return;

    canvas.width = MAZE_WIDTH * CELL_SIZE;
    canvas.height = MAZE_HEIGHT * CELL_SIZE;

    pacmanEngine = new GameEngine('pacmanCanvas', 8);
    pacmanInput = new InputHandler();
    pacmanHighScore = new HighScoreManager('pacman');

    // Copy maze layout
    const maze = MAZE_LAYOUT.map(row => [...row]);

    pacmanGame = {
        maze: maze,
        pacman: { x: 14, y: 23, dir: 0, nextDir: 0, mouthOpen: 0 },
        ghosts: [
            { x: 13, y: 14, color: '#ff0000', mode: 'chase' },
            { x: 14, y: 14, color: '#ffb8ff', mode: 'chase' },
            { x: 13, y: 15, color: '#00ffff', mode: 'chase' },
            { x: 14, y: 15, color: '#ffb851', mode: 'chase' }
        ],
        score: 0,
        lives: 3,
        powerMode: false,
        powerTimer: 0,
        pelletsLeft: 0,
        gameOver: false,
        won: false,
        moveTimer: 0,
        moveInterval: 0.125
    };

    // Count pellets
    pacmanGame.pelletsLeft = maze.flat().filter(cell => cell === 2 || cell === 3).length;

    pacmanEngine.start(updatePacman, renderPacman);
}

function stopPacman() {
    if (pacmanEngine) {
        pacmanEngine.stop();
    }
}

function updatePacman(deltaTime) {
    if (pacmanGame.gameOver || pacmanGame.won) return;

    pacmanGame.moveTimer += deltaTime;

    // Input handling
    if (pacmanInput.isKeyDown('ArrowUp')) pacmanGame.pacman.nextDir = 3;
    if (pacmanInput.isKeyDown('ArrowDown')) pacmanGame.pacman.nextDir = 1;
    if (pacmanInput.isKeyDown('ArrowLeft')) pacmanGame.pacman.nextDir = 2;
    if (pacmanInput.isKeyDown('ArrowRight')) pacmanGame.pacman.nextDir = 0;

    if (pacmanGame.moveTimer >= pacmanGame.moveInterval) {
        pacmanGame.moveTimer = 0;

        // Try to move in next direction
        const nextPos = getNextPosition(pacmanGame.pacman.x, pacmanGame.pacman.y, pacmanGame.pacman.nextDir);
        if (canMove(nextPos.x, nextPos.y)) {
            pacmanGame.pacman.dir = pacmanGame.pacman.nextDir;
        }

        // Move pacman
        const newPos = getNextPosition(pacmanGame.pacman.x, pacmanGame.pacman.y, pacmanGame.pacman.dir);
        if (canMove(newPos.x, newPos.y)) {
            pacmanGame.pacman.x = newPos.x;
            pacmanGame.pacman.y = newPos.y;

            // Collect pellet
            const cell = pacmanGame.maze[pacmanGame.pacman.y][pacmanGame.pacman.x];
            if (cell === 2) {
                pacmanGame.maze[pacmanGame.pacman.y][pacmanGame.pacman.x] = 0;
                pacmanGame.score += 10;
                pacmanGame.pelletsLeft--;
                document.getElementById('pacmanScore').textContent = pacmanGame.score;
            } else if (cell === 3) {
                pacmanGame.maze[pacmanGame.pacman.y][pacmanGame.pacman.x] = 0;
                pacmanGame.score += 50;
                pacmanGame.pelletsLeft--;
                pacmanGame.powerMode = true;
                pacmanGame.powerTimer = 7;
                document.getElementById('pacmanScore').textContent = pacmanGame.score;
            }

            // Check win condition
            if (pacmanGame.pelletsLeft === 0) {
                pacmanGame.won = true;
            }
        }

        // Move ghosts (simple AI)
        pacmanGame.ghosts.forEach(ghost => {
            const directions = [0, 1, 2, 3];
            const validDirs = directions.filter(dir => {
                const pos = getNextPosition(ghost.x, ghost.y, dir);
                return canMove(pos.x, pos.y);
            });

            if (validDirs.length > 0) {
                // Chase pacman or flee if in power mode
                let bestDir = validDirs[0];
                let bestDist = Infinity;

                validDirs.forEach(dir => {
                    const pos = getNextPosition(ghost.x, ghost.y, dir);
                    const dist = Math.abs(pos.x - pacmanGame.pacman.x) + Math.abs(pos.y - pacmanGame.pacman.y);
                    if (pacmanGame.powerMode ? dist > bestDist : dist < bestDist) {
                        bestDist = dist;
                        bestDir = dir;
                    }
                });

                const newPos = getNextPosition(ghost.x, ghost.y, bestDir);
                ghost.x = newPos.x;
                ghost.y = newPos.y;
            }
        });

        // Check ghost collisions
        pacmanGame.ghosts.forEach(ghost => {
            if (ghost.x === pacmanGame.pacman.x && ghost.y === pacmanGame.pacman.y) {
                if (pacmanGame.powerMode) {
                    ghost.x = 14;
                    ghost.y = 14;
                    pacmanGame.score += 200;
                    document.getElementById('pacmanScore').textContent = pacmanGame.score;
                } else {
                    pacmanGame.lives--;
                    if (pacmanGame.lives <= 0) {
                        gameOverPacman();
                    } else {
                        pacmanGame.pacman.x = 14;
                        pacmanGame.pacman.y = 23;
                    }
                }
            }
        });

        pacmanGame.pacman.mouthOpen = (pacmanGame.pacman.mouthOpen + 1) % 3;
    }

    // Power mode timer
    if (pacmanGame.powerMode) {
        pacmanGame.powerTimer -= deltaTime;
        if (pacmanGame.powerTimer <= 0) {
            pacmanGame.powerMode = false;
        }
    }
}

function getNextPosition(x, y, dir) {
    const dirs = [[1, 0], [0, 1], [-1, 0], [0, -1]];
    let newX = x + dirs[dir][0];
    let newY = y + dirs[dir][1];

    // Wrap around
    if (newX < 0) newX = MAZE_WIDTH - 1;
    if (newX >= MAZE_WIDTH) newX = 0;
    if (newY < 0) newY = MAZE_HEIGHT - 1;
    if (newY >= MAZE_HEIGHT) newY = 0;

    return { x: newX, y: newY };
}

function canMove(x, y) {
    return pacmanGame.maze[y] && pacmanGame.maze[y][x] !== 1;
}

function renderPacman(ctx) {
    ctx.fillStyle = '#000';
    ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height);

    // Draw maze
    for (let y = 0; y < MAZE_HEIGHT; y++) {
        for (let x = 0; x < MAZE_WIDTH; x++) {
            const cell = pacmanGame.maze[y][x];
            if (cell === 1) {
                ctx.fillStyle = '#2121de';
                ctx.fillRect(x * CELL_SIZE, y * CELL_SIZE, CELL_SIZE, CELL_SIZE);
            } else if (cell === 2) {
                ctx.fillStyle = '#ffb897';
                ctx.fillRect(x * CELL_SIZE + 8, y * CELL_SIZE + 8, 4, 4);
            } else if (cell === 3) {
                ctx.fillStyle = '#ffb897';
                ctx.fillRect(x * CELL_SIZE + 6, y * CELL_SIZE + 6, 8, 8);
            }
        }
    }

    // Draw pacman
    ctx.fillStyle = '#ffff00';
    ctx.beginPath();
    const px = pacmanGame.pacman.x * CELL_SIZE + CELL_SIZE / 2;
    const py = pacmanGame.pacman.y * CELL_SIZE + CELL_SIZE / 2;
    const mouthAngle = pacmanGame.pacman.mouthOpen === 1 ? 0.3 : 0.1;
    const rotations = [0, Math.PI / 2, Math.PI, -Math.PI / 2];
    const startAngle = rotations[pacmanGame.pacman.dir] + mouthAngle;
    const endAngle = rotations[pacmanGame.pacman.dir] + Math.PI * 2 - mouthAngle;
    ctx.arc(px, py, CELL_SIZE / 2 - 2, startAngle, endAngle);
    ctx.lineTo(px, py);
    ctx.fill();

    // Draw ghosts
    pacmanGame.ghosts.forEach(ghost => {
        ctx.fillStyle = pacmanGame.powerMode ? '#0000ff' : ghost.color;
        const gx = ghost.x * CELL_SIZE + CELL_SIZE / 2;
        const gy = ghost.y * CELL_SIZE + CELL_SIZE / 2;
        ctx.beginPath();
        ctx.arc(gx, gy, CELL_SIZE / 2 - 2, Math.PI, 0);
        ctx.lineTo(gx + CELL_SIZE / 2 - 2, gy + CELL_SIZE / 2);
        ctx.lineTo(gx, gy + CELL_SIZE / 2 - 4);
        ctx.lineTo(gx - CELL_SIZE / 2 + 2, gy + CELL_SIZE / 2);
        ctx.closePath();
        ctx.fill();

        // Eyes
        if (!pacmanGame.powerMode) {
            ctx.fillStyle = '#fff';
            ctx.fillRect(gx - 6, gy - 4, 4, 6);
            ctx.fillRect(gx + 2, gy - 4, 4, 6);
            ctx.fillStyle = '#000';
            ctx.fillRect(gx - 4, gy - 2, 2, 3);
            ctx.fillRect(gx + 4, gy - 2, 2, 3);
        }
    });

    // Draw lives
    ctx.fillStyle = '#fff';
    ctx.font = '16px Arial';
    ctx.fillText(`Lives: ${pacmanGame.lives}`, 10, ctx.canvas.height - 10);

    // Game over
    if (pacmanGame.gameOver || pacmanGame.won) {
        ctx.fillStyle = 'rgba(0,0,0,0.7)';
        ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height);

        ctx.fillStyle = 'white';
        ctx.font = 'bold 36px Arial';
        ctx.textAlign = 'center';
        ctx.fillText(pacmanGame.won ? 'YOU WIN!' : 'GAME OVER', ctx.canvas.width / 2, ctx.canvas.height / 2 - 40);

        ctx.font = '20px Arial';
        ctx.fillText(`Score: ${pacmanGame.score}`, ctx.canvas.width / 2, ctx.canvas.height / 2 + 10);
        ctx.fillText('Press R to Restart', ctx.canvas.width / 2, ctx.canvas.height / 2 + 50);

        const highScore = pacmanHighScore.get();
        if (highScore > 0) {
            ctx.fillText(`High Score: ${highScore}`, ctx.canvas.width / 2, ctx.canvas.height / 2 + 90);
        }
    }
}

function gameOverPacman() {
    pacmanGame.gameOver = true;
    pacmanHighScore.set(pacmanGame.score);

    const restartHandler = (e) => {
        if (e.key === 'r' || e.key === 'R') {
            window.removeEventListener('keydown', restartHandler);
            initPacman();
        }
    };
    window.addEventListener('keydown', restartHandler);
}
