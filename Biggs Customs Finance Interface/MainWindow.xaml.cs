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
        ObservableCollection<Project> AllProjects = new ObservableCollection<Project>();
        ObservableCollection<Order> AllOrders = new ObservableCollection<Order>();
        ObservableCollection<Product> AllProducts = new ObservableCollection<Product>();
        ObservableCollection<string> Brands = new ObservableCollection<string>();
        ObservableCollection<string> ProductTypes = new ObservableCollection<string>();

        private readonly string app_name = "Biggs Customs Finance Interface";
        private readonly string service_email = "sheets@biggs-custom-kicks-website.iam.gserviceaccount.com";
        private readonly string sheets_id = "1GraMbmwK8VDfgNYkQVLn1QlHHxmuDRI2UdvIaHPlcik";
        private readonly string[] scopes = { SheetsService.Scope.Spreadsheets };

        private SheetsService sheets_service;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            AllProducts.CollectionChanged += AllProducts_CollectionChanged;
            allProductsListView.ItemsSource = AllProducts;
            allOrdersListView.ItemsSource = AllOrders;
            allProjectsListView.ItemsSource = AllProjects;

            addNewProductBrandCombobox.ItemsSource = Brands;
            addNewProductTypeCombobox.ItemsSource = ProductTypes;
            projectsProductCombobox.ItemsSource = AllProducts;

            Brands.Add("< Create New > ");
            ProductTypes.Add("< Create New > ");

            #region "List View Columns"
            var allProductsGridView = new GridView();
            var allOrdersGridView = new GridView();
            var allProjectsGridView = new GridView();
            allProductsListView.View = allProductsGridView;
            allOrdersListView.View = allOrdersGridView;
            allProjectsListView.View = allProjectsGridView;

            allProductsGridView.Columns.Add(new GridViewColumn() { Header = "SKU", DisplayMemberBinding = new Binding("SKU") });
            allProductsGridView.Columns.Add(new GridViewColumn() { Header = "Name", DisplayMemberBinding = new Binding("Name") });
            allProductsGridView.Columns.Add(new GridViewColumn() { Header = "Brand", DisplayMemberBinding = new Binding("Brand") });
            allProductsGridView.Columns.Add(new GridViewColumn() { Header = "Product Type", DisplayMemberBinding = new Binding("ProductType") });
            allProductsGridView.Columns.Add(new GridViewColumn() { Header = "Cost", DisplayMemberBinding = new Binding("Cost") });
            allProductsGridView.Columns.Add(new GridViewColumn() { Header = "Keywords", DisplayMemberBinding = new Binding("Keywords") });
            
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Order Number", DisplayMemberBinding = new Binding("OrderNumber") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "SKU", DisplayMemberBinding = new Binding("SKU") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Start Date", DisplayMemberBinding = new Binding("StartDate") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "End Date", DisplayMemberBinding = new Binding("EndDate") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Selling Date", DisplayMemberBinding = new Binding("SellingDate") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Custom Cost", DisplayMemberBinding = new Binding("CustomCost") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Total Income", DisplayMemberBinding = new Binding("TotalIncome") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Customer/First Name", DisplayMemberBinding = new Binding("CustomerFirstName") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Customer/Last Name", DisplayMemberBinding = new Binding("CustomerLastName") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Phone Number", DisplayMemberBinding = new Binding("PhoneNumber") });
            allOrdersGridView.Columns.Add(new GridViewColumn { Header = "Instagram", DisplayMemberBinding = new Binding("Instagram") });

            allProjectsGridView.Columns.Add(new GridViewColumn { Header = "SKU", DisplayMemberBinding = new Binding("Product.SKU") });
            allProjectsGridView.Columns.Add(new GridViewColumn { Header = "Order #", DisplayMemberBinding = new Binding("OrderNumber") });
            allProjectsGridView.Columns.Add(new GridViewColumn { Header = "Start Date", DisplayMemberBinding = new Binding("StartDate") });
            allProjectsGridView.Columns.Add(new GridViewColumn { Header = "End Date", DisplayMemberBinding = new Binding("EndDate") });
            allProjectsGridView.Columns.Add(new GridViewColumn { Header = "Custom Cost", DisplayMemberBinding = new Binding("CustomCost") });
            allProjectsGridView.Columns.Add(new GridViewColumn { Header = "Total Income", DisplayMemberBinding = new Binding("TotalIncome") });
            allProjectsGridView.Columns.Add(new GridViewColumn { Header = "Customer/First Name", DisplayMemberBinding = new Binding("CustomerFirstName") });
            allProjectsGridView.Columns.Add(new GridViewColumn { Header = "Customer/Last Name", DisplayMemberBinding = new Binding("CustomerLastName") });
            allProjectsGridView.Columns.Add(new GridViewColumn { Header = "Phone Number", DisplayMemberBinding = new Binding("PhoneNumber") });
            allProjectsGridView.Columns.Add(new GridViewColumn { Header = "Instagram", DisplayMemberBinding = new Binding("Instagram") });
            #endregion

            serviceLogin();
            GetProducts();
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

        private async void GetProducts()
        {
            var request = sheets_service.Spreadsheets.Values.Get(sheets_id, "Products!A2:F");
            var values = (await request.ExecuteAsync()).Values;

            foreach (var row in values)
            {
                AllProducts.Add(new Product(
                    int.Parse((string)row[0]),
                    (string)row[1],
                    (string)row[2],
                    (string)row[3],
                    decimal.Parse((string)row[4], NumberStyles.Currency),
                    (string)row[5]));
            }
        }

        private void AllProducts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var values = new List<IList<object>>();

            foreach (var obj in e.NewItems)
            {
                var product = (Product)obj;
                if (!Brands.Contains(product.Brand))
                    Brands.Add(product.Brand);
                if (!ProductTypes.Contains(product.ProductType))
                    ProductTypes.Add(product.ProductType);
                values.Add(new List<object> { product.SKU, product.Name, product.Brand, product.ProductType, product.Cost, product.Keywords });
            }
            
            addNewProductSKUTextbox.Text = (AllProducts.Count + 2).ToString();
        }

        private void addNewProductCostTextbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, $"^[0-9.-]+$", RegexOptions.Multiline)) {
                e.Handled = true;
            }
        }

        private async void addNewProductButton_Click(object sender, RoutedEventArgs e)
        {
            var product = new Product(
                int.Parse(addNewProductSKUTextbox.Text),
                addNewProductNameTextbox.Text,
                addNewProductBrandCombobox.Text,
                addNewProductTypeCombobox.Text,
                decimal.Parse(addNewProductCostTextbox.Text),
                addNewProductKeywordsTextbox.Text);
            AllProducts.Add(product);

            var values = new List<IList<object>>();
            values.Add(new List<object> { product.SKU, product.Name, product.Brand, product.ProductType, product.Cost, product.Keywords });

            var body = new Google.Apis.Sheets.v4.Data.ValueRange()
            {
                Values = values
            };

            var request = sheets_service.Spreadsheets.Values.Append(body, sheets_id, "Products");
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            await request.ExecuteAsync();

            addNewProductNameTextbox.Text = "";
            addNewProductBrandCombobox.Text = "";
            addNewProductTypeCombobox.Text = "";
            addNewProductCostTextbox.Text = "0.00";
            addNewProductKeywordsTextbox.Text = "";
        }

        private void addNewProductComboboxes_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                    else if (name.Contains("ProductType"))
                    {
                        ProductTypes.Add(dialog.Answer);
                    }
                    cb.SelectedIndex = cb.Items.Count - 1;
                }
                else if (dialog.ShowDialog() == true && dialog.Answer == "")
                {
                    cb.SelectedIndex = -1;
                }
            } else
            {
                var brand = Brands.ElementAtOrDefault(addNewProductBrandCombobox.SelectedIndex);
                var shoetype = ProductTypes.ElementAtOrDefault(addNewProductTypeCombobox.SelectedIndex);
                var product = AllProducts.FirstOrDefault(x => x.Brand == brand && x.ProductType == shoetype);
                if (product != null)
                {
                    addNewProductCostTextbox.Text = product.Cost.ToString();
                }
            }
        }

        private async void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.Source is TabControl)) return;
            if (productTabItem.IsSelected)
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
            else if (projectsTabItem.IsSelected)
            {
                AllProjects.Clear();

                var request = sheets_service.Spreadsheets.Values.Get(sheets_id, "Projects!A2:J");
                var values = (await request.ExecuteAsync()).Values;

                foreach (var row in values)
                {
                    AllProjects.Add(new Project
                    {
                        Product = AllProducts.FirstOrDefault(x => x.SKU == int.Parse((string)row[0])),
                        OrderNumber = int.Parse((string)row[1]),
                        StartDate = DateTime.Parse((string)row[2]),
                        EndDate = DateTime.Parse((string)row[3]),
                        CustomCost = decimal.Parse((string)row[4], NumberStyles.Currency),
                        TotalIncome = decimal.Parse((string)row[5], NumberStyles.Currency),
                        CustomerFirstName = (string)row[6],
                        CustomerLastName = (string)row[7],
                        PhoneNumber = (string)row[8],
                        Instagram = (string)row[9]
                    });
                }

                projectsCustomerOrderNumberTextbox.Text = (AllProducts.Count + 2).ToString();
            }
        }

        private void allProjectsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (allProjectsListView.SelectedIndex > -1)
            {
                var project = (Project)allProjectsListView.SelectedItem;
                projectsCustomerFirstNameTextbox.Text = project.CustomerFirstName;
                projectsCustomerLastNameTextbox.Text = project.CustomerLastName;
                projectsPhoneNumberTextbox.Text = project.PhoneNumber;
                projectsInstagramTextbox.Text = project.Instagram;
                projectsProductCombobox.SelectedItem = project.Product;
                projectsStartDateDatePicker.Text = project.StartDate.ToShortDateString();
                projectsEndDateDatePicker.Text = project.EndDate.ToShortDateString();
                projectsCustomerOrderNumberTextbox.Text = project.OrderNumber.ToString();
            } else
            {
                projectsCustomerFirstNameTextbox.Text = "";
                projectsCustomerLastNameTextbox.Text = "";
                projectsPhoneNumberTextbox.Text = "";
                projectsInstagramTextbox.Text = "";
                projectsProductCombobox.Text = "";
                projectsStartDateDatePicker.Text = "";
                projectsEndDateDatePicker.Text = "";
                projectsCustomerOrderNumberTextbox.Text = (AllProjects[AllProjects.Count - 1].OrderNumber + 1).ToString();
            }
        }

        private void projectsAddUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var product = (Product)projectsProductCombobox.SelectedItem;
            var customCost = decimal.Parse(projectsCustomerCustomCostTextbox.Text, NumberStyles.Currency);
            var index = allProjectsListView.SelectedIndex;
            var project = new Project
            {
                Product = product,
                OrderNumber = int.Parse(projectsCustomerOrderNumberTextbox.Text),
                StartDate = projectsStartDateDatePicker.SelectedDate ?? DateTime.Now,
                EndDate = projectsEndDateDatePicker.SelectedDate ?? DateTime.Now,
                CustomCost = customCost,
                TotalIncome = product.Cost + customCost,
                CustomerFirstName = projectsCustomerFirstNameTextbox.Text,
                CustomerLastName = projectsCustomerLastNameTextbox.Text,
                PhoneNumber = projectsPhoneNumberTextbox.Text,
                Instagram = projectsInstagramTextbox.Text
            };
            if (index > -1)
            {
                AllProjects[index] = project;
                allProjectsListView.SelectedIndex = index;
            } else
            {
                AllProjects.Add(project);
                allProjectsListView.SelectedIndex = AllProjects.Count - 1;
            }
        }

        private void projectsCompletedButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
