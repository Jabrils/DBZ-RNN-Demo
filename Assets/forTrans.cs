using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using MLLib;

public class forTrans : MonoBehaviour {

    public float stepSize = 1;
    public int inputsSize;
    public int hiddenSize;

    Matrix brain = new Matrix();
    Matrix inputs = new Matrix();
    NeuralNet NN = new NeuralNet();
    Text[] displayMat;
    char[] theData;
    public int dataLoc, epoch;
    Matrix outp, guess, exp, error, derivs;
    bool automate;
    string dbName;

    // Use this for initialization
    void Start()
    {
        displayMat = new Text[] {
        // This finds the texts for matrix visualization
        GameObject.Find("Text").GetComponent<Text>(), // 0
        GameObject.Find("Text1").GetComponent<Text>(),
        GameObject.Find("Text2").GetComponent<Text>(),
        GameObject.Find("Text3").GetComponent<Text>(),
        GameObject.Find("Text4").GetComponent<Text>(), // 4
        GameObject.Find("Text5").GetComponent<Text>(),
        GameObject.Find("Text_1").GetComponent<Text>(),
        GameObject.Find("Text_2").GetComponent<Text>(),
        GameObject.Find("Text_3").GetComponent<Text>(), // 8
    };

        //displayMat[0].gameObject.SetActive(false);
        //displayMat[6].gameObject.SetActive(false);

        // 
        StreamReader sR = new StreamReader("DB names.txt");
        theData = sR.ReadToEnd().ToCharArray();
        sR.Close();

        // This section is for the brain matrix
        brain.Initialize(new float[hiddenSize, inputsSize + 1], true);

        CalculateWholeNetwork();

        UpdateUI();

    }

    private void CalculateWholeNetwork()
    {
        // This section is for the inputs matrix
        inputs.Initialize(CharToMatrix(theData[dataLoc]));

        // This section is for the output matrix
        outp = new Matrix();
        Matrix outpT = new Matrix();
        outpT.SetDimensions(inputsSize, 1);
        outpT.MatrixMultiplication(brain, inputs);

        // This section is for the expected matrix
        float total = 0;

        // 
        for (int i = 0; i < outpT.matRows; i++)
        {
            total += outpT.matrix[i, 0];
        }

        // 
        outp = outpT;
        for (int i = 0; i < outpT.matrix.Length; i++)
        {
            outp.matrix[i, 0] = (outpT.matrix[i, 0] / total);
        }

        float check = 0;
        int win = 0;

        // 
        for (int o = 0; o < outp.matrix.GetLength(0); o++)
        {
            if (outp.matrix[o, 0] > check)
            {
                check = outp.matrix[o, 0];
                win = o;
            }
        }

        // 
        guess = new Matrix();
        guess.SetDimensions(outp.matRows, outp.matCols);
        for (int i = 0; i < guess.matrix.GetLength(0); i++)
        {
            guess.matrix[i, 0] = (i == win) ? 1 : 0;
        }

        // 
        exp = new Matrix();
        int where = dataLoc < theData.Length - 2 ? dataLoc + 1 : 0;
        exp.Initialize(CharToMatrix(theData[where]));

        error = new Matrix();
        error.SetDimensions(outp.matRows, outp.matCols);

        // 
        for (int j = 0; j < error.matRows; j++)
        {
            for (int i = 0; i < error.matCols; i++)
            {
                error.matrix[j, i] = outp.matrix[j, i] - exp.matrix[j, i];
            }
        }

        //
        // //
        //

        derivs = new Matrix();
        float[,] transfer = new float[brain.matRows, brain.matCols];

        // 
        for (int i = 0; i < brain.matRows; i++)
        {
            if (error.matrix[i, 0] != 0)
            {
                for (int j = 0; j < brain.matCols - 1; j++)
                {
                    transfer[i, j] = (outp.matrix[i, 0]) * (inputs.matrix[j, 0]) * ((error.matrix[i, 0]) / Mathf.Abs(error.matrix[i, 0]));
                    //transfer[i, j] = inputs.matrix[j, 0] * (outp.matrix[j, 0] * error.matrix[i, 0]);
                    transfer[i, brain.matCols - 1] = stepSize * error.matrix[i, 0];
                }
            }
        }

        derivs.Initialize(transfer);
    }

