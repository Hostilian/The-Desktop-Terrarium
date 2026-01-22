namespace Terrarium.Logic.Simulation
{
    using Terrarium.Logic.Entities;

    /// <summary>
    /// Tracks simulation statistics and history.
    /// Useful for displaying metrics and debugging ecosystem balance.
    /// </summary>
    public class StatisticsTracker
    {
        // Lifetime counters
        private int totalBirths;
        private int totalDeaths;
        private int totalPlantsGrown;
        private int totalPlantsEaten;
        private int totalHerbivoresEaten;
        private double totalFoodConsumed;

        // Session statistics
        private int sessionBirths;
        private int sessionDeaths;
        private double sessionTime;

        // Peak values
        private int peakPopulation;
        private int peakPlants;
        private int peakHerbivores;
        private int peakCarnivores;

        // Current snapshot
        private int currentPlants;
        private int currentHerbivores;
        private int currentCarnivores;

        /// <summary>
        /// Gets total number of creatures born since tracking began.
        /// </summary>
        public int TotalBirths => totalBirths;

        /// <summary>
        /// Gets total number of creatures that died.
        /// </summary>
        public int TotalDeaths => totalDeaths;

        /// <summary>
        /// Gets total plants grown (spawned).
        /// </summary>
        public int TotalPlantsGrown => totalPlantsGrown;

        /// <summary>
        /// Gets total plants eaten by herbivores.
        /// </summary>
        public int TotalPlantsEaten => totalPlantsEaten;

        /// <summary>
        /// Gets total herbivores eaten by carnivores.
        /// </summary>
        public int TotalHerbivoresEaten => totalHerbivoresEaten;

        /// <summary>
        /// Gets total nutrition value consumed by all creatures.
        /// </summary>
        public double TotalFoodConsumed => totalFoodConsumed;

        /// <summary>
        /// Gets births in the current session.
        /// </summary>
        public int SessionBirths => sessionBirths;

        /// <summary>
        /// Gets deaths in the current session.
        /// </summary>
        public int SessionDeaths => sessionDeaths;

        /// <summary>
        /// Gets total session time in seconds.
        /// </summary>
        public double SessionTime => sessionTime;

        /// <summary>
        /// Gets peak total population ever reached.
        /// </summary>
        public int PeakPopulation => peakPopulation;

        /// <summary>
        /// Gets peak plant count.
        /// </summary>
        public int PeakPlants => peakPlants;

        /// <summary>
        /// Gets peak herbivore count.
        /// </summary>
        public int PeakHerbivores => peakHerbivores;

        /// <summary>
        /// Gets peak carnivore count.
        /// </summary>
        public int PeakCarnivores => peakCarnivores;

        /// <summary>
        /// Gets current plant count.
        /// </summary>
        public int CurrentPlants => currentPlants;

        /// <summary>
        /// Gets current herbivore count.
        /// </summary>
        public int CurrentHerbivores => currentHerbivores;

        /// <summary>
        /// Gets current carnivore count.
        /// </summary>
        public int CurrentCarnivores => currentCarnivores;

        /// <summary>
        /// Gets current total population.
        /// </summary>
        public int CurrentPopulation => currentPlants + currentHerbivores + currentCarnivores;

        /// <summary>
        /// Gets average lifespan based on session data.
        /// </summary>
        public double AverageLifespan => sessionDeaths > 0 ? sessionTime / sessionDeaths : 0;

        public StatisticsTracker()
        {
            Reset();
        }

        /// <summary>
        /// Records a birth event.
        /// </summary>
        public void RecordBirth(WorldEntity entity)
        {
            totalBirths++;
            sessionBirths++;

            if (entity is Plant)
                totalPlantsGrown++;
        }

        /// <summary>
        /// Records a death event.
        /// </summary>
        public void RecordDeath(WorldEntity entity, DeathCause cause)
        {
            totalDeaths++;
            sessionDeaths++;

            if (cause == DeathCause.Predation && entity is Herbivore)
                totalHerbivoresEaten++;
        }

        /// <summary>
        /// Records a feeding event.
        /// </summary>
        public void RecordFeeding(Creature eater, WorldEntity food, double nutritionValue)
        {
            totalFoodConsumed += nutritionValue;

            if (eater is Herbivore && food is Plant)
                totalPlantsEaten++;
        }

        /// <summary>
        /// Updates current population snapshot.
        /// </summary>
        public void UpdateSnapshot(int plants, int herbivores, int carnivores)
        {
            currentPlants = plants;
            currentHerbivores = herbivores;
            currentCarnivores = carnivores;

            // Update peak values
            int totalPopulation = plants + herbivores + carnivores;
            if (totalPopulation > peakPopulation)
                peakPopulation = totalPopulation;
            if (plants > peakPlants)
                peakPlants = plants;
            if (herbivores > peakHerbivores)
                peakHerbivores = herbivores;
            if (carnivores > peakCarnivores)
                peakCarnivores = carnivores;
        }

        /// <summary>
        /// Updates session time.
        /// </summary>
        public void UpdateTime(double deltaTime)
        {
            sessionTime += deltaTime;
        }

        /// <summary>
        /// Resets session statistics.
        /// </summary>
        public void ResetSession()
        {
            sessionBirths = 0;
            sessionDeaths = 0;
            sessionTime = 0;
        }

        /// <summary>
        /// Resets all statistics.
        /// </summary>
        public void Reset()
        {
            totalBirths = 0;
            totalDeaths = 0;
            totalPlantsGrown = 0;
            totalPlantsEaten = 0;
            totalHerbivoresEaten = 0;
            totalFoodConsumed = 0;
            peakPopulation = 0;
            peakPlants = 0;
            peakHerbivores = 0;
            peakCarnivores = 0;
            ResetSession();
        }

        /// <summary>
        /// Gets a summary string of current statistics.
        /// </summary>
        /// <returns></returns>
        public string GetSummary()
        {
            return $"Population: {CurrentPopulation} (Peak: {PeakPopulation})\n" +
                   $"Plants: {CurrentPlants} | Herbivores: {CurrentHerbivores} | Carnivores: {CurrentCarnivores}\n" +
                   $"Births: {SessionBirths} | Deaths: {SessionDeaths}\n" +
                   $"Session Time: {SessionTime:F0}s";
        }
    }
}
