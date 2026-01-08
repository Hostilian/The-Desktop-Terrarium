using System.Windows.Media.Animation;
using System.Windows;

namespace Terrarium.Desktop.Rendering
{
    /// <summary>
    /// Manages sprite animations for entities.
    /// Supports both frame-based sprite sheets and procedural animations.
    /// </summary>
    public class AnimationController
    {
        private readonly Dictionary<int, Storyboard> _activeAnimations;
        private readonly Random _random;

        // Animation timing constants
        private const double IdleAnimationDuration = 2.0;
        private const double WalkAnimationDuration = 0.5;
        private const double EatAnimationDuration = 0.3;

        public AnimationController()
        {
            _activeAnimations = new Dictionary<int, Storyboard>();
            _random = new Random();
        }

        /// <summary>
        /// Plays an idle animation for an entity.
        /// </summary>
        public void PlayIdleAnimation(UIElement visual, int entityId)
        {
            StopAnimation(entityId);

            var storyboard = new Storyboard();
            
            // Create subtle bobbing motion
            var bobAnimation = new DoubleAnimation
            {
                From = 0,
                To = -5,
                Duration = TimeSpan.FromSeconds(IdleAnimationDuration),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(bobAnimation, visual);
            Storyboard.SetTargetProperty(bobAnimation, new PropertyPath("(Canvas.Top)"));
            storyboard.Children.Add(bobAnimation);

            storyboard.Begin();
            _activeAnimations[entityId] = storyboard;
        }

        /// <summary>
        /// Plays a walk animation for a moving creature.
        /// </summary>
        public void PlayWalkAnimation(UIElement visual, int entityId, bool movingRight)
        {
            // Walk animation could involve sprite frame cycling or transform animations
            // For now, we'll use a simple wobble effect
            
            StopAnimation(entityId);

            var storyboard = new Storyboard();
            
            var rotateAnimation = new DoubleAnimation
            {
                From = -5,
                To = 5,
                Duration = TimeSpan.FromSeconds(WalkAnimationDuration),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            // Apply rotation transform if it doesn't exist
            if (visual.RenderTransform is not System.Windows.Media.RotateTransform)
            {
                visual.RenderTransform = new System.Windows.Media.RotateTransform();
            }

            Storyboard.SetTarget(rotateAnimation, visual);
            Storyboard.SetTargetProperty(rotateAnimation, 
                new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            storyboard.Children.Add(rotateAnimation);

            storyboard.Begin();
            _activeAnimations[entityId] = storyboard;
        }

        /// <summary>
        /// Plays an eating animation.
        /// </summary>
        public void PlayEatAnimation(UIElement visual, int entityId)
        {
            StopAnimation(entityId);

            var storyboard = new Storyboard();
            
            // Scale up and down to simulate eating
            var scaleXAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 1.2,
                Duration = TimeSpan.FromSeconds(EatAnimationDuration),
                AutoReverse = true
            };

            var scaleYAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 1.2,
                Duration = TimeSpan.FromSeconds(EatAnimationDuration),
                AutoReverse = true
            };

            // Apply scale transform if it doesn't exist
            if (visual.RenderTransform is not System.Windows.Media.ScaleTransform)
            {
                visual.RenderTransform = new System.Windows.Media.ScaleTransform(1, 1);
            }

            Storyboard.SetTarget(scaleXAnimation, visual);
            Storyboard.SetTargetProperty(scaleXAnimation, 
                new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            
            Storyboard.SetTarget(scaleYAnimation, visual);
            Storyboard.SetTargetProperty(scaleYAnimation, 
                new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

            storyboard.Children.Add(scaleXAnimation);
            storyboard.Children.Add(scaleYAnimation);

            storyboard.Begin();
            _activeAnimations[entityId] = storyboard;
        }

        /// <summary>
        /// Stops the animation for an entity.
        /// </summary>
        public void StopAnimation(int entityId)
        {
            if (_activeAnimations.TryGetValue(entityId, out var storyboard))
            {
                storyboard.Stop();
                _activeAnimations.Remove(entityId);
            }
        }

        /// <summary>
        /// Stops all animations.
        /// </summary>
        public void StopAllAnimations()
        {
            foreach (var storyboard in _activeAnimations.Values)
            {
                storyboard.Stop();
            }
            _activeAnimations.Clear();
        }
    }
}
