using System.IO;
using UnityEngine;
using System.Collections;

public class GameDriver : MonoBehaviour {

    enum GameState { Intro, Playing, Highscore, NoState };
    GameState currentState;
    GameState prevState;
    GUIStyle textStyle;

    float alpha = 0.0f;
    float score = 0;

    int finalscore = 0;

    int localHighScore = 0;

    public Control controller;
    public Ball playerBall;
    public GameObject plane;

    TextAsset localScoreText;
    string localScoreFileName = "score.txt";

    Color playerColor = new Color(.7f, 0, 0);
    Color enemyColor = new Color(0, 0, .7f);

    bool drawScoreIntro = false;

    float minX;
    float maxX;
    float minZ;
    float maxZ;

    public Material[] planeMaterials;
    public Ball[] balls;
    

    void Start()
    {
        currentState = GameState.Intro;
        prevState = GameState.NoState;
      
        Bounds bounds = plane.GetComponent<Renderer>().bounds;
        minX = bounds.min.x;
        maxX = bounds.max.x;
        minZ = bounds.min.z;
        maxZ = bounds.max.z;

        if (File.Exists( localScoreFileName))
        {
            StreamReader reader = new StreamReader(localScoreFileName); // Does this work?
            localHighScore = int.Parse(reader.ReadLine());
            reader.Close();
        }
        playerBall.GetComponent<Renderer>().material.color = playerColor;
    }
    void OnGUI()
    {

        setAlpha();
        switch (currentState) {
            case GameState.Intro:
                 drawIntro();
                 break;
            case GameState.Playing:
                drawScore();
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
            if(currentState == GameState.Intro)
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
            alpha += t * 4 ;
            if(alpha > 1.0f)
            {
                alpha = 1.0f;
            }
       }

        if (currentState == GameState.Playing)
        {
            updatePlaying(t);
            playerBall.updateAcceleration(controller.getAcceleration(), t);
            if (!isTouchingPlane(playerBall))
            {
                controller.turnOff();
                finalscore = (int)score;
                drawScoreIntro = true;
                CheckLocalHighScore();
                Reset();
                setState(GameState.Intro);
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
        textStyle.fontSize = 26;
        textStyle.fontStyle = FontStyle.Bold;
        DrawOutline(new Rect(Screen.width / 2 - 300, Screen.height / 2 - 150, 600, 50), "Help the redball stay inside the screen", textStyle);
        DrawOutline(new Rect(Screen.width / 2 - 300, Screen.height / 2 - 100, 600, 50), "Use a joystick by placing your finger anywhere", textStyle);
        if (drawScoreIntro)
        {
            DrawOutline(new Rect(Screen.width / 2 - 300, Screen.height / 2 +50, 600, 50), "Score: " + (int)finalscore, textStyle);
        }
        DrawOutline(new Rect(Screen.width / 2 - 300, Screen.height / 2 + 100, 600, 50), "Local Highscore: " + localHighScore, textStyle);
        DrawOutline(new Rect(Screen.width / 2 - 300, Screen.height / 2 + 150, 600, 50), "Online Highscore: 500 Mr. Krabs", textStyle);
    }

    void drawScore()
    {
        GUIStyle textStyle = GUI.skin.GetStyle("Label");
        textStyle.alignment = TextAnchor.MiddleLeft;
        textStyle.fontSize = 26;
        textStyle.fontStyle = FontStyle.Bold;
        DrawOutline(new Rect(20, 20, 600, 50), "Score: " + (int)score, textStyle);
    }

    void drawGameOver()
    {
        drawIntro();
    }

    bool isTouchingPlane(Ball ball)
    {
        Bounds bounds = ball.GetComponent<Renderer>().bounds;
        float ballMinX = bounds.min.x;
        float ballMaxX = bounds.max.x;
        float ballMinZ = bounds.min.z;
        float ballMaxZ = bounds.max.z;

        float diameter = ballMaxX - ballMinX;

        return (ballMinZ + diameter) >= minZ && (ballMinX + diameter) >= minX && (ballMaxX - diameter) <= maxX && (ballMaxZ - diameter) <= maxZ;
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
            StreamWriter writer = new StreamWriter( localScoreFileName); // Does this work?
            writer.Write(finalscore);
            writer.Close();
        }
    }

    void Reset()
    {
        int rand = Random.Range(0, planeMaterials.Length);
        plane.GetComponent<Renderer>().material = planeMaterials[rand];

        Destroy(playerBall.gameObject);
        playerBall = Instantiate(balls[rand]);
        if(rand == 4)
             playerBall.GetComponent<Renderer>().materials[1].color = playerColor;
        else
            playerBall.GetComponent<Renderer>().material.color = playerColor;
        playerBall.transform.position = new Vector3(0, 1, 0);

    }

    public static void DrawOutline(Rect position,  string text, GUIStyle style)
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
}
