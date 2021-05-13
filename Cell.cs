using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

//клеточка
class Cell
{
    //последняя выбранная клеточка
    public static Cell lastSelectedCell;

    //точка A
    public static Cell A;
    //точка B
    public static Cell B;

    public static Cell AUTOMATON_START;

    //клеточка, с которой начинается линия для Main.line();
    public static Cell lineStartCell;

    //положение клеточки
    //буквы заглавные, потому что [X, Y]
    //принадлежат системе координат
    //массива клеточек Main.cells
    //а не области отрисоки или экрана
    public int X;
    public int Y;

    //состояние клеточки
    public bool isBlocked;
    //имеет ли границу клеточка
    //нужна для отрисовки сетки
    private bool isHasBorder = true;

    //прямоугольник клеточки
    private Rectangle rect;
    //текст клеточки
    private TextBlock text;

    //тип точки на клеточке
    private PointType pointType;

    public Cell(int X, int Y)
    {
        this.X = X;
        this.Y = Y;

        rect = new Rectangle();

        rect.Width = Main.CELLSIZE;
        rect.Height = Main.CELLSIZE;

        rect.Stroke = Brushes.Gray;
        rect.Fill = Brushes.White;

        Canvas.SetLeft(rect, X * Main.CELLSIZE);
        Canvas.SetTop(rect, Y * Main.CELLSIZE);

        pointType = PointType.NONE;

        lastSelectedCell = this;
    }

    //заблокировать клетоку, т.е. сделать непроходимой
    public void setBlocked()
    {
        if (pointType != PointType.NONE)
            return;

        isBlocked = true;

        rect.Fill = Brushes.Black;
    }

    //разблокировать клетоку, т.е. сделать проходимой
    public void setUnblocked()
    {
        if (pointType != PointType.NONE)
            return;

        isBlocked = false;

        rect.Fill = Brushes.White;
    }

    //отметить клеточку выбранной
    //выбранными становятся клеточки под курсором
    public void setSelected()
    {
        if (isBlocked || pointType != PointType.NONE || lineStartCell == this)
            return;

        lastSelectedCell = this;

        rect.Fill = Brushes.LightCoral;
    }

    //убрать отметку выбора с клеточки
    public void setUnselected()
    {
        if (isBlocked || pointType != PointType.NONE || lineStartCell == this)
            return;

        rect.Fill = Brushes.White;
    }

    //окрасить клеточку в цвет пути
    public void setPathPoint()
    {
        rect.Fill = Brushes.Aquamarine;
    }

    //сделать клеточку стартовой для линии
    public void setLineStart()
    {
        if (lineStartCell == null)
        {
            lineStartCell = this;

            rect.Fill = Brushes.LightBlue;
        }
    }

    //сделать клеточку видимой
    public void setVisible()
    {
        if (Main.canvas.Children.Contains(rect))
            return;

        Main.canvas.Children.Add(rect);

        //if (A == this)
        //    set_A();
        //else if (B == this)
        //    set_B();
    }

    //скрыть клеточку
    public void setInvisible()
    {
        if (!Main.canvas.Children.Contains(rect))
            return;

        Main.canvas.Children.Remove(rect);

        //if (text !=null && !Main.canvas.Children.Contains(text))
        //    return;

        //Main.canvas.Children.Remove(text);
    }

    //очистить клеточку
    public void clear()
    {
        removePoint();
        setUnblocked();
    }

    //убрать точку A/B с клеточки
    public void removePoint()
    {
        if (text != null && Main.canvas.Children.Contains(text))
        {
            if (pointType == PointType.A)
                A = null;

            if (pointType == PointType.B)
                B = null;

            Main.canvas.Children.Remove(text);

            pointType = PointType.NONE;
        }
    }

    //сделать клеточку точкой A или B
    public void setPoint(PointType type)
    {
        switch (type)
        {
            case PointType.A:
                if (A != null && A != this)
                    A.clear();
                if (pointType == PointType.B)
                    clear();
                if (pointType == PointType.NONE)
                    set_A();
                break;
            case PointType.B:
                if (B != null && B != this)
                    B.clear();
                if (pointType == PointType.B)
                    clear();
                if (pointType == PointType.NONE)
                    set_B();
                break;
            case PointType.AUTOMATON_START:
                if (AUTOMATON_START != null && AUTOMATON_START != this)
                    AUTOMATON_START.clear();
                if (pointType == PointType.AUTOMATON_START)
                    clear();
                if (pointType == PointType.NONE)
                    set_automaton_start();
                break;
        }
    }

    //сделать клеточку точкой A
    private void set_A()
    {
        text = new TextBlock();

        text.Foreground = Brushes.Gray;

        Canvas.SetLeft(text, X * Main.CELLSIZE + Main.CELLSIZE / 4);
        Canvas.SetTop(text, Y * Main.CELLSIZE);

        text.Text = "A";

        Main.canvas.Children.Add(text);

        rect.Fill = Brushes.Lime;

        pointType = PointType.A;

        A = this;
    }

    //сделать клеточку точкой B
    private void set_B()
    {
        text = new TextBlock();

        text.Foreground = Brushes.Gray;

        Canvas.SetLeft(text, X * Main.CELLSIZE + Main.CELLSIZE / 4);
        Canvas.SetTop(text, Y * Main.CELLSIZE);

        text.Text = "B";

        Main.canvas.Children.Add(text);

        rect.Fill = Brushes.Orange;

        pointType = PointType.B;

        B = this;
    }

    private void set_automaton_start()
    {
        text = new TextBlock();

        text.Foreground = Brushes.Gray;

        Canvas.SetLeft(text, X * Main.CELLSIZE + Main.CELLSIZE / 4);
        Canvas.SetTop(text, Y * Main.CELLSIZE);

        text.Text = "A";

        Main.canvas.Children.Add(text);

        rect.Fill = Brushes.Blue;

        pointType = PointType.AUTOMATON_START;

        AUTOMATON_START = this;
    }

    //переключить отображение границы клеточки
    public void switchBorder()
    {
        isHasBorder = !isHasBorder;

        rect.StrokeThickness = isHasBorder ? 1 : 0;
    }

    //перечисление типов точек для клеточки
    public enum PointType { A, B, NONE, AUTOMATON_START }
}

