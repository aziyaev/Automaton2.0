using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Controls;
using Labyrinth;

//класс для хранения констант, общих данных и рабочих методов методов
//нужен в первую очередь для повышения читабельности, 
//чтобы нужные методы не потерялись среди методов событий форм
class Main
{
    //так как экземпляр этого класса у нас только один
    //мы можем кэшировать его в статичное поле 
    //и получать доступ к нестатичным данным/методам мз любого места 
    public static Main instance;

    //размеры поля в клеточках
    public static int MAPSIZEX = 512;
    public static int MAPSIZEY = 512;

    //размер ячейки в пикселях
    public static int CELLSIZE = 16;

    //размеры области видимости "камеры"
    //нужны для для отсечения клеточек
    //вне видимой области
    public static int FRUSTUMX = 64;
    public static int FRUSTUMY = 36;

    //максимальное.минимальное значения масштаба
    public static double MAXSCALE = 3;
    public static double MINSCALE = 0.8;

    //текущий инструмент
    public static BrushType brushType;

    //массив клеточек
    public static Cell[,] cells;

    //список массивов клеточек для разных вкладок
    private static List<Cell[,]> data;

    //текущая вкладка
    public static int currentTab;

    //область рисования
    public static Canvas canvas;

    public Main()
    {
        instance = this;

        canvas = MainWindow.instance.canvas;

        data = new List<Cell[,]>();

        brushType = BrushType.PENCIL;

        addLabyrinth();
        
        setCurrentTab(0);

        Pathfinding.init(MAPSIZEX, MAPSIZEY, CELLSIZE);
    }

    //пересеченеие текущего лабиринта с лабиринтом
    //под номером index в списке массивов клеточек
    //выполняется асинхронно
    public void intersectCurrentLabyrinthWith(int index)
    {
        Cell[,] _cells = data[index];

        Cell[,] result = new Cell[MAPSIZEX, MAPSIZEY];

        for (int i = 0; i < MAPSIZEX; i++)
        {
            for (int ii = 0; ii < MAPSIZEY; ii++)
            {
                MainWindow.instance.Dispatcher.Invoke(new Action(() => result[i, ii] = new Cell(i, ii)));
                if (cells[i, ii].isBlocked && _cells[i, ii].isBlocked)
                    MainWindow.instance.Dispatcher.Invoke(new Action(() => result[i, ii].setBlocked()));
            }

            string progress = "Пересечние: " + ((double)i * 100 / (double)MAPSIZEX).ToString("F2") + "%";

            MainWindow.instance.Dispatcher.Invoke(new Action(() => MainWindow.instance.status_tb.Text = progress));
            
            //если поток не приостанавливать, Invoke не срабатывает
            //возможно, есть более лучшее решение (?)
            Thread.CurrentThread.Join(2);
        }

        data.Add(result);

        MainWindow.instance.Dispatcher.Invoke(new Action(() => MainWindow.instance.status_tb.Text = "Рендеринг..."));

        Thread.CurrentThread.Join(2);

        MainWindow.instance.crutch_2();

        Thread.CurrentThread.Join(10);
    }

    //объединение текущего лабиринта с лабиринтом
    //под номером index в списке массивов клеточек
    //выполняется асинхронно
    public void concatCurrentLabyrinthWith(int index)
    {
        Cell[,] _cells = data[index];

        Cell[,] result = new Cell[MAPSIZEX, MAPSIZEY];

        for (int i = 0; i < MAPSIZEX; i++)
        {
            for (int ii = 0; ii < MAPSIZEY; ii++)
            {
                MainWindow.instance.Dispatcher.Invoke(new Action(() => result[i, ii] = new Cell(i, ii)));
                if (cells[i, ii].isBlocked || _cells[i, ii].isBlocked)
                    MainWindow.instance.Dispatcher.Invoke(new Action(() => result[i, ii].setBlocked()));
            }

            string progress = "Объединение: " + ((double)i * 100 / (double)MAPSIZEX).ToString("F2") + "%";
            
            MainWindow.instance.Dispatcher.Invoke(new Action(() => MainWindow.instance.status_tb.Text = progress));
            
            //если поток не приостанавливать, Invoke не срабатывает
            //возможно, есть более лучшее решение (?)
            Thread.CurrentThread.Join(2);
        }

        data.Add(result);

        MainWindow.instance.Dispatcher.Invoke(new Action(() => MainWindow.instance.status_tb.Text = "Рендеринг..."));

        Thread.CurrentThread.Join(2);

        MainWindow.instance.crutch_2();

        Thread.CurrentThread.Join(10);
    }

