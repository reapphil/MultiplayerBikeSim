using UnityEngine;

namespace Common
{
    public class LevelSettings : MonoBehaviour
    {
        public Transform[] SpawnPoints;
        
        private static LevelSettings _instance;
        public static LevelSettings Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType<LevelSettings>();
                }

                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance)
            {
                Destroy(this);
                return;
            }
            _instance = this;
        }
    }
}