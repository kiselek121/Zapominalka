
using System;              
using System.Collections.Generic;   
using System.Drawing;     
using System.Linq;         
using System.Windows.Forms; 


namespace КР               
{
    
    public partial class Form1 : Form   
    {
        //ПЕРЕМЕННЫЕ НАСТРОЕК СЛОЖНОСТИ
        private int gridSize = 3;        // Размер сетки: 3x3, 4x4 или 5x5
        private int showTimeMs = 2000;   // Время показа кружков в миллисекундах (2 сек, 1.5 сек, 1 сек)
        private int answerTimeSec = 5;   // Время на ответ в секундах (5 сек, 4 сек, 3 сек)
        private int initialCircles = 1;  // Начальное количество кружков в игре (1, 2 или 3)

        //ПЕРЕМЕННЫЕ СОСТОЯНИЯ ИГРЫ
        private int circlesCount;         // Текущее количество кружков в этом раунде
        private List<Point> currentCircles = new List<Point>();   // Список координат где были кружки 
        private List<Point> userAnswers = new List<Point>();      // Список координат которые отметил игрок
        private int errorsInRow = 0;       // Счётчик ошибок подряд 
        private bool canPlay = false;      // Можно ли кликать по клеткам
        private bool isShowing = false;    // Показываются ли кружки в данный момент
        private bool gameStarted = false;  // Начата ли игра

        //ТАЙМЕРЫ
        private Timer showTimer;           // Таймер, который скрывает кружки через N секунд
        private Timer answerTimer;         // Таймер обратного отсчёта времени на ответ
        private int timeLeft;              // Сколько секунд осталось на ответ

        //ГРОВОЕ ПОЛЕ
        private DataGridView dgv;          // Таблица для отображения игрового поля

        //КОНСТРУКТОР ФОРМЫ
        public Form1()
        {
            InitializeComponent();         // Инициализация элементов формы

            CreateGameField();             // Создаём DataGridView

            //ПРИВЯЗКА СОБЫТИЙ К МЕТОДАМ
            radioButton1.CheckedChanged += RadioButton_CheckedChanged;   // При выборе Низкий
            radioButton2.CheckedChanged += RadioButton_CheckedChanged;   // При выборе Средний
            radioButton3.CheckedChanged += RadioButton_CheckedChanged;   // При выборе Высокий
            button1.Click += Button1_Click;                              // При нажатии на кнопку "Запуск" - вызываем метод

            ApplyDifficultyAndRebuild();   // Применяем настройки сложности и создаём сетку
        }

        //МЕТОД СОЗДАНИЯ ИГРОВОГО ПОЛЯ
        private void CreateGameField()
        {
            dgv = new DataGridView();                      // Создаём новый объект таблицы
            dgv.Location = new Point(20, 140);            // Устанавливаем положение на форме (X=20, Y=140)
            dgv.Size = new Size(380, 380);                // Устанавливаем размер поля (ширина 380, высота 380)
            dgv.AllowUserToAddRows = false;               // Запрещаем пользователю добавлять новые строки
            dgv.RowHeadersVisible = false;                // Скрываем серые заголовки строк (слева)
            dgv.ColumnHeadersVisible = false;             // Скрываем серые заголовки столбцов (сверху)
            dgv.ScrollBars = ScrollBars.None;             // Отключаем полосы прокрутки
            dgv.DefaultCellStyle.SelectionBackColor = Color.White;   // Убираем синее выделение
            dgv.ReadOnly = true;                          // Запрещаем редактирование ячеек
            dgv.CellClick += Dgv_CellClick;               // При клике на ячейку - вызываем метод Dgv_CellClick
            dgv.AllowUserToResizeColumns = false;         // Запрещаем изменение ширины столбцов
            dgv.AllowUserToResizeRows = false;            // Запрещаем изменение высоты строк
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;  // Запрет изменения высоты заголовков
            dgv.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;          // Запрет изменения ширины заголовков
            this.Controls.Add(dgv);                       // Добавляем созданную таблицу на форму
        }