    //создать пустой лабиринт
    public static void addLabyrinth()
    {
        Cell[,] cells = new Cell[MAPSIZEX, MAPSIZEY];

        for (int i = 0; i < MAPSIZEX; i++)
        {
            for (int ii = 0; ii < MAPSIZEY; ii++)
            {
                cells[i, ii] = new Cell(i, ii);
            }
        }

        data.Add(cells);
    }

    //установить текущую вкладку
    public void setCurrentTab(int index)
    {
        if (index >= data.Count)
            return;

        cells = data[index];

        currentTab = index;

        MainWindow.instance.reinitCanvas();
    }

    //удалить вкладку под номером index
    //в списке массивов клеточек
    public void removeTab(int index)
    {
        data.RemoveAt(index);
    }

    //сохранить лабиринт текущей вкладки в файл
    //fiepath - полный путь до файла
    public static void save(string filepath)
    {
        FileStream file = File.Create(filepath);

        BinaryFormatter formatter = new BinaryFormatter();

        SaveData data = new SaveData();

        for (int i = 0; i < MAPSIZEX; i++)
        {
            for (int ii = 0; ii < MAPSIZEY; ii++)
            {
                data.cell_states[i, ii] = cells[i, ii].isBlocked;
            }
        }

        formatter.Serialize(file, data);

        file.Close();
    }

    //загрузить лабиринт из файла
    //fiepath - полный путь до файла
    //filename - только имя файла
    public static void open(string filepath, string filename)
    {
        FileStream file = File.OpenRead(filepath);

        BinaryFormatter formatter = new BinaryFormatter();

        SaveData data = (SaveData)formatter.Deserialize(file);

        Cell[,] result = new Cell[MAPSIZEX, MAPSIZEY];

        for (int i = 0; i < MAPSIZEX; i++)
        {
            for (int ii = 0; ii < MAPSIZEY; ii++)
            {
                MainWindow.instance.Dispatcher.Invoke(new Action(() => result[i, ii] = new Cell(i, ii)));
                if (data.cell_states[i, ii])
                    MainWindow.instance.Dispatcher.Invoke(new Action(() => result[i, ii].setBlocked()));
            }

            string progress = "Загрузка \"" + filename + "\": " + ((double)i * 100 / (double)MAPSIZEX).ToString("F2") + "%";

            MainWindow.instance.Dispatcher.Invoke(new Action(() => MainWindow.instance.status_tb.Text = progress));
            Thread.CurrentThread.Join(5);
        }

        Main.data.Add(result);

        MainWindow.instance.Dispatcher.Invoke(new Action(() => MainWindow.instance.status_tb.Text = "Рендеринг..."));

        Thread.CurrentThread.Join(2);

        MainWindow.instance.crutch_2();

        Thread.CurrentThread.Join(10);
    }

    //заливка с центром в точке [X, Y]
    public static void bucketFillFrom(int X, int Y)
    {
        Stack<Cell> stack = new Stack<Cell>();

        stack.Push(cells[X, Y]);

        while (stack.Count > 0)
        {
            Cell currentCell = stack.Pop();

            int x = currentCell.X;
            int y = currentCell.Y;

            currentCell.setBlocked();

            if (isPositionValid(x + 1, y) && !cells[x + 1, y].isBlocked)
                stack.Push(cells[x+1, y]);
            
            if (isPositionValid(x, y - 1) && !cells[x, y - 1].isBlocked)
                stack.Push(cells[x, y - 1]);
            
            if (isPositionValid(x - 1, y) && !cells[x - 1, y].isBlocked)
                stack.Push(cells[x - 1, y]);
            
            if (isPositionValid(x, y + 1) && !cells[x, y + 1].isBlocked)
                stack.Push(cells[x, y + 1]);
        }
    }

    //заливка вниз от координаты Y
    public static void fillBottom(int Y)
    {
        for (int i = 0; i < MAPSIZEX; i++)
        {
            for (int ii = Y; ii < MAPSIZEY; ii++)
            {
                cells[i, ii].setBlocked();
            }
        }
    }

    //заливка вверх от координаты Y
    public static void fillUp(int Y)
    {
        for (int i = 0; i < MAPSIZEX; i++)
        {
            for (int ii = 0; ii < Y + 1; ii++)
            {
                cells[i, ii].setBlocked();
            }
        }
    }

