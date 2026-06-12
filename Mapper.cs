using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using MZDO;

namespace MSZDialogueMap
{
    public class Mapper : MelonMod
    {
        DialogueTree[] trees;
        readonly string savePath = Path.Combine(MelonEnvironment.ModsDirectory, "mapper", "nodes.json");

        public override void OnInitializeMelon()
        {
            MZDO.Core.UserPacksEnabled = false;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName != "Version 1.9 POST") return;

            Stopwatch sw = Stopwatch.StartNew();
            trees = GameObject.FindObjectsOfType<DialogueTree>();

            DialoguePack pack = new DialoguePack
            {
                TargetGameVersion = Application.version,
                trees = new List<DialogueTreeDTO>()
            };
            pack.MatchGameVersion();

            foreach (DialogueTree tree in trees)
            {
                List<DialogueNode> treeNodes = tree.GetAllNodes();
                int treeIndex = pack.trees.Count;

                List<DialogueNodeDTO> dtos = treeNodes.Select((node, i) =>
                {
                    LoggerInstance.Msg($"{node.speakerName}: {node.dialogueText}");
                    return new DialogueNodeDTO
                    {
                        id = i,
                        dialogueText = node.dialogueText,
                        speakerName = node.speakerName,
                        delay = node.delay,
                        nextNodeIds = node.nextNodes?
                            .Where(n => n != null)
                            .Select(n => treeNodes.IndexOf(n))
                            .ToArray(),
                        expression = node.expression
                    };
                }).ToList();

                pack.trees.Add(new DialogueTreeDTO
                {
                    nodes = dtos,
                    startNodeIds = tree.startNodes
                        .ToArray()
                        .Where(n => n != null)                        
                        .Select(n => treeNodes.IndexOf(n))
                        .ToList(),
                    chirpTime = tree.chirpTime,
                    exitDelay = tree.exitDelay,
                    initialDelay = tree.initialDelay,
                    name = tree.gameObject.name
                });

                LoggerInstance.Msg($"Tree {treeIndex}: {dtos.Count} nodes");
            }

            sw.Stop();
            LoggerInstance.Msg($"Mapped {pack.trees.Sum(t => t.nodes.Count)} nodes across {pack.trees.Count} trees in {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            string json = JsonConvert.SerializeObject(pack, Formatting.Indented);
            File.WriteAllText(savePath, json);
            sw.Stop();
            LoggerInstance.Msg($"Saved to {savePath} in {sw.ElapsedMilliseconds}ms");
        }
    }
}