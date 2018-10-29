using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Windows.Speech;

public class Bot : MonoBehaviour {
    /// <summary>
    /// Static instance of this class
    /// </summary>
    public static Bot Instance;
    public TextMesh botResponseText;


    /// <summary>
    /// Material of the sphere representing the Bot in the scene
    /// </summary>
    internal Material botMaterial;

    /// <summary>
    /// Speech recognizer class reference, which will convert speech to text.
    /// </summary>
    private DictationRecognizer dictationRecognizer;

    /// <summary>
    /// Use this variable to identify the Bot Id
    /// Can be any value
    /// </summary>
    private string botId = "MRBotId";

    /// <summary>
    /// Use this variable to identify the Bot Name
    /// Can be any value
    /// </summary>
    private string botName = "MRBotName";

    /// <summary>
    /// The Bot Secret key found on the Web App Bot Service on the Azure Portal
    /// </summary>
    private string botSecret = "5WiIZhn9Sfg.cwA.zks._QKuxtzV_Pg3p1zFK9bmhA49ipPOkCsyJJi_6eeskKo";
    //private string botSecret = "7GiWhltjqGg.cwA.wxk.i_4EL0UADUeJ4Ewb14YVvgCPJNEIPshJMgh6-AD06SY";
    //private string botSecret = "ULQTU5lLUFc.cwA.Fz8.GfVI0vD524pgz3G6e3EzCwCe2LE8mc7bOZxXtoC5O5I";

    /// <summary>
    /// Bot Endpoint, v4 Framework uses v3 endpoint at this point in time
    /// </summary>
    private string botEndpoint = "https://directline.botframework.com/v3/directline";
    //private string botEndpoint = "https://msgobot.azurewebsites.net/api/messages";

    /// <summary>
    /// The conversation object reference
    /// </summary>
    private ConversationObject conversation;

    /// <summary>
    /// Bot states to regulate the application flow
    /// </summary>
    internal enum BotState { ReadyToListen, Listening, Processing }

    /// <summary>
    /// Flag for the Bot state
    /// </summary>
    internal BotState botState;

    /// <summary>
    /// Flag for the conversation status
    /// </summary>
    internal bool conversationStarted = false;

    /// <summary>
    /// Called on Initialization
    /// </summary>
    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Called immediately after Awake method
    /// </summary>
    void Start()
    {
        botState = BotState.ReadyToListen;

        SetBotResponseText("接続中です...");
        StartCoroutine(StartConversation());
        
        StartCapturingAudio();
    }

    KeywordRecognizer keywordRecognizer = null;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    /// <summary>
    /// Start microphone capture.
    /// </summary>
    public void StartCapturingAudio()
    {
        botState = BotState.Listening;
        //botMaterial.color = Color.red;
        gameObject.GetComponent<Renderer>().material.color = Color.red;

        //// Start dictation
        //dictationRecognizer = new DictationRecognizer();
        //dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
        //dictationRecognizer.Start();

        //Keyword recognizer
        keywords.Add("ヘルプ", () => { Debug.Log("add help"); });
        keywords.Add("こんにちは", () => { Debug.Log("add Hello"); });
        keywords.Add("チェック", () => { Debug.Log("add search"); });
        keywords.Add("お会計", () => { Debug.Log("add check"); });

        // Tell the KeywordRecognizer about our keywords.
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();

    }
    
    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log($"User just said: {args.text}");
        SetBotResponseText(args.text);

        System.Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            StartCoroutine(SendMessageToBot(args.text, botId, botName, "message"));
            gameObject.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    /// <summary>
    /// Stop microphone capture.
    /// </summary>
    public void StopCapturingAudio()
    {
        botState = BotState.Processing;
        gameObject.GetComponent<Renderer>().material.color = Color.blue;
        dictationRecognizer.Stop();
    }

    /// <summary>
    /// This handler is called every time the Dictation detects a pause in the speech. 
    /// </summary>
    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        // Update UI with dictation captured
        Debug.Log($"User just said: {text}");

