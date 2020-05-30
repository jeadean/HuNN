using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuNN
{
   public class Neuron
    {
        #region -- Properties --
        public Guid Id { get; set; }
        public List<Synapse> InputSynapses { get; set; }
        public List<Synapse> OutputSynapses { get; set; }

        public double Bias { get; set; } //偏置  大于这个偏置则激活，小于这个偏置则不激活
        public double BiasDelta { get; set; } //delta偏置
        public double Gradient { get; set; } //梯度
        public double InputValue { get; set; }
        public double OutputValue { get; set; } //值
        public double Error { get; set; }
        #endregion

        #region -- Constructors --
        public Neuron()
        {
            Id = Guid.NewGuid();
            InputSynapses = new List<Synapse>();
            OutputSynapses = new List<Synapse>();
            Bias = Network.GetRandom(); //
            
        }

        //:this(),多态，继承自构造函数，先执行neuron（）的构造函数
        public Neuron(IEnumerable<Neuron> inputNeurons) : this()
        {
            foreach (var inputNeuron in inputNeurons)
            {
                var synapse = new Synapse(inputNeuron, this);
                inputNeuron.OutputSynapses.Add(synapse);
                this.InputSynapses.Add(synapse);
            }
        }
        #endregion

        //计算输出值 ，输入值加上偏置，然后应用sigmoid函数
        public virtual double CalculateValue()
        {
            this.InputValue = InputSynapses.Sum(a => a.Weight * a.InputNeuron.OutputValue);
            return OutputValue = Sigmoid.Output(InputValue+ Bias);
        }

        //有target代表是输出层,没有target代表是隐藏层
        public void CalculateErrorAndGradient(double? target=null)
        {
             CalculateError(target);
             CalculateGradient(target);
        }      

        //误差值,如果有target代表是输出层
        //误差值，如果没有target，是隐藏层，隐藏层的误差是前一层传播过来的，结果是对连接到改节点的所有突触的权重*所连接节点的误差求和。
        public double CalculateError(double? target=null)
        {    
            if(target==null)
                return Error = OutputSynapses.Sum(a => a.Weight * a.OutputNeuron.Error);

            return Error=target.Value - OutputValue;
        }
     
        //计算梯度，target是目标值，如果有目标值，则是输出节点
        public double CalculateGradient(double? target = null)
        {
            //如果没有目标值，说明是中间层
            if (target == null)
                //return Gradient = OutputSynapses.Sum(a => a.OutputNeuron.Gradient * a.Weight) * Sigmoid.Derivative(OutputValue);
                return Gradient =Error * Sigmoid.Derivative(OutputValue);

            //如果有目标值，说明是输出层
             return Gradient = Error * Sigmoid.Derivative(OutputValue);
           // return Gradient = Sigmoid.Derivative(OutputValue);
        }

        //https://www.cnblogs.com/jiaxblog/p/9695042.html

        //更新权重,learnrate是学习因子
        public void UpdateWeights(double learnRate, double momentum)
        {
            //更新自己的偏置
            var prevDelta = BiasDelta; //上一次的偏置delta
            BiasDelta = learnRate * Gradient; //学习速率*梯度    梯度越大，偏置越大
            Bias += BiasDelta + momentum * prevDelta; //偏置=偏置delta+上一次偏置*momentum

            //对于每一个输入的突触，要更新它们的权重
            Parallel.ForEach(InputSynapses, synapse =>
            {
                prevDelta = synapse.WeightDelta;
                synapse.WeightDelta = learnRate* Gradient * synapse.InputNeuron.OutputValue; 
                synapse.Weight += synapse.WeightDelta + momentum * prevDelta;
            });

            //foreach (var synapse in InputSynapses)
            //{
            //    prevDelta = synapse.WeightDelta;
            //    synapse.WeightDelta = learnRate * Gradient * synapse.InputNeuron.OutputValue;
            //    synapse.Weight += synapse.WeightDelta + momentum * prevDelta;
            //}
        }


        /*  momentum 动量能够在一定程度上解决这个问题。momentum 动量是依据物理学的势能与动能之间能量转换原理提出来的。
        当 momentum 动量越大时，其转换为势能的能量也就越大，就越有可能摆脱局部凹域的束缚，进入全局凹域。momentum 动量主
        要用在权重更新的时候。
                一般，神经网络在更新权值时，采用如下公式:
                                                               w = w - learning_rate * dw

                引入momentum后，采用如下公式：
                                                               v = mu * v - learning_rate * dw
                                                               w = w + v
                其中，v初始化为0，mu是设定的一个超变量，最常见的设定值是0.9。可以这样理解上式：如果上次的momentum()与这次的
        负梯度方向是相同的，那这次下降的幅度就会加大，从而加速收敛。
        */
    }
}
