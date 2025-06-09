using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public interface ICutsceneController
{
    void ShowPagedDialog(bool show);
    void AnimateTextInPagedDialog(string text, Action completion);
    Coroutine StartCoroutine(IEnumerator routine);
    void StopCoroutine(Coroutine routine);
    void ActivateObject(string id);
    void DeactivateObject(string id);
    void SkipCutscene();
}

public class CutsceneController : MonoBehaviour, ICutsceneController
{
    [Header("UI References")]
    public GameObject dialogPanel;
    public Text dialogText;
    public GameObject skipButtonObj;

    [Header("Cutscene Settings")]
    public float dialogTextSpeed = 0.05f;
    public CutscenePrefabs prefabs;
    public string skipToSceneName = "Day1";

    private CutsceneQueue _cutsceneQueue;
    private Coroutine _pagedDialogAnimationCoroutine;

    public void RunCutscene(Cutscene cutscene, Action completion)
    {
        // Show skip button
        if (skipButtonObj != null)
        {
            skipButtonObj.SetActive(true);
        }

        var context = new CutsceneQueueContext(this, () =>
        {
            if (skipButtonObj != null)
            {
                skipButtonObj.SetActive(false);
            }
            completion?.Invoke();
        });
        _cutsceneQueue = new CutsceneQueue(cutscene, context);
        _cutsceneQueue.Run();
    }

    public void SkipCutscene()
    {
        Debug.Log("Skipping cutscene - loading Day1 scene...");

        // Stop all running cutscenes
        if (_cutsceneQueue != null)
        {
            StopAllCoroutines();
        }

        // Hide dialog panel
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }

        // Load the target scene
        SceneManager.LoadScene(skipToSceneName);
    }

    // Method for skip button onClick event
    public void OnSkipBtnClick()
    {
        Debug.Log("Skip button clicked!");
        SkipCutscene();
    }

    public void ShowPagedDialog(bool show)
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(show);
        }

        // Show skip button when dialog is shown
        if (skipButtonObj != null)
        {
            skipButtonObj.SetActive(show);
        }
    }

    public void AnimateTextInPagedDialog(string text, Action completion)
    {
        // Simple implementation - just display text without pagination
        if (_pagedDialogAnimationCoroutine != null)
        {
            StopCoroutine(_pagedDialogAnimationCoroutine);
        }
        _pagedDialogAnimationCoroutine = StartCoroutine(AnimateTextCoroutine(text, completion));
    }

    private IEnumerator AnimateTextCoroutine(string text, Action completion)
    {
        if (dialogText != null)
        {
            for (var i = 0; i <= text.Length; i++)
            {
                dialogText.text = text.Substring(0, i);
                yield return new WaitForSeconds(dialogTextSpeed);
            }
        }

        // Wait a moment before proceeding to next dialog
        yield return new WaitForSeconds(1f);
        completion();
    }

    public void ActivateObject(string id)
    {
        if (prefabs == null)
        {
            Debug.LogError("CutscenePrefabs is not assigned to CutsceneController!");
            return;
        }

        var prefabObj = prefabs.GetPrefab(id);
        if (prefabObj == null)
        {
            Debug.LogError($"Prefab with ID '{id}' not found in CutscenePrefabs.");
            return;
        }

        GameObject sceneObject = FindObjectInCanvas(prefabObj.name);
        if (sceneObject != null)
        {
            sceneObject.SetActive(true);
            Debug.Log($"Activated object '{id}' ({prefabObj.name})");
        }
        else
        {
            Debug.LogError($"Could not find object '{prefabObj.name}' in the canvas. Make sure it's placed on the canvas and inactive.");
        }
    }

    public void DeactivateObject(string id)
    {
        if (prefabs == null)
        {
            Debug.LogError("CutscenePrefabs is not assigned to CutsceneController!");
            return;
        }

        var prefabObj = prefabs.GetPrefab(id);
        if (prefabObj == null)
        {
            Debug.LogError($"Prefab with ID '{id}' not found in CutscenePrefabs.");
            return;
        }

        GameObject sceneObject = FindObjectInCanvas(prefabObj.name);
        if (sceneObject != null)
        {
            sceneObject.SetActive(false);
            Debug.Log($"Deactivated object '{id}' ({prefabObj.name})");
        }
        else
        {
            Debug.LogWarning($"Could not find object '{prefabObj.name}' to deactivate.");
        }
    }

    private GameObject FindObjectInCanvas(string objectName)
    {
        Canvas mainCanvas = GetComponentInParent<Canvas>();
        if (mainCanvas == null)
        {
            mainCanvas = FindObjectOfType<Canvas>();
        }

        if (mainCanvas != null)
        {
            Transform foundTransform = mainCanvas.transform.Find(objectName);
            if (foundTransform != null)
            {
                return foundTransform.gameObject;
            }

            return FindChildRecursive(mainCanvas.transform, objectName);
        }

        Debug.LogError("Could not find Canvas to search for objects!");
        return null;
    }

    private GameObject FindChildRecursive(Transform parent, string objectName)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == objectName)
            {
                return child.gameObject;
            }

            GameObject found = FindChildRecursive(child, objectName);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    private IEnumerable<DialogPage> GetDialogPages(string text)
    {
        // Simple implementation - just return one page
        List<DialogPage> dialogPages = new List<DialogPage>();
        dialogPages.Add(new DialogPage(text));
        return dialogPages;
    }

    // Test method for skip button
    [ContextMenu("Test Skip Button")]
    public void TestSkipButton()
    {
        Debug.Log("Testing skip button functionality...");
        OnSkipBtnClick();
    }
}