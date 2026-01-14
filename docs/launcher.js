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
    const launcherScreen = document.getElementById('launcherScreen');
    const gameScreen = document.getElementById(`${gameName}Screen`);
    
    if (!gameScreen) {
        console.error(`Game screen for ${gameName} not found`);
        return;
    }
    
    // Fade out launcher
    launcherScreen.style.opacity = '0';
    launcherScreen.style.transform = 'scale(0.95)';
    
    setTimeout(() => {
        launcherScreen.classList.remove('active');
        gameScreen.classList.add('active');
        
        // Initialize the game
        if (window[`init${capitalize(gameName)}`]) {
            window[`init${capitalize(gameName)}`]();
        }
    }, 500);
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
