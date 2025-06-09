using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Cutscenes
{
    public static Cutscene IntroCutscene
    {
        get
        {
            return new Cutscene() {
                new ShowDialogAction(true),
                new DialogAction("Welcome to Blade Harvest 2045..."),
                new DialogAction("The year is 2045, and the world has changed forever."),
                new DialogAction("You are one of the last farmers in a cyberpunk world."),
                // Activate the Player object that's already on the canvas
                new ActivateObjectAction("Player"),
                new DialogAction("Your spaceship has landed on a remote farming colony."),
                new DialogAction("Here, you must grow crops and survive using advanced farming technology."),
                new DialogAction("Use your tools wisely - each action requires stamina."),
                new DialogAction("Rest in your bed each night to restore your energy and advance to the next day."),
                new DialogAction("Good luck, farmer. The future of agriculture depends on you!"),
                new ShowDialogAction(false),
                new WaitAction(1f),
                // Deactivate the Player object
                new DeactivateObjectAction("Player")
            };
        }
    }
}
