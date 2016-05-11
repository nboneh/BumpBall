using UnityEngine;
using System.Collections;

public class GameDriver : MonoBehaviour {

    enum GameState { Intro, Playing, GameOver, Highscore, NoState };
    GameState currentState;
    GameState prevState;
    GUIStyle textStyle;

    float alpha = 0.0f;
    float score = 0;

    public Control controller;
    public Ball playerBall;

    void Start()
    {
        currentState = GameState.Intro;
        prevState = GameState.NoState;
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
        GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 - 150, 600, 50), "Help the redball stay inside the screen", textStyle);
        GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 - 100, 600, 50), "Use a joystick by placing your finger anywhere", textStyle);
        GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 + 50, 600, 50), "Local Highscore: 0", textStyle);
        GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 + 100, 600, 50), "Online Highscore: 500 Mr. Krabs", textStyle);
    }

    void drawScore()
    {
        GUIStyle textStyle = GUI.skin.GetStyle("Label");
        textStyle.alignment = TextAnchor.MiddleLeft;
        textStyle.fontSize = 26;
        textStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(20, 20, 600, 50), "Score: " + (int)score, textStyle);
    }

    void updatePlaying(float t)
    {
        score += t * 4;
    }

}
