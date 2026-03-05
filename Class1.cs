using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSZDialogueMap
{
    public class Mapper : MelonMod
    {
        DialogueTree[] trees;
        string activeScene;
        bool isGameScene => activeScene == "Version 1.9 POST";

        string savePath = Path.Combine(MelonEnvironment.ModsDirectory, "mapper", "nodes.json");

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            activeScene = sceneName;
            if (!isGameScene) return;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            trees = UnityEngine.Object.FindObjectsOfType<DialogueTree>();
            List<DialogueNode> nodes = new List<DialogueNode>();
            foreach (DialogueTree t in trees)
            {
                foreach(DialogueNode node in t.GetAllNodes())
                {
                    LoggerInstance.Msg($"{node.speakerName}: {node.dialogueText}");
                    nodes.Add(node);
                }
            }
            sw.Stop();
            LoggerInstance.Msg($"Found {nodes.Count} nodes in {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            LoggerInstance.Msg($"Serializing {nodes.Count} nodes...");

            List<DialogueNodeDTO> dtos = nodes.Select(node => new DialogueNodeDTO
            {
                id = nodes.IndexOf(node),
                dialogueText = node.dialogueText,
                speakerName = node.speakerName,
                delay = node.delay,
                nextNodeIds = node.nextNodes?
                     .Where(n => n != null)
                     .Select(n => nodes.IndexOf(n))
                     .ToArray()
            }).ToList();

            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            string json = JsonConvert.SerializeObject(dtos, Formatting.Indented);
            File.WriteAllText(savePath, json);

            sw.Stop();
            LoggerInstance.Msg($"Sucessfuly saved nodes to {savePath}. Duration: {sw.ElapsedMilliseconds}ms");
        }
    }

    public class DialogueNodeDTO
    {
        public int id;
        public int[] nextNodeIds;
        public string dialogueText;
        public string speakerName;
        public float delay;
        public string[] nextNodeTexts;
    }

    public static class Cool
    {
        public static List<DialogueNode> GetAllNodes(this DialogueTree tree)
        {
            List<DialogueNode> visited = new List<DialogueNode>();

            foreach (DialogueNode firstNode in tree.startNodes)
            {
                TraverseNode(firstNode, visited);
            }

            return visited;
        }
        public static List<DialogueNode> TraverseNode(DialogueNode node, List<DialogueNode> visited)
        {
            if (node == null || visited.Contains(node))
                return visited;
            visited.Add(node);

            if (node.nextNodes != null)
            {
                foreach (DialogueNode next in node.nextNodes)
                {
                    TraverseNode(next, visited);
                }
            }
            return visited;
        }
    }
}
