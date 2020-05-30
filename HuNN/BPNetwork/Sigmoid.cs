using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuNN
{
    public static class Sigmoid
    {
        //如果x大于等于45.0则返回1.0，小于-45则返回0，在中间的话返回1/1+e-x
        public static double Output(double x)
        {
            return x < -45.0 ? 0.0 : x > 45.0 ? 1.0 : 1.0 / (1.0 + Math.Exp(-x));
        }


        //导数
        public static double Derivative(double x)
        {
            // return Output(x) * (1 - Output(x)); //这个输入的x不是 x，是经过sigmoid运算之后的值,其实是sigmoid(x)   
            return x * (1 - x);
        }
    }
}
