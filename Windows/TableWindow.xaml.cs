using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using Microsoft.Win32;

namespace Labyrinth.Windows
{
    /// <summary>
    /// Логика взаимодействия для TableWindow.xaml
    /// </summary>
    public partial class TableWindow : Window
    {
        private CommandModel MainViewModel { get; } = new CommandModel();

        private SaveFileDialog save { get; set; }

        private OpenFileDialog open { get; set; }

        private string text { get; set; }

        public ICommand AddRowCommand { get; set; }

        /*public List<Transition> Transitions { get; set; }*/
        public List<string> Inputs { get; set; }
        public List<string> Outputs { get; set; }

        public TableWindow(int automaton_number)
        {
            Inputs = new List<string>();
            Outputs = new List<string>();
            foreach (string item in AutomatonInMaze.directions_input)
            {
                Inputs.Add(item);
            }

            foreach (string item in AutomatonInMaze.directions_output)
            {
                Outputs.Add(item);
            }

            InitializeComponent();

            Input.ItemsSource = Inputs;
            Output.ItemsSource = Outputs;

            text = $"Автомат {AutomatonInMaze.SELECTED_AUTOMATON}";

            save = new SaveFileDialog();
            open = new OpenFileDialog();

            save.Filter = "Text Files | *.txt";
            open.Filter = "Text Files | *.txt";

            save.DefaultExt = "txt";
            open.DefaultExt = "txt";

            save.FileName = text;

            save.Title = "Сохранить как...";
            open.Title = "Открыть файл...";

            AutomatonNumber.Text = text;

            DataContext = MainViewModel;

            CommandModel.Transitions = new ObservableCollection<AutomatonInMaze>();

            if (AutomatonInMaze.AUTOMATONS_TABLES.ContainsKey(AutomatonInMaze.SELECTED_AUTOMATON))
            {
                List<(ChessMazeInformation, string, ChessMazeInformation, string)> selected_automaton_table = AutomatonInMaze.AUTOMATONS_TABLES[AutomatonInMaze.SELECTED_AUTOMATON];

                for (int i = 0; i < selected_automaton_table.Count; i++)
                {
                    CommandModel.Transitions.Add(new AutomatonInMaze(selected_automaton_table[i].Item1.ToString(), selected_automaton_table[i].Item2, selected_automaton_table[i].Item3.ToString(), selected_automaton_table[i].Item4));
                }
            }
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        public void button_set_table(object sender, RoutedEventArgs e)
        {
            List<(ChessMazeInformation, string, ChessMazeInformation, string)> CheckedList = new List<(ChessMazeInformation, string, ChessMazeInformation, string)>();

            try
            {
                CheckedList = get_list(CheckedList);
            }
            catch
            {
                string text_check_information = $"Сохранение/Считывание не может быть завершено." +
                               $"\nВходные символы должны содержать только: 'n', 'e', 's', 'w'" +
                               $"\nВыходные символы должны содержать только: 'n', 'e', 's', 'w' или 'l'.";
                MessageBox.Show(text_check_information, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (CheckedList == null)
                return;


            if (AutomatonInMaze.AUTOMATONS_TABLES.ContainsKey(AutomatonInMaze.SELECTED_AUTOMATON))
            {
                AutomatonInMaze.AUTOMATONS_TABLES[AutomatonInMaze.SELECTED_AUTOMATON] = CheckedList;
            }
            else
            {
                AutomatonInMaze.AUTOMATONS_TABLES.Add(AutomatonInMaze.SELECTED_AUTOMATON, CheckedList);
            }

            var automaton = AutomatonInMaze.SetAutomaton(CheckedList);

            MessageBox.Show("Автомат считан", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);

            AutomatonInMaze.AUTOMATONS_COUNT[AutomatonInMaze.SELECTED_AUTOMATON] = automaton;
        }

        private void button_load_from_file(object sender, RoutedEventArgs e)
        {
            List<(string, string, string, string)> loadList = new List<(string, string, string, string)>();

            bool ? result = open.ShowDialog();

            if(result == true)
            {
                Stream fileStream = open.OpenFile();
                StreamReader sr = new StreamReader(fileStream);
                bool split_first = true;
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if(split_first)
                    {
                        split_first = false;
                        continue;
                    }
                    var sm = line.Split('(', ')', ',');
                    try
                    {
                        loadList.Add((sm[1], sm[2], sm[3], sm[4]));
                    }
                    catch
                    {
                        continue;
                    }
                }
                sr.Close();
            }

            foreach((string, string, string, string) tuple in loadList)
            {
                CommandModel.Transitions.Add(new AutomatonInMaze(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));
            }
            
        }

        private void button_load_to_file(object sender, RoutedEventArgs e)
        {
            List<(ChessMazeInformation, string, ChessMazeInformation, string)> list = new List<(ChessMazeInformation, string, ChessMazeInformation, string)>();

            list = get_list(list);

            if (list == null)
                return;

            bool? result = save.ShowDialog();

            if (result == true)
            {
                Stream fileStream = save.OpenFile();
                StreamWriter sw = new StreamWriter(fileStream);
                sw.WriteLine($"Автомат {AutomatonInMaze.SELECTED_AUTOMATON}");
                foreach ((ChessMazeInformation, string, ChessMazeInformation, string) tuple in list)
                {
                    sw.WriteLine($"({tuple.Item1.ToString()},{tuple.Item2},{tuple.Item3.ToString()},{tuple.Item4})");
                }
                sw.Flush();
                sw.Close();
            }
        }

        private void automaton_table_CellValidating(object sender, DataGridCellEditEndingEventArgs e)
        {
            
        }

        private void TableWindowCanceled(object sender, CancelEventArgs e)
        {
            MainWindow.TableWindows.Remove(AutomatonInMaze.SELECTED_AUTOMATON);
        }

        private List<(ChessMazeInformation, string, ChessMazeInformation, string)> get_list(List<(ChessMazeInformation, string, ChessMazeInformation, string)> list)
        {
            int count = automaton_table.Items.Count;
            if (count == 0)
            {
                MessageBox.Show("Таблица пуста", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
                return null;
            }

            string wrongTransitions = "";
            List<int> WrongInputsList = new List<int>();

            for (int i = 0; i < count; i++)
            {
                ChessMazeInformation input = CommandModel.Transitions[i].Input;
                ChessMazeInformation output = CommandModel.Transitions[i].Output;

                string currentState = CommandModel.Transitions[i].CurrentState;
                string nextState = CommandModel.Transitions[i].NextState;

                bool isContain = true;
                bool once = true;
                

                if (!input.Contains(output))
                {
                    if (once)
                    {
                        wrongTransitions += (i + 1).ToString();
                        once = false;
                        WrongInputsList.Add(i);
                    }
                    else
                        wrongTransitions += ", " + (i + 1).ToString();
                }

                if (isContain)
                {
                    var tuple = (input, currentState, output, nextState);
                    list.Add(tuple);
                }
            }

            string question = $"\nУдалить некорректные строки?";
            
            string text_contain_or_not;
            if (wrongTransitions.Length != 0)
            {
                text_contain_or_not = $"Сохранение/Считывание не может быть завершено." +
                    $"\nСтроки {wrongTransitions} не могут содержать такие выходы.";

                MessageBoxResult result = MessageBox.Show(text_contain_or_not + "\n" + question, "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    foreach (int index in WrongInputsList)
                        CommandModel.Transitions.RemoveAt(index);
                }
                return null;
            }

            
            
            List<(ChessMazeInformation, string, ChessMazeInformation, string)> SortedList = list.Distinct().ToList();

            List<(string, string)> input_Information = new List<(string, string)>();
            List<(int, int)> WrongInputTuples = new List<(int, int)>();

            foreach (var row in SortedList)
            {
                var input_inform_tuple = (row.Item1.ToString(), row.Item2);

                if (input_Information.Contains(input_inform_tuple))
                {
                    WrongInputTuples.Add((input_Information.IndexOf(input_inform_tuple) + 1, SortedList.IndexOf(row) + 1));
                }      
                else
                {
                    input_Information.Add(input_inform_tuple);
                }
            }

            if (WrongInputTuples.Count != 0)
            {
                string text_check_input_output;
                string indexes_input = "";

                if (WrongInputTuples.Count == 1)
                    indexes_input += WrongInputTuples[0].ToString();
                else
                    foreach (var item in WrongInputTuples)
                        indexes_input += item.ToString() + ", ";
                text_check_input_output = $"Сохранение/Считывание не может быть завершено." +
                    $"\nПары строк входных символов и текущих сотояний конфликтуют между собой {indexes_input}";

                MessageBoxResult result = MessageBox.Show(text_check_input_output + "\n" + question, "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                WrongInputsList.Clear();

                if (result == MessageBoxResult.Yes)
                {
                    foreach ((int, int) tuple in WrongInputTuples)
                        WrongInputsList.Add(tuple.Item2 - 1);

                    WrongInputsList.Reverse();
                    foreach (int index in WrongInputsList)
                        CommandModel.Transitions.RemoveAt(index);
                }
                return null;
            }



            return SortedList;
        }
    }
}
