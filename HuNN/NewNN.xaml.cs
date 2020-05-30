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
    /// NewNN.xaml 的交互逻辑
    /// </summary>
    public partial class NewNN : Window
    {
        public NewNN()
        {
            InitializeComponent();
            this.comboBoxNNType.SelectionChanged += ComboBoxNNType_SelectionChanged;
        }

        private void ComboBoxNNType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HideAllGroupBox();
            if (comboBoxNNType.SelectedIndex == 0)
            {
                this.groupBoxBPNNStruct.Visibility = Visibility.Visible;
            }
        }
      
        void HideAllGroupBox()
        {
            this.groupBoxBPNNStruct.Visibility = Visibility.Collapsed;
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {          
            if(this.comboBoxNNType.SelectedIndex==0)
            {               
                try
                {
                    int.Parse(this.txtBPNNInputSize.Text.ToString().Trim());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("请输入正确的输入层神经元数目，必须是数字!");
                    return;
                }

                try
                {
                    int.Parse(this.txtBPNNOutputSize.Text.ToString().Trim());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("请输入正确的输出层神经元数目，必须是数字!");
                    return;
                }

                string str = this.txtBPNNHiddenSizes.Text.ToString().Trim();
                if (!str.Substring(0, 1).Equals("{") || !str.Substring(str.Length - 1, 1).Equals("}"))
                {
                    MessageBox.Show("请输入正确的隐藏层神经元格式，如{50,20,1}");
                }

                try
                {
                    string[] numbers = str.Substring(1, str.Length - 2).Split(',');
                    foreach (string s in numbers)
                    {
                        int.Parse(s);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("请输入正确的隐藏层神经元格式，如{50,20,1}");
                }

                try
                {
                    double.Parse(this.txtLearnrate.Text.ToString().Trim());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("请输入正确的学习速率，必须是数字!");
                    return;
                }

                try
                {
                    double.Parse(this.txtMomentum.Text.ToString().Trim());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("请输入正确的动量，必须是数字!");
                    return;
                }


                try
                {
                    int.Parse(this.txtBPNNInputSize.Text.ToString().Trim());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("请输入正确的输入层神经元数目，必须是数字!");
                    return;
                }
            }           
          
            this.DialogResult = true;
            this.Close();
        }

        

        private void BtnCancle_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
