using System;
using System.Collections.Generic;
using System.Linq;

namespace MazeRobot
{
    public class Robot
    {
        public RobotState State { get; set; }  // Текущее состояние робота
        public Position CurrentPosition { get; private set; }  // Текущая позиция робота

        private Position startPos;  // Стартовая позиция
        private Position endPos;    // Конечная позиция
        private int mazeWidth;      // Ширина лабиринта
        private int mazeHeight;     // Высота лабиринта

        // Структуры данных для алгоритма A*
        private HashSet<Position> visited;                    // Посещенные клетки
        private Dictionary<Position, Position> cameFrom;      // Откуда пришли в каждую клетку
        private Dictionary<Position, double> gScore;          // Стоимость пути от старта
        private Dictionary<Position, double> fScore;          // Общая стоимость (g + h)
        private SortedDictionary<double, Queue<Position>> openSet;  // Очередь с приоритетом

        // Конструктор робота
        public Robot(int mazeWidth, int mazeHeight)
        {
            this.mazeWidth = mazeWidth;
            this.mazeHeight = mazeHeight;
            Reset();  // Инициализируем начальное состояние
        }

        // Сброс робота в начальное состояние
        public void Reset()
        {
            State = RobotState.Initialization;  // Устанавливаем состояние "Инициализация"
            startPos = new Position();          // Сбрасываем стартовую позицию
            endPos = new Position();            // Сбрасываем конечную позицию
            CurrentPosition = new Position();   // Сбрасываем текущую позицию

            // Инициализируем все структуры данных заново
            visited = new HashSet<Position>();
            cameFrom = new Dictionary<Position, Position>();
            gScore = new Dictionary<Position, double>();
            fScore = new Dictionary<Position, double>();
            openSet = new SortedDictionary<double, Queue<Position>>();
        }

        // Инициализация робота для начала поиска
        public void Initialize(Position start, Position end)
        {
            startPos = start;           // Устанавливаем стартовую позицию
            endPos = end;               // Устанавливаем конечную позицию
            CurrentPosition = start;    // Начинаем со стартовой позиции
            State = RobotState.PathSearch;  // Переходим в состояние поиска пути

            // Очищаем все структуры данных от предыдущих запусков
            visited.Clear();
            cameFrom.Clear();
            gScore.Clear();
            fScore.Clear();
            openSet.Clear();

            // Инициализируем начальные значения для алгоритма A*
            gScore[start] = 0;                                  // Стоимость пути до старта = 0
            fScore[start] = Heuristic(start, end);              // Общая стоимость = эвристика
            Enqueue(start, fScore[start]);                      // Добавляем старт в очередь
        }

        // Эвристическая функция (манхэттенское расстояние)
        private double Heuristic(Position a, Position b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);  // Сумма разниц по X и Y
        }

        // Получение соседних клеток (сенсорный интерфейс)
        public List<Position> GetNeighbors(Position pos, CellType[,] maze)
        {
            var neighbors = new List<Position>();

            // Получаем реальные размеры массива maze
            int mazeHeight = maze.GetLength(0); // Количество строк (Y)
            int mazeWidth = maze.GetLength(1);  // Количество столбцов (X)

            var directions = new (int, int)[]
                {
                (0, -1),  // Вверх
                (0, 1),   // Вниз
                (-1, 0),  // Влево
                (1, 0)    // Вправо
                    };

            foreach (var (dx, dy) in directions)
            {
                int newX = pos.X + dx;
                int newY = pos.Y + dy;

                // Используем реальные размеры массива
                if (newX >= 0 && newX < mazeWidth && newY >= 0 && newY < mazeHeight)
                {
                    if (maze[newY, newX] != CellType.Wall)
                    {
                        neighbors.Add(new Position(newX, newY));
                    }
                }
            }

            return neighbors;
        }

