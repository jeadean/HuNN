
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using HuNN.Helper;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Controls;

namespace HuNN
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public Network Network { get; set; }      
        public List<DataSet> TrainDataSets { get; set; }
        public List<DataSet> TestDataSets { get; set; }


        //Data for charts visualization
        public SeriesCollection SeriesCollectionCPUUsage { get; set; } = new SeriesCollection(); //CPU Usage
        public List<String> TimeElapse { get; set; } = new List<string>();
        public List<double> CPUUsagePercent { get; set; } = new List<double>();

        public SeriesCollection SeriesCollectionEpoch { get; set; } = new SeriesCollection();
        public List<int> EpochNum { get; set; } = new List<int>();
        public List<double> ErrorEpoch { get; set; } = new List<double>();

        public SeriesCollection SeriesCollectionAccuracy { get; set; } = new SeriesCollection();
        public List<double> Accuracy { get; set; } = new List<double>();


        //private PerformanceCounter totalCPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        //private PerformanceCounter totalMemCounter = new PerformanceCounter("Memory", "Available MBytes");
        
       //To calculate the CPU Usage for this Process
        private PerformanceCounter theCPUCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);
        //  private PerformanceCounter theMemCounter = new PerformanceCounter("Process", "Working Set", Process.GetCurrentProcess().ProcessName);

        //To draw image for the network
        int canvasWidth { get; set; }
        int canvasHeight { get; set; }
        WriteableBitmap wBitmap { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            //init all charts
            chartErrorEpoch.DataContext = this;
            chartCpuUsage.DataContext = this;

            chartAccuracy.DataContext = this;

            SeriesCollectionCPUUsage.Add(new LineSeries
            {
                Title = "CPU Usage",
                Values = new ChartValues<float> { }
            });

            SeriesCollectionEpoch.Add(new LineSeries
            {
                Title = "Total Error",
                Values = new ChartValues<double> { }
            });

            SeriesCollectionAccuracy.Add(new LineSeries
            {
                Title = "Accuracy",
                Values = new ChartValues<double> { }
            });

            //make the canvas can Zoom in and Zoomout
            var st = new ScaleTransform();
            OutCanvas.RenderTransform = st;
            OutCanvas.MouseWheel += (sender, e) =>
            {
                if (e.Delta > 0)
                {
                    if (st.ScaleX >= 0.83) //prevent too big
                    {
                        st.ScaleX = 1;
                        st.ScaleY = 1;

                    }
                    else
                    {
                        st.ScaleX *= 1.2;
                        st.ScaleY *= 1.2;
                    }

                }
                else
                {
                    st.ScaleX /= 1.2;
                    st.ScaleY /= 1.2;
                }
            };
        }


        private void New_Click(object sender, RoutedEventArgs e)
        {
            NewNN newNN = new NewNN();
            if (newNN.ShowDialog() == true)
            {
                try
                {
                    //get all layers' sizes
                    int inputSize = int.Parse(newNN.txtBPNNInputSize.Text.ToString().Trim());
                    int outputSize = int.Parse(newNN.txtBPNNOutputSize.Text.ToString().Trim());                    
                    List<int> hiddenSize = new List<int>();
                    string str = newNN.txtBPNNHiddenSizes.Text.ToString().Trim();
                    string[] numbers = str.Substring(1, str.Length - 2).Split(',');
                    foreach (string s in numbers)
                    {
                        hiddenSize.Add(int.Parse(s));
                    }
                    //get learnrate and momentum
                    double learnRate = double.Parse(newNN.txtLearnrate.Text.ToString().Trim());
                    double momentum = double.Parse(newNN.txtMomentum.Text.ToString().Trim());

                    this.Network = new Network(this, inputSize, hiddenSize.ToArray(), outputSize, learnRate, momentum);
                    this.Log.Text += "Initiate Neural Network Successfully!\n";
                    this.txtNNStruct.DataContext = this.Network;
                    this.txtLearnRate.DataContext = this.Network;
                    this.txtMomentum.DataContext = this.Network;
                    this.txtLearnRate.IsEnabled = true;
                    this.txtMomentum.IsEnabled = true;
                    resetAllButtonState();

                }
                catch (Exception)
                {

                }
            }

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (this.Network != null)
            {
                ExportHelper.ExportNetwork(this.Network);
            }else
            {
                MessageBox.Show("There is no Network!");
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Network = ImportHelper.ImportNetwork();

                if (this.Network != null)
                {
                    this.btnLoadTrainData.IsEnabled = true;
                    this.btnTrain.IsEnabled = true;
                    this.btnTrainOneStep.IsEnabled = true;
                    this.btnLoadTestData.IsEnabled = true;
                    this.btnTest.IsEnabled = true;
                }

            }
            catch (Exception ex)
            {
                return;
            }

        }

        private void LoadTrainData_Click(object sender, RoutedEventArgs e)
        {
            if (this.Network == null)
            {
                MessageBox.Show("Please set a new Neural Network first!");
                return;
            }

            //_dataSets = ImportHelper.ImportDatasetsFromCSV();  

            //the dataset must be normolized to a standard format, for example a network with 10 input and 3 output,the format will be:
            // 0,1,0,1,1,1,1,1,1,1,1,0,0    in which the first three number correspond to the desired out and the rest 10 correspon to the input data
            //There is a Dataset conver Tool to convert standard NMIST dataset, you can go to the Exporthelper.cs to find the details
            this.TrainDataSets = ImportHelper.ImportDatasetsFromCSV(this.Network.InputLayer.Count, this.Network.OutputLayer.Count);

            if (TrainDataSets == null)
            {
                this.btnLoadTrainData.IsEnabled = true;
                this.btnTrain.IsEnabled = false;
                this.btnTrainOneStep.IsEnabled = false;

                MessageBox.Show("Fail to load Dataset,please check the format of the file!");
                return;
            }

            if (TrainDataSets.Any(x => x.Values.Length != this.Network.InputLayer.Count() || x.Targets.Length != this.Network.OutputLayer.Count()))
            {
                MessageBox.Show("The length of Dataset are not equal to the length of the input size of Neural Network!");
                return;
            }

            resetAllButtonState();
            this.btnLoadTrainData.IsEnabled = true;
            this.btnTrain.IsEnabled = true;
            this.btnTrainOneStep.IsEnabled = true;
        }

        private void TrainOneStep_Click(object sender, RoutedEventArgs e)
        {
            if (this.Network == null)
            {
                MessageBox.Show("Please set a new Neural Network first!");
                return;
            }
            if (this.TrainDataSets == null)
            {
                MessageBox.Show("Please train the Network first by choosing the Dataset file！");
                return;
            }

            resetAllButtonState();
            
            if (this.checkboxTrainWithTest.IsChecked == true)
            {
                trainEpoch(1, true);
            }
            else
            {
                trainEpoch(1, false);
            }
        }

        private void Train_Click(object sender, RoutedEventArgs e)
        {
            if (this.Network == null)
            {
                MessageBox.Show("Please set a new Neural Network first!");
                return;
            }

            if (this.TrainDataSets == null)
            {
                MessageBox.Show("Please train the Network first by choosing the Dataset file！");
                return;
            }

            if (this.checkboxTrainWithTest.IsChecked == true && this.TestDataSets == null)
            {
                MessageBox.Show("Please load Train Dataset beacause you checked the CheckBox 'Test while Trainning'！");
                return;
            }

            TrainParameterSet trainParameterSet = new TrainParameterSet();
            if (trainParameterSet.ShowDialog() == true)
            {
                resetAllButtonState();
                this.btnStopTrain.IsEnabled = true;

                this.stopTrain = false;

                //choose train by epoch or by min error
                if (trainParameterSet.radioBtnByEpoch.IsChecked == true)
                {
                    if(this.checkboxTrainWithTest.IsChecked==true)
                    {
                        trainEpoch(int.Parse(trainParameterSet.txtEpochNumber.Text.ToString().Trim()), true);
                    }
                    else
                    {
                        trainEpoch(int.Parse(trainParameterSet.txtEpochNumber.Text.ToString().Trim()), false);
                    }
                }
                else if (trainParameterSet.radioBtnByError.IsChecked == true)
                {
                    if(this.checkboxTrainWithTest.IsChecked==true)
                    {
                        trainError(double.Parse(trainParameterSet.txtMinimumError.Text.ToString().Trim()),true);
                    }else
                    {
                        trainError(double.Parse(trainParameterSet.txtMinimumError.Text.ToString().Trim()), false);
                    }                    
                }
            }
        }


        bool stopTrain = false;

        void trainError(Object minimumError,bool isTest)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                TrainEpocNum.Content = "0";
                TrainElapseTime.Content = "0";
            }));

            var error = 1.0;
            var numEpochs = 0;

            ProgressDialogResult result = ProgressDialog.ProgressDialog.Execute(this, "Trainning Network", () =>
            {
                while (error > (double)minimumError && numEpochs < int.MaxValue)
                {
                    var errors = new List<double>();
                    int j = 0;
                    foreach (var dataSet in TrainDataSets)
                    {
                        ProgressDialog.ProgressDialog.Current.Report("Trained Epoch {0}... \n ", numEpochs);

                        if (this.stopTrain == true)
                        {
                            break;
                        }

                        this.Network.TrainOneStep(dataSet);

                        errors.Add(this.Network.TotalError);
                        j++;
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            TrainNum.Content = j.ToString();
                            TrainEpocNum.Content = numEpochs.ToString();
                            TrainElapseTime.Content = (sw.ElapsedMilliseconds/60.0).ToString("0.00")+"s";
                            TotalError.Content = string.Format("{0:N3}", this.Network.TotalError);
                        }));
                    }
                    error = errors.Average();
                    numEpochs++;

                    if (isTest)
                    {
                        if (this.TestDataSets == null)
                        {
                            this.btnTest.IsEnabled = false;
                            return;
                        }

                        int right = 0;
                        int sum = 0;

                        for (int k = 0; k < TestDataSets.Count(); k++)
                        {
                            ProgressDialog.ProgressDialog.Current.Report("Testing {0}/{1}... \n ", k, (int)TestDataSets.Count());
                            double[] a = this.Network.Compute(TestDataSets[k].Values);
                            if (findMaxIndex(a) == findMaxIndex(TestDataSets[k].Targets))
                            {
                                right++;
                            }
                            sum++;
                        }

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {

                            SeriesCollectionAccuracy[0].Values.Add(right / (double)sum);
                            if (SeriesCollectionAccuracy[0].Values.Count > 50)
                                SeriesCollectionAccuracy[0].Values.RemoveAt(0);
                        }));
                    }

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        EpochNum.Add(numEpochs);
                        SeriesCollectionEpoch[0].Values.Add(error);
                        if (SeriesCollectionEpoch[0].Values.Count > 50)
                            SeriesCollectionEpoch[0].Values.RemoveAt(0);


                        TimeElapse.Add(DateTime.Now.ToString("mm-ss"));
                        SeriesCollectionCPUUsage[0].Values.Add(theCPUCounter.NextValue() / Environment.ProcessorCount);
                        if (SeriesCollectionCPUUsage[0].Values.Count > 50)
                            SeriesCollectionEpoch[0].Values.RemoveAt(0);
                    }));
                }

            }, ProgressDialogSettings.WithSubLabel);

            if (result.OperationFailed)
                MessageBox.Show("Train failed.");
            else
                MessageBox.Show("Train successfully.");


            sw.Stop();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                resetAllButtonState();

                this.btnTrainOneStep.IsEnabled = true;
                this.btnTrain.IsEnabled = true;
                this.btnLoadTestData.IsEnabled = true;
                this.btnSave.IsEnabled = true;
                if (this.TestDataSets != null)
                {
                    this.btnTest.IsEnabled = true;
                }
            }));
        }

        void trainEpoch(Object EpochNumber,bool isTest)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                TrainEpocNum.Content = "0";
                TrainElapseTime.Content = "0";
            }));


            ProgressDialogResult result = ProgressDialog.ProgressDialog.Execute(this, "Trainning Network", () =>
            {
                for (int i = 0; i < (int)EpochNumber; i++)
                {
                    ProgressDialog.ProgressDialog.Current.Report("Trained Epoch {0}/{1}... \n ", i, (int)EpochNumber);
                    int j = 0;
                    foreach (var dataSet in TrainDataSets)
                    {
                        if (this.stopTrain == true)
                        {
                            break;
                        }
                        this.Network.TrainOneStep(dataSet);
                        j++;
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {

                            TrainNum.Content = j.ToString();
                            TrainEpocNum.Content = i.ToString();
                            TrainElapseTime.Content = (sw.ElapsedMilliseconds / 60.0).ToString("0.00") + "s";
                            TotalError.Content = string.Format("{0:N3}", this.Network.TotalError);

                        }));
                    }

                    if (isTest)
                    {
                        if (this.TestDataSets == null)
                        {
                            this.btnTest.IsEnabled = false;
                            return;
                        }

                        int right = 0;
                        int sum = 0;

                        for (int k = 0; k < TestDataSets.Count(); k++)
                        {
                            ProgressDialog.ProgressDialog.Current.Report("Testing {0}/{1}... \n ", k, (int)TestDataSets.Count());
                            double[] a = this.Network.Compute(TestDataSets[k].Values);
                            if (findMaxIndex(a) == findMaxIndex(TestDataSets[k].Targets))
                            {
                                right++;
                            }
                            sum++;
                        }

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {

                            SeriesCollectionAccuracy[0].Values.Add(right / (double)sum);
                            if (SeriesCollectionAccuracy[0].Values.Count > 50)
                                SeriesCollectionAccuracy[0].Values.RemoveAt(0);
                        }));
                    }


                    this.Dispatcher.BeginInvoke(new Action(() =>
                      {
                          EpochNum.Add(i);
                          SeriesCollectionEpoch[0].Values.Add(this.Network.TotalError);
                          if (SeriesCollectionEpoch[0].Values.Count > 50)
                              SeriesCollectionEpoch[0].Values.RemoveAt(0);


                          TimeElapse.Add(DateTime.Now.ToString("mm-ss"));
                          SeriesCollectionCPUUsage[0].Values.Add(theCPUCounter.NextValue() / Environment.ProcessorCount);
                          if (SeriesCollectionCPUUsage[0].Values.Count > 50)
                              SeriesCollectionCPUUsage[0].Values.RemoveAt(0);
                      }));
                }

            }, ProgressDialogSettings.WithSubLabel);

            if (result.OperationFailed)
                MessageBox.Show("Train failed.");
            else
                MessageBox.Show("Train successfully.");

            sw.Stop();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                resetAllButtonState();

                this.btnTrainOneStep.IsEnabled = true;
                this.btnTrain.IsEnabled = true;
                this.btnLoadTestData.IsEnabled = true;
                this.btnSave.IsEnabled = true;
                if (this.TestDataSets != null)
                {
                    this.btnTest.IsEnabled = true;
                }
            }));
        }


        List<double[]> res = new List<double[]>();
        List<bool> rightOrWrong = new List<bool>();

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            testOnce();
        }

        void testOnce()
        {
            if (this.TestDataSets == null)
            {
                this.btnTest.IsEnabled = false;
                return;
            }

            int right = 0;
            int sum = 0;

            ProgressDialogResult result = ProgressDialog.ProgressDialog.Execute(this, "Testing Network", () =>
            {
                for (int i = 0; i < TestDataSets.Count(); i++)
                {
                    ProgressDialog.ProgressDialog.Current.Report("Testing Number {0}/{1}... \n ", i, TestDataSets.Count());
                    double[] a = this.Network.Compute(TestDataSets[i].Values);
                    if (findMaxIndex(a) == findMaxIndex(TestDataSets[i].Targets))
                    {
                        right++;
                    }
                    sum++;
                }

            }, ProgressDialogSettings.WithSubLabel);

            if (result.OperationFailed)
                MessageBox.Show("Test failed.");
            else
                MessageBox.Show("Total test sample Number:" + sum.ToString() + "\n" + "Right number:" + right.ToString() + "\n"
                              + "Wrong number:" + (sum - right).ToString() + "\n"
                              + "Accuracy:" + (right / (double)sum).ToString());

            this.Dispatcher.BeginInvoke(new Action(() =>
            {

                SeriesCollectionAccuracy[0].Values.Add(right / (double)sum);
                if (SeriesCollectionAccuracy[0].Values.Count > 50)
                    SeriesCollectionAccuracy[0].Values.RemoveAt(0);
            }));
        }



        int findMaxIndex(double[] a)
        {
            int k = 0;
            double max = 0;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] > max)
                {
                    max = a[i];
                    k = i;
                }
            }
            return k;
        }

        int findMaxLayerSize(Network nn)
        {
            int n = nn.InputLayer.Count();
            foreach (var layer in nn.HiddenLayers)
            {
                if (layer.Count > n)
                    n = layer.Count;
            }
            if (nn.OutputLayer.Count() > n)
                n = nn.OutputLayer.Count;

            return n;
        }


        private void TxtLearnRate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void TxtMomentum_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }

        }

        private void BtnLoadTestData_Click(object sender, RoutedEventArgs e)
        {
            if (this.Network == null)
            {
                MessageBox.Show("Please set a new Neural Network first!");
                return;
            }

            this.TestDataSets = ImportHelper.ImportDatasetsFromCSV(this.Network.InputLayer.Count, this.Network.OutputLayer.Count);

            if (this.TestDataSets == null)
            {
                this.btnTest.IsEnabled = false;
                return;
            }
            this.btnTest.IsEnabled = true;
        }

        private void StopTrain_Click(object sender, RoutedEventArgs e)
        {
            this.stopTrain = true;
            btnStopTrain.IsEnabled = false;

            btnTrain.IsEnabled = true;
            btnTrainOneStep.IsEnabled = true;
            btnLoadTestData.IsEnabled = true;
            if (this.TrainDataSets == null)
            {
                btnTest.IsEnabled = false;
            }
            else
            {
                btnTest.IsEnabled = true;
            }
        }

        void resetAllButtonState()
        {
            this.btnNew.IsEnabled = true;
            this.btnSave.IsEnabled = this.Network != null ? true : false;
            this.btnLoad.IsEnabled = true;

            this.btnLoadTrainData.IsEnabled = this.Network != null ? true : false;

            this.btnTrainOneStep.IsEnabled = (this.Network != null && this.TrainDataSets != null) ? true : false;
            this.btnTrain.IsEnabled = (this.Network != null && this.TrainDataSets != null) ? true : false;
            this.btnStopTrain.IsEnabled = false;
            this.btnLoadTestData.IsEnabled = (this.Network != null && this.TrainDataSets != null) ? true : false;
            this.btnTest.IsEnabled = (this.Network != null && this.TestDataSets != null) ? true : false;
        }

        private void BtnDatasetTools_Click(object sender, RoutedEventArgs e)
        {
            DatasetTools datasetTools = new DatasetTools();
            datasetTools.ShowDialog();
        }

        private void ChartError_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }



        private void BtnShowNNTreeView_Click(object sender, RoutedEventArgs e)
        {
            if (this.Network == null)
                return;


            TreeViewItem root = new TreeViewItem();
            root.Header = "NeuralNetwork";
            TreeViewItem tvInputLayer = new TreeViewItem();
            tvInputLayer.Header = "InputLayer";
            root.Items.Add(tvInputLayer);
            this.Network.InputLayer.ForEach((neuron) =>
            {
                TreeViewItem nnode = new TreeViewItem();
                nnode.Header = "Neuron";
                nnode.Items.Add("Bias:" + neuron.Bias.ToString("0.00"));
                nnode.Items.Add("Gradient:" + neuron.Gradient.ToString("0.00"));
                nnode.Items.Add("InputValue:" + neuron.OutputValue.ToString("0.00"));
                nnode.Items.Add("OutputValue:" + neuron.OutputValue.ToString("0.00"));

                neuron.OutputSynapses.ForEach((synapse) =>
                {
                    TreeViewItem snode = new TreeViewItem();
                    snode.Header = "Synapse";
                    snode.Items.Add("Weight:" + synapse.Weight.ToString("0.00"));
                    snode.Items.Add("WeightDelta:" + synapse.WeightDelta.ToString("0.00"));
                    nnode.Items.Add(snode);
                });
                tvInputLayer.Items.Add(nnode);
            });


            TreeViewItem tvHiddenLayers = new TreeViewItem();
            tvHiddenLayers.Header = "HiddenLayers";
            root.Items.Add(tvHiddenLayers);

            this.Network.HiddenLayers.ForEach((layer) =>
            {
                TreeViewItem lnode = new TreeViewItem();
                lnode.Header = "Layer";

                layer.ForEach((neuron) =>
                {
                    TreeViewItem nnode = new TreeViewItem();
                    nnode.Header = "Neuron";
                    nnode.Items.Add("Bias:" + neuron.Bias.ToString("0.00"));
                    nnode.Items.Add("Gradient:" + neuron.Gradient.ToString("0.00"));
                    nnode.Items.Add("InputValue:" + neuron.OutputValue.ToString("0.00"));
                    nnode.Items.Add("OutputValue:" + neuron.OutputValue.ToString("0.00"));

                    neuron.OutputSynapses.ForEach((synapse) =>
                    {
                        TreeViewItem snode = new TreeViewItem();
                        snode.Header = "Synapse";
                        snode.Items.Add("Weight:" + synapse.Weight.ToString("0.00"));
                        snode.Items.Add("WeightDelta:" + synapse.WeightDelta.ToString("0.00"));
                        nnode.Items.Add(snode);
                    });
                    lnode.Items.Add(nnode);
                });
                tvHiddenLayers.Items.Add(lnode);
            });
            TreeViewItem tvOutputLayer = new TreeViewItem();
            tvOutputLayer.Header = "OutputLayer";
            root.Items.Add(tvOutputLayer);
            this.Network.OutputLayer.ForEach((neuron) =>
            {
                TreeViewItem nnode = new TreeViewItem();
                nnode.Header = "Neuron";
                nnode.Items.Add("Bias" + neuron.Bias.ToString("0.00"));
                nnode.Items.Add("Gradient:" + neuron.Gradient.ToString("0.00"));
                nnode.Items.Add("InputValue" + neuron.OutputValue.ToString("0.00"));
                nnode.Items.Add("OutputValue" + neuron.OutputValue.ToString("0.00"));

                neuron.OutputSynapses.ForEach((synapse) =>
                {
                    TreeViewItem snode = new TreeViewItem();
                    snode.Header = "Synapse";
                    snode.Items.Add("Weight" + synapse.Weight.ToString("0.00"));
                    snode.Items.Add("WeightDelta" + synapse.WeightDelta.ToString("0.00"));
                    nnode.Items.Add(snode);
                });
                tvOutputLayer.Items.Add(nnode);
            });


            this.treeViewNN.Items.Clear();
            this.treeViewNN.Items.Add(root);


        }

        private void BtnShowNNPic_Click(object sender, RoutedEventArgs e)
        {
            if (this.Network == null)
                return;

            int neuronWidth = 10;
            int neuronDistance = 5;
            int layerDistance = 40;
            int fontSize = 6;

            //Init Canvas and image buffer
            // canvasWidth = (int)OutCanvas.ActualWidth;
            canvasHeight = (int)OutCanvas.ActualHeight;

            int w = findMaxLayerSize(this.Network) * (neuronWidth + neuronDistance);


            DisplayImage.Width = w;
            DisplayImage.Height = canvasHeight;

            wBitmap = new WriteableBitmap(w, canvasHeight, 72, 72, PixelFormats.Bgr24, null);
            DisplayImage.Source = wBitmap;


            wBitmap.Lock();
            Bitmap backBitmap = new Bitmap(w, canvasHeight, wBitmap.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format24bppRgb, wBitmap.BackBuffer);

            Graphics graphics = Graphics.FromImage(backBitmap);
            graphics.Clear(System.Drawing.Color.WhiteSmoke);

            int x, y = 0;
           
            //drwa input layer first
            y = 20;
            for (int i = 0; i < this.Network.InputLayer.Count; i++)
            {
                x = 20 + (neuronWidth + neuronDistance) * i;
                graphics.FillEllipse(System.Drawing.Brushes.LightSeaGreen, x, y, neuronWidth, neuronWidth);
                graphics.DrawEllipse(Pens.SeaGreen, x, y, neuronWidth, neuronWidth);

                //graphics.DrawString(this.Network.InputLayer[i].OutputValue.ToString("0.00"), new Font("微软雅黑", fontSize), System.Drawing.Brushes.Black, x + 2, y + (neuronWidth / 2) - 5);
            }

            y += 80;
            for (int i = 0; i < this.Network.HiddenLayers.Count; i++)
            {
                y += layerDistance;
                for (int j = 0; j < this.Network.HiddenLayers[i].Count; j++)
                {
                    x = 20 + (neuronWidth + neuronDistance) * j;

                    graphics.FillEllipse(System.Drawing.Brushes.LightSkyBlue, x, y, neuronWidth, neuronWidth);
                    graphics.DrawEllipse(Pens.SkyBlue, x, y, neuronWidth, neuronWidth);

                    // graphics.DrawString(this.Network.HiddenLayers[i][j].OutputValue.ToString("0.00"), new Font("微软雅黑", fontSize), System.Drawing.Brushes.Black, x + 2, y + (neuronWidth / 2) - 5);
                }
            }

            y += 80;

            for (int i = 0; i < this.Network.OutputLayer.Count; i++)
            {
                x = 20 + (neuronWidth + neuronDistance) * i;

                graphics.FillEllipse(System.Drawing.Brushes.LightCyan, x, y, neuronWidth, neuronWidth);
                graphics.DrawEllipse(Pens.DarkCyan, x, y, neuronWidth, neuronWidth);
                // graphics.DrawString(this.Network.OutputLayer[i].OutputValue.ToString("0.00"), new Font("微软雅黑", fontSize), System.Drawing.Brushes.Black, x + 2, y + (neuronWidth / 2) - 5);
            }
            graphics.Flush();
            graphics.Dispose();
            graphics = null;

            backBitmap.Dispose();
            backBitmap = null;

            wBitmap.AddDirtyRect(new Int32Rect(0, 0, w, canvasHeight));
            wBitmap.Unlock();
        }

        private void CheckboxTrainWithTest_Click(object sender, RoutedEventArgs e)
        {
            this.btnLoadTestData.IsEnabled = (bool)this.checkboxTrainWithTest.IsChecked;
        }
    }
}
