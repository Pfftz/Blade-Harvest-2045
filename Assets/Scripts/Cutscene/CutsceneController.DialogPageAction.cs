using System;
using System.Collections;
using UnityEngine;

public class DialogPageAction : ICutsceneAction
{
    private DialogPage _dialogPage;
    private ICutsceneController _cutsceneController;
    private bool _isCompleted;

    public DialogPageAction(DialogPage dialogPage, ICutsceneController cutsceneController)
    {
        _dialogPage = dialogPage;
        _cutsceneController = cutsceneController;
    }

    public void Run(ICutsceneController controller, Action completion)
    {
        _isCompleted = false;

        // Simply animate the text and complete when done
        controller.AnimateTextInPagedDialog(_dialogPage.Text, () =>
        {
            Complete(completion);
        });
    }

    private void Complete(Action completion)
    {
        if (_isCompleted)
            return;

        _isCompleted = true;
        completion();
    }
}