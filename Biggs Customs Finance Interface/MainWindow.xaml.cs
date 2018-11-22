using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Threading;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Biggs_Customs_Finance_Interface
{
    public partial class MainWindow : Window
    {
        ObservableCollection<Order> AllOrders = new ObservableCollection<Order>();
        ObservableCollection<Shoe> AllShoes = new ObservableCollection<Shoe>();
        ObservableCollection<string> Brands = new ObservableCollection<string>();
        ObservableCollection<string> ShoeTypes = new ObservableCollection<string>();

        private readonly string app_name = "Biggs Customs Finance Interface";
        private readonly string service_email = "sheets@biggs-custom-kicks-website.iam.gserviceaccount.com";
        private readonly string sheets_id = "1GraMbmwK8VDfgNYkQVLn1QlHHxmuDRI2UdvIaHPlcik";
        private readonly string[] scopes = { SheetsService.Scope.Spreadsheets };

        private SheetsService sheets_service;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            AllShoes.CollectionChanged += AllShoes_CollectionChanged;
            allShoesListView.ItemsSource = AllShoes;
            allOrdersListView.ItemsSource = AllOrders;
            addNewShoeBrandCombobox.ItemsSource = Brands;
            addNewShoeTypeCombobox.ItemsSource = ShoeTypes;
            shoeInProgressShoeCombobox.ItemsSource = AllShoes;

            Brands.Add("< Create New > ");
            ShoeTypes.Add("< Create New > ");

            #region "List View Columns"
            var allShoesGridView = new GridView();
            var allOrdersGridView = new GridView();
            allShoesListView.View = allShoesGridView;
            allOrdersListView.View = allOrdersGridView;

            allShoesGridView.Columns.Add(new GridViewColumn() { Header = "SKU", DisplayMemberBinding = new Binding("SKU") });
            allShoesGridView.Columns.Add(new GridViewColumn() { Header = "Name", DisplayMemberBinding = new Binding("Name") });
            allShoesGridView.Columns.Add(new GridViewColumn() { Header = "Brand", DisplayMemberBinding = new Binding("Brand") });
            allShoesGridView.Columns.Add(new GridViewColumn() { Header = "Shoe Type", DisplayMemberBinding = new Binding("ShoeType") });
            allShoesGridView.Columns.Add(new GridViewColumn() { Header = "Cost", DisplayMemberBinding = new Binding("Cost") });
            allShoesGridView.Columns.Add(new GridViewColumn() { Header = "Keywords", DisplayMemberBinding = new Binding("Keywords") });
            
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Order Number", DisplayMemberBinding = new Binding("OrderNumber") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Shoe SKU", DisplayMemberBinding = new Binding("ShoeSKU") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Start Date", DisplayMemberBinding = new Binding("StartDate") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "End Date", DisplayMemberBinding = new Binding("EndDate") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Selling Date", DisplayMemberBinding = new Binding("SellingDate") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Custom Cost", DisplayMemberBinding = new Binding("CustomCost") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Total Income", DisplayMemberBinding = new Binding("TotalIncome") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Customer/First Name", DisplayMemberBinding = new Binding("CustomerFirstName") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Customer/Last Name", DisplayMemberBinding = new Binding("CustomerLastName") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Phone Number", DisplayMemberBinding = new Binding("PhoneNumber") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Instagram", DisplayMemberBinding = new Binding("Instagram") });
            #endregion

            serviceLogin();
            GetShoes();
        }

        private void serviceLogin()
        {
            ServiceAccountCredential creds;
            string service_file = @"Assets\service.json";

            using (Stream stream = new FileStream(service_file, FileMode.Open, FileAccess.Read))
            {
                creds = (ServiceAccountCredential)GoogleCredential.FromStream(stream).UnderlyingCredential;
                var initializer = new ServiceAccountCredential.Initializer(creds.Id)
                {
                    User = service_email,
                    Key = creds.Key,
                    Scopes = scopes
                };
                creds = new ServiceAccountCredential(initializer);
            }
            sheets_service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = creds,
                ApplicationName = app_name
            });
        }

        private async void GetShoes()
        {
            var request = sheets_service.Spreadsheets.Values.Get(sheets_id, "Shoes!A2:F");
            var values = (await request.ExecuteAsync()).Values;

            foreach (var row in values)
            {
                AllShoes.Add(new Shoe(
                    int.Parse((string)row[0]),
                    (string)row[1],
                    (string)row[2],
                    (string)row[3],
                    decimal.Parse((string)row[4], NumberStyles.Currency),
                    (string)row[5]));
            }
        }

        private void AllShoes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var values = new List<IList<object>>();

            foreach (var obj in e.NewItems)
            {
                var shoe = (Shoe)obj;
                if (!Brands.Contains(shoe.Brand))
                    Brands.Add(shoe.Brand);
                if (!ShoeTypes.Contains(shoe.ShoeType))
                    ShoeTypes.Add(shoe.ShoeType);
                values.Add(new List<object> { shoe.SKU, shoe.Name, shoe.Brand, shoe.ShoeType, shoe.Cost, shoe.Keywords });
            }
            
            addNewShoeSKUTextbox.Text = (AllShoes.Count + 2).ToString();
        }

        private void addNewShoeCostTextbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, $"^[0-9.-]+$", RegexOptions.Multiline)) {
                e.Handled = true;
            }
        }

        private async void addNewShoeButton_Click(object sender, RoutedEventArgs e)
        {
            var shoe = new Shoe(
                int.Parse(addNewShoeSKUTextbox.Text),
                addNewShoeNameTextbox.Text,
                addNewShoeBrandCombobox.Text,
                addNewShoeTypeCombobox.Text,
                decimal.Parse(addNewShoeCostTextbox.Text),
                addNewShoeKeywordsTextbox.Text);
            AllShoes.Add(shoe);

            var values = new List<IList<object>>();
            values.Add(new List<object> { shoe.SKU, shoe.Name, shoe.Brand, shoe.ShoeType, shoe.Cost, shoe.Keywords });

            var body = new Google.Apis.Sheets.v4.Data.ValueRange()
            {
                Values = values
            };

            var request = sheets_service.Spreadsheets.Values.Append(body, sheets_id, "Shoes");
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            await request.ExecuteAsync();

            addNewShoeNameTextbox.Text = "";
            addNewShoeBrandCombobox.Text = "";
            addNewShoeTypeCombobox.Text = "";
            addNewShoeCostTextbox.Text = "0.00";
            addNewShoeKeywordsTextbox.Text = "";
        }

        private void addNewShoeComboboxes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = (ComboBox)sender;
            var name = cb.Name;
            if (cb.SelectedIndex == 0)
            {
                var dialog = new InputBox("Enter name for new item:");
                if (dialog.ShowDialog() == true && dialog.Answer != "")
                {
                    if (name.Contains("Brand"))
                    {
                        Brands.Add(dialog.Answer);
                    }
                    else if (name.Contains("ShoeType"))
                    {
                        ShoeTypes.Add(dialog.Answer);
                    }
                    cb.SelectedIndex = cb.Items.Count - 1;
                }
                else if (dialog.ShowDialog() == true && dialog.Answer == "")
                {
                    cb.SelectedIndex = -1;
                }
            } else
            {
                var brand = Brands.ElementAtOrDefault(addNewShoeBrandCombobox.SelectedIndex);
                var shoetype = ShoeTypes.ElementAtOrDefault(addNewShoeTypeCombobox.SelectedIndex);
                var shoe = AllShoes.FirstOrDefault(x => x.Brand == brand && x.ShoeType == shoetype);
                if (shoe != null)
                {
                    addNewShoeCostTextbox.Text = shoe.Cost.ToString();
                }
            }
        }

        private async void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (shoesTabItem.IsSelected)
            {
                //do something eventually....
            }
            else if (ordersTabItem.IsSelected)
            {
                AllOrders.Clear();
                var request = sheets_service.Spreadsheets.Values.Get(sheets_id, "Orders!A2:K");
                var values = (await request.ExecuteAsync()).Values;

                foreach (var row in values)
                {
                    AllOrders.Add(new Order
                    {
                        OrderNumber = int.Parse((string)row[0]),
                        SKU = int.Parse((string)row[1]),
                        StartDate = DateTime.Parse((string)row[2]),
                        EndDate = DateTime.Parse((string)row[3]),
                        SellingDate = DateTime.Parse((string)row[4]),
                        CustomCost = decimal.Parse((string)row[5], NumberStyles.Currency),
                        TotalIncome = decimal.Parse((string)row[6], NumberStyles.Currency),
                        CustomerFirstName = (string)row[7],
                        CustomerLastName = (string)row[8],
                        PhoneNumber = (string)row[9],
                        Instagram = (string)row[10]
                    });
                }
            }
            else if (shoesInProgressTabItem.IsSelected)
            {
                
            }
        }
    }
}
