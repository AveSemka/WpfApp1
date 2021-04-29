using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Data.SqlClient;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;

namespace WpfApp1
{

    public partial class MainWindow : Window
    {
        public static bool[,] prevGen;
        public static bool[,] currentGen;
        public bool[,] nextGen;
        public static readonly int cols = 174;
        public static readonly int rows = 67;
        public int born = 0;
        public int died = 0;
        public int gen;
        private DispatcherTimer timer;            
        public static WriteableBitmap field;
        public int population = 0;
        bool changed;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartGame_click(object sender, RoutedEventArgs e)
        {            
            gen = 1;
            genNumber.Content = gen;            

            if (CountPopulation() > 0)
            {
                born = CountPopulation();
                bornCells.Content = CountPopulation();
                survivedCount.Content = CountPopulation();
                TimerStart();
            }

            else
            {
                FirstGen(cols, rows);
                bornCells.Content = CountPopulation();
                survivedCount.Content = CountPopulation();
                TimerStart();
            }


            buttonStart.IsEnabled = false;
            buttonPause.IsEnabled = true;
            buttonStop.IsEnabled = true;
            ltdUniverse.IsEnabled = false;
            Universe.IsEnabled = false;
            
        }

        private void PauseGame_click(object sender, RoutedEventArgs e)

        {
            string Pause = buttonPause.Content.ToString();
            if (Pause == "Пауза")
            {
                timer.Stop();
                buttonPause.Content = "Продолжить";
            }

            else
                timer.Start();

            if (Pause == "Продолжить")
                buttonPause.Content = "Пауза";
        }

        private void StopGame_click(object sender, RoutedEventArgs e)
        {
            string stop = buttonStop.Content.ToString();
            switch (stop)
            {
                case "Остановить безумие":
                    gameStop();
                    buttonStop.Content = "Ликвидировать выживших";
                    break;
                case "Ликвидировать выживших":
                    clearField();
                    survivedCount.Content = CountPopulation();
                    born = 0;
                    died = 0;
                    gen = 0;
                    genNumber.Content = gen;
                    bornCells.Content = born;
                    diedCells.Content = died;
                    buttonStop.Content = "Остановить безумие";
                    buttonPause.Content = "Пауза";
                    break;
            }

        }

        private void gameStop()
        { 
            timer.Stop();
            buttonPause.IsEnabled = false;
            buttonStop.Content = "Ликвидировать выживших";                
                        
        }

        private void clearField()
        {
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    currentGen[x, y] = false;
                }
            }

