using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Networking;


public class ShowModelButton : NetworkBehaviour {

    //These variables stores the position of the dropdown Box and it Action. 
    private int positionToShow;
    private Action<int> clickAction;

    //This store a reference to the Dropdown Box it self.
    private Dropdown dropdown;

    //Initializes the Position and Action of the Dropdown Box, while also getting a refrence to characters name in the ShowModelController script.
    public void Initialize(int positionToShow,Action<int> clickAction)
    {
        this.positionToShow = positionToShow;
        this.clickAction = clickAction;
        GetComponentInChildren<Text>().text = ShowModelController.Singleton.models[positionToShow].gameObject.name;
    }

	// Use this for initialization
	void Start () {

        //Get the Dropdown Box component.
        dropdown = GetComponent<Dropdown>();

        //These functions generate a list of name to be displayed in the Dropdown Box from the characters in the ShowModelController.
        dropdown.value = ShowModelController.Singleton.index;
        List<string> beh = ShowModelController.Singleton.models.Select(o => (o.name)).ToList();
        beh.Sort();
        dropdown.AddOptions(beh);
        dropdown.onValueChanged.AddListener(o => ShowModelController.Singleton.ToIndex = o);

        //Debugs to tell the user if the previous function completedit task.
        Debug.Log("hey");
		
	}
   
    //Debugs infomation of the Dropdown Box like position and ClickActions.
    private void HandleButtonClick()
    {
        Debug.Log(positionToShow);
        clickAction(positionToShow);
    }
}
