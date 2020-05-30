using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuNN
{
   
    public class Synapse
    {
        #region -- Properties --
        public Guid Id { get; set; }   //ID
        public Neuron InputNeuron { get; set; } //输入神经元
        public Neuron OutputNeuron { get; set; } //输出神经元
        public double Weight { get; set; }  //权重
        public double WeightDelta { get; set; }  //权重增量
        #endregion

        #region -- Constructor --
        public Synapse() { }

        public Synapse(Neuron inputNeuron, Neuron outputNeuron)
        {
            Id = Guid.NewGuid();
            InputNeuron = inputNeuron;
            OutputNeuron = outputNeuron;
            Weight = Network.GetRandom();//生成大于等于0.0 小于1.0的随机数
        }
        #endregion
    }
}
