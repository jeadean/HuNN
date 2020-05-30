
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HuNN
{
    public class Network:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region -- Properties --
        private double learnRate=0.0;
        public double LearnRate {
            get {
                return learnRate;
            }
            set
            {
                if (this.learnRate != value)           
                {
                    this.learnRate = value;                                          
                    if (PropertyChanged != null)                                    
                    {
                        //通知绑定此变量的textbox在前台更新
                        PropertyChanged(this, new PropertyChangedEventArgs("LearnRate"));
                    }
                }
            }
        } //学习速率

        private double momentum=0.0;
        public double Momentum
        {
            get
            {
                return momentum;
            }
            set
            {
                if (this.momentum != value)           
                {
                    this.momentum = value;                                          
                    if (PropertyChanged != null)                                    
                    {
                        //通知绑定此变量的textbox在前台更新
                        PropertyChanged(this, new PropertyChangedEventArgs("Momentum"));
                    }
                }
            }
        } //动量 典型值0.9

        private string nnStruct="";
        public string NNStruct
        {
            get
            {
                return nnStruct;
            }
            set
            {
                if (!this.nnStruct.Equals(value))           
                {
                    this.nnStruct = value;                                         
                    if (PropertyChanged != null)                                    
                    {
                        //通知绑定此变量的textbox在前台更新
                        PropertyChanged(this, new PropertyChangedEventArgs("NNStruct"));
                    }
                }
            }
        }

        public List<Neuron> InputLayer { get; set; }   //输入层 是一组神经元
        public List<List<Neuron>> HiddenLayers { get; set; } //隐藏层 是好多组神经元
        public List<Neuron> OutputLayer { get; set; }  //输出层是一组神经元    
        
        public double TotalError { get; set; }
        #endregion

        #region -- Globals --
        private static readonly Random Random = new Random();
        #endregion

        #region -- Constructor --
        public Network()
        {
            LearnRate = 0;
            Momentum = 0;
            InputLayer = new List<Neuron>();
            HiddenLayers = new List<List<Neuron>>();
            OutputLayer = new List<Neuron>();
        }

        MainWindow mainWindow;
        //int? 表示可空的整形
        public Network(MainWindow mainWindow,int inputSize, int[] hiddenSizes, int outputSize, double? learnRate = null, double? momentum = null)
        {
            this.mainWindow = mainWindow;
            //初始化学习速率和学习动量
            LearnRate = learnRate ?? 0.4;  //?? 空合并运算符，左边不为null则返回左边，否则返回右边
            Momentum = momentum ?? 0.9;

            string s = "";
            for(int i=0;i<hiddenSizes.Count();i++)
            {
                s += " "+hiddenSizes[i] ;
            }
            NNStruct+="I:"+inputSize.ToString()+"  H:"+"{"+s+" }" + "  O:" + outputSize.ToString();

            //初始化神经元
            InputLayer = new List<Neuron>();
            HiddenLayers = new List<List<Neuron>>();
            OutputLayer = new List<Neuron>();

            for (var i = 0; i < inputSize; i++)
                InputLayer.Add(new Neuron());

            var firstHiddenLayer = new List<Neuron>();
            for (var i = 0; i < hiddenSizes[0]; i++)
                firstHiddenLayer.Add(new Neuron(InputLayer));

            HiddenLayers.Add(firstHiddenLayer);

            for (var i = 1; i < hiddenSizes.Length; i++)
            {
                var hiddenLayer = new List<Neuron>();
                for (var j = 0; j < hiddenSizes[i]; j++)
                    hiddenLayer.Add(new Neuron(HiddenLayers[i - 1]));
                HiddenLayers.Add(hiddenLayer);
            }

            for (var i = 0; i < outputSize; i++)
                OutputLayer.Add(new Neuron(HiddenLayers.Last()));
        }
        #endregion

        #region -- Training --
        public void TrainOneStep(DataSet dataSet)
        {
            ForwardPropagate(dataSet.Values);//前向推演            
            BackPropagate(dataSet.Targets);//误差反向传播    
        }
        //按代数训练
        public void Train(List<DataSet> dataSets, int numEpochs)
        {
            //Parallel.For(0, numEpochs, i =>
            //{
            //    Parallel.ForEach(dataSets, dataSet =>
            //    {
            //        ForwardPropagate(dataSet.Values);//前向推演            
            //        BackPropagate(dataSet.Targets);//误差反向传播  
            //    });
            //});

            for (var i = 0; i < numEpochs; i++)
            {
                foreach (var dataSet in dataSets)
                {
                    ForwardPropagate(dataSet.Values);//前向推演
                    BackPropagate(dataSet.Targets);//误差反向传播
                }
            }
        }
      

        //前向演进
        public void ForwardPropagate(params double[] inputs)
        {
            var i = 0;
            InputLayer.ForEach(a => a.OutputValue = inputs[i++]);
            //HiddenLayers.ForEach(a => a.ForEach(b => b.CalculateValue()));
            //OutputLayer.ForEach(a => a.CalculateValue());

           foreach(List<Neuron> HiddenLayer in HiddenLayers)
            {
                Parallel.ForEach(HiddenLayer, a =>
                {
                    a.CalculateValue();
                });
            }

            Parallel.ForEach(OutputLayer, a =>
            {
                a.CalculateValue();
            });
        }

        //误差反向传播,先不要更新权重，先计算所有的误差传播，然后再更新权重
        public void BackPropagate(params double[] targets)
        {
            var i = 0;
            OutputLayer.ForEach(a => a.CalculateErrorAndGradient(targets[i++])); //计算输出层的误差
            TotalError = OutputLayer.Sum(a => Math.Abs(a.Error));
            HiddenLayers.Reverse(); //将隐藏层反转，从后往前反推
            
            //HiddenLayers.ForEach(a => a.ForEach(b => b.CalculateErrorAndGradient()));
            foreach (List<Neuron> HiddenLayer in HiddenLayers)
            {
                Parallel.ForEach(HiddenLayer, a =>
                {
                    a.CalculateErrorAndGradient();
                });
            }
            //更新连接权重
            OutputLayer.ForEach(a => a.UpdateWeights(LearnRate, Momentum));
            //HiddenLayers.ForEach(a => a.ForEach(b => b.UpdateWeights(LearnRate, Momentum)));
            foreach (List<Neuron> HiddenLayer in HiddenLayers)
            {
                Parallel.ForEach(HiddenLayer, a =>
                {
                    a.UpdateWeights(LearnRate, Momentum);
                });
            }

            HiddenLayers.Reverse();//将隐藏层反转回去    
        }

      
        //计算
        public double[] Compute(params double[] inputs)
        {
            ForwardPropagate(inputs);
            return OutputLayer.Select(a => a.OutputValue).ToArray();           
        }

        private double CalculateTotalError(params double[] targets)
        {
            var i = 0;
            return TotalError=OutputLayer.Sum(a => Math.Abs(a.CalculateError(targets[i++])));           
        }
        #endregion

        #region -- Helpers --
        public static double GetRandom()
        {
            return 2 * Random.NextDouble() - 1;
        }
        #endregion
    }

    #region -- Enum --
    public enum TrainingType
    {
        Epoch,
        MinimumError
    }
    #endregion
}