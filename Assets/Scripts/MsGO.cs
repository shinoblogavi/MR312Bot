using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Windows.Speech;

public class MsGO : MonoBehaviour
{
    public TextMesh botResponseText;
    public GameObject BotCharacter;


    private string botId = "MRBotId";
    private string botName = "MRBotName";
    private string botSecret = "5WiIZhn9Sfg.cwA.zks._QKuxtzV_Pg3p1zFK9bmhA49ipPOkCsyJJi_6eeskKo";
    private string botEndpoint = "https://directline.botframework.com/v3/directline";

    private ConversationObject conversation;

    KeywordRecognizer keywordRecognizer = null;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    void Start()
    {
        BotCharacter.GetComponent<Animator>().Play("Take 001", 0);
        SetBotResponseText("接続中です...");
        //最後に conversationUpdate のメッセージを飛ばします
        StartCoroutine(StartConversation());

        StartCapturingAudio();
    }


    public void StartCapturingAudio()
    {

        keywords.Add("ヘルプ", () => {
            StartCoroutine(SendMessageToBot("ヘルプ", botId, botName, "message"));
        });
        keywords.Add("こんにちは", () => {
            StartCoroutine(SendMessageToBot("こんにちは", botId, botName, "message"));
        });
        keywords.Add("チェック", () => {
            botResponseText.text = "爽健美茶 120円です";
        });
        keywords.Add("購入", () => { botResponseText.text = "リストに追加します"; });
        keywords.Add("お会計", () => {
            StartCoroutine(SendMessageToBot("お会計", botId, botName, "message"));
        });

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
            keywordAction.Invoke();
            //StartCoroutine(SendMessageToBot(args.text, botId, botName, "message"));
            //BotCharacter.GetComponent<Renderer>().material.color = Color.red;
            //BotCharacter.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        }
    }

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
        }

        // The following call is necessary to create and inject an activity of type //"conversationUpdate" to request a first "introduction" from the Bot Service.
        StartCoroutine(SendMessageToBot("", botId, botName, "conversationUpdate"));
    }

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

            //BotCharacter.GetComponent<Renderer>().material.color = Color.blue;
            //BotCharacter.transform.localScale = new Vector3(1f, 1f, 1f);


        }
    }

    internal void SetBotResponseText(string responseString)
    {
        if ((responseString.StartsWith("Sorry")) || (responseString == "")) return;
        botResponseText.text = responseString;
    }
}
