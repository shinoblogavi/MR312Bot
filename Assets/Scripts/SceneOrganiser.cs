using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneOrganiser : MonoBehaviour {

    /// <summary>
    /// Static instance of this class
    /// </summary>
    public static SceneOrganiser Instance;

    /// <summary>
    /// The 3D text representing the Bot response
    /// </summary>
    //internal TextMesh botResponseText;
    public TextMesh botResponseText;

    /// <summary>
    /// Called on Initialization
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Called immediately after Awake method
    /// </summary>
    void Start()
    {
        // Add the GazeInput class to this object
        //gameObject.AddComponent<GazeInput>();

        // Add the Interactions class to this object
        //gameObject.AddComponent<Interactions>();

        // Create the Bot in the scene
        //CreateBotInScene();

    }

    /// <summary>
    /// Create the Sign In button object in the scene
    /// and sets its properties
    /// </summary>
    //private void CreateBotInScene()
    //{
    //    GameObject botObjInScene = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //    botObjInScene.name = "Bot";

    //    // Add the Bot class to the Bot GameObject
    //    botObjInScene.AddComponent<Bot>();

    //    // Create the Bot UI
    //    botResponseText = CreateBotResponseText();

    //    // Set properties of Bot GameObject
    //    Bot.Instance.botMaterial = new Material(Shader.Find("Diffuse"));
    //    botObjInScene.GetComponent<Renderer>().material = Bot.Instance.botMaterial;
    //    Bot.Instance.botMaterial.color = Color.blue;
    //    botObjInScene.transform.position = new Vector3(0f, 2f, 10f);
    //    botObjInScene.tag = "BotTag";
    //}

    /// <summary>
    /// Spawns cursor for the Main Camera
    /// </summary>
    //private TextMesh CreateBotResponseText()
    //{
    //    // Create a sphere as new cursor
    //    GameObject textObject = new GameObject();
    //    textObject.transform.parent = Bot.Instance.transform;
    //    textObject.transform.localPosition = new Vector3(0, 0.7f, 0);

    //    // Resize the new cursor
    //    textObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

    //    // Creating the text of the Label
    //    TextMesh textMesh = textObject.AddComponent<TextMesh>();
    //    textMesh.anchor = TextAnchor.MiddleCenter;
    //    textMesh.alignment = TextAlignment.Center;
    //    textMesh.fontSize = 30;
    //    textMesh.text = "こんにちは\n 始めるときは\n私をタップしてください";

    //    return textMesh;
    //}

}
