using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class Interactions : MonoBehaviour
{
    /// <summary>
    /// Allows input recognition with the HoloLens
    /// </summary>
    private GestureRecognizer _gestureRecognizer;

    /// <summary>
    /// Called on initialization, after Awake
    /// </summary>
    internal virtual void Start()
    //internal override void Start()
    {
        //base.Start();

        //Register the application to recognize HoloLens user inputs
        _gestureRecognizer = new GestureRecognizer();
        _gestureRecognizer.SetRecognizableGestures(GestureSettings.Tap);
        _gestureRecognizer.Tapped += GestureRecognizer_Tapped;
        _gestureRecognizer.StartCapturingGestures();
    }

    /// <summary>
    /// Detects the User Tap Input
    /// </summary>
    private void GestureRecognizer_Tapped(TappedEventArgs obj)
    {
        // Ensure the bot is being gazed upon.
        //if (base.FocusedObject != null)
        //{
            // If the user is tapping on Bot and the Bot is ready to listen
            //if (base.FocusedObject.name == "Bot" && Bot.Instance.botState == Bot.BotState.ReadyToListen)
            if (Bot.Instance.botState == Bot.BotState.ReadyToListen)
            {
                // If a conversation has not started yet, request one
                if (Bot.Instance.conversationStarted)
                {
                    Bot.Instance.SetBotResponseText("お話ください...");
                    Bot.Instance.StartCapturingAudio();
                }
                else
                {
                    Bot.Instance.SetBotResponseText("接続中です...");
                    StartCoroutine(Bot.Instance.StartConversation());
                }
            }
        //}
    }
}
