using System;
using System.Collections;
using System.Collections.Generic;
using elZach.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Linq;
using UnityEngine.Events;

namespace elZach.Robots
{
    public class GameManager : MonoBehaviour
    {
        static Lazy<GameManager> _instance = new Lazy<GameManager>(FindObjectOfType<GameManager>); 
        public static GameManager Instance => _instance.Value;
        public static bool ApplicationIsQuitting = false;
        public static bool PlayerCanInteract = true;

        private NetSynch _netSynch;
        NetSynch netSynch {get
        {
            if (!_netSynch) _netSynch = GetComponent<NetSynch>();
            return _netSynch;
        }}

        public static string ManufacturerName = "Player Incorporated";
        public string manufacturerName = "Test Industries";
        public Slider playSlider;
        public Button playButton;
        public Button pauseButton;
        public Button uploadButton;
        public Robot robotPrefab;

        public Text playAgainstText;

        public Button<GameManager> playAgainstFadeInTest =
            new Button<GameManager>(x => x.playAgainstText.GetComponent<Animatable>().Play(1));
        public Button<GameManager> playAgainstFadeOutest =
            new Button<GameManager>(x => x.playAgainstText.GetComponent<Animatable>().Play(0));
        
        private Coroutine playRoutine;
        public float gameTime = 30f;

        public UnityEvent<string> OnWinMessage;

        public Dictionary<Robot.Faction, SerializablePlan> factionPlayers = new Dictionary<Robot.Faction, SerializablePlan>();

        void OnEnable()
        {
            manufacturerName = ManufacturerName;
        }
        
        void Start()
        {
            //Physics.queriesHitTriggers = true;
            playSlider.onValueChanged.AddListener(PlaySliderChanged);
            playButton.onClick.AddListener(Play);
            pauseButton.onClick.AddListener(Pause);
            uploadButton.onClick.AddListener(()=>Pack());
            Application.quitting += () => ApplicationIsQuitting = true;
        }

        private void Pause()
        {
            if(playRoutine != null) StopCoroutine(playRoutine);
            playRoutine = null;
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

        public void PlaySliderChanged(float arg0)
        {
            float currentTime = arg0 * gameTime;
            foreach (var robot in currentRobots)
            {
                if (!robot.living)
                {
                    if(robot.timeOfDeath < currentTime)
                        continue;
                    else
                    {
                        robot.Revive();
                    }
                }
                if (robot.path != null && robot.path.pathPoints.Count > 0)
                {
                    robot.transform.position = robot.path.Evaluate(currentTime, out var dir, out var state);
                    robot.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                    bool stateChange = robot.state != state;
                    robot.state = state;
                    if(stateChange) robot.GetComponentInChildren<VisionCone>().GetComponent<Animatable>().Play(state == PathPoint.PathAction.TakeAim ? 1 : 0);
                }
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

        public void PlayAll()
        {
            StartCoroutine(PlayAllEnemies());
        }

        private int plays = 0, wins = 0;
        IEnumerator PlayAllEnemies()
        {
            plays = 0;
            wins = 0;
            foreach (var valid in validPlans)
                yield return PlayForReal(valid);
            
            var textAnim = playAgainstText.GetComponent<Animatable>();
            playAgainstText.text = $"{manufacturerName} won {wins} of {plays} rounds!";
            textAnim.Play(1);
            // yield return new WaitForSeconds(5f);
            // textAnim.Play(0);
        }

        IEnumerator PlayForReal(SerializablePlan enemyPlan)
        {
            Unpack(enemyPlan);
            plays++;
            var textAnim = playAgainstText.GetComponent<Animatable>();
            playAgainstText.text = $"Playing {enemyPlan.manufacturer}";
            textAnim.Play(1);
            yield return new WaitForSeconds(3f);
            textAnim.Play(0);
            // playSlider.interactable = false;
            PlayerCanInteract = false;
            playSlider.value = 0f;
            while (playSlider.value < 1f)
            {
                playSlider.value += Time.deltaTime / gameTime;
                yield return null;
            }
            
            var winnerFaction = WinCheck.Instance.Check();
            string winnerText;
            if (winnerFaction == PathPlaner.Instance.currentFaction)
            {
                winnerText = $"{manufacturerName} won the round!";
                wins++;
            }
            else
            {
                winnerText = $"{factionPlayers[winnerFaction].manufacturer} won the round!";
            }
            OnWinMessage.Invoke(winnerText);
            playAgainstText.text = winnerText;
            textAnim.Play(1);
            yield return new WaitForSeconds(5f);
            textAnim.Play(0);
        }

        public Button<GameManager> loadValidEnemies = new Button<GameManager>(x=>x.LoadValidEnemyPlans());
        //(x => x.Unpack(x.netSynch.onlinePlans[UnityEngine.Random.Range(0, x.netSynch.onlinePlans.Length)]));
        public Button<GameManager> loadAValidPlan =
            new Button<GameManager>(x => x.GetSampleEnemy());

        public List<SerializablePlan> validPlans;
        public void LoadValidEnemyPlans()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            int currentFaction = (int) PathPlaner.Instance.currentFaction;
            validPlans = NetSynch.onlinePlans.ToList()
                .FindAll(x => x.scene == sceneName && x.faction != currentFaction);
        }

        public void GetSampleEnemy()
        {
            Unpack(validPlans[UnityEngine.Random.Range(0, validPlans.Count)]);
        }
        
        string testJSON;
        
        public SerializablePlan Pack()
        {
            string manufacturer = manufacturerName;
            List<string> robots = new List<string>();
            foreach (var robot in currentRobots)
            {
                if(robot.faction == PathPlaner.Instance.currentFaction)
                    robots.Add(new Robot.SerializableRobot(robot).ToJSON());
            }

            var uploadPlan = new SerializablePlan(SceneManager.GetActiveScene().name, PathPlaner.Instance.currentFaction, manufacturer, robots);
            return uploadPlan;
            // testJSON = uploadPlan.ToJSON();
            // return testJSON;
        }

        public void Unpack(SerializablePlan plan)
        {
            var downloadPlan = plan;
            Robot.Faction faction = (Robot.Faction) downloadPlan.faction;
            foreach (var robotString in downloadPlan.robots)
            {
                var serializedRobot = new Robot.SerializableRobot(robotString);
                var newBot = serializedRobot.Spawn(robotPrefab, faction);
                // newBot.faction = faction;
                var oldBot = currentRobots.Find(x => x.robotName == newBot.robotName && x.faction == faction);
                if (oldBot) Destroy(oldBot.gameObject);
            }

            if (!factionPlayers.ContainsKey(faction)) factionPlayers.Add(faction, downloadPlan);
            else factionPlayers[faction] = downloadPlan;
        }

        public class SerializablePlan
        {
            public string scene;
            public int faction;
            public string manufacturer;
            public List<string> robots;

            public SerializablePlan(){}
            
            public SerializablePlan(string scene, Robot.Faction faction, string manufacturer, List<string> robots)
            {
                this.scene = scene;
                this.faction = (int) faction;
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

                scene = serialized.scene;
                faction = serialized.faction;
            }

            public string ToJSON()
            {
                return JsonUtility.ToJson(this);
            }
        }

        
    }
}
