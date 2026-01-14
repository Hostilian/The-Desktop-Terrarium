// T-Rex Runner Game
let trexGame = null;
let trexEngine = null;
let trexInput = null;
let trexHighScore = null;

function initTrex() {
    const canvas = document.getElementById('trexCanvas');
    if (!canvas) return;

    canvas.width = 800;
    canvas.height = 400;

    trexEngine = new GameEngine('trexCanvas', 60);
    trexInput = new InputHandler();
    trexHighScore = new HighScoreManager('trex');

    trexGame = {
        player: {
            x: 100,
            y: 300,
            width: 40,
            height: 50,
            velocityY: 0,
            jumping: false
        },
        obstacles: [],
        score: 0,
        gameSpeed: 5,
        gravity: 0.6,
        jumpPower: -12,
        gameOver: false,
        frameCount: 0
    };

    trexEngine.start(updateTrex, renderTrex);
}

function stopTrex() {
    if (trexEngine) {
        trexEngine.stop();
    }
}

function updateTrex(deltaTime) {
    if (trexGame.gameOver) {
        if (trexInput.isKeyDown(' ') || trexInput.isKeyDown('Space')) {
            initTrex();
        }
        return;
    }

    trexGame.frameCount++;
    trexGame.score = Math.floor(trexGame.frameCount / 10);
    document.getElementById('trexScore').textContent = trexGame.score;

    // Increase difficulty over time
    if (trexGame.frameCount % 500 === 0) {
        trexGame.gameSpeed += 0.5;
    }

    // Jump
    if ((trexInput.isKeyDown(' ') || trexInput.isKeyDown('Space') || trexInput.isKeyDown('ArrowUp')) && !trexGame.player.jumping) {
        trexGame.player.velocityY = trexGame.jumpPower;
        trexGame.player.jumping = true;
    }

    // Apply gravity
    trexGame.player.velocityY += trexGame.gravity;
    trexGame.player.y += trexGame.player.velocityY;

    // Ground collision
    if (trexGame.player.y >= 300) {
        trexGame.player.y = 300;
        trexGame.player.velocityY = 0;
        trexGame.player.jumping = false;
    }

    // Spawn obstacles
    if (trexGame.frameCount % 100 === 0) {
        const height = 30 + Math.random() * 30;
        trexGame.obstacles.push({
            x: 800,
            y: 350 - height,
            width: 20,
            height: height
        });
    }

    // Update obstacles
    for (let i = trexGame.obstacles.length - 1; i >= 0; i--) {
        trexGame.obstacles[i].x -= trexGame.gameSpeed;

        // Remove off-screen obstacles
        if (trexGame.obstacles[i].x + trexGame.obstacles[i].width < 0) {
            trexGame.obstacles.splice(i, 1);
            continue;
        }

        // Check collision
        if (checkCollision(trexGame.player, trexGame.obstacles[i])) {
            gameOverTrex();
        }
    }
}

function renderTrex(ctx) {
    // Clear canvas
    ctx.fillStyle = '#f7f7f7';
    ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height);

    // Draw ground
    ctx.fillStyle = '#535353';
    ctx.fillRect(0, 350, ctx.canvas.width, 2);

    // Draw player (T-Rex)
    ctx.fillStyle = '#535353';
    ctx.fillRect(trexGame.player.x, trexGame.player.y, trexGame.player.width, trexGame.player.height);

    // Draw eye
    ctx.fillStyle = '#ffffff';
    ctx.fillRect(trexGame.player.x + 25, trexGame.player.y + 10, 5, 5);

    // Draw obstacles (cacti)
    ctx.fillStyle = '#2d8659';
    trexGame.obstacles.forEach(obstacle => {
        ctx.fillRect(obstacle.x, obstacle.y, obstacle.width, obstacle.height);
    });

    // Draw game over
    if (trexGame.gameOver) {
        ctx.fillStyle = 'rgba(0,0,0,0.5)';
        ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height);

        ctx.fillStyle = 'white';
        ctx.font = 'bold 48px Arial';
        ctx.textAlign = 'center';
        ctx.fillText('GAME OVER', ctx.canvas.width / 2, ctx.canvas.height / 2 - 40);

        ctx.font = '24px Arial';
        ctx.fillText(`Score: ${trexGame.score}`, ctx.canvas.width / 2, ctx.canvas.height / 2 + 10);
        ctx.fillText('Press Space to Restart', ctx.canvas.width / 2, ctx.canvas.height / 2 + 50);

        const highScore = trexHighScore.get();
        if (highScore > 0) {
            ctx.fillText(`High Score: ${highScore}`, ctx.canvas.width / 2, ctx.canvas.height / 2 + 90);
        }
    }
}

function gameOverTrex() {
    trexGame.gameOver = true;
    trexHighScore.set(trexGame.score);
}
