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
using System.Text.RegularExpressions;

namespace Primitives
{
    /// <summary>
    /// Interaction logic for CreateCone.xaml
    /// </summary>
    public partial class ConnectDatabaseForm : Window
    {
        public ConnectDatabaseForm()
        {
            InitializeComponent();
        }

        public string ConnectionString;

        private void Label_KeyDown(object sender, KeyEventArgs e)
        {

        }


        private void Ok_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Auth_Checked(object sender, RoutedEventArgs e)
        {
            Password.IsEnabled = false;
            UserName.IsEnabled = false;
            LPassword.IsEnabled = false;
            LUser.IsEnabled = false;
        }

        private void Auth_Unchecked(object sender, RoutedEventArgs e)
        {
            Password.IsEnabled = true;
            UserName.IsEnabled = true;
            LPassword.IsEnabled = true;
            LUser.IsEnabled = true;
        }
    }
}
