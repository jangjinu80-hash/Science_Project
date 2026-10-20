using UnityEngine;

namespace JJW.Code.Player
{
    [CreateAssetMenu(menuName = "JJW/Stage")]
    public sealed class StageDefinition : ScriptableObject
    {
        public int number = 1;
        public string title;
        [TextArea] public string objective;
        [TextArea] public string hint;
        public float supply = 60;
        [Range(1, 3)] public int capacitorCount = 1;
        [TextArea] public string question;
        public bool answer;
        [TextArea] public string explanation;
    }
}
