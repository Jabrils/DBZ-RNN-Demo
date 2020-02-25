using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BriljaSanLib;
using UnityEngine.UI;
using MLLib;

public class newboy : MonoBehaviour {
    public float stepSize = .01f;
    public bool automate;
    public int recursion;

    List<float[,]> proc = new List<float[,]>();

    Image[] img;
    Text[] txt;
    Matrix input = new Matrix(), brain = new Matrix(), output = new Matrix(), guess = new Matrix(), expect = new Matrix(), error = new Matrix(), deriv = new Matrix();

	// Use this for initialization
	void Start () {
        txt = new Text[]
        {
            Get.QuickComponent<Text>("Text1"), // 0
            Get.QuickComponent<Text>("Text"),
            Get.QuickComponent<Text>("Text2"),
            Get.QuickComponent<Text>("Text3"),
            Get.QuickComponent<Text>("Text4"), // 4
            Get.QuickComponent<Text>("Text5"),
            Get.QuickComponent<Text>("Text_1"),
            Get.QuickComponent<Text>("Text_3"),
        };

        img = new Image[]
        {
            Get.QuickComponent<Image>("Image"), // 0
            Get.QuickComponent<Image>("Image1"),
            Get.QuickComponent<Image>("Image2"),

            Get.QuickComponent<Image>("Image3"),
            Get.QuickComponent<Image>("Image4"), // 4
            Get.QuickComponent<Image>("Image5"),
            Get.QuickComponent<Image>("Image6"),
            Get.QuickComponent<Image>("Image7"),
            Get.QuickComponent<Image>("Image8"), // 8
            Get.QuickComponent<Image>("Image9"),
            Get.QuickComponent<Image>("Image10"),
            Get.QuickComponent<Image>("Image11"),
            Get.QuickComponent<Image>("Image12"), // 12
            Get.QuickComponent<Image>("Image13"),
            Get.QuickComponent<Image>("Image14"),
            Get.QuickComponent<Image>("Image15"),
            Get.QuickComponent<Image>("Image16"), // 16
            Get.QuickComponent<Image>("Image17"),
            Get.QuickComponent<Image>("Image18"),
            Get.QuickComponent<Image>("Image19"),
            Get.QuickComponent<Image>("Image20"), // 20

        };

        Ini();
        Calculate();
	}
	
	// Update is called once per frame
	void Update () {
        UpdateUI();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

            if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Application.LoadLevel(Application.loadedLevel);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                automate = (automate) ? false : true;
            }
            else
            {
                backProp();
            }
        }

        if(automate)
        {
            backProp();
        }
    }

    void backProp()
    {
        float[,] m = brain.matrix;
        for (int i = 0; i < brain.matRows; i++)
        {
            for (int j = 0; j < brain.matCols; j++)
            {
                m[i, j] -= deriv.matrix[i, j];
            }
        }

        brain.Initialize(m);
        input.Initialize(guess.matrix);
        proc.Add(input.matrix);
        Calculate();
        recursion++;
    }

    void Ini()
    {
        float[,] inp = new float[5, 1];
        int ran = Random.RandomRange(0, inp.Length);
        inp[ran, 0] = 1;
        input.Initialize(inp);
        proc.Add(input.matrix);

        float[,] br = new float[5, 6];

        for (int j = 0; j < br.GetLength(0); j++)
        {
            for (int i = 0; i < br.GetLength(1); i++)
            {
                br[j, i] = Random.RandomRange(0f, 1f);
            }
        }

        brain.Initialize(br);
    }

    void Calculate()
    { 
        output.MatrixMultiplication(brain, input);

        int max = 0;
        float track = 0;
        for (int i = 0; i < output.matrix.GetLength(0); i++)
        {
            if (output.matrix[i, 0] > track)
            {
                track = output.matrix[i, 0];
                max = i;
            }
        }

        float[,] gu = new float[input.matRows, input.matCols];
        gu[max, 0] = 1;

        guess.Initialize(gu);

        expect.Initialize(inputToExpect(input.matrix));

        float[,] er = new float[input.matRows, input.matCols];
        for (int i = 0; i < er.GetLength(0); i++)
        {
            er[i, 0] = output.matrix[i, 0] - expect.matrix[i, 0];
        }
        error.Initialize(er);

        float[,] der = new float[brain.matRows, brain.matCols];

        // 
        for (int i = 0; i < brain.matRows; i++)
        {
            for (int j = 0; j < brain.matCols - 1; j++)
            {
                der[i, j] = (DerivDirection(i) * input.matrix[j, 0]) * stepSize;
            }
            der[i, brain.matCols - 1] = DerivDirection(i) * stepSize;
        }

        deriv.Initialize(der);
    }

    void UpdateUI()
    {
        // 
        txt[0].text = "input\n" + input.DisplayAsText(true);
        txt[1].text = "brain\n" + brain.DisplayAsText(true);
        txt[2].text = "output\n" + output.DisplayAsText(true);
        txt[3].text = "guess\n" + guess.DisplayAsText(true);
        txt[4].text = "expect\n" + expect.DisplayAsText(true);
        txt[5].text = "error\n" + error.DisplayAsText(true);
        txt[6].text = "derivs\n" + deriv.DisplayAsText(true);
        txt[7].text = "Recurision: " + recursion;

        img[0].sprite = InputToSprite(input.matrix);
        img[1].sprite = InputToSprite(guess.matrix);
        img[2].sprite = InputToSprite(expect.matrix);
        // 
        for (int i = 0; i < proc.Count; i++)
        {
            if(i+3 <= img.Length-1)
            {
                img[i + 3].sprite = InputToSprite(proc[proc.Count - (i + 1)]);
            }
        }
    }

    float DerivDirection(int i)
    {
        return error.matrix[i, 0] / Mathf.Abs(error.matrix[i, 0]);
    }

    private static Sprite InputToSprite(float[,] mat)
    {
        Sprite spr = null;

        if (mat[1, 0] == 1)
        {
            spr = Resources.Load<Sprite>("ss0");
        }
        else if (mat[4, 0] == 1)
        {
            spr = Resources.Load<Sprite>("ss1");
        }
        else if (mat[3, 0] == 1)
        {
            spr = Resources.Load<Sprite>("ss2");
        }
        else if (mat[0, 0] == 1)
        {
            spr = Resources.Load<Sprite>("ss3");
        }
        else if (mat[2, 0] == 1)
        {
            spr = Resources.Load<Sprite>("ssg");
        }
        return spr;
    }

    float[,] inputToExpect(float[,] inp)
    {
        float[,] ret = new float[5, 1];
        float[][,] next = new float[5][,];

        for (int i = 0; i < next.Length; i++)
        {
            next[i] = new float[5, 1];
        next[i][iToNumb(i), 0] = 1;
        }

        // 
        for (int i = 0; i < next.Length; i++)
        {
            if (inp[iToNumb(i), 0] == next[i][iToNumb(i), 0])
            {
                ret = i!= next.Length-1 ? next[i + 1] : next[0];
            }
        }

        return ret;
    }

    int iToNumb(int ii)
    {
        int ret = 0;

        if (ii == 0)
        {
            ret = 1;
        }
        else if (ii == 1)
        {
            ret = 4;
        }
        else if (ii == 2)
        {
            ret = 3;
        }
        else if (ii == 3)
        {
            ret = 0;
        }
        else if (ii == 4)
        {
            ret = 2;
        }

        return ret;
    }
}
