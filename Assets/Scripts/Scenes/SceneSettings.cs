using UnityEngine;

namespace JammerDash.Unused
{

    [CreateAssetMenu(fileName = "SceneSettings", menuName = "ScriptableObjects/SceneSettings", order = 1)]
    public class SceneSettings : ScriptableObject
    {
        public Vector3 cubePosition;
        public Vector3 sawPosition;
        public Vector3 goodPosition;
        public Vector3 badPosition;
    }
}