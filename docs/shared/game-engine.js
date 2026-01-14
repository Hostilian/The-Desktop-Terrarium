// Shared Game Engine Utilities

class GameEngine {
    constructor(canvasId, fps = 60) {
        this.canvas = document.getElementById(canvasId);
        if (!this.canvas) {
            console.error(`Canvas #${canvasId} not found`);
            return;
        }
        this.ctx = this.canvas.getContext('2d');
        this.fps = fps;
        this.running = false;
        this.lastFrameTime = 0;
        this.frameInterval = 1000 / fps;
        this.animationId = null;
    }

    start(updateCallback, renderCallback) {
        this.running = true;
        this.updateCallback = updateCallback;
        this.renderCallback = renderCallback;
        this.lastFrameTime = performance.now();
        this.gameLoop();
    }

    stop() {
        this.running = false;
        if (this.animationId) {
            cancelAnimationFrame(this.animationId);
        }
    }

    gameLoop(currentTime = performance.now()) {
        if (!this.running) return;

        this.animationId = requestAnimationFrame((time) => this.gameLoop(time));

        const elapsed = currentTime - this.lastFrameTime;

        if (elapsed > this.frameInterval) {
            this.lastFrameTime = currentTime - (elapsed % this.frameInterval);

            // Update game state
            if (this.updateCallback) {
                this.updateCallback(elapsed / 1000); // Pass delta time in seconds
            }

            // Render
            if (this.renderCallback) {
                this.renderCallback(this.ctx);
            }
        }
    }

    clear() {
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
    }

    drawRect(x, y, width, height, color) {
        this.ctx.fillStyle = color;
        this.ctx.fillRect(x, y, width, height);
    }

    drawCircle(x, y, radius, color) {
        this.ctx.fillStyle = color;
        this.ctx.beginPath();
        this.ctx.arc(x, y, radius, 0, Math.PI * 2);
        this.ctx.fill();
    }

    drawText(text, x, y, color, font = '20px Arial') {
        this.ctx.fillStyle = color;
        this.ctx.font = font;
        this.ctx.fillText(text, x, y);
    }
}

// Input handling
class InputHandler {
    constructor() {
        this.keys = {};
        this.setupListeners();
    }

    setupListeners() {
        window.addEventListener('keydown', (e) => {
            this.keys[e.key] = true;
            this.keys[e.code] = true;
        });

        window.addEventListener('keyup', (e) => {
            this.keys[e.key] = false;
            this.keys[e.code] = false;
        });
    }

    isKeyDown(key) {
        return this.keys[key] === true;
    }
}

// Collision detection
function checkCollision(rect1, rect2) {
    return rect1.x < rect2.x + rect2.width &&
        rect1.x + rect1.width > rect2.x &&
        rect1.y < rect2.y + rect2.height &&
        rect1.y + rect1.height > rect2.y;
}

// High score management
class HighScoreManager {
    constructor(gameName) {
        this.gameName = gameName;
        this.storageKey = `terrarium_${gameName}_highscore`;
    }

    get() {
        return parseInt(localStorage.getItem(this.storageKey)) || 0;
    }

    set(score) {
        const current = this.get();
        if (score > current) {
            localStorage.setItem(this.storageKey, score.toString());
            return true; // New high score!
        }
        return false;
    }
}
