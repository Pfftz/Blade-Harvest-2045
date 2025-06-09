using UnityEngine;

[System.Serializable]
public class DialogPage
{
    public string Text { get; private set; }
    public float ReadingTime { get; private set; }

    public DialogPage(string text, float readingTime = 2f)
    {
        Text = text;
        ReadingTime = readingTime;
    }
}