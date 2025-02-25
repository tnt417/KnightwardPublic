using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using TonyDev.Game.Global.Network;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TonyDev.Game.Global.Console
{
    public class GameConsole : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private GameObject consoleUIObject;
        [SerializeField] private TMP_Text consoleUIText;

        [SerializeField] private TMP_InputField consoleUIInput;
        //

        private readonly StringBuilder _consoleStringBuilder = new StringBuilder();

        private static GameConsole _instance;

        public static bool IsTyping => _instance.consoleUIInput.isFocused;

        private float _sleepTimer;

        private void Awake()
        {
            //Singleton code
            if (_instance != null)
            {
                Destroy(transform.root.gameObject);
                return;
            }
            _instance = this;
            //

            SceneManager.sceneLoaded += OnSceneLoad;

            DontDestroyOnLoad(gameObject.transform.root.gameObject);

            _disabled = true;
            consoleUIObject.SetActive(false);
            
            consoleUIInput.onFocusSelectAll = false;

            consoleUIInput.onSubmit.AddListener(OnTextInput);
            consoleUIInput.onDeselect.AddListener((a) => consoleUIInput.text = ""); //Clear the text on deselect
        }

        private void Start()
        {
            Log("Welcome!");
        }

        private bool _disabled = true;
        
        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenuScene")
            {
                _disabled = true;
                consoleUIObject.SetActive(false);
            }
            else
            {
                _disabled = false;
            }
        }

        private void Update()
        {
            if (_disabled) return;
            
            if (consoleUIObject.activeSelf)
            {
                if (!consoleUIInput.isFocused)
                {
                    _sleepTimer += Time.deltaTime;
                }

                if (_sleepTimer > 3f)
                {
                    consoleUIObject.SetActive(false);
                    _sleepTimer = 0;
                }
            }
        }

        public void OnChat(InputValue value)
        {
            if (!value.isPressed) return;
            
            if (!consoleUIInput.isFocused)
            {
                consoleUIObject.SetActive(true);
                consoleUIInput.Select();
                _sleepTimer = 0;
            }
        }

        public void OnCommand(InputValue value)
        {
            if (value.isPressed && !consoleUIInput.isFocused)
            {
                consoleUIObject.SetActive(true);
                consoleUIInput.ActivateInputField();
                consoleUIInput.text = "/";
                consoleUIInput.MoveToEndOfLine(false, false);
                _sleepTimer = 0;
            }
        }

        private void OnTextInput(string input)
        {
            if (_disabled) return;
            
            consoleUIInput.OnDeselect(new BaseEventData(EventSystem.current));
            consoleUIInput.text = "";

            if (string.IsNullOrEmpty(input)) return;

            if (input.StartsWith("/"))
            {
                var rawInput = input.Trim('/').ToLower();
                foreach (var subCommand in rawInput.Split(","))
                {
                    var repeat = subCommand.Split(" x");
                    var repeatNum = 1;
                    
                    if (repeat.Length == 2)
                    {
                        repeatNum = int.Parse(repeat[1]);
                    }
                    
                    for(var i = 0; i < repeatNum; i++) OnSendCommand(subCommand);
                }
                return;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CmdWriteChatMessage(input, CustomRoomPlayer.Local);
            }
        }

        private readonly Dictionary<string, MethodInfo> _commandMethods = new();

        private void OnSendCommand(string input)
        {
            if (_disabled) return;
            
            var words = input.Split(' ');
            var keyword = words[0];

            if (_commandMethods.Count == 0)
            {
                var assembly = Assembly.Load("TonyDev");
                
                var methods = assembly.GetTypes()
                    .Where(x => x.IsClass)
                    .SelectMany(x => x.GetMethods())
                    .Where(m => m.GetCustomAttributes(typeof(GameCommand), false).FirstOrDefault() != null);
                foreach (var m in methods)
                {
                    if (m.GetCustomAttributes(typeof(GameCommand), false).FirstOrDefault() is GameCommand gc)
                    {
                        _commandMethods.Add(gc.Keyword, m);
                    }
                }
            }

            var method = _commandMethods.ContainsKey(keyword) ? _commandMethods[keyword] : null;

            if (method == null || method.DeclaringType == null)
            {
                Log("<color=red>Invalid command!</color>");
                return;
            }

            var obj = FindObjectOfType(method.DeclaringType); // Instantiate the class

            var paramText = words[1..words.Length];
            var parameters = method.GetParameters();

            var castedParameters = new object[parameters.Length];

            var gameCommand = (GameCommand) method.GetCustomAttributes(typeof(GameCommand), false).FirstOrDefault();

            if (gameCommand?.PermissionLevel == PermissionLevel.Cheat)
            {
                if (GameManager.IsDemo)
                {
                    Log("<color=red>Cheat commands are disabled in the demo!</color>");
                    return;
                }
                
                GameObject.FindWithTag("CheatDisclaimer").GetComponent<TextMeshProUGUI>().enabled = true;
            }
            
            if (paramText.Length < parameters.Length)
            {
                Log("<color=red>Insufficient parameters!</color>");
                return;
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                castedParameters[i] = Convert.ChangeType(paramText[i], paramType);
            }

            method.Invoke(obj, castedParameters); // invoke the method

            var successMessage = gameCommand?.SuccessMessage;

            Log(successMessage);
        }

        public static void Log(string text)
        {
            if (string.IsNullOrEmpty(text) || _instance == null) return;

            _instance._consoleStringBuilder.AppendLine(text);
            _instance.consoleUIText.text = _instance._consoleStringBuilder.ToString();
            
            if (_instance._disabled) return;
            
            _instance._sleepTimer = 0;
            _instance.consoleUIObject.SetActive(true);
        }
    }
}