// ============================================
// Premium Animations with GSAP
// ============================================

// Initialize particles background
particlesJS('particles-js', {
    particles: {
        number: { value: 80, density: { enable: true, value_area: 800 } },
        color: { value: '#ffffff' },
        shape: { type: 'circle' },
        opacity: { value: 0.5, random: true },
        size: { value: 3, random: true },
        line_linked: {
            enable: true,
            distance: 150,
            color: '#ffffff',
            opacity: 0.4,
            width: 1
        },
        move: {
            enable: true,
            speed: 2,
            direction: 'none',
            random: false,
            straight: false,
            out_mode: 'out',
            bounce: false
        }
    },
    interactivity: {
        detect_on: 'canvas',
        events: {
            onhover: { enable: true, mode: 'repulse' },
            onclick: { enable: true, mode: 'push' }
        }
    }
});

// Hero animations
gsap.from('.hero-title', {
    duration: 1.2,
    y: 100,
    opacity: 0,
    ease: 'power4.out',
    delay: 0.3
});

gsap.from('.hero-subtitle', {
    duration: 1,
    y: 50,
    opacity: 0,
    ease: 'power3.out',
    delay: 0.6
});

gsap.from('.cta-button', {
    duration: 0.8,
    scale: 0,
    opacity: 0,
    ease: 'back.out(1.7)',
    delay: 0.9
});

// Scroll to games section
function scrollToGames() {
    gsap.to(window, {
        duration: 1.5,
        scrollTo: '#gamesSection',
        ease: 'power4.inOut'
    });
}

// Game cards entrance animation
gsap.registerPlugin(ScrollTrigger);

gsap.from('.section-title', {
    scrollTrigger: {
        trigger: '.section-title',
        start: 'top 80%'
    },
    duration: 1,
    y: 50,
    opacity: 0,
    ease: 'power3.out'
});

gsap.from('.game-card', {
    scrollTrigger: {
        trigger: '.games-grid',
        start: 'top 70%'
    },
    duration: 0.8,
    y: 100,
    opacity: 0,
    stagger: 0.2,
    ease: 'power3.out'
});

// 3D tilt effect for cards
document.querySelectorAll('[data-tilt]').forEach(card => {
    card.addEventListener('mousemove', (e) => {
        const rect = card.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;

        const centerX = rect.width / 2;
        const centerY = rect.height / 2;

        const rotateX = (y - centerY) / 10;
        const rotateY = (centerX - x) / 10;

        gsap.to(card, {
            duration: 0.3,
            rotateX: rotateX,
            rotateY: rotateY,
            transformPerspective: 1000,
            ease: 'power2.out'
        });
    });

    card.addEventListener('mouseleave', () => {
        gsap.to(card, {
            duration: 0.5,
            rotateX: 0,
            rotateY: 0,
            ease: 'power2.out'
        });
    });
});

// Launch game with animation
function launchGame(gameName) {
    const container = document.getElementById('gameContainer');
    const gameScreen = document.getElementById(`${gameName}Screen`);
    const gameTitle = document.getElementById('gameTitle');

    // Set title
    const titles = {
        'snake': '🐍 Snake',
        '2048': '🎯 2048',
        'trex': '🦖 T-Rex Runner'
    };
    gameTitle.textContent = titles[gameName] || gameName;

    // Animate out games section
    gsap.to('.games-section', {
        duration: 0.5,
        opacity: 0,
        scale: 0.95,
        ease: 'power3.in',
        onComplete: () => {
            container.classList.remove('hidden');
            gameScreen.classList.add('active');

            // Animate in game container
            gsap.from(container, {
                duration: 0.6,
                scale: 0.9,
                opacity: 0,
                ease: 'power3.out'
            });

            // Initialize game
            if (window[`init${capitalize(gameName)}`]) {
                setTimeout(() => {
                    window[`init${capitalize(gameName)}`]();
                }, 300);
            }
        }
    });
}

// Back to menu
function backToMenu() {
    const container = document.getElementById('gameContainer');
    const activeGame = document.querySelector('.game-screen.active');

    if (activeGame) {
        const gameName = activeGame.id.replace('Screen', '');
        if (window[`stop${capitalize(gameName)}`]) {
            window[`stop${capitalize(gameName)}`]();
        }
        activeGame.classList.remove('active');
    }

    // Animate out game container
    gsap.to(container, {
        duration: 0.5,
        scale: 0.9,
        opacity: 0,
        ease: 'power3.in',
        onComplete: () => {
            container.classList.add('hidden');

            // Animate in games section
            gsap.to('.games-section', {
                duration: 0.6,
                opacity: 1,
                scale: 1,
                ease: 'power3.out'
            });
        }
    });
}

function capitalize(str) {
    return str.charAt(0).toUpperCase() + str.slice(1);
}

// Fullscreen toggle
function toggleFullscreen() {
    if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen();
    } else {
        document.exitFullscreen();
    }
}

// Smooth scroll
gsap.registerPlugin(ScrollToPlugin);
