// 2048 Game
let game2048 = null;
let game2048Engine = null;
let game2048Input = null;
let game2048HighScore = null;

function init2048() {
    const container = document.getElementById('game2048Container');
    if (!container) return;

    // Create game board HTML
    container.innerHTML = `
        <div class="game-2048">
            <div class="grid-container" id="grid2048"></div>
        </div>
    `;

    game2048Input = new InputHandler();
    game2048HighScore = new HighScoreManager('2048');

    game2048 = {
        size: 4,
        grid: [],
        score: 0,
        gameOver: false,
        moved: false
    };

    initGrid2048();
    addRandomTile2048();
    addRandomTile2048();
    render2048();

    setupInput2048();
}

function stop2048() {
    // Clean up
}

function initGrid2048() {
    game2048.grid = [];
    for (let i = 0; i < game2048.size; i++) {
        game2048.grid[i] = [];
        for (let j = 0; j < game2048.size; j++) {
            game2048.grid[i][j] = 0;
        }
    }
}

function addRandomTile2048() {
    const emptyCells = [];
    for (let i = 0; i < game2048.size; i++) {
        for (let j = 0; j < game2048.size; j++) {
            if (game2048.grid[i][j] === 0) {
                emptyCells.push({ i, j });
            }
        }
    }

    if (emptyCells.length > 0) {
        const { i, j } = emptyCells[Math.floor(Math.random() * emptyCells.length)];
        game2048.grid[i][j] = Math.random() < 0.9 ? 2 : 4;
    }
}

function setupInput2048() {
    const moveHandler = (e) => {
        if (game2048.gameOver) {
            if (e.key === 'r' || e.key === 'R') {
                init2048();
            }
            return;
        }

        game2048.moved = false;

        if (e.key === 'ArrowUp') {
            e.preventDefault();
            moveUp2048();
        } else if (e.key === 'ArrowDown') {
            e.preventDefault();
            moveDown2048();
        } else if (e.key === 'ArrowLeft') {
            e.preventDefault();
            moveLeft2048();
        } else if (e.key === 'ArrowRight') {
            e.preventDefault();
            moveRight2048();
        }

        if (game2048.moved) {
            addRandomTile2048();
            render2048();
            checkGameOver2048();
        }
    };

    window.addEventListener('keydown', moveHandler);
}

function moveLeft2048() {
    for (let i = 0; i < game2048.size; i++) {
        let row = game2048.grid[i].filter(val => val !== 0);
        for (let j = 0; j < row.length - 1; j++) {
            if (row[j] === row[j + 1]) {
                row[j] *= 2;
                game2048.score += row[j];
                row.splice(j + 1, 1);
                game2048.moved = true;
            }
        }
        while (row.length < game2048.size) {
            row.push(0);
        }
        if (JSON.stringify(row) !== JSON.stringify(game2048.grid[i])) {
            game2048.moved = true;
        }
        game2048.grid[i] = row;
    }
}

function moveRight2048() {
    for (let i = 0; i < game2048.size; i++) {
        let row = game2048.grid[i].filter(val => val !== 0);
        for (let j = row.length - 1; j > 0; j--) {
            if (row[j] === row[j - 1]) {
                row[j] *= 2;
                game2048.score += row[j];
                row.splice(j - 1, 1);
                game2048.moved = true;
            }
        }
        while (row.length < game2048.size) {
            row.unshift(0);
        }
        if (JSON.stringify(row) !== JSON.stringify(game2048.grid[i])) {
            game2048.moved = true;
        }
        game2048.grid[i] = row;
    }
}

function moveUp2048() {
    for (let j = 0; j < game2048.size; j++) {
        let col = [];
        for (let i = 0; i < game2048.size; i++) {
            if (game2048.grid[i][j] !== 0) {
                col.push(game2048.grid[i][j]);
            }
        }
        for (let i = 0; i < col.length - 1; i++) {
            if (col[i] === col[i + 1]) {
                col[i] *= 2;
                game2048.score += col[i];
                col.splice(i + 1, 1);
                game2048.moved = true;
            }
        }
        for (let i = 0; i < game2048.size; i++) {
            const oldVal = game2048.grid[i][j];
            game2048.grid[i][j] = col[i] || 0;
            if (oldVal !== game2048.grid[i][j]) {
                game2048.moved = true;
            }
        }
    }
}