    //заливка вправо от координаты X
    public static void fillRight(int X)
    {
        for (int i = X; i < MAPSIZEX; i++)
        {
            for (int ii = 0; ii < MAPSIZEY; ii++)
            {
                cells[i, ii].setBlocked();
            }
        }
    }

    //заливка влево от координаты X
    public static void fillLeft(int X)
    {
        for (int i = 0; i < X + 1; i++)
        {
            for (int ii = 0; ii < MAPSIZEY; ii++)
            {
                cells[i, ii].setBlocked();
            }
        }
    }

    //линия
    //вызывается дважды, для установки двух точек cell
    //алгоритм Брезенхэма
    public static void line(Cell cell)
    {
        if (Cell.lineStartCell == null)
        {
            cell.setLineStart();
            return;
        }

        int X0 = Cell.lineStartCell.X;
        int Y0 = Cell.lineStartCell.Y;

        int X1 = cell.X;
        int Y1 = cell.Y;

        int dx = X1 - X0;
        int dy = -(Y1 - Y0);

        int dx_a = Math.Abs(X1 - X0);
        int dy_a = Math.Abs(Y1 - Y0);

        if (dx < 0 && dx_a > dy_a)
            line_L(Cell.lineStartCell, cell);
        if (dx > 0 && dx_a > dy_a)
            line_R(Cell.lineStartCell, cell);
        if(dy > 0 && dy_a > dx_a)
            line_U(Cell.lineStartCell, cell);
        if (dy < 0 && dy_a > dx_a)
            line_D(Cell.lineStartCell, cell);

        cell.setBlocked();

        Cell.lineStartCell.setBlocked();

        Cell.lineStartCell = null;
    }

    //линия от клеточки start до end
    public static void line(Cell start, Cell end)
    {
        int X0 = start.X;
        int Y0 = start.Y;

        int X1 = end.X;
        int Y1 = end.Y;

        int dx = X1 - X0;
        int dy = -(Y1 - Y0);

        int dx_a = Math.Abs(X1 - X0);
        int dy_a = Math.Abs(Y1 - Y0);

        if (dx < 0 && dx_a > dy_a)
            line_L(start, end);
        if (dx > 0 && dx_a > dy_a)
            line_R(start, end);
        if (dy > 0 && dy_a > dx_a)
            line_U(start, end);
        if (dy < 0 && dy_a > dx_a)
            line_D(start, end);

        start.setBlocked();
        
        end.setBlocked();
    }

    //далее идут четыре реализации алгоритма Брезенхэма
    //для различных секторов его действия

    #region Bresenham

    //-----------------------------------------------------------//

    private static void line_R(Cell start, Cell end)
    {
        int X0 = start.X;
        int Y0 = start.Y;

        int X1 = end.X;
        int Y1 = end.Y;

        int delta_x = Math.Abs(X1 - X0);
        int delta_y = Math.Abs(Y1 - Y0);

        int error = 0;

        int delta_error = delta_y + 1;

        int y = Y0;

        int direction = Y1 - Y0 > 0 ? 1 : -1;

        for (int x = X0; x < X1; x++)
        {
            cells[x, y].setBlocked();

            error += delta_error;

            if (error >= delta_x + 1)
            {
                y += direction;
                error -= delta_x + 1;
            }
        }
    }

    private static void line_L(Cell start, Cell end)
    {
        int X0 = start.X;
        int Y0 = start.Y;

        int X1 = end.X;
        int Y1 = end.Y;

        int delta_x = Math.Abs(X1 - X0);
        int delta_y = Math.Abs(Y1 - Y0);

        int error = 0;

        int delta_error = delta_y + 1;

        int y = Y0;

        int direction = Y1 - Y0 > 0 ? 1 : -1;

        for (int x = X0; x > X0 - Math.Abs(X0 - X1); x--)
        {
            cells[x, y].setBlocked();

            error += delta_error;

            if (error >= delta_x + 1)
            {
                y += direction;
                error -= delta_x + 1;
            }
        }
    }

    private static void line_D(Cell start, Cell end)
    {
        int X0 = start.X;
        int Y0 = start.Y;

        int X1 = end.X;
        int Y1 = end.Y;

        int delta_x = Math.Abs(X1 - X0);
        int delta_y = Math.Abs(Y1 - Y0);

        int error = 0;

        int delta_error = delta_x + 1;

        int x = X0;

        int direction = X1 - X0 > 0 ? 1 : -1;

        for (int y = Y0; y < Y1; y++)
        {
            cells[x, y].setBlocked();

            error += delta_error;

            if (error >= delta_y + 1)
            {
                x += direction;
                error -= delta_y + 1;
            }
        }
    }

