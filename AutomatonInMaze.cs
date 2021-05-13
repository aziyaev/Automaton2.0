using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Labyrinth
{
    public partial class AutomatonInMaze
    {
        public string Input_Single { get; set; }
        public string Current_State { get; set; }
        public string Output_Single { get; set; }
        public string Next_State { get; set; }
        //public string Predicates { get; set; }

        // список точек, которые прошел автомат
        private static List<Cell> automaton_path = new List<Cell>();
        public static void automatonStart()
        {
            if (automaton_path != null)
            {
                foreach (var item in automaton_path)
                {
                    if (!item.isBlocked)
                        item.clear();
                }

                automaton_path.Clear();
            }

            if (Cell.AUTOMATON_START == null)
                return;

            
        }
    }
}
