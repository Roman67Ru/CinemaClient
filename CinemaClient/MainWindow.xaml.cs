using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace CinemaClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        public void UpdateFilms()
        {
            string result = GetDell("GET", "https://localhost:44364/api/FilmSessions");
            JToken parse = JToken.Parse(result);

            List<FilmSession> films = new List<FilmSession>();
            for (int i = 0; i < parse.Count(); i++)
            {
                films.Add(new FilmSession
                {
                    Id = parse[i]["Id"].ToString(),
                    Name = parse[i]["Name"].ToString(),
                    DateTime = "[" + Convert.ToDateTime(parse[i]["DateTime"]).ToShortDateString() + "|" + Convert.ToDateTime(parse[i]["DateTime"]).ToShortTimeString() + "]"
                });
            }
            Film.ItemsSource = films.Select(x => new { x.Id, Name = x.DateTime + "   " + x.Name }).ToList();

            Film.DisplayMemberPath = "Name";
            Film.SelectedValuePath = "Id";
            Film.SelectedIndex = 0;
        }

        public void UpdateOrders(string FilmId)
        {
            try
            {
                string result = GetDell("GET", "https://localhost:44364/api/Orders/" + FilmId);
                JToken parse = JToken.Parse(result);

                List<Order> orders = new List<Order>();
                for (int i = 0; i < parse.Count(); i++)
                {
                    orders.Add(new Order
                    {
                        Id = parse[i]["Id"].ToString(),
                        Count = parse[i]["Count"].ToString(),
                        DateTime = Convert.ToDateTime(parse[i]["DateTime"]).ToString(),
                        FilmSession_Id = parse[i]["FilmSession_Id"].ToString()
                    });
                }


                OrdersGrid.Columns.Clear();
                OrdersGrid.ItemsSource = orders.Select(x => new { x.Id, x.DateTime, x.Count }).ToList();
                OrdersGrid.Columns[0].Visibility = Visibility.Hidden;

                DataGridTemplateColumn col = new DataGridTemplateColumn();
                string xaml = "<DataTemplate><Button FontSize=\"10\" Height=\"20\" Content=\"Удалить\"/></DataTemplate>";
                MemoryStream sr = new MemoryStream(Encoding.UTF8.GetBytes(xaml));
                ParserContext pc = new ParserContext();
                pc.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                pc.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
                DataTemplate datatemplate = (DataTemplate)XamlReader.Load(sr, pc);
                col.CellTemplate = datatemplate;

                OrdersGrid.Columns.Add(col);


            }
            catch (Exception) { }
        }
        public string GetDell(string Method, string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = Method;
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            Stream stream = res.GetResponseStream();
            StreamReader sr = new System.IO.StreamReader(stream);
            string Out = sr.ReadToEnd();
            sr.Close();
            return Out;
        }

        private void Film_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string Id = Film.SelectedValue.ToString();
                UpdateOrders(Id);
            }
            catch (Exception)
            {
                UpdateFilms();
            }
        }

        private void AddOrder(object sender, RoutedEventArgs e)
        {
            try
            {
                Order order = new Order
                {
                    FilmSession_Id = Film.SelectedValue.ToString(),
                    DateTime = DateTime.Now.ToString(),
                    Count = CountOrder.Text
                };

                JToken parse = JToken.FromObject(order);

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://localhost:44364/api/Orders");
                req.Method = "POST";
                req.ContentType = "application/json;charset=utf-8";

                using (Stream requestStream = req.GetRequestStream())
                using (StreamWriter writer = new StreamWriter(requestStream))
                {
                    writer.Write(parse.ToString());
                }
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                Stream Stream = res.GetResponseStream();
                StreamReader sr = new System.IO.StreamReader(Stream);
                string Out = sr.ReadToEnd();
                sr.Close();

                UpdateOrders(Film.SelectedValue.ToString());
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка");
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UpdateFilms();
        }

        private async Task AutoUpdateFilms()
        {
            while (true)
            {
                UpdateFilms();
                await Task.Delay(30000);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await AutoUpdateFilms();
        }

        private void OrdersGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (OrdersGrid.CurrentColumn.DisplayIndex == 3)
            {
                if (MessageBox.Show("Подтвердите удаление", "admin", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    JToken parse = JToken.FromObject(OrdersGrid.CurrentItem);
                    string Id = parse["Id"].ToString();
                    string result = GetDell("DELETE", "https://localhost:44364/api/Orders/" + Id);
                    UpdateOrders(Film.SelectedValue.ToString());
                }
            }
        }

        private void CountOrder_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }
    }
}