        //МЕТОД ПРИМЕНЕНИЯ НАСТРОЕК СЛОЖНОСТИ
        private void ApplyDifficultyAndRebuild()
        {
            //ОПРЕДЕЛЯЕМ ПАРАМЕТРЫ В ЗАВИСИМОСТИ ОТ ВЫБРАННОЙ РАДИОКНОПКИ
            if (radioButton1.Checked)        // Если выбран Низкий уровень
            {
                gridSize = 3;                // Сетка 3x3
                showTimeMs = 2000;           // Кружки видны 2 секунды
                answerTimeSec = 5;           // На ответ 5 секунд
                initialCircles = 1;          // Начинаем с 1 кружка
            }
            else if (radioButton2.Checked)   // Если выбран Средний уровень
            {
                gridSize = 4;                // Сетка 4x4
                showTimeMs = 1500;           // Кружки видны 1.5 секунды
                answerTimeSec = 4;           // На ответ 4 секунды
                initialCircles = 2;          // Начинаем с 2 кружков
            }
            else if (radioButton3.Checked)   // Если выбран Высокий уровень
            {
                gridSize = 5;                // Сетка 5x5
                showTimeMs = 1000;           // Кружки видны 1 секунду
                answerTimeSec = 3;           // На ответ 3 секунды
                initialCircles = 3;          // Начинаем с 3 кружков
            }

            //ОБНОВЛЯЕМ ИГРОВЫЕ ПЕРЕМЕННЫЕ
            circlesCount = initialCircles;    // Количество кружков = начальному
            errorsInRow = 0;                  // Сбрасываем счётчик ошибок подряд
            label2.Text = $"Кружков: {circlesCount}";   // Обновляем надпись на форме

            //Выводим: показываем в заголовке окна текущий размер
            

            RebuildGrid();                    // Перестраиваем сетку с новым размером
        }

        //МЕТОД ПЕРЕСТРОЙКИ СЕТКИ (УДАЛЯЕМ И СОЗДАЁМ ЗАНОВО)
        private void RebuildGrid()
        {
            //ОСТАНАВЛИВАЕМ ВСЕ ТАЙМЕРЫ
            showTimer?.Stop();      // Останавливаем таймер показа кружков (если он существует)
            answerTimer?.Stop();    // Останавливаем таймер ответа (если он существует)

            //СБРАСЫВАЕМ СОСТОЯНИЕ ИГРЫ
            gameStarted = false;    // Игра не начата
            canPlay = false;        // Кликать по клеткам нельзя

            //ОЧИЩАЕМ СТАРЫЕ СТОЛБЦЫ И СТРОКИ
            dgv.Columns.Clear();    // Удаляем все столбцы
            dgv.Rows.Clear();       // Удаляем все строки

            //СОЗДАЁМ НОВЫЕ СТОЛБЦЫ
            for (int i = 0; i < gridSize; i++)   // Цикл от 0 до размера сетки
            {
                DataGridViewColumn col = new DataGridViewTextBoxColumn(); // Создаём новый столбец
                col.SortMode = DataGridViewColumnSortMode.NotSortable;    // Отключаем сортировку
                dgv.Columns.Add(col);            // Добавляем столбец в таблицу
            }

            dgv.RowCount = gridSize;    // Устанавливаем количество строк

            //ВЫЧИСЛЯЕМ РАЗМЕР ЯЧЕЙКИ
            int cellSize = 380 / gridSize;   // Делим ширину поля на количество ячеек
            if (cellSize < 40) cellSize = 40; // Минимальный размер ячейки - 40 пикселей

            //УСТАНАВЛИВАЕМ ОДИНАКОВУЮ ШИРИНУ И ВЫСОТУ ДЛЯ ВСЕХ ЯЧЕЕК
            for (int i = 0; i < gridSize; i++)   // Цикл по всем столбцам и строкам
            {
                dgv.Columns[i].Width = cellSize;   // Устанавливаем ширину столбца
                dgv.Rows[i].Height = cellSize;     // Устанавливаем высоту строки
            }

            //ОЧИЩАЕМ ВСЕ ЯЧЕЙКИ (ДЕЛАЕМ БЕЛЫМИ И ПУСТЫМИ)
            for (int row = 0; row < gridSize; row++)       // Перебираем все строки
            {
                for (int col = 0; col < gridSize; col++)   // Перебираем все столбцы
                {
                    dgv[col, row].Style.BackColor = Color.White;  // Белый фон
                    dgv[col, row].Value = "";                    // Пустое значение
                }
            }

            dgv.Refresh();   // Принудительно перерисовываем таблицу
        }

        // МЕТОД, ВЫЗЫВАЕМЫЙ ПРИ СМЕНЕ РАДИОКНОПКИ
        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;  // Преобразуем отправителя в RadioButton
            if (rb != null && rb.Checked)            // Если это RadioButton И он выбран
            {
                ApplyDifficultyAndRebuild();         // Применяем новую сложность и перестраиваем сетку
            }
        }

