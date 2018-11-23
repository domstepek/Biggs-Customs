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
using Microsoft.Win32;
using Google.Apis.Sheets.v4.Data;
using System.Net.Http;
using System.Net;

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
        private readonly int projects_id = 127709572;

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

        private void AllProducts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null) return;
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

            var body = new ValueRange()
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
                AllProducts.Clear();
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

                var request = sheets_service.Spreadsheets.Values.Get(sheets_id, "Projects!A2:K");
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
                        Instagram = (string)row[9],
                        ThumbnailLocation = (string)row[10]
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
                projectsCustomerCustomCostTextbox.Text = project.CustomCost.ToString();
                projectsStartDateDatePicker.Text = project.StartDate.ToShortDateString();
                projectsEndDateDatePicker.Text = project.EndDate.ToShortDateString();
                projectsCustomerOrderNumberTextbox.Text = project.OrderNumber.ToString();
                projectsCustomerPayingProductCheckBox.IsChecked = project.TotalIncome == project.CustomCost;
                projectsThumbnailLocationTextbox.Text = project.ThumbnailLocation;
            } else
            {
                projectsCustomerFirstNameTextbox.Text = "";
                projectsCustomerLastNameTextbox.Text = "";
                projectsPhoneNumberTextbox.Text = "";
                projectsInstagramTextbox.Text = "";
                projectsProductCombobox.Text = "";
                projectsCustomerCustomCostTextbox.Text = "0.00";
                projectsStartDateDatePicker.Text = "";
                projectsEndDateDatePicker.Text = "";
                projectsCustomerOrderNumberTextbox.Text = (AllProjects[AllProjects.Count - 1].OrderNumber + 1).ToString();
                projectsThumbnailLocationTextbox.Text = "";
            }
        }

        private async void projectsAddUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var product = (Product)projectsProductCombobox.SelectedItem;
            var customCost = decimal.Parse(projectsCustomerCustomCostTextbox.Text, NumberStyles.Currency);
            var index = allProjectsListView.SelectedIndex;
            var values = new List<IList<object>>();

            var project = new Project
            {
                Product = product,
                OrderNumber = int.Parse(projectsCustomerOrderNumberTextbox.Text),
                StartDate = projectsStartDateDatePicker.SelectedDate ?? DateTime.Now,
                EndDate = projectsEndDateDatePicker.SelectedDate ?? DateTime.Now,
                CustomCost = customCost,
                TotalIncome = (projectsCustomerPayingProductCheckBox.IsChecked ?? false) ? customCost : customCost + product.Cost,
                CustomerFirstName = projectsCustomerFirstNameTextbox.Text,
                CustomerLastName = projectsCustomerLastNameTextbox.Text,
                PhoneNumber = projectsPhoneNumberTextbox.Text,
                Instagram = projectsInstagramTextbox.Text,
                ThumbnailLocation = projectsThumbnailLocationTextbox.Text
            };

            values.Add(new List<object> {
                project.Product.SKU,
                project.OrderNumber,
                project.StartDate.ToString("dd MMM yyyy"),
                project.EndDate.ToString("dd MMM yyyy"),
                project.CustomCost,
                project.TotalIncome,
                project.CustomerFirstName,
                project.CustomerLastName,
                project.PhoneNumber,
                project.Instagram,
                project.ThumbnailLocation
            });

            var body = new ValueRange()
            {
                Values = values
            };

            if (index > -1)
            {
                AllProjects[index] = project;
                allProjectsListView.SelectedIndex = index;

                var request = sheets_service.Spreadsheets.Values.Update(body, sheets_id, $"Projects!A{index + 2}:K");
                request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                await request.ExecuteAsync();
            }
            else
            {
                AllProjects.Add(project);
                allProjectsListView.SelectedIndex = AllProjects.Count - 1;

                var request = sheets_service.Spreadsheets.Values.Append(body, sheets_id, "Projects");
                request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
                request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                await request.ExecuteAsync();
            }
            
        }

        private async void projectsCompletedButton_Click(object sender, RoutedEventArgs e)
        {
            var product = (Product)projectsProductCombobox.SelectedItem;
            var customCost = decimal.Parse(projectsCustomerCustomCostTextbox.Text, NumberStyles.Currency);
            var index = allProjectsListView.SelectedIndex;
            var values = new List<IList<object>>();

            var order = new Order
            {
                OrderNumber = int.Parse(projectsCustomerOrderNumberTextbox.Text),
                SKU = product.SKU,
                StartDate = projectsStartDateDatePicker.SelectedDate ?? DateTime.Now,
                EndDate = projectsEndDateDatePicker.SelectedDate ?? DateTime.Now,
                SellingDate = DateTime.Now,
                CustomCost = customCost,
                TotalIncome = (projectsCustomerPayingProductCheckBox.IsChecked ?? false) ? customCost : customCost + product.Cost,
                CustomerFirstName = projectsCustomerFirstNameTextbox.Text,
                CustomerLastName = projectsCustomerLastNameTextbox.Text,
                PhoneNumber = projectsPhoneNumberTextbox.Text,
                Instagram = projectsInstagramTextbox.Text,
            };

            AllOrders.Add(order);

            values.Add(new List<object> {
                order.OrderNumber,
                order.SKU,
                order.StartDate.ToString("dd MMM yyyy"),
                order.EndDate.ToString("dd MMM yyyy"),
                order.SellingDate.ToString("dd MMM yyyy"),
                order.CustomCost,
                order.TotalIncome,
                order.CustomerFirstName,
                order.CustomerLastName,
                order.PhoneNumber,
                order.Instagram
            });

            var body = new ValueRange()
            {
                Values = values
            };

            var appendRequest = sheets_service.Spreadsheets.Values.Append(body, sheets_id, "Orders");
            appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            await appendRequest.ExecuteAsync();

            Request RequestBody = new Request()
            {
                DeleteDimension = new DeleteDimensionRequest()
                {
                    Range = new DimensionRange()
                    {
                        SheetId = projects_id,
                        Dimension = "ROWS",
                        StartIndex = index + 1,
                        EndIndex = index + 2
                    }
                }
            };

            List<Request> RequestContainer = new List<Request>
            {
                RequestBody
            };

            BatchUpdateSpreadsheetRequest DeleteRequest = new BatchUpdateSpreadsheetRequest
            {
                Requests = RequestContainer
            };

            SpreadsheetsResource.BatchUpdateRequest Deletion = new SpreadsheetsResource.BatchUpdateRequest(sheets_service, DeleteRequest, sheets_id);
            await Deletion.ExecuteAsync();

            AllProjects.RemoveAt(index);
        }

        private void projectsAddImageButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog { Title = "Select thumbnail", CheckFileExists = true, AddExtension = true };
            if (ofd.ShowDialog() == true)
            {
                projectsThumbnailLocationTextbox.Text = ofd.FileName;
            }
        }

        private void allProjectsListView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DependencyObject source = e.OriginalSource as DependencyObject;
            if (source == null) return;

            FrameworkElement selectedItem = ItemsControl.ContainerFromElement((ItemsControl)sender, source) as FrameworkElement;
            if (selectedItem == null) return;

            var projectsViewThumbnailMenuItem = new MenuItem { Header = "View Thumbnail" };
            projectsViewThumbnailMenuItem.Click += (s, args) => {
                var thumbnail = new Thumbnail(((Project)allProjectsListView.SelectedItem).ThumbnailLocation);
                thumbnail.ShowDialog();
            };

            var cm = new ContextMenu();
            cm.Items.Add(projectsViewThumbnailMenuItem);
            selectedItem.ContextMenu = cm; 
        }

        private void allOrdersListView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DependencyObject source = e.OriginalSource as DependencyObject;
            if (source == null) return;

            FrameworkElement selectedItem = ItemsControl.ContainerFromElement((ItemsControl)sender, source) as FrameworkElement;
            if (selectedItem == null) return;

            var ordersGenerateInvoiceMenuItem = new MenuItem { Header = "Generate Invoice" };
            ordersGenerateInvoiceMenuItem.Click += (s, args) => {
                var order = (Order)allOrdersListView.SelectedItem;
                var sfd = new SaveFileDialog
                {
                    Title = "Save PDF To",
                    AddExtension = true,
                    DefaultExt = ".pdf",
                    FileName = $"invoice {order.OrderNumber} - {order.CustomerFirstName} {order.CustomerLastName}",
                };
                if (sfd.ShowDialog() == true)
                {
                    string invoice_file = @"Assets\invoice.json";
                    string json = File.ReadAllText(invoice_file);
                    json = Regex.Replace(json, @"{customer_first} {customer_last}", $"{order.CustomerFirstName} {order.CustomerLastName}");
                    json = Regex.Replace(json, @"{order_number}", order.OrderNumber.ToString());
                    json = Regex.Replace(json, @"{selling_date}", order.SellingDate.ToString("dd MMM yyyy"));
                    json = Regex.Replace(json, @"{product_sku}", order.SKU.ToString());
                    json = Regex.Replace(json, @"{product_name}", AllProducts.First(x => x.SKU == order.SKU).Name);
                    json = Regex.Replace(json, @"{total_income}", order.TotalIncome.ToString());
                    json = Regex.Replace(json, @"{tax}", (order.TotalIncome * 0.075m).ToString());

                    var request = (HttpWebRequest)WebRequest.Create("https://invoice-generator.com");
                    request.Method = "POST";

                    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                    var byteArray = encoding.GetBytes(json);

                    request.ContentLength = byteArray.Length;
                    request.ContentType = @"application/json";

                    using (Stream dataStream = request.GetRequestStream())
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);
                    }

                    using (Stream output = File.OpenWrite(sfd.FileName))
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream input = response.GetResponseStream())
                    {
                        input.CopyTo(output);
                    }
                }
            };

            var cm = new ContextMenu();
            cm.Items.Add(ordersGenerateInvoiceMenuItem);
            selectedItem.ContextMenu = cm;
        }
    }
}
