﻿using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject settings;

    public void StartSectorCreator()
    {
        SceneManager.LoadScene("SectorCreator");
    }

    public static void StartGame(bool nullifyTestJsonPath = false)
    {
        if (nullifyTestJsonPath)
        {
            SectorManager.testJsonPath = null;
            SectorManager.testResourcePath = null;
        }

        SceneManager.LoadScene("SampleScene");
    }

    [SerializeField]
    private InputField addressField;
    
    [SerializeField]
    private InputField portField;
    [SerializeField]
    private InputField nameField;
    [SerializeField]
    private InputField blueprintField;

    public void Start()
    {
       var args = GetCommandlineArgs();
       if (args.TryGetValue("-mode", out string mode))
        {
            switch (mode)
            {
                case "server":
                    NetworkDuel(MasterNetworkAdapter.NetworkMode.Server);
                    break;
            }
        }
    }

    private Dictionary<string, string> GetCommandlineArgs()
    {
        Dictionary<string, string> argDictionary = new Dictionary<string, string>();

        var args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; ++i)
        {
            var arg = args[i].ToLower();
            if (arg.StartsWith("-"))
            {
                var value = i < args.Length - 1 ? args[i + 1].ToLower() : null;
                value = (value?.StartsWith("-") ?? false) ? null : value;

                argDictionary.Add(arg, value);
            }
        }
        return argDictionary;
    }

    public void NetworkDuel(string mode)
    {
        switch (mode)
        {
            case "server":
                NetworkDuel(MasterNetworkAdapter.NetworkMode.Server);
                break;
            case "client":
                NetworkDuel(MasterNetworkAdapter.NetworkMode.Client);
                break;
            case "host":
                NetworkDuel(MasterNetworkAdapter.NetworkMode.Host);
                break;
        }
    }

    public void NetworkDuel(MasterNetworkAdapter.NetworkMode mode)
    {
        MasterNetworkAdapter.port = portField.text;
        MasterNetworkAdapter.address = addressField.text;
        MasterNetworkAdapter.blueprint = blueprintField.text;
        MasterNetworkAdapter.playerName = nameField.text;
        if (mode != MasterNetworkAdapter.NetworkMode.Client)
        {
            var world = "test";
            var path = System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors", world);
            if (!System.IO.Directory.Exists(path)) return;
            if (mode == MasterNetworkAdapter.NetworkMode.Host)
                MasterNetworkAdapter.StartHost();
            else MasterNetworkAdapter.StartServer();
            NetworkManager.Singleton.OnClientConnectedCallback += (u) => 
            { 
                MasterNetworkAdapter.instance.GetWorldNameClientRpc(world, u);
            };
            WCWorldIO.LoadTestSave(path, true);
        }
        else
        {
            MasterNetworkAdapter.StartClient();
            SystemLoader.AllLoaded = false;
        }
    }

    public void OpenSettings()
    {
        if (settings)
        {
            settings.GetComponentInChildren<GUIWindowScripts>().ToggleActive(true);
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/TXaenta");
    }

    public void OpenTwitter()
    {
        Application.OpenURL("https://twitter.com/rudderbucky");
    }

    public void StartWorldCreator()
    {
        WCGeneratorHandler.DeleteTestWorld();
        SceneManager.LoadScene("WorldCreator");
    }
}
