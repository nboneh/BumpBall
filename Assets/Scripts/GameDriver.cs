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

    int index = 0;

    public Control controller;
    public Ball playerBall;
    public GameObject plane;
    public GameObject particleCollision;

    TextAsset localScoreText;
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
        currentState = GameState.Intro;
        prevState = GameState.NoState;

        Bounds bounds = plane.GetComponent<Collider>().bounds;
        minX = bounds.min.x;
        maxX = bounds.max.x;
        minZ = bounds.min.z;
        maxZ = bounds.max.z;

        centerX = (maxX + minX) / 2.0f;
        centerZ = (maxZ + minZ) / 2.0f;

        if (File.Exists(localScoreFileName))
        {
            StreamReader reader = new StreamReader(localScoreFileName); // Does this work?
            localHighScore = int.Parse(reader.ReadLine());
            reader.Close();
        }
        playerBall.GetComponent<Renderer>().material.color = playerColor;

        enemyballs = new ArrayList();
        resetEnemyBalls();
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
        timeToGenerateBall = 1.5f;
        timeToGenerateBallCounter = timeToGenerateBall - .5f;
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
                int numberOfBallsToGenerate = (int)Random.Range(1, 4);

                for(int i = 0; i < numberOfBallsToGenerate; i++)
                {
                    generateEnemyBall();
                }

                if (timeToGenerateBall > .4f )
                    timeToGenerateBall -= .03f;

            }

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
            DrawOutline(new Rect(Screen.width / 2 - 300, Screen.height / 2 + 50, 600, 50), "Score: " + (int)finalscore, textStyle);
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
            StreamWriter writer = new StreamWriter(localScoreFileName); // Does this work?
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
        playerBall = (Ball)Instantiate(balls[index], new Vector3(0, 1, 0), Quaternion.identity);
        if (index == 4)
            playerBall.GetComponent<Renderer>().materials[1].color = playerColor;
        else
            playerBall.GetComponent<Renderer>().material.color = playerColor;
    }

    void generateEnemyBall()
    {
        float radius = Random.Range(0.75f, 2);
        int side =(int)(Random.Range(0, 4));
            
        float x =0;
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
            if (enemyball.isTouchingBall(radius,x,z))
            {
                return;
            }
        }

        Ball ball = (Ball)Instantiate(balls[index], new Vector3(x, radius, z), Quaternion.identity);
        if (index == 4)
            ball.GetComponent<Renderer>().materials[1].color = enemyColor;
        else
            ball.GetComponent<Renderer>().material.color = enemyColor;

        ball.SetRadius(radius);
        ball.SetDensity(500);

        enemyballs.Add(ball);
        float velocityMag = Random.Range(10,16);
        float angle = getAngle(centerX, x, centerZ, z) + Random.Range(-0.175f,0.175f);

        Vector3 velocity = new Vector3();
        velocity.x = velocityMag * Mathf.Cos(angle);
        velocity.y = 0;
        velocity.z = velocityMag * Mathf.Sin(angle) ;

        ball.GetComponent<Rigidbody>().velocity = velocity;

        ball.enabled = true;


    }

    void DrawOutline(Rect position,  string text, GUIStyle style)
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
