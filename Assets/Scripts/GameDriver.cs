using System.IO;
using UnityEngine;
using System.Collections;

public class GameDriver : MonoBehaviour
{


    public class Highscore
    {
        public int score;
        public string name;
    }

    enum GameState { Intro, Playing, Highscore, NoState };
    GameState currentState;
    GameState prevState;
    GUIStyle textStyle;

    float alpha = 0.0f;
    float score = 0;

    int finalscore = 0;

    int localHighScore = 0;

    int onlineHighScore = -1;
    string onlineHighScoreName = "";
    string inputname = "";
    int numOfDots = 3;

    bool sending = false;
    bool failedToSend = false;

    float timerForDotChange = 0;
    float timeTillChangeDot = .5f;

    int index = 0;

    public Control controller;
    public Ball playerBall;
    public GameObject plane;
    public Font font;

    TextAsset localScoreText;

    string saveFileLocation;
    string localScoreFileName = "score.txt";

    Color playerColor = new Color(1f, 0, 0);
    Color enemyColor = new Color(0, 0, 1f);

    bool drawScoreIntro = false;

    float minX;
    float maxX;
    float minZ;
    float maxZ;
    float centerX;
    float centerZ;

    public Material[] planeMaterials;
    public Ball[] balls;

    ArrayList enemyballs;

    float timeToGenerateBall;
    float timeToGenerateBallCounter;

    void Start()
    {
        saveFileLocation = Application.persistentDataPath;
        currentState = GameState.Intro;
        prevState = GameState.NoState;

        Bounds bounds = plane.GetComponent<Collider>().bounds;
        minX = bounds.min.x;
        maxX = bounds.max.x;
        minZ = bounds.min.z;
        maxZ = bounds.max.z;

        centerX = (maxX + minX) / 2.0f;
        centerZ = (maxZ + minZ) / 2.0f;

        if (File.Exists(saveFileLocation + "/" + localScoreFileName))
        {
            StreamReader reader = new StreamReader(saveFileLocation + "/" + localScoreFileName); // Does this work?
            localHighScore = int.Parse(reader.ReadLine());
            reader.Close();
        }
        playerBall.GetComponent<Renderer>().material.color = playerColor;

        enemyballs = new ArrayList();
        resetEnemyBalls();
        startRequestForOnlineHighScore();
    }

    void startRequestForOnlineHighScore()
    {
        string url = "https://bumpballscores.appspot.com/highscore";
        WWW www = new WWW(url);
        StartCoroutine(WaitForRequest(www));
    }

    IEnumerator WaitForRequest(WWW www)
    {
        yield return www;

        // check for errors
        if (www.error == null)
        {
            Highscore highscore = JsonUtility.FromJson<Highscore>(www.text);
            onlineHighScore = highscore.score;
            onlineHighScoreName = highscore.name;
        }
        else
        {
            onlineHighScore = -2;
        }
    }

    void resetEnemyBalls()
    {
        for (int i = enemyballs.Count - 1; i >= 0; i--)
        {
            Ball ball = (Ball)enemyballs[i];
            Destroy(ball.gameObject);
            Destroy(ball);
        }
        enemyballs.Clear();
        timeToGenerateBall = 1.8f;
        timeToGenerateBallCounter = timeToGenerateBall - .5f;
    }

    void OnGUI()
    {
        setAlpha();
        switch (currentState)
        {
            case GameState.Intro:
                drawIntro();
                break;
            case GameState.Playing:
                drawScore();
                break;
            case GameState.Highscore:
                drawHighscore();
                break;
        }
        if (alpha == 1.0f || prevState == GameState.NoState)
            return;
        setPrevAlpha();
        switch (prevState)
        {
            case GameState.Intro:
                drawIntro();
                break;
            case GameState.Playing:
                drawScore();
                break;
            case GameState.Highscore:
                drawHighscore();
                break;
        }
    }

    void setState(GameState state)
    {
        prevState = currentState;
        currentState = state;
        alpha = 0;
    }

