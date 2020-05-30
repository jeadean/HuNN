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

namespace HuNN
{
    /// <summary>
    /// TrainParameterSet.xaml 的交互逻辑
    /// </summary>
    public partial class TrainParameterSet : Window
    {
        public TrainParameterSet()
        {
            InitializeComponent();

            HideAll();
            
        }

        void HideAll()
        {
            this.stackMinError.Visibility = Visibility.Collapsed;
            this.stackMaxEpoch.Visibility = Visibility.Collapsed;
        }

        private void RadioBtnByEpoch_Click(object sender, RoutedEventArgs e)
        {
            HideAll();
            this.stackMaxEpoch.Visibility = Visibility.Visible;
            
        }

        private void RadioBtnByError_Click(object sender, RoutedEventArgs e)
        {
            HideAll();
            this.stackMinError.Visibility = Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(this.radioBtnByEpoch.IsChecked==false && this.radioBtnByError.IsChecked==false)
             {
                MessageBox.Show("You must choose one train mode!");
            }

            try
            {
                int.Parse(this.txtEpochNumber.Text.ToString().Trim());
            }catch(Exception ex)
            {
                MessageBox.Show("The number of Epoch must be a Integer!");
            }

            try
            {
                double.Parse(this.txtMinimumError.Text.ToString().Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show("The number of Minimum Error must be a Double!");
            }

            this.DialogResult = true;
            this.Close();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
