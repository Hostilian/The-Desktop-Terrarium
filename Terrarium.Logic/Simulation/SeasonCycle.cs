namespace Terrarium.Logic.Simulation
{
    /// <summary>
    /// Manages a simple season cycle that can influence ecosystem behavior.
    /// </summary>
    public class SeasonCycle
    {
        private const double SeasonDurationSeconds = 120.0;
        private const double TotalCycleDuration = SeasonDurationSeconds * 4.0;

        private double _currentTime;

        /// <summary>
        /// Current time in the season cycle (0.0 to TotalCycleDuration).
        /// </summary>
        public double CurrentTime => _currentTime;

        /// <summary>
        /// Current season.
        /// </summary>
        public Season CurrentSeason
        {
            get
            {
                double t = _currentTime % TotalCycleDuration;
                if (t < SeasonDurationSeconds)
                    return Season.Spring;
                if (t < SeasonDurationSeconds * 2.0)
                    return Season.Summer;
                if (t < SeasonDurationSeconds * 3.0)
                    return Season.Autumn;
                return Season.Winter;
            }
        }

        /// <summary>
        /// Progress within the current season (0.0 to 1.0).
        /// </summary>
        public double SeasonProgress
        {
            get
            {
                double t = _currentTime % TotalCycleDuration;
                double within = t % SeasonDurationSeconds;
                return within / SeasonDurationSeconds;
            }
        }

        /// <summary>
        /// Multiplier applied to plant spawn chance.
        /// </summary>
        public double PlantSpawnChanceMultiplier => CurrentSeason switch
        {
            Season.Spring => 1.4,
            Season.Summer => 1.2,
            Season.Autumn => 0.9,
            Season.Winter => 0.6,
            _ => 1.0
        };

        public SeasonCycle()
        {
            _currentTime = 0;
        }

        public void Update(double deltaTime)
        {
            _currentTime += deltaTime;
            if (_currentTime >= TotalCycleDuration)
            {
                _currentTime -= TotalCycleDuration;
            }
        }

        public void SetTime(double time)
        {
            _currentTime = time % TotalCycleDuration;
            if (_currentTime < 0)
            {
                _currentTime += TotalCycleDuration;
            }
        }

        public void SetSeason(Season season)
        {
            _currentTime = season switch
            {
                Season.Spring => 0,
                Season.Summer => SeasonDurationSeconds,
                Season.Autumn => SeasonDurationSeconds * 2.0,
                Season.Winter => SeasonDurationSeconds * 3.0,
                _ => 0
            };
        }
    }

    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }
}
