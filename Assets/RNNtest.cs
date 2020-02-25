using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BriljaSanLib;

public class RNNtest : MonoBehaviour
{
    public Vector3 input = new Vector3(1, 0, 0);
    public Vector4[] matrix;
    public Vector6[] derivs = new Vector6[3];

    public Vector3 exp;
    public Text[] inp;
    public Text[] hid;
    public Text[] outt;
    public Text[] ders;
    public int times;

    public Image[] guesses;
    public Sprite[] history;

    Text impTxt;
    Text hidTxt;
    bool cruching;

    // Use this for initialization
    void Start()
    {
        matrix = new Vector4[]
{
        new Vector4(Random.Range(0,2),Random.Range(0,2),Random.Range(0,2),Random.Range(0,2)),
        new Vector4(Random.Range(0,2),Random.Range(0,2),Random.Range(0,2),Random.Range(0,2)),
        new Vector4(Random.Range(0,2),Random.Range(0,2),Random.Range(0,2),Random.Range(0,2)),
};
        Recursion();
    }

    void Recursion()
    {
        if (input == new Vector3(1, 0, 0))
        {
            exp = new Vector3(0, 0, 1);
        }
        else if (input == new Vector3(0, 0, 1))
        {
            exp = new Vector3(0, 1, 0);
        }
        else if (input == new Vector3(0, 1, 0))
        {
            exp = new Vector3(1, 0, 0);
        }

        Sprite[] tHistory = history;
        int fig = tHistory.Length < 8 ? tHistory.Length + 1 : 8;

        history = new Sprite[fig];

        history[0] = GrabSprite(input);
        for (int i = 1; i < history.Length; i++)
        {
            history[i] = tHistory[i - 1];
        }


        guesses = GameObject.Find("Guesses").GetComponentsInChildren<Image>();

        for (int i = 0; i < history.Length; i++)
        {
            guesses[i].sprite = history[i];
        }

        impTxt = GameObject.Find("inputTxt").GetComponent<Text>();
        impTxt.text = string.Format("[{0}]\n[{1}]\n[{2}]", input.x, input.y, input.z);

        hidTxt = GameObject.Find("matrixTxt").GetComponent<Text>();
        hidTxt.text = string.Format("[{0},{1},{2},{9}]\n[{3},{4},{5},{10}]\n[{6},{7},{8},{11}]", matrix[0].x, matrix[0].y, matrix[0].z, matrix[1].x, matrix[1].y, matrix[1].z, matrix[2].x, matrix[2].y, matrix[2].z, matrix[0].w, matrix[1].w, matrix[2].w);

        inp = GameObject.Find("i").GetComponentsInChildren<Text>();
        inp[0].text = input.x.ToString();
        inp[1].text = input.y.ToString();
        inp[2].text = input.z.ToString();

        hid = GameObject.Find("h").GetComponentsInChildren<Text>();
        hid[0].text = ((input.x * matrix[0].x) + (input.y * matrix[0].y) + (input.z * matrix[0].z) + matrix[0].w).ToString();
        hid[1].text = matrix[0].x.ToString();
        hid[2].text = matrix[0].y.ToString();
        hid[3].text = matrix[0].z.ToString();
        hid[4].text = matrix[0].w.ToString();

        hid[5].text = ((input.x * matrix[1].x) + (input.y * matrix[1].y) + (input.z * matrix[1].z) + matrix[1].w).ToString();
        hid[6].text = matrix[1].x.ToString();
        hid[7].text = matrix[1].y.ToString();
        hid[8].text = matrix[1].z.ToString();
        hid[9].text = matrix[1].w.ToString();

        hid[10].text = ((input.x * matrix[2].x) + (input.y * matrix[2].y) + (input.z * matrix[2].z) + matrix[2].w).ToString();
        hid[11].text = matrix[2].x.ToString();
        hid[12].text = matrix[2].y.ToString();
        hid[13].text = matrix[2].z.ToString();
        hid[14].text = matrix[2].w.ToString();

        int totalHL = 3;

        float getMax = 0;
        float retSub = 0;
        for (int i = 0; i < totalHL; i++)
        {
            float curVal = float.Parse(hid[i * 5].text);

            getMax = (getMax < curVal) ? curVal : getMax;
            retSub = (getMax > 1) ? getMax - 1 : 0;
        }

        outt = GameObject.Find("o").GetComponentsInChildren<Text>();
        outt[0].text = Mathf.Clamp((float.Parse(hid[0].text) - retSub), 0, 1).ToString();
        outt[1].text = exp.x.ToString();
        outt[2].text = Mathf.Abs(float.Parse(outt[1].text) - float.Parse(outt[0].text)).ToString();

        outt[3].text = Mathf.Clamp((float.Parse(hid[5].text) - retSub), 0, 1).ToString();
        outt[4].text = exp.y.ToString();
        outt[5].text = Mathf.Abs(float.Parse(outt[4].text) - float.Parse(outt[3].text)).ToString();

        outt[6].text = Mathf.Clamp((float.Parse(hid[10].text) - retSub), 0, 1).ToString();
        outt[7].text = exp.z.ToString();
        outt[8].text = Mathf.Abs(float.Parse(outt[7].text) - float.Parse(outt[6].text)).ToString();

        // DERIVITIVES!
        derivs[0].x = float.Parse(outt[0].text) < float.Parse(outt[1].text) ? -1 : float.Parse(outt[0].text) > float.Parse(outt[1].text) ? 1 : 0;
        derivs[1].x = float.Parse(outt[3].text) < float.Parse(outt[4].text) ? -1 : float.Parse(outt[3].text) > float.Parse(outt[4].text) ? 1 : 0;
        derivs[2].x = float.Parse(outt[6].text) < float.Parse(outt[7].text) ? -1 : float.Parse(outt[6].text) > float.Parse(outt[7].text) ? 1 : 0;

        derivs[0].y = derivs[0].x;
        derivs[1].y = derivs[1].x;
        derivs[2].y = derivs[2].x;

        derivs[0].z = derivs[0].y;
        derivs[1].z = derivs[1].y;
        derivs[2].z = derivs[2].y;

        derivs[0].w = (derivs[0].z * input[0]);
        derivs[1].w = (derivs[1].z * input[0]);
        derivs[2].w = (derivs[2].z * input[0]);

        derivs[0].h = (derivs[0].z * input[1]);
        derivs[1].h = (derivs[1].z * input[1]);
        derivs[2].h = (derivs[2].z * input[1]);

        derivs[0].l = (derivs[0].z * input[2]);
        derivs[1].l = (derivs[1].z * input[2]);
        derivs[2].l = (derivs[2].z * input[2]);

        //
        ders = GameObject.Find("ders").GetComponentsInChildren<Text>();

        ders[0].text = derivs[0].x.ToString();
        ders[0 + 6].text = derivs[1].x.ToString();
        ders[0 + 12].text = derivs[2].x.ToString();

        ders[1].text = derivs[0].y.ToString();
        ders[1 + 6].text = derivs[1].y.ToString();
        ders[1 + 12].text = derivs[2].y.ToString();

        ders[2].text = derivs[0].z.ToString();
        ders[2 + 6].text = derivs[1].z.ToString();
        ders[2 + 12].text = derivs[2].z.ToString();

        ders[3].text = derivs[0].w.ToString();
        ders[3 + 6].text = derivs[1].w.ToString();
        ders[3 + 12].text = derivs[2].w.ToString();

        ders[4].text = derivs[0].h.ToString();
        ders[4 + 6].text = derivs[1].h.ToString();
        ders[4 + 12].text = derivs[2].h.ToString();

        ders[5].text = derivs[0].l.ToString();
        ders[5 + 6].text = derivs[1].l.ToString();
        ders[5 + 12].text = derivs[2].l.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        Enable.ExitGame(KeyCode.Escape);

        //
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Automate();
        }