    void UpdateUI()
    {
        // This sets the text to the brain's text
        displayMat[0].text = "\n" + brain.DisplayAsText(true);

        //
        // //
        //

        // This sets the text to the input text
        displayMat[1].text = MatrixToString(inputs) + "\n" + inputs.DisplayAsText(true);

        //
        // //
        //

        // This sets the text to the output text
        displayMat[2].text = "\n" + outp.DisplayAsText(true);

        //
        // //
        //

        // This sets the text to the guess text
        displayMat[3].text = MatrixToString(guess) + "\n" + guess.DisplayAsText(true);

        // This sets the text to the expected text
        displayMat[4].text = MatrixToString(exp) + "\n" + exp.DisplayAsText(true);

        // This sets the text to the error text
        displayMat[5].text = "\n" + error.DisplayAsText(true);

        //
        displayMat[6].text = "\n" + derivs.DisplayAsText(true);

        //
        displayMat[7].text = "Epoch: " + epoch;

        // 
        displayMat[8].text = "Generated DB Name: " + dbName;
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            Application.LoadLevel(Application.loadedLevel);
        }

        // 
        if (Input.GetKeyDown(KeyCode.B))
        {
            // 
            if (Input.GetKey(KeyCode.LeftShift))
            {
                automate = automate ? false : true;
            }
            else
            {
                BackProp();
            }
        }

        if(!automate && Input.GetKeyDown(KeyCode.G))
        {
            dbName = GenerateDBName();
            UpdateUI();
        }

        // 
        if (automate)
        {
            BackProp();
        }

        // 
        if (Input.GetKeyDown(KeyCode.Space))
        {
            brain.SaveMatrix("testing");
            UpdateUI();
        }

        // 
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            brain.LoadMatrix("testing");
            outp.MatrixMultiplication(brain, inputs);
            UpdateUI();
        }

    }

    string GenerateDBName()
    {
        int which = Random.RandomRange(0, inputs.matRows - 1);
        inputs.SetDimensions(inputs.matRows, 1);
        inputs.matrix[which, 0] = 1;

        string nameRet = "";

        for (int i = 0; i < 100; i++)
        {
            if (MatrixToString(inputs) != ",")
            {
                nameRet += MatrixToString(inputs);
                inputs = guess;
            }
            else
            {
                break;
            }
        }

        return nameRet;
    }

    private void BackProp()
    {
        if (inputs.matrix != new float[29, 1])
        {
            for (int i = 0; i < error.matRows; i++)
            {
                if (error.matrix[i, 0] != 0)
                {
                    for (int j = 0; j < brain.matCols-1; j++)
                    {
                        brain.matrix[i, j] -= derivs.matrix[i, j];
                        brain.matrix[i, brain.matCols - 1] -= derivs.matrix[i, j];

                        /*
                        if (brain.matrix[i, j] < 0)
                        {
                            brain.matrix[i, j] = 0;
                        }

                        if (brain.matrix[i, brain.matCols - 1] < 0)
                        {
                            brain.matrix[i, brain.matCols - 1] = 0;
                        }
                        */

                        //brain.matrix[i, j] -= (brain.matrix[i, j] > 0) ? derivs.matrix[i, j] * stepSize : 0;
                        //brain.matrix[i, brain.matCols - 1] -= (brain.matrix[i, brain.matCols - 1] > 0) ? derivs.matrix[i, j] * stepSize : 0;
                    }
                }
            }
        }
        CalculateWholeNetwork();

        // 
        if (dataLoc < theData.Length-1)
        {
            dataLoc++;
        }
        else
        {
            epoch++;
            dataLoc = 0;
        }
        UpdateUI();
    }

    string MatrixToString(Matrix mat)
    {
        StreamReader sW = new StreamReader("world.txt");
        string[] world = sW.ReadToEnd().Split('\n');
        sW.Close();

        // 
        for (int i = 0; i < world.Length; i++)
        {
            if (mat.matrix[i, 0] == 1)
            {
                return world[i];
            }
        }

        return "";
    }

    // 
    float[,] CharToMatrix(char ch)
    {
        StreamReader sW = new StreamReader("world.txt");
        string[] world = sW.ReadToEnd().Split('\n');
        sW.Close();

        float[,] retMat = new float[29, 1];

        // 
        for (int i = 0; i < world.Length; i++)
        {
            if (world[i].Trim() == ch.ToString())
            {
                retMat[i, 0] = 1;
            }
        }


        return retMat;
    }
}
