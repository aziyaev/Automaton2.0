using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using AbstractMachine;

namespace Labyrinth
{
    public partial class AutomatonInMaze
    {
        public static string Input_Single { get; set; }
        public static string Current_State { get; set; }
        public static string Output_Single { get; set; }
        public static string Next_State { get; set; }

        public string Input { get; set; }
        public string CurrentState { get; set; }
        public string Output { get; set; }
        public string NextState { get; set; }

        public static string[] directions_input = { "nesw", "nes", "new", "nsw", "wes", "ne", "ns", "nw", "es", "ew", "sw", "n", "e", "s", "w" };
        public static string[] directions_output = { "n", "e", "s", "w", "l" };

        private static List<ChessMazeInformation> InputList = new List<ChessMazeInformation>();
        private static List<ChessMazeInformation> OutputList = new List<ChessMazeInformation>();

        //словарь клеток стартовых для автомата
        internal static Dictionary<int, Cell> AUTOMATONS_STARTS = new Dictionary<int, Cell>();

        //словарь текущих положений каждого автомата
        internal static Dictionary<int, Cell> AUTOMATONS_CURRENT_CELL = new Dictionary<int, Cell>();

        //словарь пройденного пути каждого автомата
        internal static Dictionary<int, List<Cell>> AUTOMATONS_PATH = new Dictionary<int, List<Cell>>();

        //словарь таблиц для автоматов
        public static Dictionary<int, List<(ChessMazeInformation, string, ChessMazeInformation, string)>> AUTOMATONS_TABLES = new Dictionary<int, List<(ChessMazeInformation, string, ChessMazeInformation, string)>>();

        //текущий автомат
        public static int SELECTED_AUTOMATON;

        //список номеров автомата
        public static Dictionary<int, AbstractMachine<ChessMazeInformation, ChessMazeInformation, string>> AUTOMATONS_COUNT = new Dictionary<int, AbstractMachine<ChessMazeInformation, ChessMazeInformation, string>>();

        //старт и стоп автомата
        public static bool isRun = false;

        public static bool isRunAll = false;

        //скорость хождения автомата по клеточкам
        public static Dictionary<int, int> AUTOMATON_SPEED = new Dictionary<int, int>();

        //отрисовка пути автомата
        public static bool canPathView = true;

        public AutomatonInMaze(string Input, string CurrentState, string Output, string NextState)
        {
            this.Input = Input;
            this.CurrentState = CurrentState;
            this.Output = Output;
            this.NextState = NextState;

            Input_Single = Input;
            Current_State = CurrentState;
            Output_Single = Output;
            Next_State = NextState;

            foreach (string item in directions_input)
            {
                InputList.Add(item);
            }

            foreach (string item in directions_output)
            {
                OutputList.Add(item);
            }

        }


        //создание автомата и сохранение его в словарь для дальнейшего использования
        public static AbstractMachine<ChessMazeInformation, ChessMazeInformation, string> SetAutomaton(List<(ChessMazeInformation input, string currentState, ChessMazeInformation output, string nextState)> CheckedList)
        {
            var hashSet_states = CheckedList
                    .SelectMany(r => new string[] { r.currentState, r.nextState })
                    .Distinct()
                    .ToHashSet();
            SetDomain<string> states = new SetDomain<string>(hashSet_states);

            var hashSet_input = InputList.ToHashSet();
            SetDomain<ChessMazeInformation> domain_input = new SetDomain<ChessMazeInformation>(hashSet_input);

            var hashSet_output = OutputList.ToHashSet();
            SetDomain<ChessMazeInformation> domain_output = new SetDomain<ChessMazeInformation>(hashSet_output);

            var tableBuilder = new TableMappingBuilder<ChessMazeInformation, ChessMazeInformation, string>();

            foreach (var row in CheckedList)
            {
                tableBuilder.AddRow((row.input, row.currentState), (row.output, row.nextState));
            }

            var table = new TableMapping<ChessMazeInformation, ChessMazeInformation, string>(tableBuilder);

            var abstractMachine = new AbstractMachine<ChessMazeInformation, ChessMazeInformation, string>(domain_input, domain_output, states, table, new Information<string>("q1", states));

            return abstractMachine;
        }

