using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotObjects : MonoBehaviour { }

/// <summary>
/// Object received when first opening a conversation
/// </summary>
[Serializable]
public class ConversationObject
{
    public string ConversationId;
    public string token;
    public string expires_in;
    public string streamUrl;
    public string referenceGrammarId;
}

/// <summary>
/// Object including all Activities
/// </summary>
[Serializable]
public class ActivitiesRootObject
{
    public List<Activity> activities { get; set; }
    public string watermark { get; set; }
}
[Serializable]
public class Conversation
{
    public string id { get; set; }
}
[Serializable]
public class From
{
    public string id { get; set; }
    public string name { get; set; }
}
[Serializable]
public class Activity
{
    public string type { get; set; }
    public string channelId { get; set; }
    public Conversation conversation { get; set; }
    public string id { get; set; }
    public From from { get; set; }
    public string text { get; set; }
    public string textFormat { get; set; }
    public DateTime timestamp { get; set; }
    public string serviceUrl { get; set; }
}