using Terrarium.Logic.Entities;

namespace Terrarium.Logic.Simulation
{
    /// <summary>
    /// Tracks simulation statistics and history.
    /// Useful for displaying metrics and debugging ecosystem balance.
    /// </summary>
    public class StatisticsTracker
    {
        // Lifetime counters
        private int _totalBirths;
        private int _totalDeaths;
        private int _totalPlantsGrown;
        private int _totalPlantsEaten;
        private int _totalHerbivoresEaten;
        private double _totalFoodConsumed;

        // Session statistics
        private int _sessionBirths;
        private int _sessionDeaths;
        private double _sessionTime;

        // Peak values
        private int _peakPopulation;
        private int _peakPlants;
        private int _peakHerbivores;
        private int _peakCarnivores;

        // Current snapshot
        private int _currentPlants;
        private int _currentHerbivores;
        private int _currentCarnivores;

        /// <summary>
        /// Total number of creatures born since tracking began.
        /// </summary>
        public int TotalBirths => _totalBirths;

        /// <summary>
        /// Total number of creatures that died.
        /// </summary>
        public int TotalDeaths => _totalDeaths;

        /// <summary>
        /// Total plants grown (spawned).
        /// </summary>
        public int TotalPlantsGrown => _totalPlantsGrown;

        /// <summary>
        /// Total plants eaten by herbivores.
        /// </summary>
        public int TotalPlantsEaten => _totalPlantsEaten;

        /// <summary>
        /// Total herbivores eaten by carnivores.
        /// </summary>
        public int TotalHerbivoresEaten => _totalHerbivoresEaten;

        /// <summary>
        /// Total nutrition value consumed by all creatures.
        /// </summary>
        public double TotalFoodConsumed => _totalFoodConsumed;

        /// <summary>
        /// Births in the current session.
        /// </summary>
        public int SessionBirths => _sessionBirths;

        /// <summary>
        /// Deaths in the current session.
        /// </summary>
        public int SessionDeaths => _sessionDeaths;

        /// <summary>
        /// Total session time in seconds.
        /// </summary>
        public double SessionTime => _sessionTime;

        /// <summary>
        /// Peak total population ever reached.
        /// </summary>
        public int PeakPopulation => _peakPopulation;

        /// <summary>
        /// Peak plant count.
        /// </summary>
        public int PeakPlants => _peakPlants;

        /// <summary>
        /// Peak herbivore count.
        /// </summary>
        public int PeakHerbivores => _peakHerbivores;

        /// <summary>
        /// Peak carnivore count.
        /// </summary>
        public int PeakCarnivores => _peakCarnivores;

        /// <summary>
        /// Current plant count.
        /// </summary>
        public int CurrentPlants => _currentPlants;

        /// <summary>
        /// Current herbivore count.
        /// </summary>
        public int CurrentHerbivores => _currentHerbivores;

        /// <summary>
        /// Current carnivore count.
        /// </summary>
        public int CurrentCarnivores => _currentCarnivores;

        /// <summary>
        /// Current total population.
        /// </summary>
        public int CurrentPopulation => _currentPlants + _currentHerbivores + _currentCarnivores;

        /// <summary>
        /// Average lifespan based on session data.
        /// </summary>
        public double AverageLifespan => _sessionDeaths > 0 ? _sessionTime / _sessionDeaths : 0;

        public StatisticsTracker()
        {
            Reset();
        }

        /// <summary>
        /// Records a birth event.
        /// </summary>
        public void RecordBirth(WorldEntity entity)
        {
            _totalBirths++;
            _sessionBirths++;

            if (entity is Plant)
                _totalPlantsGrown++;
        }

        /// <summary>
        /// Records a death event.
        /// </summary>
        public void RecordDeath(WorldEntity entity, DeathCause cause)
        {
            _totalDeaths++;
            _sessionDeaths++;

            if (cause == DeathCause.Predation && entity is Herbivore)
                _totalHerbivoresEaten++;
        }

        /// <summary>
        /// Records a feeding event.
        /// </summary>
        public void RecordFeeding(Creature eater, WorldEntity food, double nutritionValue)
        {
            _totalFoodConsumed += nutritionValue;

            if (eater is Herbivore && food is Plant)
                _totalPlantsEaten++;
        }

        /// <summary>
        /// Updates current population snapshot.
        /// </summary>
        public void UpdateSnapshot(int plants, int herbivores, int carnivores)
        {
            _currentPlants = plants;
            _currentHerbivores = herbivores;
            _currentCarnivores = carnivores;

            // Update peak values
            int totalPopulation = plants + herbivores + carnivores;
            if (totalPopulation > _peakPopulation)
                _peakPopulation = totalPopulation;
            if (plants > _peakPlants)
                _peakPlants = plants;
            if (herbivores > _peakHerbivores)
                _peakHerbivores = herbivores;
            if (carnivores > _peakCarnivores)
                _peakCarnivores = carnivores;
        }

        /// <summary>
        /// Updates session time.
        /// </summary>
        public void UpdateTime(double deltaTime)
        {
            _sessionTime += deltaTime;
        }

        /// <summary>
        /// Resets session statistics.
        /// </summary>
        public void ResetSession()
        {
            _sessionBirths = 0;
            _sessionDeaths = 0;
            _sessionTime = 0;
        }

        /// <summary>
        /// Resets all statistics.
        /// </summary>
        public void Reset()
        {
            _totalBirths = 0;
            _totalDeaths = 0;
            _totalPlantsGrown = 0;
            _totalPlantsEaten = 0;
            _totalHerbivoresEaten = 0;
            _totalFoodConsumed = 0;
            _peakPopulation = 0;
            _peakPlants = 0;
            _peakHerbivores = 0;
            _peakCarnivores = 0;
            ResetSession();
        }

        /// <summary>
        /// Gets a summary string of current statistics.
        /// </summary>
        public string GetSummary()
        {
            return $"Population: {CurrentPopulation} (Peak: {PeakPopulation})\n" +
                   $"Plants: {CurrentPlants} | Herbivores: {CurrentHerbivores} | Carnivores: {CurrentCarnivores}\n" +
                   $"Births: {SessionBirths} | Deaths: {SessionDeaths}\n" +
                   $"Session Time: {SessionTime:F0}s";
        }
    }
}
