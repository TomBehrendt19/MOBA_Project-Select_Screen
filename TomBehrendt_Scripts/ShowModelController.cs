using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShowModelController : MonoBehaviour
{

    public static ShowModelController Singleton;       //Stores a reference to character prefabs.
    public List<Transform> models;
   
    //Index and ToIndex are references to the current character in the current index and who next in the index.
    public int index;
    [HideInInspector]
    public int ToIndex;

    private void Start()
    {
        Singleton = this;
        //The Index will equals to what character Prefab is currently selected in the Dropdown Box.
        index = PlayerPrefs.GetInt("CharacterSelected");
        ToIndex = index;
        Debug.Log(index);

        //These functions create a new list to be used by the Index depending on how child Prefabs are in the parent.
        models = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var model = transform.GetChild(i);
            models.Add(model);

            model.gameObject.SetActive(i == index);
        }
        models = models.OrderBy(o => o.name).ToList();
        NewEnableModel(index);
    }

    //This function checks every time a new Child Prefab is selected from the Dropdown Box and see if the current enable model count is lower then zero then enable the newly selected child Prefab.
    public void NewEnableModel(int position)
    {

        Debug.Log(position);
        for (int i = 0; i < models.Count; i++)
        {

            models[i].gameObject.SetActive(i == position);
        }

        index = position;
    }

    public void Update()
    {
        //Debugs the current Index and new Index when selected.
        Debug.Log(ToIndex);
        Debug.Log(index);
        //If the new Index doesn't equals the current Index then enable new Prefab and disable the old one.
        if (ToIndex != index)
            NewEnableModel(ToIndex);
    }

    //This function enables the child Prefab in the parent depending on what was selected in the Dropdown Box.
    public void EnableModel(Transform modelTransform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var transformToToggle = transform.GetChild(i);
            bool shouldBeActive = transformToToggle == modelTransform;

            transformToToggle.gameObject.SetActive(shouldBeActive);
        }
    }
}