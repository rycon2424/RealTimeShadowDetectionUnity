using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputHandler
{
    [Header("Customizable Controls")]
    public string inputHorizontal = "Horizontal"; // Unity axis horizontal
    public string inputVertical = "Vertical"; // Unity axis vertical

    private List<KeyCommand> keyCommands = new List<KeyCommand>();

    public List<KeyCode> commandsToRemove = new List<KeyCode>();
    public List<KeyCommand> commandsToStore = new List<KeyCommand>();

    public void BindInputToCommand(KeyCode _keyCode, ICommand _command, KeyCommand.KeyType _keyType)
    {
        commandsToStore.Add(new KeyCommand()
        {
            key = _keyCode,
            command = _command,
            keyType = _keyType
        });
    }

    public void UnBindInput(KeyCode keyCode)
    {
        commandsToRemove.Add(keyCode);;
    }
    
    public void UnBindAll()
    {
        keyCommands = new List<KeyCommand>();
    }

    public void HandleInput(PlayerBehaviour pb)
    {
        foreach (var keyCommand in keyCommands)
        {
            switch (keyCommand.keyType)
            {
                case KeyCommand.KeyType.OnKeyDown:
                    if (Input.GetKeyDown(keyCommand.key))
                    {
                        keyCommand.command.Execute(pb);
                    }
                    break;
                case KeyCommand.KeyType.OnKeyUp:
                    if (Input.GetKeyUp(keyCommand.key))
                    {
                        keyCommand.command.Execute(pb);
                    }
                    break;
                case KeyCommand.KeyType.OnKey:
                    if (Input.GetKey(keyCommand.key))
                    {
                        keyCommand.command.Execute(pb);
                    }
                    break;
                default:
                    break;
            }
        }
        RemoveMarkedCommands(); // Remove commands that were removed during the loop
        StoreNewCommands(); // Store commands that were stored during the loop
    }

    void RemoveMarkedCommands()
    {
        foreach (var key in commandsToRemove)
        {
            Debug.Log(key.ToString());
            var keycodes = keyCommands.FindAll(x => x.key == key);
            keycodes.ForEach(x => keyCommands.Remove(x));
        }
        commandsToRemove = new List<KeyCode>();
    }

    void StoreNewCommands()
    {
        foreach (var key in commandsToStore)
        {
            keyCommands.Add(key);
        }
        commandsToStore = new List<KeyCommand>();
    }
}

public class KeyCommand
{
    public KeyCode key;
    public ICommand command;
    public KeyType keyType;
    public enum KeyType { OnKeyDown, OnKeyUp, OnKey}
}
