using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Cutscenes
{
    public static Cutscene BadEnding
    {
        get
        {
            return new Cutscene() {
                new ShowDialogAction(true),
                new DialogAction("Your farming venture has come to an end..."),
                new DialogAction("Unfortunately, you didn't reach the goal of 1000 currency."),
                new DialogAction("The colony's food supply remains uncertain."),
                new DialogAction("But don't give up - every farmer faces challenges."),
                new DialogAction("Perhaps with better planning and resource management, you could succeed next time."),
                new DialogAction("The cyberpunk world is harsh, but there's always another opportunity."),
                new DialogAction("Consider this a learning experience for your next farming venture."),
                new DialogAction("Thank you for playing Blade Harvest 2045!"),
                new ShowDialogAction(false),
                new WaitAction(2f)
            };
        }
    }
}
