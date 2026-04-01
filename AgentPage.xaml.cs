using Muhametshin_Глазки_save;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace Muhametshin_Глазки_save
{
    /// <summary>
    /// Логика взаимодействия для AgentsPage.xaml
    /// </summary>
    public partial class AgentPage : Page
    {
        private MessageService _messageService = new MessageService();
        private List<Agent> _filteredAgents;
        private int pageSize = 10;
        private int currentPage = 1;

        public AgentPage()
        {
            InitializeComponent();

            ComboSort.SelectedIndex = 0;
            ComboType.SelectedIndex = 0;
            UpdateAgents();
        }
        private void UpdateAgents()
        {
            var currentAgent = MuhametshinEyesEntities.GetContext().Agent.ToList();

            //int totalNumAgents = currentAgent.Count;

            if (ComboSort.SelectedIndex == 1)
            {
                currentAgent = currentAgent.OrderBy(a => a.Title).ToList();
            }
            if (ComboSort.SelectedIndex == 2)
            {
                currentAgent = currentAgent.OrderByDescending(a => a.Title).ToList();
            }
            if (ComboSort.SelectedIndex == 4)
            {
                currentAgent = currentAgent.OrderBy(a => a.Discount).ToList();
            }
            if (ComboSort.SelectedIndex == 5)
            {
                currentAgent = currentAgent.OrderByDescending(a => a.Discount).ToList();
            }
            if (ComboSort.SelectedIndex == 7)
            {
                currentAgent = currentAgent.OrderBy(a => a.Priority).ToList();
            }
            if (ComboSort.SelectedIndex == 8)
            {
                currentAgent = currentAgent.OrderByDescending(a => a.Priority).ToList();
            }

            if (ComboType.SelectedIndex == 1)
            {
                currentAgent = currentAgent.Where(a => a.AgentType.Title == "ЗАО").ToList();
            }
            if (ComboType.SelectedIndex == 2)
            {
                currentAgent = currentAgent.Where(a => a.AgentType.Title == "МКК").ToList();
            }
            if (ComboType.SelectedIndex == 3)
            {
                currentAgent = currentAgent.Where(a => a.AgentType.Title == "МФО").ToList();
            }
            if (ComboType.SelectedIndex == 4)
            {
                currentAgent = currentAgent.Where(a => a.AgentType.Title == "ОАО").ToList();
            }
            if (ComboType.SelectedIndex == 5)
            {
                currentAgent = currentAgent.Where(a => a.AgentType.Title == "ООО").ToList();
            }
            if (ComboType.SelectedIndex == 6)
            {
                currentAgent = currentAgent.Where(a => a.AgentType.Title == "ПАО").ToList();
            }

            string searchDigits = new string(TBoxSearch.Text.Where(char.IsDigit).ToArray());


            currentAgent = currentAgent
            .Where(a =>
                a.Title.ToLower().Contains(TBoxSearch.Text.ToLower()) ||
                (!string.IsNullOrEmpty(searchDigits) && new string(a.Phone.Where(char.IsDigit).ToArray()).Contains(searchDigits)) ||
                a.Email.Contains(TBoxSearch.Text.ToLower())
                )
            .ToList();


            //int currentNumAgents = currentAgent.Count;

            //TBlockNumRecords.Text = $"{currentNumAgents} из {totalNumAgents}";


            _filteredAgents = currentAgent;
            currentPage = 1;
            ChangePage();
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAgents();
        }

        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAgents();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAgents();
        }

        private void ChangePage()
        {
            PageListBox.Items.Clear();

            int totalPages = (_filteredAgents.Count + pageSize - 1) / pageSize;

            for (int i = 1; i <= totalPages; i++)
            {
                PageListBox.Items.Add(i);
            }

            PageListBox.SelectedItem = currentPage;

            var agentsPage = _filteredAgents
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize).ToList();

            AgentListView.ItemsSource = agentsPage;
        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (_filteredAgents.Count + pageSize - 1) / pageSize;
            if (currentPage > 1)
            {
                currentPage--;
                ChangePage();
            }
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (_filteredAgents.Count + pageSize - 1) / pageSize;
            if (currentPage < totalPages)
            {
                currentPage++;
                ChangePage();
            }
        }

        private void PageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageListBox.SelectedItem is int page && page != currentPage)
            {
                currentPage = page;
                ChangePage();
            }
        }
        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is Agent agent)
            {
                var addEditAgentWindow = new AddEditPageWindow(agent);

                string oldShortRelativeLogoPath = agent.Logo;

                try
                {
                    if (addEditAgentWindow.ShowDialog() == true)
                    {
                        MuhametshinEyesEntities.GetContext().SaveChanges();

                        UpdateAgents();

                        await Task.Delay(500);
                        GC.Collect();
                        await Task.Delay(200);

                        string absoluteLogoPath = "";
                        if (!string.IsNullOrEmpty(oldShortRelativeLogoPath))
                        {
                            absoluteLogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "images", oldShortRelativeLogoPath.TrimStart('/'));
                            DeletePrevLogo(absoluteLogoPath);
                        }
                    }
                    else
                    {
                        MuhametshinEyesEntities.GetContext().Entry(agent).Reload();
                        UpdateAgents();
                    }
                }
                catch (Exception ex)
                {
                    _messageService.ShowError($"Ошибка сохранения информации.\n\n{ex.Message}");
                }
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void DeletePrevLogo(string absolutePath)
        {
            string shortRelativePath = $"/agents/{Path.GetFileName(absolutePath)}";

            try
            {
                if (!MuhametshinEyesEntities.GetContext().Agent.Any(a => a.Logo == shortRelativePath))
                {
                    File.Delete(absolutePath);
                    _messageService.ShowInfo($"Предыдущий логотип \"{Path.GetFileName(absolutePath)}\" успешно удалён.");
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Ошибка удаления предыдущего логотипа.\n\n{ex.Message}");
            }
        }

        private void BtnAddAgent_Click(object sender, RoutedEventArgs e)
        {
            var AddEditAgentWindow = new AddEditPageWindow(null);

            try
            {
                if (AddEditAgentWindow.ShowDialog() == true)
                {
                    MuhametshinEyesEntities.GetContext().SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Ошибка добавления нового агента.\n\n{ex.Message}");
            }

            UpdateAgents();
        }

        private void AgentListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AgentListView.SelectedItems.Count is int count && count > 1)
            {
                PanelEditPriority.Visibility = Visibility.Visible;
                TBlockNumSelected.Text = $"Выделено агентов: {count}";
            }
            else
            {
                PanelEditPriority.Visibility = Visibility.Collapsed;
            }
        }
        private void BtnEditAgentPriority_Click(object sender, RoutedEventArgs e)
        {
            var selectedAgents = AgentListView.SelectedItems.Cast<Agent>().ToList();
            int maxPriority = selectedAgents.Max(a => a.Priority);
            var editAgentPriorityWindow = new EditAgentPriorityWindow(maxPriority);
            try
            {
                if (editAgentPriorityWindow.ShowDialog() == true)
                {
                    int newPriority = editAgentPriorityWindow.Priority;
                    foreach (Agent agent in selectedAgents)
                    {
                        agent.Priority = newPriority;
                    }
                    MuhametshinEyesEntities.GetContext().SaveChanges();
                    UpdateAgents();
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Ошибка изменения приоритета.\n\n{ex.Message}");
            }
        }
    }
}