    void Update()
    {
        if (controller.isOn())
        {
            if (currentState == GameState.Intro)
            {
                score = 0;
                setState(GameState.Playing);
            }
        }
    }
    void FixedUpdate()
    {
        float t = Time.deltaTime;
        if (alpha < 1.0f)
        {
            alpha += t * 4;
            if (alpha > 1.0f)
            {
                alpha = 1.0f;
            }
        }

        if ((currentState == GameState.Intro && onlineHighScore == -1) || sending)
        {
            timerForDotChange += t;
            if (timerForDotChange >= timeTillChangeDot)
            {
                timerForDotChange -= timeTillChangeDot;
                numOfDots++;
                if (numOfDots > 3)
                {
                    numOfDots = 0;
                }
            }
        }

        if (currentState == GameState.Playing)
        {
            updatePlaying(t);
            playerBall.updateAcceleration(controller.getAcceleration(), t);
            timeToGenerateBallCounter += t;
 


            for (int i = enemyballs.Count - 1; i >= 0; i--)
            {
                Ball ball = (Ball)enemyballs[i];
                if (!isTouchingPlane(ball))
                {
                    Destroy(ball.gameObject);
                    enemyballs.Remove(ball);
                }

            }


            if (timeToGenerateBallCounter >= timeToGenerateBall)
            {
                timeToGenerateBallCounter -= timeToGenerateBall;
                int numberOfBallsToGenerate = (int)Random.Range(1, 5);

                for (int i = 0; i < numberOfBallsToGenerate; i++)
                {
                    generateEnemyBall();
                }

                if (timeToGenerateBall > .4f)
                    timeToGenerateBall -= .02f;

            }

            if (!isTouchingPlane(playerBall) || playerBall.WasHit())
            {
                controller.turnOff();
                finalscore = (int)score;
                drawScoreIntro = true;
                CheckLocalHighScore();
                Reset();

                if (finalscore > onlineHighScore && onlineHighScore >= 0)
                {
                    setState(GameState.Highscore);
                }
                else
                {
                    setState(GameState.Intro);
                    startRequestForOnlineHighScore();
                }
            }
        }
    }

    void setAlpha()
    {
        Color c = GUI.color;
        c.a = alpha;
        GUI.color = c;
    }

    void setPrevAlpha()
    {
        Color c = GUI.color;
        c.a = 1 - alpha;
        GUI.color = c;
    }

