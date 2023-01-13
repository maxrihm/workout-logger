using System;
using System.Diagnostics;
namespace WorkoutLog
{ 
public class Exercise
{
	public string exercise_name;
	public int exercise_sets;
	public int exercise_reps;
	public string exercise_rest;
	public double [] weight_array = new double [10];
	public int [] reps_array = new int [10];
	public double total_weight;

		public Exercise(string ex_name, int ex_sets, int ex_reps, string ex_rest)
		{
		exercise_name = ex_name;
		exercise_sets = ex_sets;
		exercise_reps = ex_reps;
		exercise_rest = ex_rest;
		}

	public void inputWeights(int set, double weight, int rep)
        {
			weight_array[set] = weight;
			reps_array[set] = rep;
        }
	
		
}
}