        // МЕТОД БЛОКИРОВКИ/РАЗБЛОКИРОВКИ ЭЛЕМЕНТОВ УПРАВЛЕНИЯ СЛОЖНОСТЬЮ
        private void SetDifficultyControlsEnabled(bool enabled)
        {
            radioButton1.Enabled = enabled;   // Низкий уровень
            radioButton2.Enabled = enabled;   // Средний уровень
            radioButton3.Enabled = enabled;   // Высокий уровень
        }

        //МЕТОД КНОПКИ "ЗАПУСК" 
        private void Button1_Click(object sender, EventArgs e)
        {
            // Останавливаем все таймеры на всякий случай
            showTimer?.Stop();
            answerTimer?.Stop();

            //БЛОКИРУЕМ ИЗМЕНЕНИЕ СЛОЖНОСТИ
            SetDifficultyControlsEnabled(false);   // Запрещаем переключение уровня сложности

            // НАЧИНАЕМ НОВУЮ ИГРУ 
            gameStarted = true;              // Игра началась
            errorsInRow = 0;                // Обнуляем счётчик ошибок подряд
            circlesCount = initialCircles;  // Устанавливаем начальное количество кружков
            label2.Text = $"Кружков: {circlesCount}";   // Обновляем надпись

            StartNewRound();                // Запускаем первый раунд
        }

        // МЕТОД НАЧАЛА НОВОГО РАУНДА
        private void StartNewRound()
        {
            answerTimer?.Stop();      // Останавливаем таймер ответа
            userAnswers.Clear();      // Очищаем список ответов игрока
            canPlay = false;          // Пока нельзя кликать (кружки ещё видны)
            isShowing = true;         // Кружки сейчас показываются

            // ОЧИЩАЕМ ПОЛЕ ОТ ПРЕДЫДУЩИХ ОТМЕТОК 
            for (int row = 0; row < gridSize; row++)        // Перебираем все строки
                for (int col = 0; col < gridSize; col++)    // Перебираем все столбцы
                {
                    dgv[col, row].Style.BackColor = Color.White;  // Белый фон
                    dgv[col, row].Value = "";                    // Пустое значение
                }

            //ГЕНЕРАЦИЯ СЛУЧАЙНЫХ ПОЗИЦИЙ ДЛЯ КРУЖКОВ 
            currentCircles.Clear();           // Очищаем список правильных позиций
            Random rnd = new Random();         // Создаём генератор случайных чисел

            // Создаём список всех возможных клеток
            List<Point> allCells = new List<Point>();
            for (int row = 0; row < gridSize; row++)        // Перебираем все строки
                for (int col = 0; col < gridSize; col++)    // Перебираем все столбцы
                    allCells.Add(new Point(row, col));      // Добавляем координаты (row, col)

            // Если кружков больше, чем клеток - уменьшаем до максимального
            if (circlesCount > allCells.Count)
                circlesCount = allCells.Count;

            // Выбираем случайные клетки: перемешиваем список и берём первые circlesCount
            currentCircles = allCells.OrderBy(x => rnd.Next()).Take(circlesCount).ToList();

            //ОТОБРАЖАЕМ КРУЖКИ НА ПОЛЕ 
            foreach (var p in currentCircles)     // Перебираем все правильные позиции
                dgv[p.Y, p.X].Style.BackColor = Color.LightGreen;  // Зелёный фон
     
            // БЛОКИРУЕМ КНОПКУ "ЗАПУСК" НА ВРЕМЯ ПОКАЗА
            button1.Enabled = false;
            button1.Text = "Идёт игра...";

            // ЗАПУСКАЕМ ТАЙМЕР ДЛЯ СКРЫТИЯ КРУЖКОВ 
            if (showTimer != null) showTimer.Dispose();   // Удаляем старый таймер
            showTimer = new Timer();                      // Создаём новый таймер
            showTimer.Interval = showTimeMs;              // Устанавливаем интервал (время показа)

            // Подписываемся на событие Tick (что делать, когда время вышло)
            showTimer.Tick += (s, ev) =>
            {
                showTimer.Stop();   // Останавливаем таймер

                //СКРЫВАЕМ КРУЖКИ
                for (int row = 0; row < gridSize; row++)        // Перебираем все строки
                    for (int col = 0; col < gridSize; col++)    // Перебираем все столбцы
                    {
                        dgv[col, row].Style.BackColor = Color.White;  // Белый фон
                        dgv[col, row].Value = "";                    // Убираем символ кружка
                    }

                isShowing = false;      // Кружки больше не показываются
                canPlay = true;         // Теперь можно кликать по клеткам
                StartAnswerTimer();     // Запускаем таймер ответа

                button1.Enabled = true;  // Активируем кнопку "Запуск"
                button1.Text = "Запуск";
            };
            showTimer.Start();   // ЗАПУСКАЕМ ТАЙМЕР
        }

