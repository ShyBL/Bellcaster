using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace MaravStudios.DialogueSystem
{
    using static DialogueSystemInterpreter;
    public class EditorDialogueSysGraphView : GraphView
    {

        public InterpretationXL lines;
        public DialogueSystemSetting character;
        public bool AutoSave;
        TextAsset selectedAsset;
        public EditorDialogueSysGraphView()
        {

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            graphViewChanged += OnGraphViewChanged;

            GridBackground LaGrid = new GridBackground();
            LaGrid.AddToClassList("rootGraphVisualElement");
            Insert(0, LaGrid);
            LaGrid.StretchToParentSize();

            // Creamos los nodos iniciales

            // EditorDialogueSystemStyleNode
            // Buscamos la refecencia del characters
            string[] guids = AssetDatabase.FindAssets("t:DialogueSystemSetting");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                character = AssetDatabase.LoadAssetAtPath<DialogueSystemSetting>(path);
            }
            guids = AssetDatabase.FindAssets("EditorDialogueSysStyle_cduioouisdj");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                styleSheets.Add(styleSheet);
                // Debug.Log("cargó el css");
            }

        }

        public void CreateNode(NodeType tipo)
        {
            Vector2 posicion = -viewTransform.position * (1 / viewTransform.scale.x);
            var newNode = new EditorDialogueSysNode(tipo, Guid.NewGuid().ToString(), new InterpretationXL.DialogueLines(), character, this);
            newNode.SetPosition(new Rect(posicion, new Vector2(500, 500)));
            AddElement(newNode);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePort = new List<Port>();
            ports.ForEach(funcCall: port =>
            {
                if (startPort != port && startPort.node != port.node)
                {
                    compatiblePort.Add(port);
                }
            });
            return compatiblePort;
        }
        public void UpdateAll()
        {
            foreach (var item in nodes)
            {
                var nodo = item as EditorDialogueSysNode;

                nodo.UpdateData();
            }
        }
        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            UpdateAll();
            if (AutoSave && selectedAsset != null)
            {
                SaveDialogueFile(selectedAsset);
            }

            return change;
        }

        public void LoadDialogueFile(TextAsset textAsset)
        {
            character.languageSelect = 0;
            selectedAsset = textAsset;
            RemoveAllElement();
            lines = new DialogueSystemInterpreter().Interpretar(textAsset);

            Vector2 sizeNode = new Vector2(300, 200);

            // Creamos los nodos
            foreach (var item in lines.lines)
            {
                string i = item.action.Trim();

                if (i == NodeType.Start.ToString())
                {
                    EditorDialogueSysNode NodoStart = new EditorDialogueSysNode(NodeType.Start, item.GUID, item, character);
                    NodoStart.dialogueLine.output_GUID = item.output_GUID;
                    NodoStart.SetPosition(new Rect(item.position, sizeNode));
                    AddElement(NodoStart);
                }
                else if (i == NodeType.Dialogue.ToString())
                {  // Dialogo //
                    EditorDialogueSysNode node = new EditorDialogueSysNode(NodeType.Dialogue, item.GUID, item, character);
                    node.dialogueLine.output_GUID = item.output_GUID;
                    node.dialogueLine = item;
                    node.dialogueNode.CharacterSelector.value = item.characterId;
                    node.SetPosition(new Rect(item.position, sizeNode));

                    AddElement(node);
                }
                else if (i == NodeType.Switch.ToString())
                {  // Dialogo //
                    EditorDialogueSysNode node = new EditorDialogueSysNode(NodeType.Switch, item.GUID, item, character);
                    node.dialogueLine.output_GUID = item.output_GUID;
                    node.dialogueLine = item;
                    foreach (var item1 in node.dialogueLine.Switch_outputs_GUIDs)
                    {
                        node.AddNewCaseOnSwitch(item1);
                    }
                    node.SetPosition(new Rect(item.position, sizeNode));
                    AddElement(node);
                }
                else
                { // Genericos 

                    EditorDialogueSysNode node = new EditorDialogueSysNode((NodeType)Enum.Parse(typeof(NodeType), item.action), item.GUID, item, character);
                    node.dialogueLine.output_GUID = item.output_GUID;
                    node.dialogueLine = item;
                    node.SetPosition(new Rect(item.position, sizeNode));

                    AddElement(node);
                }

            }
            // conectamos los nodos

            foreach (var item in nodes)
            {
                var nodo = item as EditorDialogueSysNode;

                if (nodo.type == NodeType.Switch)
                {// conectamos  Switches

                    for (int i = 0; i < nodo.outPortsSwitch.Count; i++)
                    {

                        var nextnode = SearchNode(nodo.dialogueLine.Switch_outputs_GUIDs[i].GUID);
                        if (nextnode != null)
                            ConnectNodes(nodo.outPortsSwitch[i].outPortSwitch, nextnode.inputPort);
                    }
                }
                else // conectamos  nodos
                {
                    var nextnode = SearchNode(nodo.dialogueLine.output_GUID);
                    if (nextnode != null)
                        ConnectNodes(nodo.outPort, nextnode.inputPort);
                }
            }
            //NodoStart.SendData(new InterpretationXL.DialogueLines());

            UpdateAll();
        }
        public EditorDialogueSysNode SearchNode(string guid)
        {
            foreach (var item in nodes)
            {
                var nodo = item as EditorDialogueSysNode;
                if (nodo.dialogueLine.GUID == guid)
                {
                    return nodo;
                }
            }
            return null;
        }
        static public void ConnectNodes(Port output, Port input)
        {
            if (output == null || input == null) return;
            // Crear un nuevo Edge
            UnityEditor.Experimental.GraphView.Edge edge = new UnityEditor.Experimental.GraphView.Edge
            {
                output = output, // Asignar el puerto de salida
                input = input    // Asignar el puerto de entrada
            };

            edge.output.Connect(edge);
            edge.input.Connect(edge);

            output.node.GetFirstAncestorOfType<GraphView>().Add(edge);
        }
        void RemoveAllElement()
        {
            foreach (var item in graphElements.ToList())
            {
                RemoveElement(item);
            }
        }
        public void UnLoadDialogueFile()
        {
            selectedAsset = null;
            RemoveAllElement();
            lines.lines.Clear();

        }
        public void SaveDialogueFile(TextAsset xmlFile)
        {
            //  tomamos interpretacion xl
            InterpretationXL interpretacionXL = new InterpretationXL();
            interpretacionXL.lines = new List<InterpretationXL.DialogueLines>();
            foreach (var item in nodes)
            {
                var nodo = item as EditorDialogueSysNode;
                nodo.UpdateData();
                InterpretationXL.DialogueLines line = nodo.dialogueLine;
                line.action = nodo.type.ToString();
                interpretacionXL.lines.Add(line);
            }

            Save(interpretacionXL, xmlFile);

        }




        // //////////////////////////////////////////////   Click derecho  /////////////////////////////////////////////
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            Vector2 posicion = (new Vector2(-viewTransform.position.x, -viewTransform.position.y) + evt.localMousePosition) * (1 / viewTransform.scale.x);// evt.mousePosition; 
                                                                                                                                                          // Debug.Log(posicion.ToString());

            // A�adir una opci�n b�sica
            evt.menu.InsertAction(0, "Add Start", action => CreateNode(posicion, NodeType.Start));
            evt.menu.InsertAction(1, "Add Dialogue", action => CreateNode(posicion, NodeType.Dialogue));
            evt.menu.InsertAction(2, "Add Switch", action => CreateNode(posicion, NodeType.Switch));
            evt.menu.InsertSeparator(null, 3);

            evt.menu.InsertAction(4, "Add Trigger", action => CreateNode(posicion, NodeType.Trigger));
            evt.menu.InsertAction(5, "Add Animation", action => CreateNode(posicion, NodeType.Animation));

            evt.menu.InsertAction(6, "Hide/Add Hide Left", action => CreateNode(posicion, NodeType.HideLeft));
            evt.menu.InsertAction(6, "Hide/Add Hide Right", action => CreateNode(posicion, NodeType.HideRight));

            evt.menu.InsertSeparator(null, 7);
        }

        private void CreateNode(Vector2 position, NodeType nodeType)
        {
            var newNode = new EditorDialogueSysNode(nodeType, Guid.NewGuid().ToString(), new InterpretationXL.DialogueLines(), character, this);
            newNode.SetPosition(new Rect(position, new Vector2(500, 500)));
            AddElement(newNode);
        }


    }
}