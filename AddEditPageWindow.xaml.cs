using Microsoft.Win32;
using Muhametshin_Глазки_save;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Muhametshin_Глазки_save
{
    /// <summary>
    /// Логика взаимодействия для AddEditPageWindow.xaml
    /// </summary>
    public partial class AddEditPageWindow : Window
    {
        private MuhametshinEyesEntities _context;
        private Agent _currentAgent;

        private MessageService _messageService = new MessageService();

        public AddEditPageWindow(Agent agent)
        {
            InitializeComponent();

            _context = MuhametshinEyesEntities.GetContext();

            if (agent != null)
            {
                _currentAgent = agent;
                BtnAgentDelete.Visibility = Visibility.Visible;
                Title = "Редактирование агента";
            }
            else
            {
                _currentAgent = new Agent();
                BtnAgentDelete.Visibility = Visibility.Hidden;
                Title = "Добавление агента";
            }

            SetupUI();

            CBoxAgentType.ItemsSource = _context.AgentType.ToList();

            DataContext = _currentAgent;
        }

        private void SetupUI()
        {
            if (_currentAgent.Logo != null)
            {
                BtnLogoDelete.Visibility = Visibility.Visible;
            }
            else
            {
                BtnLogoDelete.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!DataValidation())
            {
                return;
            }

            if (_context.Entry(_currentAgent).State == System.Data.Entity.EntityState.Detached)
            {
                _context.Agent.Add(_currentAgent);
            }

            DialogResult = true;
        }

        private bool DataValidation()
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(TBoxAgentTitle.Text))
            {
                errors.AppendLine("Укажите наименование");
            }

            if (CBoxAgentType.SelectedItem == null)
            {
                errors.AppendLine("Выберите тип");
            }

            if (string.IsNullOrWhiteSpace(TBoxAgentPriority.Text))
            {
                errors.AppendLine("Укажите приоритет");
            }
            else
            {
                if (!int.TryParse(TBoxAgentPriority.Text, out int priority) || priority < 0)
                {
                    errors.AppendLine("Приоритет должен быть целым неотрицательным числом");
                }
            }

            if (string.IsNullOrWhiteSpace(TBoxAgentDirectorName.Text))
            {
                errors.AppendLine("Укажите ФИО директора");
            }

            if (string.IsNullOrWhiteSpace(TBoxAgentAddress.Text))
            {
                errors.AppendLine("Укажите адрес");
            }

            if (string.IsNullOrWhiteSpace(TBoxAgentINN.Text))
            {
                errors.AppendLine("Укажите ИНН");
            }

            if (string.IsNullOrWhiteSpace(TBoxAgentKPP.Text))
            {
                errors.AppendLine("Укажите КПП");
            }

            if (string.IsNullOrWhiteSpace(TBoxAgentPhone.Text))
            {
                errors.AppendLine("Укажите телефон");
            }

            if (string.IsNullOrWhiteSpace(TBoxAgentEmail.Text))
            {
                errors.AppendLine("Укажите email");
            }

            if (errors.Length > 0)
            {
                _messageService.ShowError(errors.ToString());
                return false;
            }

            return true;
        }

        private async void BtnAgentDelete_Click(object sender, RoutedEventArgs e)
        {
            var messageResult = _messageService.ShowWarningExtended($"Удалить агента \"{_currentAgent.Title}\"?");

            if (messageResult == MessageBoxResult.Yes)
            {
                if (!_context.ProductSale.Any(ps => ps.AgentID == _currentAgent.ID))
                {
                    var shops = _context.Shop.Where(s => s.AgentID == _currentAgent.ID).ToList();
                    if (shops.Count > 0)
                    {
                        foreach (Shop shop in shops)
                        {
                            _context.Shop.Remove(shop);
                        }
                    }

                    var priorityHistories = _context.AgentPriorityHistory.Where(aph => aph.AgentID == _currentAgent.ID).ToList();
                    if (priorityHistories.Count > 0)
                    {
                        foreach (AgentPriorityHistory priorityHistory in priorityHistories)
                        {
                            _context.AgentPriorityHistory.Remove(priorityHistory);
                        }
                    }

                    ImgLogo.Source = null;

                    await Task.Delay(500);
                    GC.Collect();
                    await Task.Delay(200);

                    _context.Agent.Remove(_currentAgent);

                    DialogResult = true;
                }
                else
                {
                    _messageService.ShowWarning($"Невозможно удалить агента \"{_currentAgent.Title}\", т.к. у него есть информация о реализованной продукции");
                }
            }
        }

        private void BtnLogoEdit_Click(object sender, RoutedEventArgs e)
        {
            string agentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "images", "agents");

            var fileDialogWindow = new OpenFileDialog
            {
                InitialDirectory = agentsFolder
            };

            if (fileDialogWindow.ShowDialog() == true)
            {
                string sourcePath = fileDialogWindow.FileName;
                string targetPath = Path.Combine(agentsFolder, Path.GetFileName(sourcePath));

                if (sourcePath != targetPath)
                {
                    if (File.Exists(targetPath))
                    {
                        _messageService.ShowError($"Установить изображение \"{Path.GetFileName(targetPath)}\" невозможно, т.к. изображение с таким именем уже существует в системе");
                        return;
                    }

                    try
                    {
                        File.Copy(sourcePath, targetPath);
                    }
                    catch (Exception ex)
                    {
                        _messageService.ShowError($"Ошибка копирования файла.\n\n{ex.Message}");
                        return;
                    }
                }

                _currentAgent.Logo = $"/agents/{fileDialogWindow.SafeFileName}";

                ImgLogo.GetBindingExpression(Image.SourceProperty)?.UpdateTarget();
                TBlockLogoPath.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();

                SetupUI();
            }
        }

        private void BtnLogoDelete_Click(object sender, RoutedEventArgs e)
        {
            var messageResult = _messageService.ShowWarningExtended("Удалить логотип агента?");

            if (messageResult == MessageBoxResult.Yes)
            {
                _currentAgent.Logo = null;

                ImgLogo.GetBindingExpression(Image.SourceProperty)?.UpdateTarget();
                TBlockLogoPath.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();

                SetupUI();
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