        //движение автомата
        //заполнение списка пройденных клеток автоматом
        public static int MoveAutomaton()
        {
            try
            {
                AbstractMachine<ChessMazeInformation, ChessMazeInformation, string> automaton;
                List<Cell> path = new List<Cell>();
                string input;
                string output;
                int error;

                if (AUTOMATONS_COUNT.ContainsKey(SELECTED_AUTOMATON))
                {
                    automaton = AUTOMATONS_COUNT[SELECTED_AUTOMATON];
                }
                else return 1;

                if (automaton == null)
                    return 1;

                Dictionary<string, Cell> possibleDirection = new Dictionary<string, Cell>();

                (input, error, possibleDirection) = GetDirections();

                if (error != 0)
                    return error;

                try
                {
                    output = automaton.Process(new AbstractMachine.Information<ChessMazeInformation>(input, automaton.InputDomain)).ToString();
                }
                catch
                {
                    return 4;
                }


                Cell currentCell = AUTOMATONS_CURRENT_CELL[SELECTED_AUTOMATON];
                Cell nextCell = possibleDirection[output];

                if (AUTOMATONS_PATH.ContainsKey(SELECTED_AUTOMATON))
                    path = AUTOMATONS_PATH[SELECTED_AUTOMATON];
                else
                    AUTOMATONS_PATH.Add(SELECTED_AUTOMATON, path);

                int state;

                if (!path.Contains(currentCell))
                    path.Add(currentCell);
                if (!path.Contains(nextCell))
                {
                    path.Add(nextCell);
                    state = 1;
                }
                else
                    state = 2;


                AUTOMATONS_CURRENT_CELL[SELECTED_AUTOMATON] = nextCell;

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    nextCell.set_automaton_current();

                    if (currentCell != AUTOMATONS_STARTS[SELECTED_AUTOMATON])
                        currentCell.clear();
                    if (currentCell != AUTOMATONS_STARTS[SELECTED_AUTOMATON] && canPathView)
                        currentCell.set_automaton_path(state);
                });

                return error;
            }
            catch
            {
                return 4;
            }
            
        }

        //получить строку input (возможные направления для автомата)
        //установка текущей клетки
        internal static (string, int, Dictionary<string, Cell>) GetDirections()
        {
            Dictionary<string, Cell> possibleToMoveCells = new Dictionary<string, Cell>();


            Cell currentCell;

            if (AUTOMATONS_CURRENT_CELL.ContainsKey(SELECTED_AUTOMATON))
            {
                currentCell = AUTOMATONS_CURRENT_CELL[SELECTED_AUTOMATON];
                possibleToMoveCells = AutomatonVision(currentCell);
            }
            else if (AUTOMATONS_STARTS.ContainsKey(SELECTED_AUTOMATON))
            {
                currentCell = AUTOMATONS_STARTS[SELECTED_AUTOMATON];
                AUTOMATONS_CURRENT_CELL.Add(SELECTED_AUTOMATON, currentCell);
                possibleToMoveCells = AutomatonVision(currentCell);
            }
            else
                return (null, 2, null);

            if (possibleToMoveCells == null)
                return (null, 3, null);

            string directions = "";

            foreach (KeyValuePair<string, Cell> cells in possibleToMoveCells)
            {
                directions += cells.Key;
            }

            return (directions, 0, possibleToMoveCells);
        }

        //получить видимые клетки для автомата
        internal static Dictionary<string, Cell> AutomatonVision(Cell currentCell)
        {
            Dictionary<string, Cell> result = new Dictionary<string, Cell>();

            int X = currentCell.X;
            int Y = currentCell.Y;

            Point current = new Point(X, Y);

            Dictionary<string, Point> directions = Pathfinding.getDirectionsPoint(current);

            if (directions == null)
                return null;

            foreach (KeyValuePair<string, Point> points in directions)
            {
                result.Add(points.Key, Main.cells[points.Value.X, points.Value.Y]);
            }

            result.Add("l", currentCell);

            return result;
        }

    }
}
