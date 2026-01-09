namespace Terrarium.Logic.Simulation
{
    /// <summary>
    /// Manages day/night cycle that affects creature behavior.
    /// Day time: creatures are active, hunt, and forage.
    /// Night time: creatures rest and move slower.
    /// </summary>
    public class DayNightCycle
    {
        // Cycle timing constants
        private const double DayDurationSeconds = 60.0;
        private const double NightDurationSeconds = 30.0;
        private const double TotalCycleDuration = DayDurationSeconds + NightDurationSeconds;
        private const double DawnStartTime = 0.0;
        private const double DuskStartTime = DayDurationSeconds;
        
        // Time of day phases
        private const double DawnDuration = 5.0;
        private const double DuskDuration = 5.0;
        
        // Behavior modifiers
        private const double NightSpeedMultiplier = 0.3;
        private const double NightHungerRateMultiplier = 0.5;
        private const double DawnDuskSpeedMultiplier = 0.7;

        private double _currentTime;

        /// <summary>
        /// Current time in the day/night cycle (0.0 to TotalCycleDuration).
        /// </summary>
        public double CurrentTime => _currentTime;

        /// <summary>
        /// Gets the current phase of the day.
        /// </summary>
        public DayPhase CurrentPhase
        {
            get
            {
                if (_currentTime < DawnDuration)
                    return DayPhase.Dawn;
                if (_currentTime < DuskStartTime - DuskDuration)
                    return DayPhase.Day;
                if (_currentTime < DuskStartTime)
                    return DayPhase.Dusk;
                return DayPhase.Night;
            }
        }

        /// <summary>
        /// Whether it is currently daytime (including dawn and dusk).
        /// </summary>
        public bool IsDay => CurrentPhase != DayPhase.Night;

        /// <summary>
        /// Whether it is currently nighttime.
        /// </summary>
        public bool IsNight => CurrentPhase == DayPhase.Night;

        /// <summary>
        /// Gets the light level (0.0 = pitch black, 1.0 = full daylight).
        /// </summary>
        public double LightLevel
        {
            get
            {
                return CurrentPhase switch
                {
                    DayPhase.Dawn => _currentTime / DawnDuration,
                    DayPhase.Day => 1.0,
                    DayPhase.Dusk => 1.0 - ((_currentTime - (DuskStartTime - DuskDuration)) / DuskDuration),
                    DayPhase.Night => 0.2, // Some moonlight
                    _ => 1.0
                };
            }
        }

        /// <summary>
        /// Gets the speed multiplier for creature movement based on time of day.
        /// </summary>
        public double SpeedMultiplier
        {
            get
            {
                return CurrentPhase switch
                {
                    DayPhase.Dawn => DawnDuskSpeedMultiplier,
                    DayPhase.Day => 1.0,
                    DayPhase.Dusk => DawnDuskSpeedMultiplier,
                    DayPhase.Night => NightSpeedMultiplier,
                    _ => 1.0
                };
            }
        }

        /// <summary>
        /// Gets the hunger rate multiplier (creatures burn less energy at night).
        /// </summary>
        public double HungerRateMultiplier
        {
            get
            {
                return CurrentPhase switch
                {
                    DayPhase.Night => NightHungerRateMultiplier,
                    _ => 1.0
                };
            }
        }

        /// <summary>
        /// Gets the normalized time of day (0.0 = midnight, 0.5 = noon).
        /// </summary>
        public double NormalizedTime => _currentTime / TotalCycleDuration;

        public DayNightCycle()
        {
            // Start at dawn
            _currentTime = 0;
        }

        /// <summary>
        /// Updates the day/night cycle.
        /// </summary>
        public void Update(double deltaTime)
        {
            _currentTime += deltaTime;

            // Wrap around when cycle completes
            if (_currentTime >= TotalCycleDuration)
            {
                _currentTime -= TotalCycleDuration;
            }
        }

        /// <summary>
        /// Sets the time to a specific point in the cycle.
        /// </summary>
        public void SetTime(double time)
        {
            _currentTime = time % TotalCycleDuration;
        }

        /// <summary>
        /// Sets the time to the start of a specific phase.
        /// </summary>
        public void SetPhase(DayPhase phase)
        {
            _currentTime = phase switch
            {
                DayPhase.Dawn => DawnStartTime,
                DayPhase.Day => DawnDuration,
                DayPhase.Dusk => DuskStartTime - DuskDuration,
                DayPhase.Night => DuskStartTime,
                _ => 0
            };
        }
    }

    /// <summary>
    /// Phases of the day.
    /// </summary>
    public enum DayPhase
    {
        Dawn,
        Day,
        Dusk,
        Night
    }
}
