using System;
using System.Windows;

namespace Labyrinth
{
    //окно ввода функции
    public partial class FunctionDefiner : Window
    {
        private string function;
        
        private static string lastFunction;

        public FunctionDefiner()
        {
            InitializeComponent();

            textBoxFunction.Text = lastFunction;

            switch (Functions.currentFunctionType)
            {
                case Functions.FunctionType.EQUALS:
                    comboBoxOperation.SelectedIndex = 0;
                    break;
                case Functions.FunctionType.GREATEN:
                    comboBoxOperation.SelectedIndex = 1;
                    break;
                case Functions.FunctionType.LESS:
                    comboBoxOperation.SelectedIndex = 2;
                    break;
            }
        }

        private void buttonAccept_Click(object sender, RoutedEventArgs e)
        {
            checkInput();
            parce();
            createLambda();
        }

        private void parce()
        {
            function = textBoxFunction.Text;

            function = function.
                            Replace("pow", "Math.Pow").
                            Replace("sqrt", "Math.Sqrt").
                            Replace("sin", "Math.Sin").
                            Replace("cos", "Math.Cos").
                            Replace("log", "Math.Log").
                            Replace("abs", "Math.Abs");
;
        }

        private void checkInput()
        {
            switch (comboBoxOperation.SelectedIndex)
            {
                case 0:
                    Functions.currentFunctionType = Functions.FunctionType.EQUALS;
                    break;
                case 1:
                    Functions.currentFunctionType = Functions.FunctionType.GREATEN;
                    break;
                case 2:
                    Functions.currentFunctionType = Functions.FunctionType.LESS;
                    break;
            }

            if (textBoxFunction.Text == String.Empty)
            {
                MessageBox.Show("Пустое тело функции", "Ошибка заполнения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            lastFunction = textBoxFunction.Text;
        }

        private void createLambda()
        {
            bool isLambdaCreated = Functions.createLambda(function);

            if (!isLambdaCreated)
            {
                createErrorMessage();
                return;
            }

            Main.brushType = Main.BrushType.MATHFUNCTION;

            Close();
        }

        private void createErrorMessage()
        {
            MessageBox.Show("Неподдерживаемая операция", "Ошибка заполнения", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