        // МЕТОД ЗАПУСКА ТАЙМЕРА ОТВЕТА
        private void StartAnswerTimer()
        {
            timeLeft = answerTimeSec;               // Устанавливаем оставшееся время
            UpdateTimerDisplay();                   // Обновляем отображение таймера
            label1.ForeColor = Color.Lime;          // Зелёный цвет цифр

            answerTimer = new Timer();               // Создаём таймер
            answerTimer.Interval = 1000;             // Срабатывает каждую секунду

            // Подписываемся на событие Tick (что делать каждую секунду)
            answerTimer.Tick += (s, ev) =>
            {
                timeLeft--;                         // Уменьшаем оставшееся время на 1 секунду
                UpdateTimerDisplay();               // Обновляем отображение

                if (timeLeft <= 2)                  // Если осталось 2 секунды или меньше
                    label1.ForeColor = Color.Red;   // Меняем цвет на красный

                if (timeLeft <= 0)                  // Если время вышло
                {
                    answerTimer.Stop();             // Останавливаем таймер
                    AutoFail();                     // Автоматический проигрыш
                }
            };
            answerTimer.Start();    // ЗАПУСКАЕМ ТАЙМЕР!
        }

        // МЕТОД ОБНОВЛЕНИЯ ОТОБРАЖЕНИЯ ТАЙМЕРА 
        private void UpdateTimerDisplay()
        {
            int minutes = timeLeft / 60;             // Вычисляем минуты (деление нацело)
            int seconds = timeLeft % 60;             // Вычисляем секунды (остаток от деления)
            label1.Text = $"{minutes:00}:{seconds:00}";  // Формат ММ:СС (например "00:05")
        }

        //МЕТОД АВТОМАТИЧЕСКОГО ПРОИГРЫША (КОГДА ВРЕМЯ ВЫШЛО)
        private void AutoFail()
        {
            if (!canPlay || isShowing) return;   // Если уже нельзя кликать - выходим

            answerTimer?.Stop();    // Останавливаем таймер
            canPlay = false;        // Запрещаем дальнейшие клики

            // ПРАВИЛО ИЗМЕНЕНИЯ КРУЖКОВ ПРИ ОШИБКЕ
            errorsInRow++;          // Увеличиваем счётчик ошибок подряд

            if (errorsInRow >= 2)   // Если две ошибки подряд
            {
                circlesCount = Math.Max(1, circlesCount - 1);  // Уменьшаем (но не меньше 1)
                errorsInRow = 0;    // Сбрасываем счётчик ошибок
            }
            // Если ошибка первая - количество кружков не меняется

            label2.Text = $"Кружков: {circlesCount}";   // Обновляем надпись

            //ПРОВЕРКА НА ПОБЕДУ
            if (circlesCount > gridSize * gridSize)   // Если кружков больше, чем клеток
            {
                MessageBox.Show($"ПОБЕДА! Вы дошли до {circlesCount} кружков!", "Поздравляем!");
                ResetGameState();   // Сбрасываем игру
                return;
            }

            // ЗАПУСКАЕМ СЛЕДУЮЩИЙ РАУНД ЧЕРЕЗ 1 СЕКУНДУ
            Timer delayTimer = new Timer();
            delayTimer.Interval = 1000;   // 1 секунда
            delayTimer.Tick += (s, ev) =>
            {
                delayTimer.Stop();        // Останавливаем таймер
                StartNewRound();          // Начинаем новый раунд
            };
            delayTimer.Start();           // Запускаем таймер
        }

        // МЕТОД СБРОСА СОСТОЯНИЯ ИГРЫ
        private void ResetGameState()
        {
            showTimer?.Stop();      // Останавливаем таймер показа
            answerTimer?.Stop();    // Останавливаем таймер ответа

            gameStarted = false;    // Игра не начата
            canPlay = false;        // Кликать нельзя
            isShowing = false;      // Кружки не показываются

            userAnswers.Clear();    // Очищаем список ответов игрока
            currentCircles.Clear(); // Очищаем список правильных позиций

            label1.Text = "00:00";              // Сбрасываем таймер
            label1.ForeColor = Color.Lime;      // Зелёный цвет
            button1.Enabled = true;             // Активируем кнопку
            button1.Text = "Запуск";            // Возвращаем текст кнопки

            // РАЗБЛОКИРУЕМ ИЗМЕНЕНИЕ СЛОЖНОСТИ
            SetDifficultyControlsEnabled(true);    // Разрешаем переключение уровня сложности

            // Очищаем все ячейки поля
            for (int row = 0; row < gridSize; row++)        // Перебираем все строки
                for (int col = 0; col < gridSize; col++)    // Перебираем все столбцы
                {
                    dgv[col, row].Style.BackColor = Color.White;  // Белый фон
                    dgv[col, row].Value = "";                    // Пустое значение
                }
        }

