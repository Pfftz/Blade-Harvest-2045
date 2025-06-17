using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Cutscenes
{
    public static Cutscene GoodEndingCutscene
    {
        get
        {
            return new Cutscene() {
                new ShowDialogAction(true),
                new DialogAction("Congratulations, farmer!"),
                new DialogAction("You have successfully reached your goal of 1000 currency."),
                new DialogAction("Your hard work and dedication have paid off in this cyberpunk world."),
                new DialogAction("The colony's food supply is secure thanks to your efforts."),
                new DialogAction("You've proven that even in 2045, traditional farming combined with technology can thrive."),
                new DialogAction("The future looks bright for agriculture on this remote colony."),
                new DialogAction("Your name will be remembered as one of the great farmers of the new age."),
                new DialogAction("Thank you for playing Blade Harvest 2045!"),
                new ShowDialogAction(false),
                new WaitAction(2f)
            };
        }
    }

    public static Cutscene BadEndingCutscene
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
