using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MazeRobot
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();// Включает визуальные стили для приложения
            Application.SetCompatibleTextRenderingDefault(false);// Устанавливает совместимость отрисовки текста
            Application.Run(new Form1());// Запускает приложение и создает главную форму Form1
        }
    }
}