    private static void line_U(Cell start, Cell end)
    {
        int X0 = start.X;
        int Y0 = start.Y;

        int X1 = end.X;
        int Y1 = end.Y;

        int delta_x = Math.Abs(X1 - X0);
        int delta_y = Math.Abs(Y1 - Y0);

        int error = 0;

        int delta_error = delta_x + 1;

        int x = X0;

        int direction = X1 - X0 > 0 ? 1 : -1;

        for (int y = Y0; y > Y0 - Math.Abs(Y1 - Y0); y--)
        {
            cells[x, y].setBlocked();

            error += delta_error;

            if (error >= delta_y + 1)
            {
                x += direction;
                error -= delta_y + 1;
            }
        }
    }

    //-----------------------------------------------------------//

    #endregion

    //рисование текущей функции от точки [X, Y]
    public static void drawFunctionFrom(int X, int Y)
    {
        //область опреления функции
        int range = 100;

        Cell lastPoint = null;

        for (int x = -range; x < range; x++)
        {
            double func = Functions.getResult(x);

            int y = Y - (int)func;

            if (!isPositionValid(x + X, y))
                continue;

            if (lastPoint == null)
                cells[x + X, y].setBlocked();
            else
                line(lastPoint, cells[x + X, y]);

            if(Functions.currentFunctionType == Functions.FunctionType.GREATEN)
                for (int i = y; i < MAPSIZEY; i++)
                {
                    cells[x + X, i].setBlocked();
                }

            if (Functions.currentFunctionType == Functions.FunctionType.LESS)
                for (int i = y; i > 0; i--)
                {
                    cells[x + X, i].setBlocked();
                }

            lastPoint = cells[x + X, y];
        }
    }

    //список точек текущего пути
    private static List<Cell> path = new List<Cell>();

    //нахождение пути и его отрисовка
    public static void tracePath()
    {
        if (path != null)
        {
            foreach (var item in path)
            {
                if (!item.isBlocked)
                    item.clear();
            }

            path.Clear();
        }

        if (Cell.A == null || Cell.B == null)
            return;

        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();

        path = getPath(Cell.A, Cell.B);

        stopwatch.Stop();

        if(path == null)
        {
            MainWindow.instance.status_tb.Text = "Путь не найден";
            return;
        }

        string length = path.Count.ToString();

        string suffix = "клеточек";

        if (length.Last() == '1')
            suffix = "клеточка";
        if (length.Last() == '2' || length.Last() == '3' || length.Last() == '4')
            suffix = "клеточки";

        MainWindow.instance.status_tb.Text = "Путь найден за " + stopwatch.ElapsedMilliseconds + " мс" + " //Длина " + path.Count + " " + suffix;

        foreach (var item in path)
        {
            item.setPathPoint();
        }
    }

    //возвращает список клеточек от клеточки startCell до endCell
    private static List<Cell> getPath(Cell startCell, Cell endCell)
    {
        List<Cell> result = new List<Cell>();

        int X0 = startCell.X;
        int Y0 = startCell.Y;

        int X1 = endCell.X;
        int Y1 = endCell.Y;

        Point start = new Point(X0, Y0);
        Point end = new Point(X1, Y1);

        List<Point> points = Pathfinding.getPoints(start, end);

        if(points == null)
            return null;


        foreach (var item in points)
        {
            result.Add(cells[item.X, item.Y]);
        }

        if(result.Contains(Cell.A))
            result.Remove(Cell.A);
        if (result.Contains(Cell.B))
            result.Remove(Cell.B);

        return result;
    }

    //проверка на пределы массива
    public static bool isPositionValid(int X, int Y)
    {
        return X > -1 && X < MAPSIZEX && Y > -1 && Y < MAPSIZEY;
    }

    //перечисление инструментов
    public enum BrushType
    {
        PENCIL, PEN, ERASER, BUCKET, FILLUP, FILLRIGHT, FILLBOTTOM, FILLLEFT, LINE, SET_A, SET_B, MATHFUNCTION, SET_AUTOMATON
    }

    //контейнер для сохранения данных
    [Serializable]
    public class SaveData
    {
        public bool[,] cell_states;

        public SaveData()
        {
            cell_states = new bool[MAPSIZEX,MAPSIZEY];
        }
    }
}

