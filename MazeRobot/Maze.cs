using System.Drawing;

namespace MazeRobot
{
    public class Maze
    {
        // Свойства лабиринта
        public int Width { get; private set; }      // Ширина лабиринта в клетках
        public int Height { get; private set; }     // Высота лабиринта в клетках
        public int CellSize { get; private set; }   // Размер одной клетки в пикселях
        public CellType[,] Grid { get; private set; }  // Двумерный массив клеток лабиринта
        public Position? StartPos { get; private set; } // Позиция старта (может быть null)
        public Position? EndPos { get; private set; }   // Позиция финиша (может быть null)

        // Конструктор лабиринта
        public Maze(int width, int height, int cellSize = 40)
        {
            Width = width;          // Устанавливаем ширину
            Height = height;        // Устанавливаем высоту
            CellSize = cellSize;    // Устанавливаем размер клетки
            Grid = new CellType[height, width];  // Создаем массив клеток
            Reset();                // Инициализируем начальное состояние
        }

        // Сброс лабиринта к начальному состоянию
        public void Reset()
        {
            Grid = new CellType[Height, Width];  // Создаем новый пустой массив
            StartPos = null;        // Сбрасываем позицию старта
            EndPos = null;          // Сбрасываем позицию финиша
        }

        // Установка типа клетки в указанной позиции
        public void SetCell(int x, int y, CellType cellType)
        {
            // Проверяем, что координаты в пределах лабиринта
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                // Если устанавливаем Empty и это текущая стартовая позиция
                if (cellType == CellType.Empty)
                {
                    // Очищаем StartPos если это стартовая клетка
                    if (StartPos.HasValue && StartPos.Value.X == x && StartPos.Value.Y == y)
                    {
                        StartPos = null;
                    }

                    // Очищаем EndPos если это конечная клетка
                    if (EndPos.HasValue && EndPos.Value.X == x && EndPos.Value.Y == y)
                    {
                        EndPos = null;
                    }
                }
                // Если устанавливаем стартовую клетку
                else if (cellType == CellType.Start)
                {
                    // Если уже есть стартовая позиция, очищаем ее
                    if (StartPos.HasValue)
                    {
                        var oldPos = StartPos.Value;
                        Grid[oldPos.Y, oldPos.X] = CellType.Empty;  // Очищаем старую клетку
                    }
                    StartPos = new Position(x, y);
                }
                // Если устанавливаем конечную клетку
                else if (cellType == CellType.End)
                {
                    // Если уже есть конечная позиция, очищаем ее
                    if (EndPos.HasValue)
                    {
                        var oldPos = EndPos.Value;
                        Grid[oldPos.Y, oldPos.X] = CellType.Empty;  // Очищаем старую клетку
                    }
                    EndPos = new Position(x, y);
                }

                Grid[y, x] = cellType;  // Устанавливаем тип клетки в сетке
            }
        }

        // Проверка готовности лабиринта к поиску пути
        public bool IsReadyForSearch()
        {
            // Лабиринт готов если установлены и старт и финиш
            return StartPos.HasValue && EndPos.HasValue;
        }

        // Получение цвета для отрисовки клетки в зависимости от ее типа
        public Color GetCellColor(CellType cellType)
        {
            // Возвращаем цвет в зависимости от типа клетки
            switch (cellType)
            {
                case CellType.Empty:
                    return Color.White;      // Белый для пустых клеток
                case CellType.Wall:
                    return Color.Black;      // Черный для стен
                case CellType.Start:
                    return Color.Green;      // Зеленый для старта
                case CellType.End:
                    return Color.Red;        // Красный для финиша
                case CellType.Visited:
                    return Color.LightBlue;  // Голубой для посещенных клеток
                case CellType.Path:
                    return Color.Yellow;     // Желтый для клеток пути
                default:
                    return Color.White;      // Белый по умолчанию
            }
        }
    }
}