    void drawIntro()
    {
        GUIStyle textStyle = GUI.skin.GetStyle("Label");
        textStyle.alignment = TextAnchor.UpperCenter;
        textStyle.normal.textColor = Color.white;
        int fontSize = getFontSize();
        textStyle.fontSize = fontSize;
        textStyle.font = font;

        float width = fontSize * 36;
        float height = fontSize * 2;
        float relativeHeight = Screen.height / 15;
        textStyle.fontStyle = FontStyle.Bold;
        DrawOutline(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - relativeHeight * 3, width, height), "Dodge the blue balls!", textStyle);
        DrawOutline(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - relativeHeight * 2, width, height), "Use a joystick by placing your finger anywhere", textStyle);
        if (drawScoreIntro)
        {
            DrawOutline(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 + relativeHeight, width, height), "Score: " + (int)finalscore, textStyle);
        }
        DrawOutline(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 + relativeHeight * 2, width, height), "Local Highscore: " + localHighScore, textStyle);
        if (onlineHighScore == -1)
        {
            string text = "Loading Online Highscore";
            for (int i = 0; i < numOfDots; i++)
            {
                text += ".";
            }
            DrawOutline(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 + relativeHeight * 3, width, height), text, textStyle);
        }
        else if (onlineHighScore == -2)
        {
            DrawOutline(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 + relativeHeight * 3, width, height), "Error Loading Online Highscore", textStyle);
        }
        else
        {
            DrawOutline(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 + relativeHeight * 3, width, height), "Online Highscore: " + onlineHighScore + " " + onlineHighScoreName, textStyle);
        }
    }

    void drawScore()
    {
        GUIStyle textStyle = GUI.skin.GetStyle("Label");
        textStyle.normal.textColor = Color.white;
        textStyle.alignment = TextAnchor.MiddleLeft;
        int fontSize = getFontSize();
        textStyle.fontSize = fontSize;
        float width = fontSize * 20;
        float height = fontSize * 2;
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.font = font;
        DrawOutline(new Rect(Screen.width / 40.0f, Screen.height / 40.0f, width, height), "Score: " + (int)score, textStyle);
    }


    void drawHighscore()
    {
        controller.turnOff();
        GUIStyle textStyle = GUI.skin.GetStyle("Label");
        textStyle.normal.textColor = Color.white;
        textStyle.alignment = TextAnchor.UpperCenter;
        int fontSize = getFontSize();
        textStyle.fontSize = fontSize;
        textStyle.font = font;

        float width = fontSize * 36;
        float height = fontSize * 2;
        float relativeHeight = Screen.height / 15;
        textStyle.fontStyle = FontStyle.Bold;
        DrawOutline(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - relativeHeight * 6, width, height), "Congratulations! You made the online highscore!", textStyle);
        DrawOutline(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - relativeHeight * 5, width, height), "Your score was " + finalscore + ", enter your name below", textStyle);
        if (sending)
        {
            string text = "Sending Online Highscore";
            for (int i = 0; i < numOfDots; i++)
            {
                text += ".";
            }
            DrawOutline(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - relativeHeight * 4, width, height), text, textStyle);
        }
        else
        {
            GUIStyle textFieldStyle = GUI.skin.GetStyle("TextField");
            textFieldStyle.fontSize = fontSize;
            textFieldStyle.font = font;

            float textFieldwidth = fontSize * 13;
            float textFieldHeight = fontSize * 1.5f;
            inputname = GUI.TextField(new Rect(Screen.width / 2 - textFieldwidth / 2, Screen.height / 2 - relativeHeight * 3.8f, textFieldwidth, textFieldHeight), inputname, textFieldStyle);
            if (inputname.Length >= 12)
            {
                inputname = inputname.Substring(0, 12);
            }
            GUIStyle buttonStyle = GUI.skin.GetStyle("Button");
            buttonStyle.fontSize = fontSize;
            buttonStyle.font = font;
            float buttonWidth = fontSize * 7;
            bool clicked = GUI.Button(new Rect(Screen.width / 2 - buttonWidth / 2, Screen.height / 2 - relativeHeight * 2.6f, buttonWidth, height), "Submit", buttonStyle);
            if (clicked)
            {
                sendOnlineHighScore();
            }
        }
        DrawOutline(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 + relativeHeight * 2, width, height), "Note: Your score will be deleted in one week", textStyle);
        DrawOutline(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 + relativeHeight * 3, width, height), "or if someone else gets a higher score", textStyle);

        if (failedToSend)
        {
            textStyle.normal.textColor = Color.red;
            DrawOutline(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 + relativeHeight * 4, width, height), "Failed to send please try again", textStyle);
        }
    }

    void sendOnlineHighScore()
    {
        sending = true;
        string url = "https://bumpballscores.appspot.com/highscore";
        WWWForm form = new WWWForm();
        form.AddField("name", inputname);
        form.AddField("pass", "");
        form.AddField("score", finalscore);

        WWW www = new WWW(url, form);
        StartCoroutine(Send(www));
    }
    
    IEnumerator Send(WWW www)
    {
        yield return www;
        sending = false;
        // check for errors
        if (www.error == null)
        {
            onlineHighScore = -1;
            setState(GameState.Intro);
            startRequestForOnlineHighScore();
        }
        else
        {
            failedToSend = true;
        }
    }
    int getFontSize()
    {
        int fontSize = (int)(48.0f * (float)(Screen.width) / 1920.0f); //scale size font;
        if (fontSize < 14)
            fontSize = 14;
        return fontSize;
    }


    bool isTouchingPlane(Ball ball)
    {
        float radius = ball.GetRadius();
        float x = ball.GetComponent<Rigidbody>().transform.position.x;
        float z = ball.GetComponent<Rigidbody>().transform.position.z;

        return (z + radius) >= minZ && (x + radius) >= minX && (x - radius) <= maxX && (z - radius) <= maxZ;
    }

    void updatePlaying(float t)
    {
        score += t * 4;
    }


    void CheckLocalHighScore()
    {
        if (finalscore > localHighScore)
        {
            localHighScore = finalscore;
            StreamWriter writer = new StreamWriter(saveFileLocation + "/" + localScoreFileName); // Does this work?
            writer.Write(finalscore);
            writer.Close();
        }
    }

    void Reset()
    {
        resetEnemyBalls();
        Destroy(playerBall.gameObject);
        index = Random.Range(0, planeMaterials.Length);
        plane.GetComponent<Renderer>().material = planeMaterials[index];
        Invoke("createPlayerBall", .01f);

    }

    void createPlayerBall()
    {
        playerBall = (Ball)Instantiate(balls[index], new Vector3(0, .98f, 0), Quaternion.identity);
        if (index == 4)
            playerBall.GetComponent<Renderer>().materials[1].color = playerColor;
        else
            playerBall.GetComponent<Renderer>().material.color = playerColor;
    }

    void generateEnemyBall()
    {
        float radius = Random.Range(0.65f, 1.65f);
        int side = (int)(Random.Range(0, 4));

        float x = 0;
        float z = 0;

        float minXGen = minX - radius;
        float minZGen = minZ - radius;
        float maxXGen = maxX + radius;
        float maxZGen = maxZ + radius;

        switch (side)
        {
            case 0:
                x = Random.Range(minXGen, maxXGen);
                z = minZGen;
                break;
            case 1:
                x = minXGen;
                z = Random.Range(minZGen, maxZGen);
                break;
            case 2:
                x = Random.Range(minXGen, maxXGen);
                z = maxZGen;
                break;
            case 3:
                x = maxXGen;
                z = Random.Range(minZGen, maxZGen);
                break;
        }

        for (int i = enemyballs.Count - 1; i >= 0; i--)
        {
            Ball enemyball = (Ball)enemyballs[i];
            if (enemyball.isTouchingBall(radius + 2, x, z))
            {
                return;
            }
        }

        Ball ball = (Ball)Instantiate(balls[index], new Vector3(x, radius - .02f, z), Quaternion.identity);
        if (index == 4)
            ball.GetComponent<Renderer>().materials[1].color = enemyColor;
        else
            ball.GetComponent<Renderer>().material.color = enemyColor;

        ball.SetRadius(radius);
        ball.SetDensity(5);

        enemyballs.Add(ball);
        float velocityMag = Random.Range(8, 16);
        float angle = getAngle(centerX, x, centerZ, z) + Random.Range(-0.175f, 0.175f);

        Vector3 velocity = new Vector3();
        velocity.x = velocityMag * Mathf.Cos(angle);
        velocity.y = 0;
        velocity.z = velocityMag * Mathf.Sin(angle);

        ball.GetComponent<Rigidbody>().velocity = velocity;
        ball.GetComponent<Rigidbody>().transform.localEulerAngles = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

        ball.enabled = true;


    }

    void DrawOutline(Rect position, string text, GUIStyle style)
    {
        var outColor = Color.black;
        var backupStyle = style;
        var oldColor = style.normal.textColor;
        style.normal.textColor = outColor;
        position.x--;
        GUI.Label(position, text, style);
        position.x += 2;
        GUI.Label(position, text, style);
        position.x--;
        position.y--;
        GUI.Label(position, text, style);
        position.y += 2;
        GUI.Label(position, text, style);
        position.y--;
        style.normal.textColor = oldColor;
        GUI.Label(position, text, style);
        style = backupStyle;
    }

    float getAngle(float x1, float x2, float z1, float z2)
    {
        return Mathf.Atan2(z1 - z2, x1 - x2);
    }


}
