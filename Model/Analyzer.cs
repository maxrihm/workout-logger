using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfApp1
{
    /// <summary>
    /// A class for analyzing exercise data from a database.
    /// </summary>
    public class ExerciseAnalyzer
    {
        private readonly DB _database;
        private List<Tuple<int, string, double, int, int, DateTime, double>> _exerciseData;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExerciseAnalyzer"/> class with the specified database.
        /// </summary>
        /// <param name="database">The database to use for querying exercise data.</param>
        public ExerciseAnalyzer(DB database)
        {
            _database = database;
        }

        /// <summary>
        /// The name of the exercise to analyze.
        /// </summary>
        public string ExerciseName { get; set; }

        /// <summary>
        /// Indicates whether the database returned an empty result set for the exercise query.
        /// </summary>
        public bool IsDatabaseEmpty { get; private set; }

        /// <summary>
        /// Queries the database for exercise data.
        /// </summary>
        public void QueryExerciseData()
        {
            string sql = $"SELECT * FROM my_exercises WHERE exercise_name = '{ExerciseName}' AND exercise_set=1;";
            _exerciseData = _database.QueryExerciseReader(sql);
            IsDatabaseEmpty = !_exerciseData.Any();
        }

        /// <summary>
        /// Gets the total volume lifted in the most recent exercise session.
        /// </summary>
        /// <returns>The total volume lifted in the most recent exercise session, or 0 if no data is available.</returns>
        public double GetLastSessionVolume()
        {
            if (_exerciseData == null || !_exerciseData.Any())
            {
                return 0;
            }

            return _exerciseData.Last().Item7;
        }

        /// <summary>
        /// Gets the personal record for the exercise.
        /// </summary>
        /// <returns>The highest total volume lifted for the exercise, or 0 if no data is available.</returns>
        public double GetPersonalRecordVolume()
        {
            if (_exerciseData == null || !_exerciseData.Any())
            {
                return 0;
            }

            return _exerciseData.Max(e => e.Item7);
        }
    }
}
