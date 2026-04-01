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

namespace Muhametshin_Глазки_save
{
    /// <summary>
    /// Логика взаимодействия для AgentSalesHistoryWindow.xaml
    /// </summary>
    public partial class AgentSalesHistoryWindow : Window
    {
        private MuhametshinEyesEntities _context;
        private Agent _currentAgent;

        private MessageService _messageService = new MessageService();

        public AgentSalesHistoryWindow(Agent agent)
        {
            InitializeComponent();

            _context = MuhametshinEyesEntities.GetContext();

            _currentAgent = agent;

            Title = $"История продаж агента \"{_currentAgent.Title}\"";

            var products = _context.Product.OrderBy(p => p.Title).ToList();
            CBoxProducts.ItemsSource = products;

            DPickerSaleDate.SelectedDate = DateTime.Today;

            UpdateSales();
        }

        private void BtnAddSale_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (!(CBoxProducts.SelectedItem is Product))
            {
                errors.AppendLine("Введите наименование товара");
            }

            if (DPickerSaleDate.SelectedDate == null)
            {
                errors.AppendLine("Укажите дату продажи");
            }

            if (!int.TryParse(TBoxProductCount.Text, out int count) || count < 1)
            {
                errors.AppendLine("Количество должно быть целым положительным числом");
            }

            if (errors.Length > 0)
            {
                _messageService.ShowError(errors.ToString());
                return;
            }

            var selectedProduct = CBoxProducts.SelectedItem as Product;

            var sale = new ProductSale
            {
                AgentID = _currentAgent.ID,
                ProductID = selectedProduct.ID,
                SaleDate = DPickerSaleDate.SelectedDate.Value,
                ProductCount = count
            };

            _context.ProductSale.Add(sale);

            _context.SaveChanges();
            UpdateSales();

            CBoxProducts.SelectedItem = null;
            DPickerSaleDate.SelectedDate = DateTime.Today;
            TBoxProductCount.Text = "1";
        }

        private void UpdateSales()
        {
            var currentSales = _context.ProductSale.Where(ps => ps.AgentID == _currentAgent.ID).OrderByDescending(ps => ps.SaleDate).ToList();
            ListViewSales.ItemsSource = currentSales;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ListViewSales.SelectedItems.Count is int count && count > 0)
            {
                var result = _messageService.ShowWarningExtended($"Будет удалено записей: {count}");

                if (result == MessageBoxResult.Yes)
                {
                    var selectedSales = ListViewSales.SelectedItems.Cast<ProductSale>().ToList();
                    foreach (var sale in selectedSales)
                    {
                        _context.ProductSale.Remove(sale);
                    }

                    _context.SaveChanges();
                    UpdateSales();
                }
            }
        }

        private void ListViewSales_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListViewSales.SelectedItems.Count > 0)
            {
                PanelBottom.Visibility = Visibility.Visible;
            }
            else
            {
                PanelBottom.Visibility = Visibility.Collapsed;
            }
        }
    }
}
