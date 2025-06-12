using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogAction : ICutsceneAction
{
    private string _text;
    public string Text { get { return _text; } }
    public DialogAction(string text)
    {
        _text = text;
    }

    public void Run(ICutsceneController controller, Action finish)
    {
        controller.AnimateTextInPagedDialog(_text, finish);
    }
}

public class ShowDialogAction : ICutsceneAction
{
    private bool _show;

    public ShowDialogAction(bool show)
    {
        _show = show;
    }

    public void Run(ICutsceneController controller, Action finish)
    {
        controller.ShowPagedDialog(_show);
        finish();
    }
}

// New Action: Activate GameObject by ID
public class ActivateObjectAction : ICutsceneAction
{
    private string _prefabId;

    public ActivateObjectAction(string prefabId)
    {
        _prefabId = prefabId;
    }

    public void Run(ICutsceneController controller, Action finish)
    {
        controller.ActivateObject(_prefabId);
        finish();
    }
}

// New Action: Deactivate GameObject by ID
public class DeactivateObjectAction : ICutsceneAction
{
    private string _prefabId;

    public DeactivateObjectAction(string prefabId)
    {
        _prefabId = prefabId;
    }

    public void Run(ICutsceneController controller, Action finish)
    {
        controller.DeactivateObject(_prefabId);
        finish();
    }
}

public class WaitAction : ICutsceneAction
{
    private float _secondsToWait;
    public float SecondsToWait { get { return _secondsToWait; } }
    public WaitAction(float secondsToWait)
    {
        _secondsToWait = secondsToWait;
    }

    public void Run(ICutsceneController controller, Action finish)
    {
        controller.StartCoroutine(WaitCoroutine(finish));
    }

    private IEnumerator WaitCoroutine(Action finish)
    {
        yield return new WaitForSeconds(_secondsToWait);
        finish();
    }
}