        // Send dictation to Bot
        StartCoroutine(SendMessageToBot(text, botId, botName, "message"));
        StopCapturingAudio();
    }

    /// <summary>
    /// Request a conversation with the Bot Service
    /// </summary>
    internal IEnumerator StartConversation()
    {
        string conversationEndpoint = string.Format("{0}/conversations", botEndpoint);

        WWWForm webForm = new WWWForm();

        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(conversationEndpoint, webForm))
        {
            unityWebRequest.chunkedTransfer = false;
            unityWebRequest.SetRequestHeader("Authorization", "Bearer " + botSecret);
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

            yield return unityWebRequest.SendWebRequest();
            string jsonResponse = unityWebRequest.downloadHandler.text;

            conversation = new ConversationObject();
            conversation = JsonConvert.DeserializeObject<ConversationObject>(jsonResponse);
            Debug.Log($"Start Conversation - Id: {conversation.ConversationId}");
            conversationStarted = true;
        }

        // The following call is necessary to create and inject an activity of type //"conversationUpdate" to request a first "introduction" from the Bot Service.
        StartCoroutine(SendMessageToBot("", botId, botName, "conversationUpdate"));
    }

    /// <summary>
    /// Send the user message to the Bot Service in form of activity
    /// and call for a response
    /// </summary>
    private IEnumerator SendMessageToBot(string message, string fromId, string fromName, string activityType)
    {
        Debug.Log($"SendMessageCoroutine: {conversation.ConversationId}, message: {message} from Id: {fromId} from name: {fromName}");

        // Create a new activity here
        Activity activity = new Activity();
        activity.from = new From();
        activity.conversation = new Conversation();
        activity.from.id = fromId;
        activity.from.name = fromName;
        activity.text = message;
        activity.type = activityType;
        activity.channelId = "DirectLineChannelId";
        activity.conversation.id = conversation.ConversationId;

        // Serialize the activity
        string json = JsonConvert.SerializeObject(activity);

        string sendActivityEndpoint = string.Format("{0}/conversations/{1}/activities", botEndpoint, conversation.ConversationId);

        // Send the activity to the Bot
        using (UnityWebRequest www = new UnityWebRequest(sendActivityEndpoint, "POST"))
        {
            //www.chunkedTransfer = false;
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));

            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Bearer " + botSecret);
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            // extrapolate the response Id used to keep track of the conversation
            string jsonResponse = www.downloadHandler.text;
            string cleanedJsonResponse = jsonResponse.Replace("\r\n", string.Empty);
            string responseConvId = cleanedJsonResponse.Substring(10, 30);

            // Request a response from the Bot Service
            StartCoroutine(GetResponseFromBot(activity));
        }
    }

    /// <summary>
    /// Request a response from the Bot by using a previously sent activity
    /// </summary>
    private IEnumerator GetResponseFromBot(Activity activity)
    {
        string getActivityEndpoint = string.Format("{0}/conversations/{1}/activities", botEndpoint, conversation.ConversationId);

        using (UnityWebRequest unityWebRequest1 = UnityWebRequest.Get(getActivityEndpoint))
        {
            //unityWebRequest1.chunkedTransfer = false;
            unityWebRequest1.downloadHandler = new DownloadHandlerBuffer();
            unityWebRequest1.SetRequestHeader("Authorization", "Bearer " + botSecret);

            yield return unityWebRequest1.SendWebRequest();

            string jsonResponse = unityWebRequest1.downloadHandler.text;

            ActivitiesRootObject root = new ActivitiesRootObject();
            root = JsonConvert.DeserializeObject<ActivitiesRootObject>(jsonResponse);

            foreach (var act in root.activities)
            {
                Debug.Log($"Bot Response: {act.text}");
                SetBotResponseText(act.text);
            }

            botState = BotState.ReadyToListen;
            //botMaterial.color = Color.blue;
            gameObject.GetComponent<Renderer>().material.color = Color.blue;

        }
    }

    /// <summary>
    /// Set the UI Response Text of the bot
    /// </summary>
    internal void SetBotResponseText(string responseString)
    {
        if ((responseString.StartsWith("Sorry")) || (responseString == "" )) return;
        //SceneOrganiser.Instance.botResponseText.text = responseString;
        botResponseText.text = responseString;
    }
}
