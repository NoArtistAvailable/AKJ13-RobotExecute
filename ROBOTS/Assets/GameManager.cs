using System;
using System.Collections;
using System.Collections.Generic;
using elZach.Common;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace elZach.Robots
{
    public class GameManager : MonoBehaviour
    {
        static Lazy<GameManager> _instance = new Lazy<GameManager>(FindObjectOfType<GameManager>); 
        public static GameManager Instance => _instance.Value;
        public static bool ApplicationIsQuitting = false;

        private NetSynch _netSynch;
        NetSynch netSynch {get
        {
            if (!_netSynch) _netSynch = GetComponent<NetSynch>();
            return _netSynch;
        }}
        
        public Slider playSlider;
        public Button playButton;
        public Button uploadButton;
        public Robot robotPrefab;
        private Coroutine playRoutine;
        public float gameTime = 30f;
        void Start()
        {
            //Physics.queriesHitTriggers = true;
            playSlider.onValueChanged.AddListener(PlaySliderChanged);
            playButton.onClick.AddListener(Play);
            uploadButton.onClick.AddListener(()=>Pack());
            Application.quitting += () => ApplicationIsQuitting = true;
        }

        private void Play()
        {
            if(playRoutine != null) StopCoroutine(playRoutine);
            playRoutine = StartCoroutine(PlayRoutine());
        }

        public List<Robot> currentRobots = new List<Robot>();

        public static void Register(Robot robot)
        {
            Instance.currentRobots.Add(robot);
            robot.onDestroy += () => Instance.currentRobots.Remove(robot);
        }

        private void PlaySliderChanged(float arg0)
        {
            float currentTime = arg0 * gameTime;
            foreach (var robot in currentRobots)
            {
                robot.rb.MovePosition(robot.path.Evaluate(currentTime));
            }
        }

        IEnumerator PlayRoutine()
        {
            while (playSlider.value < 1f)
            {
                playSlider.value += Time.deltaTime / gameTime;
                yield return null;
            }

            playRoutine = null;
        }

        public Button<GameManager> loadRandomPlanButton = new Button<GameManager>(x=>x.Unpack(x.netSynch.onlinePlans[UnityEngine.Random.Range(0,x.netSynch.onlinePlans.Length)]));
        
        string testJSON;
        public string manufacturerName = "Test Industries";
        public SerializablePlan Pack()
        {
            string manufacturer = manufacturerName;
            List<string> robots = new List<string>();
            foreach(var robot in currentRobots)
                robots.Add(new Robot.SerializableRobot(robot).ToJSON());
            var uploadPlan = new SerializablePlan(manufacturer, robots);
            return uploadPlan;
            // testJSON = uploadPlan.ToJSON();
            // return testJSON;
        }

        public void Unpack(SerializablePlan plan)
        {
            var downloadPlan = plan;
            foreach (var robotString in downloadPlan.robots)
            {
                var serializedRobot = new Robot.SerializableRobot(robotString);
                var newBot = serializedRobot.Spawn(robotPrefab);
                var oldBot = currentRobots.Find(x => x.robotName == newBot.robotName);
                if (oldBot) Destroy(oldBot.gameObject);
            }
        }

        public class SerializablePlan
        {
            public string manufacturer;
            public List<string> robots;

            public SerializablePlan(){}
            
            public SerializablePlan(string manufacturer, List<string> robots)
            {
                this.manufacturer = manufacturer;
                this.robots = robots;
            }

            public SerializablePlan(string json)
            {
                var serialized = JsonUtility.FromJson<SerializablePlan>(json);
                if (serialized == null)
                {
                    Debug.LogError("Couldn't Deserialize Plan");
                    return;
                }
                manufacturer = serialized.manufacturer;
                robots = serialized.robots;
            }

            public string ToJSON()
            {
                return JsonUtility.ToJson(this);
            }
        }

        
    }
}