function moveDown2048() {
    for (let j = 0; j < game2048.size; j++) {
        let col = [];
        for (let i = 0; i < game2048.size; i++) {
            if (game2048.grid[i][j] !== 0) {
                col.push(game2048.grid[i][j]);
            }
        }
        for (let i = col.length - 1; i > 0; i--) {
            if (col[i] === col[i - 1]) {
                col[i] *= 2;
                game2048.score += col[i];
                col.splice(i - 1, 1);
                game2048.moved = true;
            }
        }
        for (let i = game2048.size - 1; i >= 0; i--) {
            const oldVal = game2048.grid[i][j];
            game2048.grid[i][j] = col.pop() || 0;
            if (oldVal !== game2048.grid[i][j]) {
                game2048.moved = true;
            }
        }
    }
}

function checkGameOver2048() {
    // Check for empty cells
    for (let i = 0; i < game2048.size; i++) {
        for (let j = 0; j < game2048.size; j++) {
            if (game2048.grid[i][j] === 0) return;
        }
    }

    // Check for possible merges
    for (let i = 0; i < game2048.size; i++) {
        for (let j = 0; j < game2048.size; j++) {
            if (j < game2048.size - 1 && game2048.grid[i][j] === game2048.grid[i][j + 1]) return;
            if (i < game2048.size - 1 && game2048.grid[i][j] === game2048.grid[i + 1][j]) return;
        }
    }

    game2048.gameOver = true;
    game2048HighScore.set(game2048.score);
}

function render2048() {
    const grid = document.getElementById('grid2048');
    if (!grid) return;

    document.getElementById('2048Score').textContent = game2048.score;

    grid.innerHTML = '';
    grid.style.cssText = `
        display: grid;
        grid-template-columns: repeat(4, 100px);
        grid-gap: 10px;
        background: #bbada0;
        padding: 10px;
        border-radius: 10px;
        margin: 20px auto;
        width: fit-content;
    `;

    for (let i = 0; i < game2048.size; i++) {
        for (let j = 0; j < game2048.size; j++) {
            const tile = document.createElement('div');
            const value = game2048.grid[i][j];
            tile.className = 'tile';
            tile.textContent = value || '';

            const colors = {
                0: '#cdc1b4', 2: '#eee4da', 4: '#ede0c8', 8: '#f2b179',
                16: '#f59563', 32: '#f67c5f', 64: '#f65e3b', 128: '#edcf72',
                256: '#edcc61', 512: '#edc850', 1024: '#edc53f', 2048: '#edc22e'
            };

            tile.style.cssText = `
                width: 100px;
                height: 100px;
                background: ${colors[value] || '#3c3a32'};
                color: ${value > 4 ? '#f9f6f2' : '#776e65'};
                display: flex;
                align-items: center;
                justify-content: center;
                font-size: ${value > 512 ? '30px' : '40px'};
                font-weight: bold;
                border-radius: 5px;
                transition: all 0.15s;
            `;

            grid.appendChild(tile);
        }
    }

    if (game2048.gameOver) {
        const overlay = document.createElement('div');
        overlay.style.cssText = `
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: rgba(0,0,0,0.7);
            color: white;
            padding: 30px;
            border-radius: 10px;
            text-align: center;
            font-size: 24px;
        `;
        overlay.innerHTML = `
            <div>GAME OVER</div>
            <div style="margin-top: 10px;">Score: ${game2048.score}</div>
            <div style="margin-top: 10px; font-size: 16px;">Press R to Restart</div>
        `;
        document.getElementById('game2048Container').style.position = 'relative';
        document.getElementById('game2048Container').appendChild(overlay);
    }
}
