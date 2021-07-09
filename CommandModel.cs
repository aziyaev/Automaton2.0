using Labyrinth.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Labyrinth
{
    class CommandModel
    {
        //коллекция переходов для текущего автомата
        public static ObservableCollection<AutomatonInMaze> Transitions { get; set; }

        //текущая строка в коллекции
        public AutomatonInMaze SelectedTransition { get; set; }

        //добавить строку в коллекцию 
        public ICommand AddRowCommand { get; set; }

        //получить строку в коллекции
        public ICommand GetRowInfoCommand { get; set; }

        public CommandModel()
        {
            AddRowCommand = new RelayCommand(AddRow);
        }

        //добавление строки заполненной по умолчанию
        public void AddRow()
        {
            Transitions.Add(new AutomatonInMaze("nesw", "q1", "n", "q1"));
        }

    }
}
