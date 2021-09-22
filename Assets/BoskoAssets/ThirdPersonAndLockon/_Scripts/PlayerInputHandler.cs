using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputHandler
{
    [Header("Customizable Controls")]
    public string inputHorizontal = "Horizontal"; // Unity axis horizontal
    public string inputVertical = "Vertical"; // Unity axis vertical

    private List<KeyCommand> keyCommands = new List<KeyCommand>();

    public void BindInputToCommand(KeyCode _keyCode, ICommand _command, KeyCommand.KeyType _keyType)
    {
        keyCommands.Add(new KeyCommand()
        {
            key = _keyCode,
            command = _command,
            keyType = _keyType
        });
    }

    public void UnBindInput(KeyCode keyCode)
    {
        var keycodes = keyCommands.FindAll(x => x.key == keyCode);
        keycodes.ForEach(x => keyCommands.Remove(x));
    }

    public void UnBindAll()
    {
        keyCommands = new List<KeyCommand>();
    }

    public void HandleInput(PlayerBehaviour pb)
    {
        List<KeyCommand> allCurrentCommands = keyCommands;
        foreach (var keyCommand in allCurrentCommands)
        {
            Debug.Log(keyCommand.command.ToString());
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

    }
}

public class KeyCommand
{
    public KeyCode key;
    public ICommand command;
    public KeyType keyType;
    public enum KeyType { OnKeyDown, OnKeyUp, OnKey}
}
