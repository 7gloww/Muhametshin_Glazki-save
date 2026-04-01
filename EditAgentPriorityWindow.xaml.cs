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
    /// Логика взаимодействия для EditAgentPriorityWindow.xaml
    /// </summary>
    public partial class EditAgentPriorityWindow : Window
    {
        private MessageService _messageService = new MessageService();
        public int Priority { get; private set; }
        public EditAgentPriorityWindow(int maxPriority)
        {
            InitializeComponent();

            TBoxAgentPriority.Text = maxPriority.ToString();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TBoxAgentPriority.Text, out int priority) && priority >= 0)
            {
                Priority = priority;
                DialogResult = true;
            }
            else
            {
                _messageService.ShowError("Приоритет должен быть целым неотрицательным числом");
            }
        }
    }
}