            LastGen(cols, rows);
            buttonStart.IsEnabled = true;
            buttonStop.IsEnabled = false;
            ltdUniverse.IsEnabled = true;
            Universe.IsEnabled = true;
        }

        private bool Stable(int cols, int rows)
        {
            bool changed2 = false;            
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (prevGen[x, y] != nextGen[x, y])
                        changed2 = true;
                }
            }            
            return changed2;
        }

        public int CountPopulation()
        {
            population = 0;

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (currentGen[x, y])
                        population++;
                }
            }

            return population;
        }

        public int CountCells(int x, int y)
        {
            int count = 0;

            if (ltdUniverse.IsChecked == true)
            {
                for (int j = -1; j < 2; j++)
                {
                    int col = x + j;
                    if (col < 0)
                        col = 0;
                    else if (col >= cols)
                        col = cols - 1;
                    else
                        col = x + j;

                    for (int l = -1; l < 2; l++)
                    {
                        int row = y + l;
                        if (row < 0)
                            row = 0;
                        else if (row >= rows)
                            row = rows - 1;
                        else
                            row = y + l;
                        bool itSelf = col == x && row == y;
                        if (currentGen[col, row] && !itSelf)
                            count++;
                    }
                }
            }
            else
            {
                for (int j = -1; j < 2; j++)
                {
                    for (int l = -1; l < 2; l++)
                    {
                        int col = (x + j + cols) % cols;
                        int row = (y + l + rows) % rows;
                        bool itSelf = col == x && row == y;
                        if (currentGen[col, row] && !itSelf)
                            count++;
                    }
                }

            }
            return count;
        }

        public void TimerStart()
        {
            timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Tick += new EventHandler(TimerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, (int)timeSpeed.Value * 100);
            timer.Start();
        }

        public void TimerTick(object sender, EventArgs e)
        {
            int Speed = (int)timeSpeed.Value;
            timer.Interval = new TimeSpan(0, 0, 0, 0, Speed * 100);

            NextGen(cols, rows);
           
            if (!changed || !Stable(cols, rows))
                gameStop();
                        
            prevGen = currentGen;
            currentGen = nextGen;
            survivedCount.Content = CountPopulation();
            genNumber.Content = gen++;
            bornCells.Content = born;
            diedCells.Content = died;            
            matrix.Source = field;
                        
        }
        private void createField(object sender, RoutedEventArgs e)
        {
            currentGen = new bool[cols, rows];
            prevGen = new bool[cols, rows];
            field = new WriteableBitmap(cols, rows, 96, 96, PixelFormats.Pbgra32, null);
            matrix.Width = cols;
            matrix.Height = rows;
            matrix.Source = field;
        }
        
        public void FirstGen(int cols, int rows)
        {
            Random random = new Random();
            field = new WriteableBitmap(cols, rows, 96, 96, PixelFormats.Pbgra32, null);
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    currentGen[x, y] = random.Next(6) == 0;
                    if (currentGen[x, y])
                        placeCell(x, y);
                }
            }
            gen++;
            born = CountPopulation();
            matrix.Source = field;
        }
                
        public void NextGen(int cols, int rows)
        {
            
            changed = false;
            nextGen = new bool[cols, rows];
            field = new WriteableBitmap(cols, rows, 96, 96, PixelFormats.Pbgra32, null);
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    int Neighbours = CountCells(x, y);

                    if (!currentGen[x, y] && Neighbours == 3)
                    {
                        nextGen[x, y] = true;
                        born++;
                        changed = true;
                    }
                    else if (currentGen[x, y] && (Neighbours < 2 || Neighbours > 3))
                    {
                        nextGen[x, y] = false;
                        died++;
                        changed = true;
                    }
                    else
                    {
                        nextGen[x, y] = currentGen[x, y];      
                    }

                    if (nextGen[x, y])
                        placeCell(x, y);
                }

            }
            
        }

        private void LastGen(int cols, int rows)
        {
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    deleteCell(x, y);
                }
            }
        }

        public void placeCell(int x, int y)
        {
            byte[] color = { 150, 160, 80, 255 };
            field.WritePixels(new Int32Rect(x, y, 1, 1), color, 4, 0);
        }

        public void deleteCell(int x, int y)
        {
            byte[] uncolor = { 0, 0, 0, 0 };
            field.WritePixels(new Int32Rect(x, y, 1, 1), uncolor, 4, 0);
        }

        public bool InField(int x, int y)
        {
            return x >= 0 && y >= 0 && x < cols && y < rows;
        }

         public void cellControl(object sender, MouseEventArgs e)
        {
            if (buttonStop.Content.ToString() == "Ликвидировать выживших")
                return;
            else if (e.LeftButton == MouseButtonState.Pressed)
            {

                var position = e.GetPosition(matrix);
                int col = (int)position.X;
                int row = (int)position.Y;
                if (InField(col, row))
                {
                    if (!currentGen[col, row])
                    {
                        currentGen[col, row] = true;
                        placeCell(col, row);
                        bornCells.Content = born++;
                    }
                }

                survivedCount.Content = CountPopulation();

            }


            else if (e.RightButton == MouseButtonState.Pressed)
            {

                var position = e.GetPosition(matrix);
                int col = (int)position.X;
                int row = (int)position.Y;
                if (InField(col, row))
                {
                    if (currentGen[col, row])
                    {
                        currentGen[col, row] = false;
                        deleteCell(col, row);
                        died++;
                    }
                }

                survivedCount.Content = CountPopulation();

            }

        }

       private void buttonSave_click(object sender, RoutedEventArgs e)
        {
            Saveload saveload = new Saveload();
            saveload.ShowDialog();       
        }
        
    }
        
}
