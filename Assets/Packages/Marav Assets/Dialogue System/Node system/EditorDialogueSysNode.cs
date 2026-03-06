using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MaravStudios.DialogueSystem
{
    using static DialogueSystemInterpreter.InterpretationXL.DialogueLines;
    using static DialogueSystemInterpreter;


    public class EditorDialogueSysNode : Node
    {


        // var generales
        DialogueSystemSetting character;


        public bool EntryPoint = false;
        public Port inputPort, outPort;
        public InterpretationXL.DialogueLines dialogueLine = new InterpretationXL.DialogueLines();

        public NodeType type;

        // var Dialogue Node
        public VarDialogueNode dialogueNode = new VarDialogueNode();
        [Serializable]
        public class VarDialogueNode
        {

            public PopupField<string> CharacterSelector;
            public PopupField<string> variantSpriteCharacterSelector;
            public Image CharacterImage = new Image();
            public List<TextField> text = new List<TextField>();
        }
        // switch
        public List<VarSwitchNode> outPortsSwitch = new List<VarSwitchNode>();
        [Serializable]
        public class VarSwitchNode
        {
            public Port outPortSwitch;
            public List<TextField> text = new List<TextField>();
        }

        // //////////////////////
        // /////////////////////////////////////////////////////////////////////////////  Creación y logica
        // //////////////////////
        public EditorDialogueSysNode(NodeType nodeType, string guid, InterpretationXL.DialogueLines dL, DialogueSystemSetting ch, EditorDialogueSysGraphView thix = null)
        {
            character = ch;

            dialogueLine.GUID = guid;
            title = nodeType.ToString();
            type = nodeType;
            EntryPoint = false;


            // Espacion inicial
            VisualElement space = new VisualElement();
            space.style.height = 1;
            extensionContainer.Add(space);

            switch (nodeType)
            {
                case NodeType.Dialogue: // Nodo Dialogue  
                    contentContainer.AddToClassList("NodeDialogue");
                    VisualElement configDiv = new VisualElement(); configDiv.AddToClassList("configDiv");
                    VisualElement textDiv = new VisualElement(); textDiv.AddToClassList("textDiv");
                    VisualElement div1 = new VisualElement(); configDiv.AddToClassList("div");
                    VisualElement div2 = new VisualElement(); configDiv.AddToClassList("div");
                    extensionContainer.AddToClassList("extensionContainer");
                    Herarquizacion();
                    //ClearAll();
                    void ClearAll()
                    {
                        configDiv.Clear();
                        textDiv.Clear();
                        div1.Clear();
                        div2.Clear();
                        extensionContainer.Clear();
                    }
                    void Herarquizacion()
                    {
                        configDiv.Add(div1);
                        configDiv.Add(div2);
                        extensionContainer.Add(configDiv);
                        extensionContainer.Add(textDiv);
                    }

                    // creamos los puertos
                    inputPort = GeneratedPort(Direction.Input, Port.Capacity.Multi);
                    inputPort.portName = "Input";
                    inputPort.portColor = DialogueSystemNodePortColor.Dialogue;
                    inputContainer.Add(inputPort);

                    outPort = GeneratedPort(Direction.Output);
                    outPort.portName = "output";
                    outPort.portColor = DialogueSystemNodePortColor.Dialogue;
                    outputContainer.Add(outPort);

                    space.style.height = 20;
                    // Hacemos la lista de los characters
                    List<string> ids = new List<string>();
                    ids.Add("...");
                    foreach (var item in ch.characterList)
                    {
                        ids.Add(item._name);
                    }
                    dialogueNode.CharacterSelector = new PopupField<string>("Select Character", ids, 0);
                    dialogueNode.CharacterSelector.RegisterValueChangedCallback(evt =>
                    {
                        dialogueLine.characterId = evt.newValue;
                        AfterSelectCharacter(evt.newValue);
                    });

                    div2.Add(dialogueNode.CharacterSelector);

                    // en el caso de que haya informacion que cargar, Lo cargamos 
                    if (dL.characterId != null)
                    {
                        dialogueLine = dL;
                        AfterSelectCharacter(dL.characterId);
                    }
                    void AfterSelectCharacter(string id)
                    {
                        // primero limpiamos
                        ClearAll();
                        Herarquizacion();
                        div2.Add(dialogueNode.CharacterSelector);
                        if (id == "...") return;



                        // separamos el character para hacerlo mas facil de trabajar
                        DialogueSystemSetting.Character ch_select = new DialogueSystemSetting.Character();
                        ch_select = ch.GetCharacterFromId(id);

                        // bregamos la variante y la imagen del character que aparece en pantalla
                        List<string> ids = new List<string>();
                        ids.Add("sprite Defaul");
                        foreach (var item in ch_select.variantsSprite) { ids.Add(item._name); }
                        dialogueNode.variantSpriteCharacterSelector = new PopupField<string>("variantSprite", ids, 0);
                        dialogueNode.variantSpriteCharacterSelector.choices = ids;
                        dialogueNode.variantSpriteCharacterSelector.value = dialogueLine.variantSpriteId;
                        dialogueNode.variantSpriteCharacterSelector.RegisterValueChangedCallback(evt =>
                        {
                            dialogueLine.variantSpriteId = evt.newValue;
                            dialogueNode.CharacterImage.sprite = ch_select.GetvariantsSprite(evt.newValue);
                        });


                        // Ahora trabajamos con la variante del sprite

                        div2.Add(dialogueNode.variantSpriteCharacterSelector);
                        dialogueNode.CharacterImage.sprite = ch_select.GetvariantsSprite(dialogueNode.variantSpriteCharacterSelector.value);

                        div1.Add(dialogueNode.CharacterImage);

                        // la animacion de ese dialogo
                        List<string> AnimationList = new List<string>();
                        AnimationList.Add("Defaul");
                        foreach (var item in character.charactersAnimations) { AnimationList.Add(item.name.Replace("DialogueSystemAnimation_", "").Trim().ToString()); }
                        PopupField<string> animaciones = new PopupField<string>("Animation", AnimationList, 0);
                        if (dL.animation != null)
                        {
                            animaciones.value = dL.animation;
                            dialogueLine.animation = dL.animation;
                        }

                        animaciones.RegisterValueChangedCallback(evt =>
                        {
                            dialogueLine.animation = evt.newValue;
                        });

                        div2.Add(animaciones);
                        // El nombre del personaje

                        textDiv.Add(new Label(ch_select._name));

                        textDiv.Add(new Label("Position Defaul: " + ch_select.positionDefaul.ToString()));
                        // cuadro de texto
                        if (dialogueLine.translations.Count > 0)
                        { // para cargar
                            foreach (var item in character.languageList)
                            {

                                TextField textField = new TextField() { name = item, value = dialogueLine.Traduccion(item, dialogueLine.translations).text };
                                textField.RegisterValueChangedCallback(evt =>
                                {
                                    dialogueLine.Traduccion(item, dialogueLine.translations).text = evt.newValue;
                                });
                                textField.multiline = true;
                                dialogueNode.text.Add(textField);
                                textDiv.Add(textField);
                            }
                        }
                        else
                        { // para crear

                            foreach (var item in ch.languageList)
                            {
                                TextField textField = new TextField() { name = item, value = "" };
                                textField.multiline = true;
                                textField.RegisterValueChangedCallback(evt =>
                                {
                                    dialogueLine.Traduccion(item, dialogueLine.translations).text = evt.newValue;
                                });
                                dialogueNode.text.Add(textField);
                                textDiv.Add(textField);
                            }
                        }
                        //UpdateData();

                    }



                    break;
                case NodeType.Switch:

                    contentContainer.AddToClassList("NodeSwitch");
                    // creamos los puertos
                    inputPort = GeneratedPort(Direction.Input, Port.Capacity.Multi);
                    inputPort.portName = "Input";
                    inputPort.portColor = DialogueSystemNodePortColor.Switch;
                    inputContainer.Add(inputPort);

                    outputContainer.Add(new Button(clickEvent: () => { AddNewCaseOnSwitch(null); }) { text = "Add Option" });

                    break;
                case NodeType.Start: // Nodo start Creación
                    outPort = GeneratedPort(Direction.Output);
                    outPort.portName = "";
                    outPort.portColor = DialogueSystemNodePortColor.start;
                    outputContainer.Add(outPort);
                    contentContainer.AddToClassList("NodeStart");

                    break;
                case NodeType.Animation: // Nodo start Creación
                    contentContainer.AddToClassList("Node" + type.ToString());
                    inputPort = GeneratedPort(Direction.Input, Port.Capacity.Multi);
                    inputPort.portName = "Input";
                    inputPort.portColor = DialogueSystemNodePortColor.Animations;
                    inputContainer.Add(inputPort);

                    outPort = GeneratedPort(Direction.Output);
                    outPort.portName = "Output";
                    outPort.portColor = DialogueSystemNodePortColor.Animations;
                    outputContainer.Add(outPort);
                    DropdownField orientacion = new DropdownField()
                    {
                        label = "Orientation",
                        value = "Right",
                        choices = { "Right", "Left" }
                    };
                    dialogueLine.orientation = "Right";
                    orientacion.RegisterValueChangedCallback(evt =>
                    {
                        dialogueLine.orientation = evt.newValue;
                    });
                    DropdownField animaciones = new DropdownField()
                    {
                        label = "Animation",
                        value = "Defaul",
                        choices = { "Defaul" }
                    };
                    dialogueLine.animation = "Defaul";
                    animaciones.RegisterValueChangedCallback(evt =>
                    {
                        dialogueLine.animation = evt.newValue;
                    });
                    foreach (var item in character.charactersAnimations)
                    {
                        animaciones.choices.Add(item.name.Replace("DialogueSystemAnimation_", "").Trim().ToString());
                    }
                    if (dL.orientation != null)
                        orientacion.value = dL.orientation;
                    if (dL.animation != null)
                        animaciones.value = dL.animation;
                    extensionContainer.Add(orientacion);
                    extensionContainer.Add(animaciones);

                    break;
                default: // Nodo generico
                    contentContainer.AddToClassList("Node" + type.ToString());
                    inputPort = GeneratedPort(Direction.Input, Port.Capacity.Multi);
                    inputPort.portName = "Input";
                    inputPort.portColor = DialogueSystemNodePortColor.others;
                    inputContainer.Add(inputPort);

                    outPort = GeneratedPort(Direction.Output);
                    outPort.portName = "Output";
                    outPort.portColor = DialogueSystemNodePortColor.others;
                    outputContainer.Add(outPort);

                    break;
            }

            RefreshExpandedState();
            RefreshPorts();

        }


        // //////////////////////
        // ////////////////////////////////////////////////////////    Evento o acción para manejar datos
        // //////////////////////
        public void AddNewCaseOnSwitch(Switch_outputs_GUID text)
        {
            // creamos todo lo que hay que crear
            var SwitchElement = new VisualElement();
            SwitchElement.AddToClassList("SwitchElement");
            VarSwitchNode newOption = new VarSwitchNode();
            newOption.text = new List<TextField>();

            newOption.outPortSwitch = GeneratedPort(Direction.Output, Port.Capacity.Single); // creamos el puerto
            newOption.outPortSwitch.portName = "";
            newOption.outPortSwitch.portColor = DialogueSystemNodePortColor.Switch;
            extensionContainer.visible = true;// por si estaba desactivado
            var nameCase = new VisualElement();// creamos el contenedor de el texto con sus versiones

            // creamos todos los textos 
            if (text == null)
            {
                foreach (var item in character.languageList)
                {
                    TextField field = new TextField()
                    {
                        name = item
                    };
                    newOption.text.Add(field);
                    nameCase.Add(field);
                }
            }
            else
            { // Cargamos los textos
                foreach (var item in text.translations)
                {
                    TextField field = new TextField()
                    {
                        name = item.id,
                        value = item.text
                    };
                    newOption.text.Add(field);
                    nameCase.Add(field);
                }
            }

            Button x = new Button(clickEvent: () =>
            { // el boton de eliminar opcion
                outPortsSwitch.Remove(newOption);
                extensionContainer.Remove(SwitchElement);
            })
            { text = "x" };

            // añadimos todo lo que hay que añadir
            outPortsSwitch.Add(newOption);
            SwitchElement.Add(x);
            SwitchElement.Add(nameCase);
            SwitchElement.Add(newOption.outPortSwitch);
            extensionContainer.Add(SwitchElement);
        }
        public void SendData(InterpretationXL.DialogueLines data)
        {
            if (outPort == null) return;

            // Iterar sobre las conexiones del outputPort
            foreach (var edge in outPort.connections)
            {
                if (edge.input.node is EditorDialogueSysNode targetNode)
                {
                    dialogueLine.output_GUID = targetNode.dialogueLine.GUID;
                    targetNode.ReceiveData(data); // Llamar al método del nodo conectado
                }
            }

        }

        public void ReceiveData(InterpretationXL.DialogueLines data)
        {

            SendData(dialogueLine);
        }
        public void UpdateData()
        {
            // /////////////////  Actualizamos la informacion de los imputs y output
            switch (type)
            {
                case NodeType.Switch:
                    dialogueLine.Switch_outputs_GUIDs = new List<Switch_outputs_GUID>();
                    foreach (var item in outPortsSwitch)
                    {
                        Switch_outputs_GUID switchData = new Switch_outputs_GUID() { translations = new List<Translation>() };
                        foreach (var item1 in item.text)
                        {
                            switchData.translations.Add(new Translation() { id = item1.name, text = item1.value });
                        }

                        foreach (var edge in item.outPortSwitch.connections)
                        {
                            if (edge.input.node is EditorDialogueSysNode targetNode)
                            {
                                switchData.GUID = targetNode.dialogueLine.GUID;
                                break;
                            }
                        }
                        dialogueLine.Switch_outputs_GUIDs.Add(switchData);
                    }
                    break;
                default:
                    if (outPort != null)
                    {
                        foreach (var edge in outPort.connections)
                        {
                            if (edge.input.node is EditorDialogueSysNode targetNode)
                            {
                                dialogueLine.output_GUID = targetNode.dialogueLine.GUID;
                                break;
                            }
                        }
                        if (outPort.connections.Count() == 0) dialogueLine.output_GUID = null;
                    }

                    break;
            }
            // /////////////////  en el caso de no tener Guid, este es creado
            if (dialogueLine.GUID == null)
            {
                dialogueLine.GUID = Guid.NewGuid().ToString();
            }
            dialogueLine.position = new Vector2(GetPosition().position.x, GetPosition().position.y); // actualizamos posicion
                                                                                                     // /////////////////  mostrar el idioma que actualmente se este trabajando
            if (type == NodeType.Dialogue)
            {
                if (character.languageSelect == -1) // en el caso de que sea un Show All
                {
                    foreach (var item in dialogueNode.text)
                    {
                        item.label = item.name;
                        item.style.display = DisplayStyle.Flex;
                    }
                }
                else
                { // mostar idioma espesifico
                    for (int i = 0; i < dialogueNode.text.Count; i++)
                    {
                        if (dialogueNode.text[i].name == character.GetlanguageSelectID())
                        {
                            dialogueNode.text[i].label = null;
                            dialogueNode.text[i].style.display = DisplayStyle.Flex;
                        }
                        else
                        {
                            dialogueNode.text[i].label = null;
                            dialogueNode.text[i].style.display = DisplayStyle.None;
                        }
                    }
                }
            }
            else if (type == NodeType.Switch)
            {
                foreach (var item in outPortsSwitch)
                {
                    foreach (var item1 in item.text)
                    {
                        if (character.languageSelect == -1)
                        { // todos 
                            item1.label = item1.name;
                            item1.style.display = DisplayStyle.Flex;
                        }
                        else if (item1.name == character.GetlanguageSelectID())
                        {
                            item1.label = null;
                            item1.style.display = DisplayStyle.Flex;
                        }
                        else
                        {
                            item1.label = null;
                            item1.style.display = DisplayStyle.None;
                        }
                    }
                }
            }
        }
        // //////////////////////
        //  ////////////////////////////////////////////////////////    crear puertos
        // //////////////////////
        Port GeneratedPort(Direction portDireccion, Port.Capacity capacity = Port.Capacity.Single)
        {

            var puerto = this.InstantiatePort(Orientation.Horizontal, portDireccion, capacity, typeof(object));


            return puerto;
        }

    }


}