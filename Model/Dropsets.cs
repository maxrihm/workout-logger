using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfApp1
{
    public class Dropsets
    {
        public double PrDropset { get; set; }
        public double LastTimeDropset { get; set; }
        public int SetIndex { get; set; }
        public int ExerciseSets { get; set; }

        public double Weight1 { get; set; }
        public double Weight2 { get; set; }

        public double CurrentPR { get; set; }
        public double CurrentTotalVolume { get; set; }
        public double LastTimeTotalVolume { get; set; }

        public Dropsets(int index)
        {
            SetIndex = index;
        }

        public List<string> CalculateList()
        {
            List<int> fullRangeListOne = new List<int>();
            double prCalculationRepsListOne = Math.Ceiling((((CurrentPR + 1) / Weight1 - (CurrentTotalVolume / (Weight1))) / (ExerciseSets - SetIndex)));
            int returnRange = (int)prCalculationRepsListOne + 1;

            try
            {
                foreach (int x in Enumerable.Range(0, returnRange))
                {
                    fullRangeListOne.Add(x);
                }
                List<string> listSecond = new List<string>();

                foreach (int x in fullRangeListOne)
                {
                    double updatedTotalVolume = (x * Weight1) + CurrentTotalVolume;
                    double prCalculationReps = Math.Ceiling(((CurrentPR + 1 - updatedTotalVolume) / Weight2) / (ExerciseSets - SetIndex));
                    listSecond.Add(prCalculationReps.ToString());
                }

                foreach (int x in fullRangeListOne)
                {
                    double updatedTotalVolume = (x * Weight1) + CurrentTotalVolume;
                    double lastTimeCalculationReps = Math.Ceiling(((LastTimeTotalVolume + 1 - updatedTotalVolume) / Weight2) / (ExerciseSets - SetIndex));
                    string addLastTime = x.ToString() + "     " + lastTimeCalculationReps.ToString() + "/" + listSecond[x];
                    listSecond[x] = addLastTime;
                }

                return listSecond;
            }
            catch (Exception ex)
            {
                List<string> list = new List<string>();
                list.Add(" ");
                return list;
            }
        }
    }
}
