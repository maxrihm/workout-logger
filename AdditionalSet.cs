using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkoutLog
{
    public class AdditionalSet
    {
        public Dictionary<string, bool> keys = new Dictionary<string, bool>();
        public int[] initial_sets = new int[10];
        public int[] addition_sets = new int[10];
        
        public AdditionalSet(Workout w)
        {
            for(int i=0; i!=w.exercises_count; i++)
            {
                initial_sets[i] = w.exercise_list[i].exercise_sets;
            }
            additionList();
        }
        private void additionList()
        {
            for(int i=0;i!=initial_sets.Length;i++)
            {
                if(initial_sets[i]!=0)
                addition_sets[i] = initial_sets[i] + 1;
            }
        }

    }
}
