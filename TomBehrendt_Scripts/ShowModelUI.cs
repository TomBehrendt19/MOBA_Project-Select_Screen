using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowModelUI : MonoBehaviour {

    //Get a reference to the buttonPrefab 
    [SerializeField] private GameObject buttonPrefab;

	// Use this for initialization
	void Start () {

        //These functions get the number of character listed in ShowModelController, and uses the function CreateButtonForModel to create a button in the Dropdown Box for each character.
        var models = ShowModelController.Singleton.models;
        for (int i = 0; i < models.Count;i++)
        {
            CreateButtonForModel(i);
        }
		
	}

    //This function create a button for each character listed in the ShowModelController.
    private void CreateButtonForModel(int position)
    {
        GameObject button = Instantiate(buttonPrefab);
        button.transform.SetParent(this.transform);
        button.transform.localScale = Vector3.one;
        button.transform.localRotation = Quaternion.identity;
    }
}
