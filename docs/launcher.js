// Launcher Logic
document.addEventListener('DOMContentLoaded', () => {
    const gameCards = document.querySelectorAll('.game-card');
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    const carousel = document.querySelector('.game-cards');

    // Carousel Navigation
    if (prevBtn && nextBtn && carousel) {
        prevBtn.addEventListener('click', () => {
            carousel.scrollBy({ left: -320, behavior: 'smooth' });
        });

        nextBtn.addEventListener('click', () => {
            carousel.scrollBy({ left: 320, behavior: 'smooth' });
        });
    }

    // Game Card Click Handlers
    gameCards.forEach(card => {
        const playBtn = card.querySelector('.play-btn');
        const gameName = card.dataset.game;

        playBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            if (playBtn.textContent === 'Coming Soon') {
                return;
            }
            launchGame(gameName);
        });
    });
});

function launchGame(gameName) {
    const gameContainer = document.getElementById('gameContainer');
    const gamesSection = document.getElementById('gamesSection');
    const gameScreen = document.getElementById(`${gameName}Screen`);
    const gameTitle = document.getElementById('gameTitle');

    if (!gameScreen) {
        console.error(`Game screen for ${gameName} not found`);
        return;
    }

    // Hide all game screens
    document.querySelectorAll('.game-screen').forEach(screen => {
        screen.classList.remove('active');
    });

    // Set game title
    const titles = {
        'snake': 'Snake',
        '2048': '2048',
        'trex': 'T-Rex Runner',
        'tetris': 'Tetris',
        'pacman': 'Pacman'
    };
    gameTitle.textContent = titles[gameName] || gameName;

    // Show game container and specific game screen
    gameContainer.classList.remove('hidden');
    gameScreen.classList.add('active');

    // Initialize the game
    if (window[`init${capitalize(gameName)}`]) {
        window[`init${capitalize(gameName)}`]();
    }
}

function backToLauncher() {
    const activeGameScreen = document.querySelector('.game-screen.active');
    const launcherScreen = document.getElementById('launcherScreen');

    if (activeGameScreen) {
        // Stop the active game
        const gameName = activeGameScreen.id.replace('Screen', '');
        if (window[`stop${capitalize(gameName)}`]) {
            window[`stop${capitalize(gameName)}`]();
        }

        activeGameScreen.style.opacity = '0';
        activeGameScreen.style.transform = 'translateY(50px)';

        setTimeout(() => {
            activeGameScreen.classList.remove('active');
            launcherScreen.classList.add('active');
        }, 500);
    }
}

function capitalize(str) {
    return str.charAt(0).toUpperCase() + str.slice(1);
}
