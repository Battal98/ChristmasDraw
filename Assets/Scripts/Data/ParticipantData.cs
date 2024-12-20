using Unity.Burst.Intrinsics;
using UnityEngine;

[CreateAssetMenu(fileName = "ParticipantsData", menuName = "MyAssets/ParticipantsData", order = 0)]
public class ParticipantData : ScriptableObject
{
    [System.Serializable]
    public class Participant
    {
        public string name;
        public string email;
    }

    public Participant[] participants;
}
