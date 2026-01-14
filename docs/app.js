// Smooth scroll for anchor links
document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (!target) return;
        target.scrollIntoView({ behavior: 'smooth' });
    });
});

// Add more floating particles dynamically
const particlesContainer = document.querySelector('.floating-particles');
if (particlesContainer) {
    for (let i = 0; i < 10; i++) {
        const particle = document.createElement('div');
        particle.className = 'particle';
        particle.style.left = `${Math.random() * 100}%`;
        particle.style.top = `${Math.random() * 100}%`;
        particle.style.animationDelay = `${Math.random() * 5}s`;
        particle.style.animationDuration = `${10 + Math.random() * 10}s`;
        particlesContainer.appendChild(particle);
    }
}

// IntersectionObserver for scroll-triggered animations
const observerOptions = {
    root: null,
    rootMargin: '0px',
    threshold: 0.1
};

const animateOnScroll = (entries, observer) => {
    entries.forEach((entry, index) => {
        if (entry.isIntersecting) {
            // Add staggered delay for multiple cards
            const delay = entry.target.dataset.delay || 0;
            setTimeout(() => {
                entry.target.classList.add('visible');
            }, delay * 100);
            
            // Optionally stop observing after animation
            // observer.unobserve(entry.target);
        }
    });
};

const scrollObserver = new IntersectionObserver(animateOnScroll, observerOptions);

// Observe feature cards with staggered delays
document.querySelectorAll('.feature-card').forEach((card, index) => {
    card.dataset.delay = index;
    scrollObserver.observe(card);
});

// Observe step cards
document.querySelectorAll('.step-card').forEach((card, index) => {
    card.dataset.delay = index;
    scrollObserver.observe(card);
});

// Observe section titles
document.querySelectorAll('.section-title').forEach((title) => {
    title.classList.add('fade-in-up');
    scrollObserver.observe(title);
});

// Observe highlight items
document.querySelectorAll('.highlight-item').forEach((item, index) => {
    item.classList.add('fade-in-up');
    item.dataset.delay = index;
    scrollObserver.observe(item);
});

// Observe creature cards
document.querySelectorAll('.creature-card').forEach((card, index) => {
    card.classList.add('fade-in-up');
    card.dataset.delay = index;
    scrollObserver.observe(card);
});

// Observe stat items
document.querySelectorAll('.stat-item').forEach((item, index) => {
    item.classList.add('fade-in-up');
    item.dataset.delay = index;
    scrollObserver.observe(item);
});

// Navbar background on scroll
const nav = document.querySelector('nav');
if (nav) {
    window.addEventListener('scroll', () => {
        if (window.scrollY > 100) {
            nav.style.background = 'rgba(15, 15, 15, 0.98)';
        } else {
            nav.style.background = 'rgba(15, 15, 15, 0.9)';
        }
    });
}

// Terrarium entity animations with random timing variations
document.querySelectorAll('.terrarium-strip .entity').forEach((entity) => {
    const randomDelay = Math.random() * 2;
    const randomDuration = 2 + Math.random() * 3;
    entity.style.animationDelay = `${randomDelay}s`;
    entity.style.animationDuration = `${randomDuration}s`;
});

// Counter animation for stats
const animateCounter = (element, target, duration = 1500) => {
    const start = 0;
    const startTime = performance.now();
    
    const updateCounter = (currentTime) => {
        const elapsed = currentTime - startTime;
        const progress = Math.min(elapsed / duration, 1);
        
        // Easing function (ease-out)
        const easeOut = 1 - Math.pow(1 - progress, 3);
        const current = Math.floor(start + (target - start) * easeOut);
        
        element.textContent = current + (element.dataset.suffix || '');
        
        if (progress < 1) {
            requestAnimationFrame(updateCounter);
        } else {
            element.textContent = target + (element.dataset.suffix || '');
        }
    };
    
    requestAnimationFrame(updateCounter);
};

// Observe stat numbers for counter animation
const statsObserver = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
        if (entry.isIntersecting && !entry.target.dataset.animated) {
            entry.target.dataset.animated = 'true';
            const h3 = entry.target.querySelector('h3');
            if (h3) {
                const text = h3.textContent;
                const match = text.match(/(\d+)/);
                if (match) {
                    const num = parseInt(match[1], 10);
                    const suffix = text.replace(/\d+/, '');
                    h3.dataset.suffix = suffix;
                    animateCounter(h3, num);
                }
            }
        }
    });
}, { threshold: 0.5 });

document.querySelectorAll('.stat-item').forEach((item) => {
    statsObserver.observe(item);
});

// Console easter egg
console.log('%c🌿 Desktop Terrarium', 'font-size: 24px; color: #2ECC71; font-weight: bold;');
console.log('%cA calm ecosystem on your desktop', 'font-size: 14px; color: #888;');
console.log('%cGitHub: https://github.com/Hostilian/The-Desktop-Terrarium', 'font-size: 12px; color: #3498DB;');
