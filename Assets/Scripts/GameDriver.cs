using UnityEngine;
using System.Collections;

public class GameDriver : MonoBehaviour {

    void Start()
    {
   
    }
    void OnGUI()
    {
        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;
        centeredStyle.fontSize = 26;
        centeredStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 -150, 600, 50), "Help the redball stay inside the screen", centeredStyle);

        GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2-100 , 600, 50), "Use a joystick by placing your finger anywhere", centeredStyle);

        GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 +50, 600, 50), "Local Highscore: 0", centeredStyle);
        GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 + 100, 600, 50), "Online Highscore: 500 Mr. Krabs", centeredStyle);

    }

}