        //
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Application.LoadLevel(Application.loadedLevel);
        }

        if (cruching || Input.GetKeyDown(KeyCode.Backspace))
        {
            Next();
        }
    }

    public void Next()
    {
        times++;
        GameObject.Find("count").GetComponent<Text>().text = "Recursion: " + times;

        // Backprop
        float stepSize = 1;
        for (int i = 0; i < 3; i++)
        {
            matrix[i] = new Vector4(
                Mathf.Clamp(matrix[i].x - (stepSize * derivs[i].w), 0, 1),
                Mathf.Clamp(matrix[i].y - (stepSize * derivs[i].h), 0, 1),
                Mathf.Clamp(matrix[i].z - (stepSize * derivs[i].l), 0, 1),
                Mathf.Clamp(matrix[i].w - (stepSize * derivs[i].z), 0, 1));
            print(i);
        }

        Vector3 outVec = new Vector3(float.Parse(outt[0].text), float.Parse(outt[3].text), float.Parse(outt[6].text));
        if (outVec == new Vector3(1, 0, 0) || outVec == new Vector3(0, 0, 1) || outVec == new Vector3(0, 1, 0))
        {
            input = outVec;
        }
        else
        {
            input = new Vector3(1, 0, 0);
        }
        Recursion();
    }

    public void Automate()
    {
        cruching = cruching ? false : true;
    }

    Sprite GrabSprite(Vector3 inp)
    {
        if (inp == new Vector3(1, 0, 0))
        {
            return Resources.Load<Sprite>("prep");
        }
        else if (inp == new Vector3(0, 0, 1))
        {
            return Resources.Load<Sprite>("cook");
        }
        else if (inp == new Vector3(0, 1, 0))
        {
            return Resources.Load<Sprite>("eat");
        }
        return null;
    }
}

[System.Serializable]
public class Vector6
{
    public float x = 0, y = 0, z = 0, w = 0, h = 0, l = 0;

    public void Set(float X, float Y, float Z, float W, float H, float L)
    {
        x = X;
        y = Y;
        z = Z;
        w = W;
        h = H;
        l = L;
    }
}

