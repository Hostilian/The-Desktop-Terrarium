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

        public DayPhase CurrentPhase =>
            _currentTime < DawnDuration ? DayPhase.Dawn :
            _currentTime < DuskStartTime - DuskDuration ? DayPhase.Day :
            _currentTime < DuskStartTime ? DayPhase.Dusk : DayPhase.Night;

        public bool IsDay => CurrentPhase != DayPhase.Night;

        public bool IsNight => CurrentPhase == DayPhase.Night;

        public double LightLevel => CurrentPhase switch
        {
            DayPhase.Dawn => _currentTime / DawnDuration,
            DayPhase.Day => 1.0,
            DayPhase.Dusk => 1.0 - ((_currentTime - (DuskStartTime - DuskDuration)) / DuskDuration),
            DayPhase.Night => 0.2,
            _ => 1.0
        };

        public double SpeedMultiplier => CurrentPhase switch
        {
            DayPhase.Dawn => DawnDuskSpeedMultiplier,
            DayPhase.Day => 1.0,
            DayPhase.Dusk => DawnDuskSpeedMultiplier,
            DayPhase.Night => NightSpeedMultiplier,
            _ => 1.0
        };

        public double HungerRateMultiplier => CurrentPhase == DayPhase.Night ? NightHungerRateMultiplier : 1.0;

        /// <summary>
        /// Gets the normalized time of day (0.0 = midnight, 0.5 = noon).
        /// </summary>
        public double NormalizedTime => _currentTime / TotalCycleDuration;

        public DayNightCycle() => _currentTime = 0;

        public void Update(double deltaTime)
        {
            _currentTime += deltaTime;
            if (_currentTime >= TotalCycleDuration)
                _currentTime -= TotalCycleDuration;
        }

        public void SetTime(double time) => _currentTime = time % TotalCycleDuration;

        public void SetPhase(DayPhase phase) => _currentTime = phase switch
        {
            DayPhase.Dawn => DawnStartTime,
            DayPhase.Day => DawnDuration,
            DayPhase.Dusk => DuskStartTime - DuskDuration,
            DayPhase.Night => DuskStartTime,
            _ => 0
        };
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
