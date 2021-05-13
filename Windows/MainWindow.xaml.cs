using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
                    currentCell.setPoint(Cell.PointType.AUTOMATON_START);
                    break;
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

        private void button_set_table(object sender, RoutedEventArgs e)
        {
            set_table_inf();
            // set table automaton
        }

        private void button_automaton_start(object sender, RoutedEventArgs e)
        {
            //start automaton
        }

        private void button_set_automaton_start(object sender, RoutedEventArgs e)
        {
            Main.brushType = Main.BrushType.SET_AUTOMATON;
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

        [DisplayName("Входной символ")]
        public string Input_Single { get; set; }
        [DisplayName("Вход")]
        public string Current_State { get; set; }
        [DisplayName("Выходной символ")]
        public string Output_Single { get; set; }
        [DisplayName("Выход")]
        public string Next_State { get; set; }

        private void set_table_inf()
        {
            List<MainWindow> tableList = new List<MainWindow>
            {
                new MainWindow {Input_Single = "w", Current_State = "q2", Output_Single="e", Next_State="q4"},
                new MainWindow {Input_Single = "s", Current_State = "q2", Output_Single="e", Next_State="q4"}
            };

            automaton_table.ItemsSource = tableList;

        }


        
    }
}
