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
    /// Interaction logic for CreateCube.xaml
    /// </summary>
    public partial class CreateCube : Window
    {
        public CreateCube()
        {
            InitializeComponent();
        }

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

        private void Radius_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
              if (!(e.Text[0]>='0' && e.Text[0] <= '9' || e.Text[0] == '.'))
                e.Handled = true;
            base.OnPreviewTextInput(e);
        }

        private void X_TextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void X_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (!(e.Text[0] >= '0' && e.Text[0] <= '9' || e.Text[0] == '.' || e.Text[0] == '-'))
            {
                e.Handled = true;
            }
            else 
            {
                textBox.BorderBrush = Brushes.Gray;
            }
        }
    }
}