        // МЕТОД ОБРАБОТКИ КЛИКА ПО КЛЕТКЕ
        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dgv.ClearSelection();   // Убираем синее выделение после клика

            // ПРОВЕРКА
            if (!gameStarted)
            {
                MessageBox.Show("Нажмите 'Запуск'!", "Внимание");   // Предупреждение
                return;
            }

            // ПРОВЕРКА: МОЖНО ЛИ СЕЙЧАС КЛИКАТЬ?
            if (!canPlay || isShowing) return;   // Нельзя кликать во время показа кружков

            int row = e.RowIndex;    // Номер строки, по которой кликнули
            int col = e.ColumnIndex; // Номер столбца
            if (row < 0 || col < 0) return;   // Если кликнули не по ячейке - выходим

            Point clicked = new Point(row, col);   // Создаём точку с координатами

            // ЕСЛИ КЛЕТКА УЖЕ ОТМЕЧЕНА 
            if (userAnswers.Contains(clicked))
            {
                userAnswers.Remove(clicked);               // Убираем отметку
                dgv[col, row].Style.BackColor = Color.White;   // Белый фон
            }
            // ЕСЛИ КЛЕТКА ЕЩЁ НЕ ОТМЕЧЕНА 
            else
            {
                // Нельзя отметить больше кружков, чем нужно
                if (userAnswers.Count >= circlesCount)
                {
                    return;
                }
                userAnswers.Add(clicked);                    // Добавляем в список ответов
                dgv[col, row].Style.BackColor = Color.LightBlue;   // Голубой фон (отметка)
            }

            //ЕСЛИ ОТМЕТИЛИ ВСЕ КРУЖКИ - АВТОМАТИЧЕСКАЯ ПРОВЕРКА
            if (userAnswers.Count == circlesCount)
            {
                CheckAnswer();   // Проверяем правильность ответа
            }
        }

        //МЕТОД ПРОВЕРКИ ОТВЕТА
        private void CheckAnswer()
        {
            answerTimer?.Stop();   // Останавливаем таймер ответа

            // СРАВНИВАЕМ ОТВЕТЫ С ПРАВИЛЬНЫМИ ПОЗИЦИЯМИ
            bool success = userAnswers.Count == currentCircles.Count && userAnswers.All(p => currentCircles.Contains(p));
            // success = true, если:
            // 1) Отмечено столько же клеток, сколько было кружков
            // 2) Все отмеченные клетки совпадают с правильными позициями

            //  ЕСЛИ ПРАВИЛЬНО 
            if (success)
            {
                circlesCount++;          // Увеличиваем количество кружков на 1
                errorsInRow = 0;         // Сбрасываем счётчик ошибок подряд
                MessageBox.Show("Правильно! +1 кружок", "Результат");
            }
            //  ЕСЛИ НЕПРАВИЛЬНО 
            else
            {
                errorsInRow++;           // Увеличиваем счётчик ошибок подряд

                if (errorsInRow >= 2)    // Если две ошибки подряд
                {
                    circlesCount = Math.Max(1, circlesCount - 1);  // Уменьшаем кружки (минимум 1)
                    errorsInRow = 0;     // Сбрасываем счётчик
                    MessageBox.Show("Две ошибки подряд! -1 кружок", "Результат");
                }
                else
                {
                    MessageBox.Show("Неправильно. Кружков осталось столько же", "Результат");
                }
            }

            label2.Text = $"Кружков: {circlesCount}";   // Обновляем надпись

            // ПРОВЕРКА НА ПОБЕДУ 
            if (circlesCount > gridSize * gridSize)   // Если кружков больше, чем клеток
            {
                MessageBox.Show($"ПОБЕДА! Вы дошли до {circlesCount} кружков!", "Поздравляем!");
                ResetGameState();   // Сбрасываем игру
                return;
            }

            //  ЗАПУСКАЕМ СЛЕДУЮЩИЙ РАУНД ЧЕРЕЗ 1 СЕКУНДУ 
            canPlay = false;   // Пока нельзя кликать
            Timer delayTimer = new Timer();
            delayTimer.Interval = 1000;   // 1 секунда
            delayTimer.Tick += (s, ev) =>
            {
                delayTimer.Stop();        // Останавливаем таймер
                StartNewRound();          // Начинаем новый раунд
            };
            delayTimer.Start();           // Запускаем таймер
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }
    }
}