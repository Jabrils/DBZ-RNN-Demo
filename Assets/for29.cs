using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using MLLib;
using BriljaSanLib;

public class for29 : MonoBehaviour {
    public bool clean;
    public float stepSize = .1f;
    public int dataLoc, epoch;
    public int inputSize=3;
    public int nodeSize=3;

    public char[] theData;

    bool automate;
    Text[] dTxt;
    Text GenTxt;
    Matrix input = new Matrix(), brain = new Matrix(), output = new Matrix(), expect = new Matrix(), error = new Matrix(), guess = new Matrix(), derivative = new Matrix(), pInput = new Matrix(), pInput2 = new Matrix();

	// Use this for initialization
	void Start ()
    {
        dTxt = new Text[]
        {
            Get.QuickComponent<Text>("Text1"),
            Get.QuickComponent<Text>("Text"),
            Get.QuickComponent<Text>("Text2"),
            Get.QuickComponent<Text>("Text4"),
            Get.QuickComponent<Text>("Text5"),
            Get.QuickComponent<Text>("Text3"),
            Get.QuickComponent<Text>("Text_1"),
            Get.QuickComponent<Text>("let1"),
            Get.QuickComponent<Text>("let2"),
            Get.QuickComponent<Text>("let3"),
            Get.QuickComponent<Text>("Text6"),
            Get.QuickComponent<Text>("let4"),
            Get.QuickComponent<Text>("let5"),
        };

        // 
        GenTxt = Get.QuickComponent<Text>("Text_3");
        GenTxt.text = "";

        // 
        StreamReader sR = new StreamReader("DB names.txt");
        theData = sR.ReadToEnd().ToCharArray();
        sR.Close();

        // 
        if (clean)
        {
            dTxt[1].gameObject.SetActive(false);
            dTxt[6].gameObject.SetActive(false);
        }

        // Create a new float matrix to initialize the input 
        float[,] fl = new float[inputSize, 1];

        // find a random alphabet to start at
        int start = Random.RandomRange(0, inputSize);

        // set that random alphabet to true
        fl[start, 0] = 1;

        // create a new float matrix to initialize the brain
        float[,] fl2 = new float[nodeSize, inputSize+1];

        // for loop through all to set it to 0 or 1
        for (int i = 0; i < nodeSize; i++)
        {
            for (int j = 0; j < inputSize + 1; j++)
            {
                fl2[i, j] = Random.RandomRange(0, 1f);
            }
        }

        // initialize the brain matrix
        brain.Initialize(fl2);

        Calculate(true);
    }

    private void Calculate(bool wI)
    {
        if (wI)
        {
            // initialize the input matrix
            int dl = dataLoc > 0 ? dataLoc - 1 : 6;
            input.Initialize(CharToMatrix(theData[dataLoc], theData[dl]));

            // 
            if (dataLoc < theData.Length - 1)
            {
                dataLoc++;
            }
            else
            {
                epoch++;
                dataLoc = 0;
            }
        }

        // the output matrix is simply multiplication between the 2
        output.MatrixMultiplication(brain, input);

        // 
        int dl2 = dataLoc > 0 ? dataLoc - 1 : 6;
        int where2 = dataLoc < theData.Length - 2 ? dataLoc : 0;
        expect.Initialize(CharToMatrix(theData[where2], theData[dl2]));

        // 
        error.MatrixSubtraction(output, expect);

        // 
        guess.OneHotHighest(output);

        // 
        derivative.OneLayerDerivative(stepSize, brain, input, error);

        UpdateUI();
    }

    void UpdateUI()
    {
        // 
        dTxt[0].text = MatrixToString(input) + "\n" + input.DisplayAsText(true);
        dTxt[1].text = "brain\n" + brain.DisplayAsText(true);
        dTxt[2].text = "out\n" + output.DisplayAsText(true);
        dTxt[3].text = MatrixToString(expect) + "\n" + expect.DisplayAsText(true);
        dTxt[4].text = "er\n" + error.DisplayAsText(true);
        dTxt[5].text = MatrixToString(guess) + "\n" + guess.DisplayAsText(true);
        dTxt[6].text = "derivs\n" + derivative.DisplayAsText(true);
        float all = 0;
        for (int i = 0; i < output.matRows; i++)
        {
            all += output.matrix[i, 0]/output.matRows;
        }
        dTxt[10].text = "Error: " + all;

        dTxt[7].text = MatrixToString(input);
        dTxt[8].text = MatrixToString(guess);
        dTxt[9].text = MatrixToString(expect);
        dTxt[11].text = MatrixToString(input,1);
        //dTxt[12].text = MatrixToString(pInput2);
    }

    // Update is called once per frame
    void Update()
    {
        // 
        if (Input.GetKeyDown(KeyCode.N))
        {
            Application.LoadLevel(Application.loadedLevel);
        }

        // 
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                automate = automate ? false : true;
            }
            else
            {
                BackProp();
            }
        }

        // 
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenTxt.text += MatrixToString(input);
            float[,] newI = new float[inputSize, 1];

            for (int i = 0; i < inputSize / 2; i++)
            {
                newI[i + 29, 0] = input.matrix[i, 0];
            }

            for (int i = 0; i < inputSize / 2; i++)
            {
                newI[i, 0] = guess.matrix[i, 0];
            }

            input.Initialize(newI);

            Calculate(false);
        }

        // 
        if (Input.GetKeyDown(KeyCode.C))
        {
            GenTxt.text = "";
        }

        // 
        if(Input.GetKeyDown(KeyCode.Q))
        {
            brain.SaveMatrix("DB WORK!");
        }

        // 
        if (Input.GetKeyDown(KeyCode.W))
        {
            brain.LoadMatrix("DB WORK!");
        }

        // 
        if (automate)
        {
            BackProp();
        }
    }

    void BackProp()
    {
        for (int i = 0; i < brain.matRows; i++)
        {
            for (int j = 0; j < brain.matCols; j++)
            {
                brain.matrix[i, j] -= derivative.matrix[i, j];
            }
        }
        Calculate(true);
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

    string MatrixToString(Matrix mat, int loc)
    {
        StreamReader sW = new StreamReader("world.txt");
        string[] world = sW.ReadToEnd().Split('\n');
        sW.Close();

        // 
        for (int i = 0; i < world.Length; i++)
        {
            if (mat.matrix[i + (29*loc), 0] == 1)
            {
                return world[i];
            }
        }

        return "";
    }

    // 
    float[,] CharToMatrix(char ch1, char ch2)
    {
        StreamReader sW = new StreamReader("world.txt");
        string[] world = sW.ReadToEnd().Split('\n');
        sW.Close();

        float[,] retMat = new float[inputSize, 1];

        // 
        for (int i = 0; i < world.Length; i++)
        {
            if (world[i].Trim() == ch1.ToString())
            {
                retMat[i, 0] = 1;
            }
        }

        // 
        for (int i = 0; i < world.Length; i++)
        {
            if (world[i].Trim() == ch2.ToString())
            {
                retMat[i+29, 0] = 1;
            }
        }


        return retMat;
    }
}
