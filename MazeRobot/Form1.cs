using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MazeRobot
{
    public partial class Form1 : Form
    {
        // Основные компоненты приложения
        private Maze maze;          // Объект лабиринта
        private Robot robot;        // Объект робота для поиска пути
        private UIState uiState;    // Текущее состояние интерфейса
        private CellType selectedTool; // Выбранный инструмент для редактирования

        // Константы размеров лабиринта
        private const int MazeWidth = 15;   // Ширина лабиринта в клетках
        private const int MazeHeight = 15;  // Высота лабиринта в клетках
        private const int CellSize = 40;    // Размер одной клетки в пикселях

        // Компоненты для анимации и отображения
        private System.Windows.Forms.Timer animationTimer;  // Таймер для анимации поиска пути
        private Font infoFont;         // Шрифт для отображения информации

        // Конструктор формы
        public Form1()
        {
            InitializeComponent();    // Инициализация компонентов дизайнера
            InitializeApplication();  // Наша кастомная инициализация
        }

        // Инициализация приложения
        private void InitializeApplication()
        {
            // Создаем объекты лабиринта и робота
            maze = new Maze(MazeWidth, MazeHeight, CellSize);
            robot = new Robot(MazeWidth, MazeHeight);

            // Начальные настройки интерфейса
            uiState = UIState.Editing;       // Начинаем в режиме редактирования
            selectedTool = CellType.Wall;    // По умолчанию выбран инструмент "Стена"

            // Настраиваем таймер для анимации
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 100;   // Интервал 100 мс
            animationTimer.Tick += AnimationTimer_Tick;  // Подписываемся на событие тика

            // Настраиваем шрифт для информации
            infoFont = new Font("Arial", 10);

            // Настраиваем свойства формы
            this.Text = "Робот в лабиринте - Алгоритм A*";  // Заголовок окна
            this.ClientSize = new Size(800, 700);           // Начальный размер
            this.DoubleBuffered = true;                     // Включаем двойную буферизацию
            this.BackColor = Color.Gray;                    // Цвет фона
        }

        // Обработчик перерисовки формы
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);    // Вызываем базовый метод
            Render(e.Graphics); // Вызываем наш метод отрисовки
        }

        // Обработчик нажатия кнопки мыши
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);    // Вызываем базовый метод
            HandleMouseClick(e);    // Вызываем наш обработчик клика
        }

        // Обработчик нажатия клавиши
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);    // Вызываем базовый метод
            HandleKeyPress(e);    // Вызываем наш обработчик клавиш
        }

        // Обработчик клика мыши для редактирования лабиринта
        private void HandleMouseClick(MouseEventArgs e)
        {
            if (uiState != UIState.Editing)  // Работаем только в режиме редактирования
                return;

            // Преобразуем координаты мыши в координаты сетки
            int gridX = e.X / CellSize;
            int gridY = e.Y / CellSize;

            // Проверяем, что клик в пределах лабиринта
            if (gridX >= 0 && gridX < MazeWidth && gridY >= 0 && gridY < MazeHeight)
            {
                if (e.Button == MouseButtons.Left)  // Левая кнопка - установка элемента
                {
                    maze.SetCell(gridX, gridY, selectedTool);
                }
                else if (e.Button == MouseButtons.Right)  // Правая кнопка - очистка
                {
                    maze.SetCell(gridX, gridY, CellType.Empty);
                }

                this.Invalidate();  // Перерисовываем форму
            }
        }

        // Обработчик нажатия клавиш для управления
        private void HandleKeyPress(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D1:
                    selectedTool = CellType.Wall;   // Клавиша 1 - инструмент "Стена"
                    break;
                case Keys.D2:
                    selectedTool = CellType.Start;  // Клавиша 2 - инструмент "Старт"
                    break;
                case Keys.D3:
                    selectedTool = CellType.End;    // Клавиша 3 - инструмент "Финиш"
                    break;
                case Keys.Space when uiState == UIState.Editing:
                    StartAlgorithm();  // Пробел - запуск алгоритма (только в режиме редактирования)
                    break;
                case Keys.P when uiState == UIState.ActiveSearch:
                    uiState = UIState.Paused;  // P - пауза (только во время поиска)
                    animationTimer.Stop();
                    break;
                case Keys.C when uiState == UIState.Paused:
                    uiState = UIState.ActiveSearch;  // C - продолжить (только в паузе)
                    animationTimer.Start();
                    break;
                case Keys.R:
                    ResetToEditing();  // R - сброс в режим редактирования
                    break;
            }

            this.Invalidate();  // Перерисовываем форму
        }

        // Запуск алгоритма поиска пути
        private void StartAlgorithm()
        {
            if (!maze.IsReadyForSearch())  // Проверяем, установлены ли старт и финиш
            {
                MessageBox.Show("Установите стартовую и конечную точки!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Переходим по состояниям: редактирование -> выполнение -> активный поиск
            uiState = UIState.AlgorithmExecution;
            robot.Initialize(maze.StartPos.Value, maze.EndPos.Value);
            uiState = UIState.ActiveSearch;
            animationTimer.Start();  // Запускаем таймер анимации
        }

        // Сброс в режим редактирования
        private void ResetToEditing()
        {
            uiState = UIState.Editing;  // Возвращаемся в режим редактирования
            maze.Reset();               // Сбрасываем лабиринт
            robot.Reset();              // Сбрасываем робота
            animationTimer.Stop();      // Останавливаем таймер
            this.Invalidate();          // Перерисовываем форму
        }

        // Обработчик тика таймера для анимации
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (uiState == UIState.ActiveSearch)  // Работаем только в активном поиске
            {
                UpdateAlgorithm();  // Выполняем шаг алгоритма
                this.Invalidate();  // Перерисовываем форму
            }
        }

        // Обновление состояния алгоритма (один шаг A*)
        private void UpdateAlgorithm()
        {
            // Выполняем один шаг алгоритма A*
            bool continueSearch = robot.PerformSearchStep(maze.Grid);

            // Визуализируем текущее состояние поиска
            UpdateVisualization();

            // Проверяем завершение поиска
            if (robot.State == RobotState.PathFound || robot.State == RobotState.PathNotFound)
            {
                uiState = UIState.ResultDisplay;  // Переходим к отображению результата
                animationTimer.Stop();            // Останавливаем таймер

                if (robot.State == RobotState.PathFound)
                {
                    ShowFinalPath();  // Показываем найденный путь
                    MessageBox.Show("Путь найден!", "Результат",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Путь не найден!", "Результат",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        // Обновление визуализации (заглушка)
        private void UpdateVisualization()
        {
        }

        // Отображение найденного пути
        private void ShowFinalPath()
        {
            var path = robot.ReconstructPath();  // Восстанавливаем путь
            foreach (var pos in path)
            {
                // Помечаем клетки пути (кроме старта и финиша)
                if (maze.Grid[pos.Y, pos.X] != CellType.Start &&
                    maze.Grid[pos.Y, pos.X] != CellType.End)
                {
                    maze.Grid[pos.Y, pos.X] = CellType.Path;
                }
            }
        }

        // Основной метод отрисовки
        private void Render(Graphics g)
        {
            // Очищаем экран
            g.Clear(Color.Gray);

            // Отрисовываем все компоненты
            DrawMaze(g);        // Лабиринт
            DrawStateInfo(g);   // Информацию о состоянии
            DrawControls(g);    // Управление
        }

        // Отрисовка лабиринта
        private void DrawMaze(Graphics g)
        {
            for (int y = 0; y < MazeHeight; y++)
            {
                for (int x = 0; x < MazeWidth; x++)
                {
                    // Создаем прямоугольник для клетки
                    var rect = new Rectangle(
                        x * CellSize,
                        y * CellSize,
                        CellSize,
                        CellSize
                    );

                    // Заливаем клетку цветом в зависимости от типа
                    var cellType = maze.Grid[y, x];
                    var color = maze.GetCellColor(cellType);
                    using (var brush = new SolidBrush(color))
                    {
                        g.FillRectangle(brush, rect);
                    }

                    // Рисуем рамку вокруг клетки
                    g.DrawRectangle(Pens.DarkGray, rect);
                }
            }
        }

        // Отрисовка информации о состоянии
        private void DrawStateInfo(Graphics g)
        {
            int infoY = MazeHeight * CellSize + 10;  // Позиция под лабиринтом

            // Состояние интерфейса
            string uiStateText = $"Состояние UI: {uiState}";
            g.DrawString(uiStateText, infoFont, Brushes.White, 10, infoY);

            // Состояние робота
            string robotStateText = $"Состояние робота: {robot.GetStateDescription()}";
            g.DrawString(robotStateText, infoFont, Brushes.White, 10, infoY + 25);

            // Выбранный инструмент (только в режиме редактирования)
            if (uiState == UIState.Editing)
            {
                string toolText = $"Инструмент: {selectedTool}";
                g.DrawString(toolText, infoFont, Brushes.White, 10, infoY + 50);
            }
        }

        // Отрисовка информации об управлении
        private void DrawControls(Graphics g)
        {
            var controls = new[]
            {
                "Управление:",
                "1 - Стена, 2 - Старт, 3 - Финиш",
                "Пробел - Запуск поиска",
                "P - Пауза, C - Продолжить",
                "R - Сброс",
                "ЛКМ - Разместить, ПКМ - Удалить"
            };

            // Позиция справа от лабиринта
            int startX = MazeWidth * CellSize + 20;  // Отступ 20px от лабиринта
            int startY = 20;                         // Отступ сверху 20px

            // Рисуем каждую строку управления
            for (int i = 0; i < controls.Length; i++)
            {
                g.DrawString(controls[i], infoFont, Brushes.LightGray, startX, startY + i * 25);
            }
        }

        // Обработчик загрузки формы
        private void Form1_Load(object sender, EventArgs e)
        {
            // Рассчитываем оптимальный размер формы:

            // Ширина = ширина лабиринта + место для надписей + отступы
            int formWidth = (MazeWidth * CellSize) + 250 + 20;

            // Высота = высота лабиринта + место для информации + отступы
            int formHeight = (MazeHeight * CellSize) + 100 + 20;

            // Устанавливаем размер формы
            this.ClientSize = new Size(formWidth, formHeight);

            // Фиксируем размер формы (запрещаем изменение)
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
        }
    }
}