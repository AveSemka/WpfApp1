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
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using static WpfApp1.MainWindow;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для Saveload.xaml
    /// </summary>
    public partial class Saveload : Window
    {
        string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=GameOfLife;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";


        public Saveload()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowSaves();
        }

        private void ShowSaves()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            string sqlExpression = "Select Имя from Saves";

            connection.Open();
            SqlCommand command = new SqlCommand(sqlExpression, connection);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                SavedGames.Items.Add(reader[0].ToString());
            }
            connection.Close();

        }

        private void SaveGame(object sender, RoutedEventArgs e)
        {            
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, currentGen);
            byte[] SerializedArray = memoryStream.GetBuffer();

            SqlConnection connection = new SqlConnection(connectionString);
            string sqlExpression = "INSERT INTO Saves (Имя, Data) VALUES (@savename, @bytearray)";

            connection.Open();
            SqlCommand command = new SqlCommand(sqlExpression, connection);
            SqlParameter param = command.Parameters.Add("bytearray", SqlDbType.VarBinary);
            SqlParameter savename = command.Parameters.Add("savename", SqlDbType.Text);
            savename.Value = SaveName.Text;
            param.Value = SerializedArray;
            command.ExecuteNonQuery();
            connection.Close();

            SavedGames.Items.Clear();
            ShowSaves();
        }

        private void deleteSave(object sender, RoutedEventArgs e)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            string sqlExpression = "Delete from Saves where Имя like @name";
            connection.Open();
            SqlCommand command = new SqlCommand(sqlExpression, connection);
            SqlParameter param = command.Parameters.Add("name", SqlDbType.Text);
            param.Value = SavedGames.SelectedItem.ToString();
            command.ExecuteNonQuery();
            SavedGames.Items.Clear();
            ShowSaves();
        }

        private void bLoadSave_Click(object sender, RoutedEventArgs e)
        {
            byte[] SerializedArray;
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            SqlConnection connection = new SqlConnection(connectionString);
            string sqlExpression = "Select Data from Saves where Имя like @name";
            connection.Open();
            SqlCommand command = new SqlCommand(sqlExpression, connection);
            SqlParameter param = command.Parameters.Add("name", SqlDbType.Text);
            param.Value = SavedGames.SelectedItem.ToString();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                SerializedArray = (byte[])reader[0];
                memoryStream.Write(SerializedArray, 0, SerializedArray.Length);
                memoryStream.Position = 0;
            }

            currentGen = (bool[,])binaryFormatter.Deserialize(memoryStream);
            prevGen = currentGen;

            connection.Close();
                        
            Close();            
        }
    }
}
