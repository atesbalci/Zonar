using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class Menu : MonoBehaviour {

	void Update ()
    {
	    if (Input.GetMouseButton(0) && GameCore.Instance.State == GameState.Menu)
	    {
	        GameCore.Instance.State = GameState.AwaitingTransmission;
            gameObject.SetActive(false);
	    }	
	}
}
