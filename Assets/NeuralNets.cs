using System;
using System.IO;

namespace MLLib
{
    public class NeuralNet
    {
    }

    public class Matrix
    {
        int _matRows;
        int _matCols;
        float[,] _matrix;

        public int matRows { get { return _matRows; } }
        public int matCols { get { return _matCols; } }
        public float[,] matrix { get { return _matrix; } }

        // This sets the dimensions to the matrix
        public void SetDimensions(int x, int y)
        {
            _matRows = x;
            _matCols = y;
            _matrix = new float[_matRows, _matCols];
        }

        // 
        public void Initialize(float[,] theMatrix)
        {
            _matRows = theMatrix.GetLength(0);
            _matCols = theMatrix.GetLength(1);

            _matrix = theMatrix;
        }

        // 
        public void Initialize(float[,] theMatrix, bool identityMat)
        {
            _matRows = theMatrix.GetLength(0);
            _matCols = theMatrix.GetLength(1)+1;

            for (int j = 0; j < _matRows; j++)
            {
                for (int i = 0; i < _matCols; i++)
                {
                    theMatrix[j, j] = 1;
                }
            }

            _matrix = theMatrix;
        }

        // 
        public void Initialize(float[,] theMatrix, int min, int max)
        {
            _matRows = theMatrix.GetLength(0);
            _matCols = theMatrix.GetLength(1)+1;

            Random r = new Random();
            for (int j = 0; j < _matRows; j++)
            {
                for (int i = 0; i < _matCols; i++)
                {
                    theMatrix[j, i] = r.Next(min, max);
                }
            }

            _matrix = theMatrix;
        }

        // 
        public void MatrixMultiplication(Matrix brain, Matrix input)
        {
            float[,] newMat = new float[brain.matrix.GetLength(0), 1];

            // 
            for (int j = 0; j < newMat.GetLength(0); j++)
            {
                // 
                for (int i = 0; i < brain.matrix.GetLength(1) - 1; i++)
                {
                    newMat[j, 0] += NodeMath(brain.matrix, input.matrix, i, j);
                }

                // 
                newMat[j, 0] += brain.matrix[j, brain.matrix.GetLength(1) - 1];
            }

            _matRows = newMat.GetLength(0);
            _matCols = newMat.GetLength(1);
            _matrix = newMat;
        }

        // 
        public void MatrixAddition(Matrix m1, Matrix m2)
        {
            float[,] newMat = new float[m1.matrix.GetLength(0), 1];

            _matRows = newMat.GetLength(0);
            _matCols = newMat.GetLength(1);

            for (int i = 0; i < m1.matRows; i++)
            {
                newMat[i, 0] = m1.matrix[i, 0] + m2.matrix[i, 0];
            }

            _matrix = newMat;
        }

        // 
        public void MatrixSubtraction(Matrix m1, Matrix m2)
        {
            float[,] newMat = new float[m1.matrix.GetLength(0), 1];

            _matRows = newMat.GetLength(0);
            _matCols = newMat.GetLength(1);

            for (int i = 0; i < m1.matRows; i++)
            {
                newMat[i, 0] = m1.matrix[i, 0] - m2.matrix[i, 0];
            }

            _matrix = newMat;
        }

        /// <summary>
        /// Turns an Nx1 matrix into a one hot encoded matrix based on highest value.
        /// </summary>
        public void OneHotHighest(Matrix which)
        {
            float check = 0;
            int win = 0;

            // 
            for (int o = 0; o < which.matrix.GetLength(0); o++)
            {
                if (which.matrix[o, 0] > check)
                {
                    check = which.matrix[o, 0];
                    win = o;
                }
            }

            // 
            SetDimensions(which.matRows, which.matCols);
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                _matrix[i, 0] = (i == win) ? 1 : 0;
            }
        }

        public void OneLayerDerivative(float stepSize, Matrix brain, Matrix input, Matrix error)
        {
            // 
            float[,] d = new float[brain.matRows, brain.matCols];

            // 
            for (int i = 0; i < brain.matRows; i++)
            {
                for (int j = 0; j < brain.matCols - 1; j++)
                {
                    d[i, j] = GetDerivDirection(error.matrix[i, 0]) * stepSize * input.matrix[j, 0];
                }
                d[i, brain.matCols - 1] = GetDerivDirection(error.matrix[i, 0]) * stepSize;
            }

            Initialize(d);
        }

        // 
        float GetDerivDirection(float val)
        {
            return val / Math.Abs(val);
        }

        // 
        private static float NodeMath(float[,] brain, float[,] input, int i, int j)
        {
            return (brain[j, i] * input[i, 0]);
        }

        // This gives you the matrix as a string for visualization
        public string DisplayAsText(bool brackets)
        {
            // This sets the rows for the display text
            string[] disMatRows = new string[_matRows];

            // 
            for (int i = 0; i < disMatRows.Length; i++)
            {
                disMatRows[i] = brackets ? "[" : "";

                // 
                for (int j = 0; j < _matCols; j++)
                {
                    string ender = (j != _matCols - 1) ? "," : brackets ? "]" : "";
                    disMatRows[i] += _matrix[i, j] + ender;
                }
            }

            string theRet = "";

            for (int i = 0; i < disMatRows.Length; i++)
            {
                string ender = i == disMatRows.Length - 1 ? "" : "\r\n";
                theRet += disMatRows[i] + ender;
            }

            return theRet;
        }

        // This saves the matrix as a data file
        public void SaveMatrix(string fileName)
        {
            StreamWriter sW = new StreamWriter(fileName + ".txt");
            sW.Write(DisplayAsText(false));
            sW.Close();
        }

        // This loads the matrix from a data file
        public void LoadMatrix(string fileName)
        {
            string fullM = "";

            StreamReader sR = new StreamReader(fileName + ".txt");
            fullM = sR.ReadToEnd();
            sR.Close();

            string[] row = fullM.Split('\n');
            string[][] matR = new string[row.Length][];

            for (int i = 0; i < matR.Length; i++)
            {
                matR[i] = row[i].Split(',');
            }

            float[,] mat = new float[row.Length, matR[0].Length];

            // 
            for (int i = 0; i < matR.Length; i++)
            {
                for (int j = 0; j < matR[0].Length; j++)
                {
                    mat[i, j] = float.Parse(matR[i][j]);
                }
            }

            _matrix = mat;
        }
    }
}
