// Snake Game
let snakeGame = null;
let snakeEngine = null;
let snakeInput = null;
let snakeHighScore = null;

function initSnake() {
    const canvas = document.getElementById('snakeCanvas');
    if (!canvas) return;

    canvas.width = 600;
    canvas.height = 600;

    snakeEngine = new GameEngine('snakeCanvas', 10);
    snakeInput = new InputHandler();
    snakeHighScore = new HighScoreManager('snake');

    snakeGame = {
        gridSize: 20,
        cellSize: 30,
        snake: [{ x: 10, y: 10 }],
        direction: { x: 1, y: 0 },
        nextDirection: { x: 1, y: 0 },
        food: null,
        score: 0,
        gameOver: false
    };

    spawnFood();
    snakeEngine.start(updateSnake, renderSnake);
}

function stopSnake() {
    if (snakeEngine) {
        snakeEngine.stop();
    }
}

function spawnFood() {
    snakeGame.food = {
        x: Math.floor(Math.random() * snakeGame.gridSize),
        y: Math.floor(Math.random() * snakeGame.gridSize)
    };

    // Make sure food doesn't spawn on snake
    const onSnake = snakeGame.snake.some(segment =>
        segment.x === snakeGame.food.x && segment.y === snakeGame.food.y
    );

    if (onSnake) {
        spawnFood();
    }
}

function updateSnake(deltaTime) {
    if (snakeGame.gameOver) return;

    // Input handling
    if (snakeInput.isKeyDown('ArrowUp') && snakeGame.direction.y === 0) {
        snakeGame.nextDirection = { x: 0, y: -1 };
    } else if (snakeInput.isKeyDown('ArrowDown') && snakeGame.direction.y === 0) {
        snakeGame.nextDirection = { x: 0, y: 1 };
    } else if (snakeInput.isKeyDown('ArrowLeft') && snakeGame.direction.x === 0) {
        snakeGame.nextDirection = { x: -1, y: 0 };
    } else if (snakeInput.isKeyDown('ArrowRight') && snakeGame.direction.x === 0) {
        snakeGame.nextDirection = { x: 1, y: 0 };
    }

    snakeGame.direction = snakeGame.nextDirection;

    // Move snake
    const head = { ...snakeGame.snake[0] };
    head.x += snakeGame.direction.x;
    head.y += snakeGame.direction.y;

    // Check wall collision
    if (head.x < 0 || head.x >= snakeGame.gridSize ||
        head.y < 0 || head.y >= snakeGame.gridSize) {
        gameOverSnake();
        return;
    }

    // Check self collision
    const hitSelf = snakeGame.snake.some(segment =>
        segment.x === head.x && segment.y === head.y
    );

    if (hitSelf) {
        gameOverSnake();
        return;
    }

    snakeGame.snake.unshift(head);

    // Check food collision
    if (head.x === snakeGame.food.x && head.y === snakeGame.food.y) {
        snakeGame.score += 10;
        document.getElementById('snakeScore').textContent = snakeGame.score;
        spawnFood();
    } else {
        snakeGame.snake.pop();
    }
}

function renderSnake(ctx) {
    // Clear canvas
    ctx.fillStyle = '#1a202c';
    ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height);

    // Draw grid
    ctx.strokeStyle = 'rgba(255,255,255,0.05)';
    ctx.lineWidth = 1;
    for (let i = 0; i <= snakeGame.gridSize; i++) {
        const pos = i * snakeGame.cellSize;
        ctx.beginPath();
        ctx.moveTo(pos, 0);
        ctx.lineTo(pos, ctx.canvas.height);
        ctx.stroke();
        ctx.beginPath();
        ctx.moveTo(0, pos);
        ctx.lineTo(ctx.canvas.width, pos);
        ctx.stroke();
    }

    // Draw food
    ctx.fillStyle = '#f56565';
    ctx.shadowBlur = 15;
    ctx.shadowColor = '#f56565';
    ctx.fillRect(
        snakeGame.food.x * snakeGame.cellSize + 2,
        snakeGame.food.y * snakeGame.cellSize + 2,
        snakeGame.cellSize - 4,
        snakeGame.cellSize - 4
    );
    ctx.shadowBlur = 0;

    // Draw snake
    snakeGame.snake.forEach((segment, index) => {
        const gradient = ctx.createLinearGradient(
            segment.x * snakeGame.cellSize,
            segment.y * snakeGame.cellSize,
            (segment.x + 1) * snakeGame.cellSize,
            (segment.y + 1) * snakeGame.cellSize
        );
        gradient.addColorStop(0, '#48bb78');
        gradient.addColorStop(1, '#2f855a');

        ctx.fillStyle = index === 0 ? '#38a169' : gradient;
        ctx.fillRect(
            segment.x * snakeGame.cellSize + 1,
            segment.y * snakeGame.cellSize + 1,
            snakeGame.cellSize - 2,
            snakeGame.cellSize - 2
        );
    });

    // Draw game over
    if (snakeGame.gameOver) {
        ctx.fillStyle = 'rgba(0,0,0,0.7)';
        ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height);

        ctx.fillStyle = 'white';
        ctx.font = 'bold 48px Arial';
        ctx.textAlign = 'center';
        ctx.fillText('GAME OVER', ctx.canvas.width / 2, ctx.canvas.height / 2 - 40);

        ctx.font = '24px Arial';
        ctx.fillText(`Score: ${snakeGame.score}`, ctx.canvas.width / 2, ctx.canvas.height / 2 + 10);
        ctx.fillText('Press R to Restart', ctx.canvas.width / 2, ctx.canvas.height / 2 + 50);

        const highScore = snakeHighScore.get();
        if (highScore > 0) {
            ctx.fillText(`High Score: ${highScore}`, ctx.canvas.width / 2, ctx.canvas.height / 2 + 90);
        }
    }
}

function gameOverSnake() {
    snakeGame.gameOver = true;
    snakeHighScore.set(snakeGame.score);

    // Listen for restart
    const restartHandler = (e) => {
        if (e.key === 'r' || e.key === 'R') {
            window.removeEventListener('keydown', restartHandler);
            initSnake();
        }
    };
    window.addEventListener('keydown', restartHandler);
}
