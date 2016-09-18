﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    Animator animator;
    Ray ray;
    RaycastHit2D hit;
    

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();

}
	
	// Update is called once per frame
	void Update () {
        Camera mainCamera = GameObject.FindObjectOfType<Camera>();
        Animator animator = GameObject.FindObjectOfType<Animator>();

        ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        foreach (Touch t in Input.touches)
        {
            if (t.phase == TouchPhase.Began)
            {
                //if they've tapped the play button, start the game
                animator.SetTrigger("PlayButton");
            }
        }

        //check mouse click on Play button (for debug purposes)
        if (Input.GetMouseButtonDown(0) & hit == Physics2D.Raycast(ray.origin, new Vector2(0,0)))
        {
            Debug.Log("Play");
            animator.SetTrigger("PlayButton");
        }

	}
}
