using Il2Cpp;
using MelonLoader;
using HarmonyLib;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader.Utils;
using System.IO;

namespace MSZDialogueMap
{
    public class Mapper : MelonMod
    {
        DialogueTree[] trees;
        string activeScene;
        bool isGameScene => activeScene == "Version 1.9 POST";


        string savePath = Path.Combine(MelonEnvironment.MelonBaseDirectory, "mapper", "nodes.json");

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            activeScene = sceneName;
            if (!isGameScene) return;
            trees = UnityEngine.Object.FindObjectsOfType<DialogueTree>();
            HashSet<DialogueNode> nodes = new HashSet<DialogueNode>();
            foreach (DialogueTree t in trees)
            {
                foreach(DialogueNode node in t.GetAllNodes())
                {
                    LoggerInstance.Msg(node.dialogueText);
                    nodes.Add(node);
                }
            }
            LoggerInstance.Msg($"Serializing {nodes.Count} nodes...");

            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            string json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
            File.WriteAllText(json, savePath);
            
            LoggerInstance.Msg($"Saved {nodes.Count} nodes to {savePath}");
        }
    }

    public class DialogueNodeDTO
    {
        public string dialogueText;
        public string speakerName;
        public float delay;
        public string[] nextNodeTexts;
    }

    public static class Cool
    {
        public static HashSet<DialogueNode> GetAllNodes(this DialogueTree tree)
        {
            HashSet<DialogueNode> visited = new HashSet<DialogueNode>();

            foreach (DialogueNode firstNode in tree.startNodes)
            {
                TraverseNode(firstNode, visited);
            }

            return visited;
        }
        public static HashSet<DialogueNode> TraverseNode(DialogueNode node, HashSet<DialogueNode> visited)
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
