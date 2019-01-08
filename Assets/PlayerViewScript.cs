﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewScript : MonoBehaviour {

	public GameObject obj;
	// Update is called once per frame
	void Start() {
		obj.SetActive(false);
	}
	void Update () {
		if(Input.GetButtonUp("Cancel")) {
			obj.SetActive(!obj.activeSelf);
		}
	}
}
