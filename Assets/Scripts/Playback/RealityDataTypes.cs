using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct RealityFrame 
{
    public Vector3 position;
    public Quaternion rotation;
    public float timestamp;

    public RealityFrame(Transform t, float time) 
    {
        position = t.position;
        rotation = t.rotation;
        timestamp = time;
    }
}

[System.Serializable]
public class ActorTrack 
{
    public string actorID; 
    public RecordedActorType actorType = RecordedActorType.Unknown;

    [Header("Optional Visual Metadata")]
    public List<RealityFrame> frames = new List<RealityFrame>();

    public ActorTrack(string id) 
    {
        actorID = id;
    }

    public ActorTrack(string id, RecordedActorType type)
    {
        actorID = id;
        actorType = type;
    }
}

[System.Serializable]
public enum RecordedActorType
{
    Unknown,
    Player,
    NonPlayer
}

[System.Serializable]
public enum DramaType { None, GiveItem, StealItem, CaughtByCamera, Argument }

[System.Serializable]
public class DramaEvent 
{
    [Header("Event Metadata")]
    public DramaType type;
    public float timestamp;
    public Vector3 worldPosition; // Where the "Action" happened
    
    [Header("Participants")]
    public string actorID;        // The "Star" of the clip
    public string victimID;       // The "Target" (if applicable)

    // Stable identifiers (preferred over actorID/victimID strings when available)
    public int actorIndex;        // PlayerIdentity.playerIndex or -1
    public int victimIndex;       // PlayerIdentity.playerIndex or -1

    [Header("Item")]
    public Resource transferredResource; // If the transferred item is a ResourceItem, we store which one.
    
    [Header("TV Production Data")]
    public int scoreImpact;       // How much this changed the game
    public float dramaIntensity;  // 0-10 scale to help the editor pick "Best Clips"

    public DramaEvent(DramaType type, float timestamp, Transform location, string actor, string victim, int actorIndex, int victimIndex, Resource transferredResource, int score,  float intensity)
    {
        this.type = type;
        this.timestamp = timestamp;
        this.worldPosition = location.position;
        this.actorID = actor;
        this.victimID = victim;
        this.actorIndex = actorIndex;
        this.victimIndex = victimIndex;
        this.transferredResource = transferredResource;
        this.scoreImpact = score;
        this.dramaIntensity = intensity;
    }
}