        // Выполнение одного шага алгоритма A*
        public bool PerformSearchStep(CellType[,] maze)
        {
            if (State != RobotState.PathSearch)  // Проверяем, что мы в состоянии поиска
                return false;

            // Проверяем, есть ли еще клетки для исследования
            if (openSet.Count == 0)
            {
                State = RobotState.PathNotFound;  // Если нет - путь не найден
                return false;
            }

            // Выбираем клетку с наименьшей общей стоимостью
            State = RobotState.CalculateNextCell;  // Переходим в состояние вычисления
            CurrentPosition = Dequeue();           // Извлекаем лучшую клетку из очереди

            // Проверяем, не достигли ли мы цели
            if (CurrentPosition.Equals(endPos))
            {
                State = RobotState.PathFound;  // Если да - путь найден
                return true;
            }

            visited.Add(CurrentPosition);     // Помечаем текущую клетку как посещенную
            State = RobotState.CheckCell;     // Переходим в состояние проверки

            // Получаем всех соседей текущей клетки
            var neighbors = GetNeighbors(CurrentPosition, maze);
            foreach (var neighbor in neighbors)
            {
                if (visited.Contains(neighbor))  // Пропускаем уже посещенных соседей
                    continue;

                // Вычисляем стоимость пути до соседа через текущую клетку
                double currentGScore;
                if (!gScore.TryGetValue(CurrentPosition, out currentGScore))
                {
                    currentGScore = double.MaxValue;  // Если нет значения, используем максимум
                }
                double tentativeGScore = currentGScore + 1;  // Стоимость увеличивается на 1

                // Получаем текущую стоимость соседа
                double neighborGScore;
                if (!gScore.TryGetValue(neighbor, out neighborGScore))
                {
                    neighborGScore = double.MaxValue;  // Если нет значения, используем максимум
                }

                // Если нашли лучший путь до соседа
                if (tentativeGScore < neighborGScore)
                {
                    // Обновляем информацию о пути
                    cameFrom[neighbor] = CurrentPosition;                     // Запоминаем, откуда пришли
                    gScore[neighbor] = tentativeGScore;                       // Обновляем стоимость пути
                    fScore[neighbor] = tentativeGScore + Heuristic(neighbor, endPos);  // Обновляем общую стоимость

                    // Добавляем соседа в очередь, если его там еще нет
                    if (!OpenSetContains(neighbor))
                    {
                        Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }

            State = RobotState.PathSearch;  // Возвращаемся в состояние поиска
            return true;                    // Продолжаем поиск
        }

        // Восстановление найденного пути
        public List<Position> ReconstructPath()
        {
            if (State != RobotState.PathFound)  // Проверяем, что путь найден
                return new List<Position>();

            var path = new List<Position>();  // Создаем список для пути
            Position current = endPos;        // Начинаем с конечной точки

            // Проходим по цепочке cameFrom от конца к началу
            while (cameFrom.ContainsKey(current))
            {
                path.Add(current);            // Добавляем текущую позицию в путь
                current = cameFrom[current];  // Переходим к предыдущей позиции
            }

            path.Add(startPos);  // Добавляем стартовую позицию
            path.Reverse();      // Разворачиваем путь (от старта к финишу)
            return path;         // Возвращаем готовый путь
        }

        // Получение текстового описания текущего состояния
        public string GetStateDescription()
        {
            switch (State)  // Возвращаем описание в зависимости от состояния
            {
                case RobotState.Initialization:
                    return "Инициализация";
                case RobotState.PathSearch:
                    return "Поиск пути";
                case RobotState.CalculateNextCell:
                    return "Вычисление следующей клетки";
                case RobotState.CheckCell:
                    return "Проверка клетки";
                case RobotState.PathFound:
                    return "Путь найден";
                case RobotState.PathNotFound:
                    return "Путь не найден";
                default:
                    return "Неизвестное состояние";
            }
        }

        // Добавление позиции в очередь с приоритетом
        private void Enqueue(Position position, double priority)
        {
            if (!openSet.ContainsKey(priority))  // Если нет очереди для этого приоритета
            {
                openSet[priority] = new Queue<Position>();  // Создаем новую очередь
            }
            openSet[priority].Enqueue(position);  // Добавляем позицию в очередь
        }

        // Извлечение позиции с наивысшим приоритетом
        private Position Dequeue()
        {
            var firstPair = openSet.First();     // Берем первый элемент (с наименьшим ключом)
            var queue = firstPair.Value;         // Получаем очередь
            var position = queue.Dequeue();      // Извлекаем позицию из очереди

            if (queue.Count == 0)                // Если очередь пуста
            {
                openSet.Remove(firstPair.Key);   // Удаляем ее из словаря
            }

            return position;  // Возвращаем извлеченную позицию
        }

        // Проверка, пуста ли очередь
        private bool OpenSetIsEmpty()
        {
            return openSet.Count == 0;  // True если словарь пуст
        }

        // Проверка, содержится ли позиция в очереди
        private bool OpenSetContains(Position position)
        {
            // Проходим по всем очередям в словаре
            foreach (var queue in openSet.Values)
            {
                if (queue.Contains(position))  // Если нашли позицию
                    return true;               // Возвращаем true
            }
            return false;  // Если не нашли - возвращаем false
        }
    }
}