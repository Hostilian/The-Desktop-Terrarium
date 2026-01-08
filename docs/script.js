/**
 * Desktop Terrarium Website JavaScript
 * Smooth animations, interactions, and dynamic behavior
 */

(function() {
    'use strict';

    // ===== Smooth Scrolling =====
    function initSmoothScrolling() {
        const links = document.querySelectorAll('a[href^="#"]');
        
        links.forEach(link => {
            link.addEventListener('click', function(e) {
                const href = this.getAttribute('href');
                
                // Skip if href is just "#"
                if (href === '#') return;
                
                const targetId = href.substring(1);
                const targetElement = document.getElementById(targetId);
                
                if (targetElement) {
                    e.preventDefault();
                    targetElement.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    }

    // ===== Scroll Animations =====
    function initScrollAnimations() {
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -100px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('visible');
                    
                    // Stagger animations for card grids
                    if (entry.target.classList.contains('about-card') ||
                        entry.target.classList.contains('feature') ||
                        entry.target.classList.contains('download-card')) {
                        const cards = entry.target.parentElement.children;
                        Array.from(cards).forEach((card, index) => {
                            setTimeout(() => {
                                card.style.opacity = '1';
                                card.style.transform = 'translateY(0)';
                            }, index * 100);
                        });
                    }
                }
            });
        }, observerOptions);

        // Observe all sections and cards
        const elementsToAnimate = document.querySelectorAll(
            '.section, .about-card, .feature, .download-card, .step, .os-card'
        );
        
        elementsToAnimate.forEach(el => {
            el.style.opacity = '0';
            el.style.transform = 'translateY(20px)';
            el.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
            observer.observe(el);
        });
    }

    // ===== Animated Counter =====
    function animateCounter(element, target, duration = 2000) {
        let start = 0;
        const increment = target / (duration / 16); // 60 FPS
        
        const timer = setInterval(() => {
            start += increment;
            if (start >= target) {
                element.textContent = Math.ceil(target);
                clearInterval(timer);
            } else {
                element.textContent = Math.ceil(start);
            }
        }, 16);
    }

    // ===== Parallax Effect =====
    function initParallax() {
        let ticking = false;
        
        function updateParallax() {
            const scrolled = window.pageYOffset;
            const parallaxElements = document.querySelectorAll('.hero-background');
            
            parallaxElements.forEach(el => {
                const speed = 0.5;
                el.style.transform = `translateY(${scrolled * speed}px)`;
            });
            
            ticking = false;
        }
        
        window.addEventListener('scroll', () => {
            if (!ticking) {
                window.requestAnimationFrame(updateParallax);
                ticking = true;
            }
        });
    }

    // ===== Interactive Entity Animations =====
    function initEntityAnimations() {
        const entities = document.querySelectorAll('.entity');
        
        entities.forEach(entity => {
            // Random movement
            const randomX = Math.random() * 10 - 5;
            const randomDelay = Math.random() * 2;
            
            entity.style.animation = `float 3s ease-in-out ${randomDelay}s infinite`;
            
            // Add hover effect
            entity.addEventListener('mouseenter', function() {
                this.style.transform = 'scale(1.2) translateY(-15px)';
                this.style.transition = 'transform 0.3s ease';
            });
            
            entity.addEventListener('mouseleave', function() {
                this.style.transform = '';
            });
        });
    }

    // ===== Dynamic Plant Growth Animation =====
    function initPlantGrowth() {
        const plants = document.querySelectorAll('.animated-plant');
        
        plants.forEach((plant, index) => {
            setInterval(() => {
                const scale = 1 + Math.random() * 0.2;
                plant.style.transform = `scaleY(${scale})`;
                plant.style.transition = 'transform 1s ease';
                
                setTimeout(() => {
                    plant.style.transform = 'scaleY(1)';
                }, 1000);
            }, 3000 + index * 1000);
        });
    }

    // ===== Creature Movement =====
    function initCreatureMovement() {
        const creatures = document.querySelectorAll('.animated-creature');
        
        creatures.forEach(creature => {
            let position = parseInt(creature.style.left) || 30;
            let direction = Math.random() > 0.5 ? 1 : -1;
            
            setInterval(() => {
                position += direction * 2;
                
                // Bounce at edges
                if (position > 80 || position < 10) {
                    direction *= -1;
                }
                
                creature.style.left = position + '%';
                creature.style.transition = 'left 0.5s linear';
            }, 500);
        });
    }

    // ===== Button Ripple Effect =====
    function initButtonRipple() {
        const buttons = document.querySelectorAll('.btn');
        
        buttons.forEach(button => {
            button.addEventListener('click', function(e) {
                const ripple = document.createElement('span');
                const rect = this.getBoundingClientRect();
                const size = Math.max(rect.width, rect.height);
                const x = e.clientX - rect.left - size / 2;
                const y = e.clientY - rect.top - size / 2;
                
                ripple.style.width = ripple.style.height = size + 'px';
                ripple.style.left = x + 'px';
                ripple.style.top = y + 'px';
                ripple.classList.add('ripple');
                
                this.appendChild(ripple);
                
                setTimeout(() => {
                    ripple.remove();
                }, 600);
            });
        });
        
        // Add ripple CSS dynamically
        const style = document.createElement('style');
        style.textContent = `
            .btn {
                position: relative;
                overflow: hidden;
            }
            .ripple {
                position: absolute;
                border-radius: 50%;
                background: rgba(255, 255, 255, 0.5);
                transform: scale(0);
                animation: ripple-animation 0.6s ease-out;
                pointer-events: none;
            }
            @keyframes ripple-animation {
                to {
                    transform: scale(4);
                    opacity: 0;
                }
            }
        `;
        document.head.appendChild(style);
    }

    // ===== Scroll Progress Indicator =====
    function initScrollProgress() {
        const progressBar = document.createElement('div');
        progressBar.id = 'scroll-progress';
        progressBar.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 0;
            height: 4px;
            background: linear-gradient(to right, #2ecc71, #3498db);
            z-index: 9999;
            transition: width 0.1s ease;
        `;
        document.body.appendChild(progressBar);
        
        window.addEventListener('scroll', () => {
            const windowHeight = document.documentElement.scrollHeight - window.innerHeight;
            const scrolled = (window.pageYOffset / windowHeight) * 100;
            progressBar.style.width = scrolled + '%';
        });
    }

    // ===== Lazy Loading Images =====
    // Note: Currently no images use lazy loading, but this is ready for future use
    // To use: Add images with data-src attribute instead of src
    function initLazyLoading() {
        if ('IntersectionObserver' in window) {
            const images = document.querySelectorAll('img[data-src]');
            if (images.length === 0) return; // Skip if no lazy-loaded images
            
            const imageObserver = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        if (img.dataset.src) {
                            img.src = img.dataset.src;
                            img.classList.add('loaded');
                            imageObserver.unobserve(img);
                        }
                    }
                });
            });
            
            images.forEach(img => imageObserver.observe(img));
        }
    }

    // ===== Platform Detection =====
    function initPlatformDetection() {
        const userAgent = navigator.userAgent.toLowerCase();
        let platform = 'windows';
        
        if (userAgent.includes('mac')) {
            platform = 'macos';
        } else if (userAgent.includes('linux')) {
            platform = 'linux';
        }
        
        // Highlight the relevant download card
        const downloadCards = document.querySelectorAll('.download-card');
        downloadCards.forEach(card => {
            const platformText = card.querySelector('h3').textContent.toLowerCase();
            if (platformText.includes(platform)) {
                card.style.border = '2px solid #2ecc71';
                card.style.transform = 'scale(1.05)';
            }
        });
    }

    // ===== Keyboard Navigation =====
    function initKeyboardNav() {
        document.addEventListener('keydown', (e) => {
            // ESC key to scroll to top
            if (e.key === 'Escape') {
                window.scrollTo({ top: 0, behavior: 'smooth' });
            }
            
            // Arrow keys for section navigation
            if (e.key === 'ArrowDown' && e.ctrlKey) {
                e.preventDefault();
                scrollToNextSection();
            }
            if (e.key === 'ArrowUp' && e.ctrlKey) {
                e.preventDefault();
                scrollToPrevSection();
            }
        });
    }

    function scrollToNextSection() {
        const sections = document.querySelectorAll('.section');
        const currentScroll = window.pageYOffset;
        
        for (let section of sections) {
            if (section.offsetTop > currentScroll + 100) {
                section.scrollIntoView({ behavior: 'smooth' });
                break;
            }
        }
    }

    function scrollToPrevSection() {
        const sections = Array.from(document.querySelectorAll('.section'));
        const currentScroll = window.pageYOffset;
        
        for (let i = sections.length - 1; i >= 0; i--) {
            if (sections[i].offsetTop < currentScroll - 100) {
                sections[i].scrollIntoView({ behavior: 'smooth' });
                break;
            }
        }
    }

    // ===== Console Easter Egg =====
    function initConsoleEasterEgg() {
        const styles = [
            'color: #2ecc71',
            'font-size: 16px',
            'font-weight: bold'
        ].join(';');
        
        console.log('%c🌱 Desktop Terrarium', styles);
        console.log('%cInterested in the code? Check out the repository:', 'color: #3498db');
        console.log('%chttps://github.com/Hostilian/The-Desktop-Terrarium', 'color: #3498db; text-decoration: underline');
        console.log('%cBuilt with ❤️ and C#', 'color: #e74c3c');
    }

    // ===== Random Creature Spawn (Demo) =====
    function spawnRandomCreature() {
        const terrariumContent = document.querySelector('.terrarium-content');
        if (!terrariumContent) return;
        
        const creature = document.createElement('div');
        creature.className = Math.random() > 0.5 ? 'entity herbivore' : 'entity carnivore';
        creature.style.left = Math.random() * 80 + 10 + '%';
        creature.style.bottom = '15%';
        creature.style.opacity = '0';
        
        terrariumContent.appendChild(creature);
        
        setTimeout(() => {
            creature.style.opacity = '1';
            creature.style.transition = 'opacity 0.5s ease';
        }, 10);
        
        // Remove after animation
        setTimeout(() => {
            creature.style.opacity = '0';
            setTimeout(() => creature.remove(), 500);
        }, 5000);
    }

    // ===== Theme Toggle (Future Enhancement) =====
    function initThemeToggle() {
        // Check for saved theme preference
        const savedTheme = localStorage.getItem('theme') || 'light';
        document.documentElement.setAttribute('data-theme', savedTheme);
    }

    // ===== Copy Code Snippet =====
    function initCodeCopy() {
        const codeElements = document.querySelectorAll('code');
        
        codeElements.forEach(code => {
            code.style.cursor = 'pointer';
            code.title = 'Click to copy';
            
            code.addEventListener('click', function() {
                const text = this.textContent;
                navigator.clipboard.writeText(text).then(() => {
                    const originalText = this.textContent;
                    this.textContent = 'Copied!';
                    setTimeout(() => {
                        this.textContent = originalText;
                    }, 1000);
                }).catch((err) => {
                    console.error('Failed to copy text: ', err);
                    this.textContent = 'Copy failed!';
                    setTimeout(() => {
                        this.textContent = text;
                    }, 1000);
                });
            });
        });
    }

    // ===== Initialize All =====
    function init() {
        // Wait for DOM to be ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', init);
            return;
        }
        
        console.log('Initializing Desktop Terrarium website...');
        
        initSmoothScrolling();
        initScrollAnimations();
        initParallax();
        initEntityAnimations();
        initPlantGrowth();
        initCreatureMovement();
        initButtonRipple();
        initScrollProgress();
        initLazyLoading();
        initPlatformDetection();
        initKeyboardNav();
        initConsoleEasterEgg();
        initThemeToggle();
        initCodeCopy();
        
        // Spawn random creatures periodically in demo
        setInterval(spawnRandomCreature, 10000);
        
        console.log('Desktop Terrarium website initialized! 🌱');
    }

    // Start initialization
    init();

})();
