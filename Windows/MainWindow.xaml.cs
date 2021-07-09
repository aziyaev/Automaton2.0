using Labyrinth.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Labyrinth;
using static Cell;
using AbstractMachine;
using System.Windows.Media.Animation;

namespace Labyrinth
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //так как экземпляр этого класса у нас только один
        //мы можем кэшировать его в статичное поле 
        //и получать доступ к нестатичным данным/методам мз любого места 
        public static MainWindow instance;

        //текущее положение области рисования
        private int canvas_shift_x;
        private int canvas_shift_y;

        //размеры области рисования
        private int width, height;

        //шаг смещения области рисования при перемещении
        private int shift_step = 24;

        //количество созданных за сессию вкладок
        //нужно для имени вкладок
        private int tab_count = 1;

        //текущий масштаб
        private double scale = 1;

        //последняя клеточка, над которой пролетала мышь
        private Cell lastHoveredCell;

        private readonly BackgroundWorker start_automaton = new BackgroundWorker();

        private readonly BackgroundWorker start_all_automatons = new BackgroundWorker();

        private TableWindow tableWindow { get; set; }

        private RadioButton currentRadioButton = new RadioButton();

        private int currentSelectedAutomaton;

        public static Dictionary<int, TableWindow> TableWindows = new Dictionary<int, TableWindow>();

        public static Dictionary<int, Brush> AutomatonBrushes = new Dictionary<int, Brush>();

        public MainWindow()
        {
            InitializeComponent();

            instance = this;

            new Main();

            width = Main.MAPSIZEX * Main.CELLSIZE;
            height = Main.MAPSIZEY * Main.CELLSIZE;

            canvas.Width = width;
            canvas.Height = height;

            setCanvasPosition(-width / 2, -height / 2);
            
            RadioButton button = AddAutomatonButton();
            AutomatonStackPanel.Children.Add(button);

            start_automaton.DoWork += AutomatonRun;
            start_all_automatons.DoWork += AllAutomatonRun;
        }

        public void MainWindowCanceled(object sender, CancelEventArgs e)
        {
            List<TableWindow> Inquisition = new List<TableWindow>();
            foreach(KeyValuePair<int, TableWindow> table in TableWindows)
            {
                Inquisition.Add(table.Value);   
            }
            foreach(TableWindow table in Inquisition)
            {
                table.Close();
            }
        }

        //установить текущую позицию области рисования в [x, y]
        public void setCanvasPosition(int x, int y)
        {
            canvas_shift_x = x;
            canvas_shift_y = y;

            Canvas.SetRight(canvas, canvas_shift_x);
            Canvas.SetTop(canvas, canvas_shift_y);

            updateOcclusionCulling();
        }

        //переместить область рислвания на расстояние [dx, dy]
        private void moveCanvas(int dx, int dy)
        {
            setCanvasPosition(canvas_shift_x + dx, canvas_shift_y + dy);
        }

        //обновить отсечение клеточек за областью видимости
        private void updateOcclusionCulling()
        {
            int X = Main.MAPSIZEX + (int)(canvas_shift_x / Main.CELLSIZE);
            int Y = (int)(-canvas_shift_y / Main.CELLSIZE);

            int frustum_x = (int)(Main.FRUSTUMX / scale);
            int frustum_y = (int)(Main.FRUSTUMY / scale);

            for (int i = 0; i < Main.MAPSIZEX; i++)
            {
                for (int ii = 0; ii < Main.MAPSIZEY; ii++)
                {
                    if (i < X + frustum_x && i > X - frustum_x && ii < Y + frustum_y && ii > Y - frustum_y)
                        Main.cells[i, ii].setVisible();
                    else
                        Main.cells[i, ii].setInvisible();
                }
            }
        }

        //обновить область рисования
        public void reinitCanvas()
        {
            canvas.Children.Clear();

            setCanvasPosition(-width / 2, -height / 2);
        }

        //создать новую вкладку
        private void createNewTab()
        {
            tab_count++;

            TabItem item = new TabItem();

            item.Header = "Лабиринт " + tab_count;

            ((TabItem)content.Parent).Content = null;

            item.Content = content;

            tab_control.Items.Insert(tab_control.Items.Count - 1, item);

            isGeneration = true;

            status_tb.Text = "Создание области...";

            Task t = new Task(crutch_1);

            t.Start();
        }

        //создать новую вкладку с именем name
        private void createNewTab(string name)
        {
            TabItem item = new TabItem();

            item.Header = name;

            ((TabItem)content.Parent).Content = null;

            item.Content = content;

            tab_control.Items.Insert(tab_control.Items.Count - 1, item);

            isGeneration = true;

            status_tb.Text = "Создание области...";

            Task t = new Task(crutch_1);

            t.Start();
        }

        //далее идут методы событий

        #region Events

        //-----------------------------------------------------------//

        private void Window_Activated(object sender, EventArgs e)
        {

        }

        private void buttonLeft_Click(object sender, RoutedEventArgs e)
        {
            moveCanvas(-shift_step, 0);
        }

        private void buttonRight_Click(object sender, RoutedEventArgs e)
        {
            moveCanvas(shift_step, 0);
        }

        private void buttonUp_Click(object sender, RoutedEventArgs e)
        {
            moveCanvas(0, -shift_step);
        }

        private void buttonDown_Click(object sender, RoutedEventArgs e)
        {
            moveCanvas(0, shift_step);
        }

        private void buttonFillWhite_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Main.cells)
            {
                item.setUnblocked();
            }
        }

        private void buttonFillDark_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Main.cells)
            {
                item.setBlocked();
            }
        }

        private void buttonPen_Click(object sender, RoutedEventArgs e)
        {
            Main.brushType = Main.BrushType.PENCIL;
        }

        private void buttonEraser_Click(object sender, RoutedEventArgs e)
        {
            Main.brushType = Main.BrushType.ERASER;
        }

        private void buttonFill_Click(object sender, RoutedEventArgs e)
        {
            Main.brushType = Main.BrushType.BUCKET;
        }

        private void buttonFillUp_Click(object sender, RoutedEventArgs e)
        {
            Main.brushType = Main.BrushType.FILLUP;
        }

        private void buttonFillDown_Click(object sender, RoutedEventArgs e)
        {
            Main.brushType = Main.BrushType.FILLBOTTOM;
        }

        private void buttonFillLeft_Click(object sender, RoutedEventArgs e)
        {
            Main.brushType = Main.BrushType.FILLLEFT;
        }

        private void buttonFillRight_Click(object sender, RoutedEventArgs e)
        {
            Main.brushType = Main.BrushType.FILLRIGHT;
        }

        private void buttonDrawLine_Click(object sender, RoutedEventArgs e)
        {
            Main.brushType = Main.BrushType.LINE;
        }

        private void buttonSetFunction_Click(object sender, RoutedEventArgs e)
        {
            FunctionDefiner window = new FunctionDefiner();
            window.ShowDialog();
        }

        private void canvasMouseMove(object sender, MouseEventArgs e)
        {
            double px = e.GetPosition(canvas).X;
            double py = e.GetPosition(canvas).Y;

            int X = (int)(px / Main.CELLSIZE);
            int Y = (int)(py / Main.CELLSIZE);

            if (Keyboard.IsKeyDown(Key.LeftShift))
                status_tb.Text = "Пересечение с...";
            else if(Keyboard.IsKeyDown(Key.LeftCtrl))
                status_tb.Text = "Объединение с...";
            else
                status_tb.Text = "X: " + X + " / Y: " + Y;

            Cell.lastSelectedCell.setUnselected();

            if (X < 0 || X >= Main.MAPSIZEX || Y < 0 || Y >= Main.MAPSIZEY)
                return;

            Cell currentCell = Main.cells[X, Y];

            if (Main.brushType == Main.BrushType.PENCIL && Mouse.LeftButton == MouseButtonState.Pressed && lastHoveredCell != null && lastHoveredCell != currentCell)
            {
                if (currentCell.isBlocked)
                    currentCell.clear();
                else
                    currentCell.setBlocked();

                lastHoveredCell = currentCell;

                return;
            }

            if (Main.brushType == Main.BrushType.PEN && Mouse.LeftButton == MouseButtonState.Pressed && lastHoveredCell != null && lastHoveredCell != currentCell)
            {
                currentCell.setBlocked();

                lastHoveredCell = currentCell;

                return;
            }

            if (Main.brushType == Main.BrushType.ERASER && Mouse.LeftButton == MouseButtonState.Pressed && lastHoveredCell != null && lastHoveredCell != currentCell)
            {
                currentCell.clear();

                lastHoveredCell = currentCell;

                return;
            }

            lastHoveredCell = currentCell;

            if (!Main.isPositionValid(X, Y))
                return;

            Main.cells[X, Y].setSelected();
        }

        private void canvasMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            double px = e.GetPosition(canvas).X;
            double py = e.GetPosition(canvas).Y;

            int X = (int)(px / Main.CELLSIZE);
            int Y = (int)(py / Main.CELLSIZE);

            if (X < 0 || X >= Main.MAPSIZEX || Y < 0 || Y >= Main.MAPSIZEY)
                return;

            Cell currentCell = Main.cells[X, Y];

            switch (Main.brushType)
            {
                case Main.BrushType.PENCIL:
                    if (currentCell.isBlocked)
                        currentCell.clear();
                    else
                        currentCell.setBlocked();
                    break;
                case Main.BrushType.ERASER:
                    currentCell.clear();
                    break;
                case Main.BrushType.BUCKET:
                    Main.bucketFillFrom(X, Y);
                    break;
                case Main.BrushType.FILLUP:
                    Main.fillUp(Y);
                    break;
                case Main.BrushType.FILLRIGHT:
                    Main.fillRight(X);
                    break;
                case Main.BrushType.FILLBOTTOM:
                    Main.fillBottom(Y);
                    break;
                case Main.BrushType.FILLLEFT:
                    Main.fillLeft(X);
                    break;
                case Main.BrushType.SET_A:
                    currentCell.setPoint(Cell.PointType.A);
                    break;
                case Main.BrushType.SET_B:
                    currentCell.setPoint(Cell.PointType.B);
                    break;
                case Main.BrushType.SET_AUTOMATON:
                    {
                        if (AutomatonInMaze.AUTOMATONS_CURRENT_CELL.ContainsKey(AutomatonInMaze.SELECTED_AUTOMATON))
                            AutomatonInMaze.AUTOMATONS_CURRENT_CELL.Remove(AutomatonInMaze.SELECTED_AUTOMATON);
                        currentCell.setPoint(Cell.PointType.AUTOMATON_START, AutomatonInMaze.SELECTED_AUTOMATON);
                        break;
                    }
                case Main.BrushType.LINE:
                    Main.line(currentCell);
                    break;
                case Main.BrushType.MATHFUNCTION:
                    Main.drawFunctionFrom(X, Y);
                    break;
                case Main.BrushType.PEN:
                    currentCell.setBlocked();
                    break;
            }
        }

        private void button_switch_grid(object sender, RoutedEventArgs e)
        {
            foreach (var item in Main.cells)
            {
                item.switchBorder();
            }
        }

        private void button_zoom_in_click(object sender, RoutedEventArgs e)
        {
            scale += scale/2;

            if (scale >= Main.MAXSCALE)
                scale = Main.MAXSCALE;

            canvas.RenderTransform = new ScaleTransform(scale, scale);

            updateOcclusionCulling();
        }

        private void button_zoom_out_click(object sender, RoutedEventArgs e)
        {
            scale -= scale/2;

            if (scale <= Main.MINSCALE)
                scale = Main.MINSCALE;

            canvas.RenderTransform = new ScaleTransform(scale, scale);

            updateOcclusionCulling();
        }

        private void tabcontrol_plus_mouse_down(object sender, MouseButtonEventArgs e)
        {
            createNewTab();
        }

        private void tabcontrol_selection_changed(object sender, SelectionChangedEventArgs e)
        {
            if(Keyboard.IsKeyDown(Key.LeftShift)&&!isGenerationCallback)
            {
                TabItem item = new TabItem();

                item.Header = "Пересечение";

                ((TabItem)content.Parent).Content = null;

                item.Content = content;

                tab_control.Items.Insert(tab_control.Items.Count - 1, item);

                int index = tab_control.SelectedIndex;

                Main.currentTab = index;

                Task c = new Task(() => Main.instance.intersectCurrentLabyrinthWith(index));

                Task t = new Task(crutch_1);

                t.Start();

                c.Start();

                return;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && !isGenerationCallback)
            {
                TabItem item = new TabItem();

                item.Header = "Объединение";

                ((TabItem)content.Parent).Content = null;

                item.Content = content;

                tab_control.Items.Insert(tab_control.Items.Count - 1, item);

                int index = tab_control.SelectedIndex;

                Main.currentTab = index;

                Task c = new Task(() => Main.instance.concatCurrentLabyrinthWith(index));

                Task t = new Task(crutch_1);

                t.Start();

                c.Start();

                return;
            }

            if (isGenerationCallback && isGeneration)
            {
                Stopwatch s = new Stopwatch();

                s.Start();

                Main.addLabyrinth();

                status_tb.Text = "Created in " + s.ElapsedMilliseconds + " ms";

                s.Stop();

                isGenerationCallback = false;
            }
            else
            {
                if (content.Parent == null)
                    return;
                Main.instance.setCurrentTab(tab_control.SelectedIndex);
                ((TabItem)content.Parent).Content = null;
                ((TabItem)tab_control.Items[Main.currentTab]).Content = content;
                isGenerationCallback = false;
            }

            isGenerationCallback = false;
            isGeneration = false;
        }

        private void button_line_click(object sender, MouseButtonEventArgs e)
        {
            Main.brushType = Main.BrushType.LINE;
        }

        private void button_pathfinding_click(object sender, RoutedEventArgs e)
        {
            Main.tracePath();
        }

        private void buttonPencil_Click(object sender, RoutedEventArgs e)
        {
            Main.brushType = Main.BrushType.PEN;
        }

        private void create_click(object sender, RoutedEventArgs e)
        {
            createNewTab();
        }

        private void save_click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog save = new Microsoft.Win32.SaveFileDialog();

            save.AddExtension = true;
            save.FileName = "Лабиринт " + (tab_control.SelectedIndex + 1);
            save.Title = "Сохранить как...";
            save.DefaultExt = "lb";

            save.Filter = "Лабиринты (*.lb)|*.lb";

            save.ShowDialog();

            Main.save(save.FileName);
        }

        private void open_click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog open = new Microsoft.Win32.OpenFileDialog();

            open.AddExtension = true;
            open.FileName = "Лабиринт " + (tab_control.SelectedIndex + 1);
            open.Title = "Открыть...";
            open.DefaultExt = "lb";

            open.Filter = "Лабиринты (*.lb)|*.lb";

            open.ShowDialog();

            status_tb.Text = "Загрузка \"" + open.SafeFileName + "\"...";

            string path = open.FileName;
            string name = open.SafeFileName;

            Task openAsync = new Task(() => Main.open(path, name));
            
            openAsync.Start();

            createNewTab(open.SafeFileName);

            crutch_2();
        }

        private void exit_click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void close_tab_click(object sender, RoutedEventArgs e)
        {
            if (tab_control.Items.Count == 2)
                return;

            ((TabItem)tab_control.SelectedItem).Content = null;

            tab_control.Items.Remove(tab_control.SelectedItem);

            Main.instance.removeTab(tab_control.SelectedIndex);

            tab_control.SelectedIndex = 0;

            Main.instance.setCurrentTab(0);

            ((TabItem)tab_control.Items[0]).Content = content;
        }

        private void SetA_Click(object sender, RoutedEventArgs e)
        {
            Main.brushType = Main.BrushType.SET_A;
        }

        private void SetB_Click(object sender, RoutedEventArgs e)
        {
            Main.brushType = Main.BrushType.SET_B;
        }

        private void clear_click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Main.cells)
            {
                item.clear();
            }
        }

        private void findA_click(object sender, RoutedEventArgs e)
        {
            if (Cell.A == null)
                return;

            setCanvasPosition((Cell.A.X - Main.MAPSIZEX) * Main.CELLSIZE, -Cell.A.Y * Main.CELLSIZE);
        }

        private void findB_click(object sender, RoutedEventArgs e)
        {
            if (Cell.B == null)
                return;

            setCanvasPosition((Cell.B.X - Main.MAPSIZEX) * Main.CELLSIZE, -Cell.B.Y * Main.CELLSIZE);
        }

        private void moveToCenter_click(object sender, RoutedEventArgs e)
        {
            setCanvasPosition(-width / 2, -height / 2);
        }

        private void default_zoom_click(object sender, RoutedEventArgs e)
        {
            scale = 1;

            canvas.RenderTransform = new ScaleTransform(scale, scale);

            updateOcclusionCulling();
        }

        private void about_click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void rename_click(object sender, RoutedEventArgs e)
        {
            RenameDialog rename = new RenameDialog();
            rename.ShowDialog();
        }

        private void button_clear_click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Main.cells)
            {
                item.clear();
            }
        }

        // Методы для автомата

        private void button_clear_automaton_path(object sender, RoutedEventArgs e)
        {
            clearAutomatonPath(AutomatonInMaze.SELECTED_AUTOMATON);
        }

        private void button_clear_all_automaton_path(object sender, RoutedEventArgs e)
        {
            foreach(KeyValuePair<int, List<Cell>> automaton in AutomatonInMaze.AUTOMATONS_PATH)
            {
                clearAutomatonPath(automaton.Key);
            }
        }

        private void button_add_automaton_click(object sender, RoutedEventArgs e)
        {
            RadioButton button = AddAutomatonButton();
            AutomatonStackPanel.Children.Add(button);
        }

        private void button_remove_automaton_click(object sender, RoutedEventArgs e)
        {
            if (currentRadioButton != null)
            {
                AutomatonStackPanel.Children.Remove(currentRadioButton);
                AutomatonInMaze.AUTOMATONS_COUNT.Remove(AutomatonInMaze.SELECTED_AUTOMATON);
                AutomatonInMaze.AUTOMATON_SPEED.Remove(AutomatonInMaze.SELECTED_AUTOMATON);
                AutomatonInMaze.SELECTED_AUTOMATON = 0;
            }
        }

        private void radiobutton_checked(object sender, RoutedEventArgs e)
        {
            currentRadioButton = (RadioButton)sender;
            AutomatonInMaze.SELECTED_AUTOMATON = int.Parse(currentRadioButton.Tag.ToString());
        }

        private void button_show_automaton_table(object sender, RoutedEventArgs e)
        {
            if(AutomatonInMaze.SELECTED_AUTOMATON != 0)
            {
                if (!TableWindows.ContainsKey(AutomatonInMaze.SELECTED_AUTOMATON))
                {
                    tableWindow = new TableWindow(AutomatonInMaze.SELECTED_AUTOMATON);
                    TableWindows.Add(AutomatonInMaze.SELECTED_AUTOMATON, tableWindow);
                    tableWindow.Show();
                }
                else if (!TableWindows[AutomatonInMaze.SELECTED_AUTOMATON].IsActive && TableWindows.ContainsKey(AutomatonInMaze.SELECTED_AUTOMATON))
                {
                    TableWindows[AutomatonInMaze.SELECTED_AUTOMATON].Activate();
                }
                else
                {
                    tableWindow = new TableWindow(AutomatonInMaze.SELECTED_AUTOMATON);
                    TableWindows.Add(AutomatonInMaze.SELECTED_AUTOMATON, tableWindow);
                    tableWindow.Show();
                }

                
            }
            else
            {
                MessageBox.Show("Укажите автомат");
            }
        }

        private void button_set_automaton_start(object sender, RoutedEventArgs e)
        {
            if (AutomatonInMaze.SELECTED_AUTOMATON == 0)
                MessageBox.Show("Укажите автомат", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
            else
            {
                Main.brushType = Main.BrushType.SET_AUTOMATON;
            }    
                
        }

        private void button_automaton_step(object sender, RoutedEventArgs e)
        {
            int result = AutomatonInMaze.MoveAutomaton();

            if (result == 1)
                MessageBox.Show("Автомат не найден", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (result == 2)
                MessageBox.Show($"Укажите стартовую позицию для Автомата {AutomatonInMaze.SELECTED_AUTOMATON}", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (result == 3)
                MessageBox.Show($"Для Автомата {AutomatonInMaze.SELECTED_AUTOMATON} не найдено путей прохода", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (result == 4)
                MessageBox.Show($"Для Автомата {AutomatonInMaze.SELECTED_AUTOMATON} не найдено состояния для перехода.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void button_all_automaton_step(object sender, RoutedEventArgs e)
        {
            currentSelectedAutomaton = AutomatonInMaze.SELECTED_AUTOMATON;

            foreach (KeyValuePair<int, AbstractMachine<ChessMazeInformation, ChessMazeInformation, string>> automaton in AutomatonInMaze.AUTOMATONS_COUNT)
            {
                try
                {
                    AutomatonInMaze.SELECTED_AUTOMATON = automaton.Key;
                }
                catch
                {
                    continue;
                }

                int result = AutomatonInMaze.MoveAutomaton();

                if (result != 0)
                    continue;
            }

            AutomatonInMaze.SELECTED_AUTOMATON = currentSelectedAutomaton;
        }

        private async void button_run_start(object sender, RoutedEventArgs e)
        {
            currentSelectedAutomaton = AutomatonInMaze.SELECTED_AUTOMATON;
            AutomatonInMaze.isRun = true;
            await Task.Run(() => start_automaton.RunWorkerAsync());
        }

        private async void button_run_all_start(object sender, RoutedEventArgs e)
        {
            currentSelectedAutomaton = AutomatonInMaze.SELECTED_AUTOMATON;
            AutomatonInMaze.isRunAll = true;
            await Task.Run(() => start_all_automatons.RunWorkerAsync());
        }

        private void button_run_stop(object sender, RoutedEventArgs e)
        {
            AutomatonInMaze.isRun = false;
            AutomatonInMaze.isRunAll = false;
            AutomatonInMaze.SELECTED_AUTOMATON = currentSelectedAutomaton;
        }

        private void button_switch_automaton_path(object sender, RoutedEventArgs e)
        {
            AutomatonInMaze.canPathView = !AutomatonInMaze.canPathView;
            
        }

        private void button_set_automaton_speed(object sender, RoutedEventArgs e)
        {
            InputBox.Visibility = Visibility.Visible;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            double speed = 1000;

            try
            {
                String input = InputTextBox.Text;
                string converted = "";

                if (input.Contains("."))
                {
                    foreach(char symbol in input)
                    {
                        if (symbol == '.')
                        {
                            converted += ',';
                            continue;
                        }
                        converted += symbol;
                    }
                }

                speed = Convert.ToDouble(converted) * 1000;

                AutomatonInMaze.AUTOMATON_SPEED[AutomatonInMaze.SELECTED_AUTOMATON] = Convert.ToInt32(speed);

                InputBox.Visibility = Visibility.Collapsed;
            }
            catch
            {
                MessageBox.Show("Неверный тип данных! Укажите скорость в секундах.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            InputTextBox.Text = String.Empty;
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            InputBox.Visibility = System.Windows.Visibility.Collapsed;
            InputTextBox.Text = String.Empty;
        }

        private void AutomatonRun(object sender, DoWorkEventArgs e)
        {
            while (AutomatonInMaze.isRun)
            {
                int result = AutomatonInMaze.MoveAutomaton();

                if (result != 0)
                    AutomatonInMaze.isRun = false;

                if (result == 1)
                    MessageBox.Show("Автомат не найден", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
                else if (result == 2)
                    MessageBox.Show($"Укажите стартовую позицию для Автомата {AutomatonInMaze.SELECTED_AUTOMATON}", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
                else if (result == 3)
                    MessageBox.Show($"Для Автомата {AutomatonInMaze.SELECTED_AUTOMATON} не найдено путей прохода", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);

                Thread.Sleep(AutomatonInMaze.AUTOMATON_SPEED[AutomatonInMaze.SELECTED_AUTOMATON]);
            }

        }

        private void AllAutomatonRun(object sender, DoWorkEventArgs e)
        {
            while (AutomatonInMaze.isRunAll)
            {
                foreach(KeyValuePair<int, AbstractMachine<ChessMazeInformation, ChessMazeInformation, string>> automaton in AutomatonInMaze.AUTOMATONS_COUNT)
                {
                    try
                    {
                        AutomatonInMaze.SELECTED_AUTOMATON = automaton.Key;
                    }
                    catch
                    {
                        continue;
                    }

                    int result = AutomatonInMaze.MoveAutomaton();

                    if (result != 0)
                        continue;
                }

                AutomatonInMaze.SELECTED_AUTOMATON = currentSelectedAutomaton;

                Thread.Sleep(AutomatonInMaze.AUTOMATON_SPEED[AutomatonInMaze.SELECTED_AUTOMATON]);
            }

        }




        private RadioButton AddAutomatonButton()
        {
            int count = AutomatonInMaze.AUTOMATONS_COUNT.Count + 1;
            if (AutomatonInMaze.AUTOMATONS_COUNT.ContainsKey(count))
            {
                count = 1;
                while(AutomatonInMaze.AUTOMATONS_COUNT.ContainsKey(count))
                {
                    count++;
                }
                AutomatonInMaze.AUTOMATONS_COUNT.Add(count, null);
            }
            else
                AutomatonInMaze.AUTOMATONS_COUNT.Add(count, null);

            count = AutomatonInMaze.AUTOMATON_SPEED.Count + 1;
            if (AutomatonInMaze.AUTOMATON_SPEED.ContainsKey(count))
            {
                count = 1;
                while (AutomatonInMaze.AUTOMATON_SPEED.ContainsKey(count))
                {
                    count++;
                }
                AutomatonInMaze.AUTOMATON_SPEED.Add(count, 1000);
            }
            else
                AutomatonInMaze.AUTOMATON_SPEED.Add(count, 1000);

            RadioButton radioButton = new RadioButton();
            radioButton.Width = 190;
            radioButton.Height = 27;
            radioButton.Content = $"Автомат {count}";
            radioButton.Name = $"Automaton_{count}";
            radioButton.Tag = count;
            radioButton.Checked += radiobutton_checked;
            radioButton.FontSize = 20;

            return radioButton;
        }

        //-----------------------------------------------------------//

        #endregion

        //зона сомнительного кода
        //вероятно, всю логику вкладок можно улучшить (?) 

        bool isGeneration;
        bool isGenerationCallback;

        //переключает текущую вкладку асинхронно
        public void crutch_1()
        {
            isGenerationCallback = true;
            Dispatcher.Invoke(new Action(() => tab_control.SelectedIndex = tab_control.Items.Count - 2));
            Thread.CurrentThread.Join(100);
            isGenerationCallback = false;
        }

        //переключает текущую вкладку асинхронно на только что созданную
        public void crutch_2()
        {
            isGenerationCallback = true;
            Dispatcher.Invoke(new Action(() => Main.instance.setCurrentTab(tab_control.Items.Count - 2)));
            Dispatcher.Invoke(new Action(() => ((TabItem)content.Parent).Content = null));
            Dispatcher.Invoke(new Action(() => ((TabItem)tab_control.Items[tab_control.Items.Count - 2]).Content = content));
            Dispatcher.Invoke(new Action(() => updateOcclusionCulling()));
            Thread.CurrentThread.Join(100);
            isGenerationCallback = false;
        }

        

       

    }
}
