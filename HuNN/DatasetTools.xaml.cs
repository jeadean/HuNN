using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// DatasetTools.xaml 的交互逻辑
    /// </summary>
    public partial class DatasetTools : Window
    {
        private string originFile;
        List<DataSet> Datasets { get; set; }

        public DatasetTools()
        {
            InitializeComponent();
            Datasets = new List<DataSet>();
            this.radioBtnThreshold.IsChecked = true;
        }

        private void BtnOpenDataSet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Multiselect = false,
                    Title = "Open Dataset File",
                    Filter = "*|*"
                };

                if (dialog.ShowDialog() != true) return ;             
                 originFile = dialog.FileName;
                this.txtOpenFilePath.Text = originFile;
            }
            catch (Exception)
            {
                return;
            }
        }

        public static double[] ToDoubleArray(string value, char separator)
        {
            return Array.ConvertAll(value.Split(separator), s => double.Parse(s));
        }

        private void BtnConvert_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                int outputSize = int.Parse(this.txtOutputSize.Text.ToString().Trim());
                StreamReader sr = new StreamReader(originFile, Encoding.Default);
                double threshold = double.Parse(this.txtThreshold.Text.ToString().Trim());
                double min = double.Parse(this.txtXmin.Text.ToString().Trim());
                double max = double.Parse(this.txtXMax.Text.ToString().Trim());
                double v = max - min;
                int k = 0;
                if (this.radioBtnThreshold.IsChecked == true)
                    k = 0;
                else if (this.radioButtonMinMax.IsChecked == true)
                    k = 1;

                this.Datasets.Clear();
                ProgressDialogResult result = ProgressDialog.ProgressDialog.Execute(this, "Converting Dataset...", () => {

                    while (sr.Peek() > 0)
                    {
                        string temp = sr.ReadLine();
                        List<double> s = ToDoubleArray(temp, ',').ToList();

                        double[] tar = new double[outputSize];//初始化                                                   
                        tar[(int)s[0]] = 1; //把第一个数对应的数组中的值改成1，其它为0
                        s.RemoveAt(0);

                        for (int i = 0; i < s.Count; i++)
                        {
                            if (k == 0)
                            {
                                if (s[i] > threshold)
                                {
                                    s[i] = 1.0;
                                }
                                else
                                {
                                    s[i] = 0.0;
                                }
                            }
                            else if (k == 1)
                            {

                                s[i] = ((s[i] - min) / v) * 0.99 + 0.01;
                            }
                        }
                        DataSet ds = new DataSet(s.ToArray(), tar);
                        Datasets.Add(ds);
                    }


                });

                if (result.OperationFailed)
                    MessageBox.Show("Converting Dataset failed.");
                else
                    MessageBox.Show("Converting Dataset successfully.");

                sr.Close();
                txtDatasetSize.Content = Datasets.Count().ToString();
            }
            catch(Exception)
            {

            }
        }

        private void BtnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Save Dataset File",
                Filter = "Text File|*.txt;"
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                ProgressDialogResult result = ProgressDialog.ProgressDialog.Execute(this, "Converting Dataset...", () => {
                    using (var file = File.CreateText(dialog.FileName))
                    {
                        //var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                        //serializer.Serialize(file, datasets);
                        foreach (DataSet ds in Datasets)
                        {
                            string s = "";
                            foreach (double d in ds.Targets)
                            {
                                s += d.ToString() + ",";
                            }
                            foreach (double d in ds.Values)
                            {
                                s += d.ToString() + ",";
                            }
                            file.WriteLine(s.Substring(0, s.Length - 1));
                        }

                    }
                });
            }catch(Exception)
            {

            }
       
        }

        private void RadioBtnThreshold_Checked(object sender, RoutedEventArgs e)
        {
            this.txtThreshold.IsEnabled = true;
            this.txtXMax.IsEnabled = false;
            this.txtXmin.IsEnabled = false;
            this.radioButtonMinMax.IsChecked = false;
        }

        private void RadioButtonMinMax_Checked(object sender, RoutedEventArgs e)
        {
            this.txtThreshold.IsEnabled = false;
            this.txtXMax.IsEnabled = true;
            this.txtXmin.IsEnabled = true;
            this.radioBtnThreshold.IsChecked = false;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
