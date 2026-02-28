using System;

namespace MazeRobot
{
    // Типы клеток лабиринта
    public enum CellType
    {
        Empty = 0,      // Пустая клетка
        Wall = 1,       // Стена - непроходимое препятствие
        Start = 2,      // Начальная точка
        End = 3,        // Конечная точка
        Visited = 4,    // Посещенная клетка во время поиска
        Path = 5        // Клетка найденного пути
    }

    // Направления движения робота
    public enum Direction
    {
        Up,    // Вверх
        Down,  // Вниз
        Left,  // Влево
        Right  // Вправо
    }

    // Состояния робота согласно диаграмме состояний
    public enum RobotState
    {
        Initialization,     // Начальная инициализация
        PathSearch,         // Поиск пути
        CalculateNextCell,  // Вычисление следующей клетки
        CheckCell,          // Проверка клетки
        PathFound,          // Путь найден
        PathNotFound        // Путь не найден
    }

    // Состояния пользовательского интерфейса
    public enum UIState
    {
        Editing,            // Режим редактирования
        AlgorithmExecution, // Выполнение алгоритма
        ActiveSearch,       // Активный поиск
        Paused,             // Пауза
        ResultDisplay       // Отображение результата
    }

    // Структура для хранения координат позиции
    public struct Position
    {
        public int X { get; set; }  // Координата X (горизонталь)
        public int Y { get; set; }  // Координата Y (вертикаль)

        // Конструктор создания позиции
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        // Проверка равенства позиций
        public override bool Equals(object obj)
        {
            return obj is Position position &&
                   X == position.X &&
                   Y == position.Y;
        }

        // Получение хэш-кода для использования в словарях
        public override int GetHashCode()
        {
            unchecked // Разрешить переполнение целых чисел
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                return hash;
            }
        }

        // Оператор равенства
        public static bool operator ==(Position left, Position right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        // Оператор неравенства
        public static bool operator !=(Position left, Position right)
        {
            return !(left == right);
        }
    }

    // Класс узла для алгоритма A* (хранит информацию о клетке)
    public class PathNode : IComparable<PathNode>
    {
        public Position Position { get; set; }  // Позиция узла
        public double FScore { get; set; }      // Общая стоимость (G + H)
        public double GScore { get; set; }      // Стоимость от старта

        // Сравнение узлов по FScore для приоритетной очереди
        public int CompareTo(PathNode other)
        {
            return FScore.CompareTo(other.FScore);
        }
    }
}