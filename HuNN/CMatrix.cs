using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuNN
{
    public static class CMatrix
    {
        //矩阵相加
        public static double[,] Add(double[,] A, double[,] B)
        {
            double[,] C = new double[A.GetLength(0), A.GetLength(1)];
            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < A.GetLength(1); j++)
                {
                    C[i, j] = A[i, j] + B[i, j];
                }
            }
            return C;
        }
        //矩阵相减
        public static double[,] Subtract(double[,] A, double[,] B)
        {
            double[,] C = new double[A.GetLength(0), A.GetLength(1)];
            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < A.GetLength(1); j++)
                {
                    C[i, j] = A[i, j] - B[i, j];
                }
            }
            return C;
        }
        //矩阵转置
        public static double[,] Transpose(double[,] A)
        {
            double[,] B = new double[A.GetLength(1), A.GetLength(0)];
            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < A.GetLength(1); j++)
                {
                    B[j, i] = A[i, j];
                }
            }
            return B;
        }

        //矩阵叉乘
        public static double[,] Multiplie(double[,] A, double[,] B)
        {
            int m1 = A.GetLength(0);
            int n1 = A.GetLength(1);
            int m2 = B.GetLength(0);
            int n2 = B.GetLength(1);
            int q, p;
            double[,] C = new double[m1, n2];
            if (n1 == m2)
            {
                for (int k = 0; k <= m1 - 1; k++)
                {
                    for (int j = 0; j <= n2 - 1; j++)
                    {
                        p = 0; q = 0;
                        while (p <= n1 - 1)
                        {
                            C[k, j] += A[k, p] * B[q, j];
                            p++;
                            q++;
                        }
                    }
                }
            }
            //else
            //{
            //    C = null;
            //}

            return C;
        }


        public static double[] Multiplie(double[,] A, double[] BCol)
        {
            int rowA = A.GetLength(0);//行数
            int colA = A.GetLength(1);//列数
            int countB = BCol.Count();

            double[] C = new double[rowA];
            if (colA == countB)
            {
                for (int k = 0; k < rowA; k++)
                {
                    for (int j = 0; j < colA; j++)
                    {
                        C[k] += A[k, j] * BCol[j];

                    }
                }
            }         
            return C;
        }

        public static double[] Multiplie(double[] ARow, double[] BCol)
        {
            double[] C = new double[ARow.Count()];
            if (ARow.Count() == BCol.Count())
            {
                for (int k = 0; k < ARow.Count(); k++)
                {
                    C[k] += ARow[k] * BCol[k];
                }
            }        
            return C;
        }

        //矩阵乘法，一行乘以一列
        public static double[,] MultiplieSpecial(double[] ACol, double[] BRow)
        {
            double[,] C = new double[ACol.Count(),BRow.Count()];
            for (int j = 0; j < ACol.Count(); j++)
            {
                for (int k = 0; k < BRow.Count(); k++)
                {
                    C[j,k] = ACol[j] * BRow[k];
                }
            }
            return C;
        }


        public static double[,] NumMul(double a, double[,] A)
        {
            int row = A.GetLength(0);
            int column = A.GetLength(1);
            double[,] B = new double[row, column];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    B[i, j] = a * A[i, j];
                }
            }
            return B;
        }//数乘

        public static double[] NumMul(double a, double[] A)
        {
            double[] B = new double[A.Length];
            for (int i = 0; i < A.Length; i++)
            {
                B[i] = a * A[i];               
            }
            return B;
        }//数乘


      
    


        /******************矩阵求逆*******************/
        public static double[,] Inverse(double[,] G)
        {
            int row = G.GetLength(0);
            double[,] invG = new double[row, row];
            double detG = Determinant(G);
            if (detG != 0)
            {
                invG = NumMul(1 / detG, Adjoint(G));
            }
            else
            {
                return invG;
            }

            return invG;
        }
        public static double Determinant(double[,] MatrixList)
        {
            int Level = MatrixList.GetLength(0);
            double[,] dMatrix = new double[Level, Level];
            for (int i = 0; i < Level; i++)
                for (int j = 0; j < Level; j++)
                    dMatrix[i, j] = MatrixList[i, j];
            double c, x;
            int k = 1;
            for (int i = 0, j = 0; i < Level && j < Level; i++, j++)
            {
                if (dMatrix[i, j] == 0)
                {
                    int m = i + 1;
                    //for (; dMatrix[m, j] == 0; m++) ;
                    if (m == Level)
                        return 0;
                    else
                    {
                        // Row change between i-row and m-row
                        for (int n = j; n < Level; n++)
                        {
                            c = dMatrix[i, n];
                            dMatrix[i, n] = dMatrix[m, n];
                            dMatrix[m, n] = c;
                        }
                        // Change value pre-value
                        k *= (-1);
                    }
                }
                // Set 0 to the current column in the rows after current row
                for (int s = Level - 1; s > i; s--)
                {
                    x = dMatrix[s, j];
                    for (int t = j; t < Level; t++)
                        dMatrix[s, t] -= dMatrix[i, t] * (x / dMatrix[i, j]);
                }
            }
            double sn = 1;
            for (int i = 0; i < Level; i++)
            {
                if (dMatrix[i, i] != 0)
                    sn *= dMatrix[i, i];
                else
                    return 0;
            }
            return k * sn;
        }
        public static double[,] Cholesky(double[,] A)
        {
            int n = A.GetLength(0);
            double[,] L = new double[n, n];
            L[0, 0] = Math.Sqrt(A[0, 0]);
            for (int i = 1; i < n; i++)
            {
                L[i, 0] = A[i, 0] / L[0, 0];
            }

            double sum;
            for (int j = 0; j < n; j++)
            {
                sum = 0;
                for (int k = 0; k < j; k++)
                {
                    sum = sum + L[j, k] * L[j, k];
                }
                if (A[j, j] - sum >= 0)
                {
                    L[j, j] = Math.Sqrt(A[j, j] - sum);
                }
                else
                {
                    L = new double[n, n];

                    for (int i = 0; i < n; i++)
                    {
                        if (L[i, i] >= 0)
                        {
                            L[i, i] = Math.Sqrt(A[i, i]);
                        }
                    }
                    break;
                }

                for (int i = j + 1; i < n; i++)
                {
                    sum = 0;
                    for (int k = 0; k <= j - 1; k++)
                    { sum = sum + L[i, k] * L[j, k]; }
                    L[i, j] = (A[i, j] - sum) / L[j, j];
                }
            }
            for (int i = 0; i < n; i++)
            {
                if (Double.IsNaN(L[i, i]))
                {
                    L[i, i] = 0;
                }
            }

            return L;
        }//cholesky分解
 
        /******************矩阵求逆*******************/

        //行向量转为列向量
        public static double[,] RtoC(double[] A)
        {
            double[,] B = new double[A.GetLength(0), 1];
            for (int i = 0; i < A.GetLength(0); i++)
            {
                B[i, 0] = A[i];
            }
            return B;
        }
        //列向量转为行向量
        public static double[] CtoR(double[,] A)
        {
            double[] B = new double[A.GetLength(0)];
            for (int i = 0; i < A.GetLength(0); i++)
            {
                B[i] = A[i, 0];
            }
            return B;
        }
        //矩阵赋值,把B赋给A
        public static double[,] Equal(double[,] A, double[,] B)
        {

            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < A.GetLength(1); j++)
                {
                    A[i, j] = B[i, j];
                }
            }
            return A;
        }

        //求单位矩阵
        public static double[,] Unit(int n)
        {

            double[,] A = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                A[i, i] = 1;
            }
            return A;
        }
        //求迹
        public static double Trace(double[,] A)
        {
            int n = A.GetLength(0);
            double a = 0;
            for (int i = 0; i < n; i++)
            {
                a = a + A[i, i];
            }
            return a;

        }

        //求伴随矩阵,只用于方阵
        public static double[,] Adjoint(double[,] G)
        {
            int row = G.GetLength(0);
            double[,] AdjointG = new double[row, row];

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < row; j++)
                {

                    AdjointG[i, j] = Algebraic(j, i, G);

                }
            }
            return AdjointG;
        }
        private static double Algebraic(int i, int j, double[,] G)//求某个元素的代数余子式
        {
            int row = G.GetLength(0);
            double[,] Gij = new double[row - 1, row - 1];
            int p = 0;
            int q = 0;
            for (int m = 0; m < row; m++)
            {

                if (m != i)
                {
                    for (int n = 0; n < row; n++)
                    {
                        if (n != j)
                        {

                            Gij[p, q] = G[m, n];
                            q = q + 1;
                        }
                        else
                            continue;

                    }
                    q = 0;
                    p = p + 1;
                }
                else
                    continue;

            }

            double gij = Math.Pow(-1, i + j) * Determinant(Gij);
            return gij;
        }

    }
}
