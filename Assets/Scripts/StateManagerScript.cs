﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class StateManagerScript : MonoBehaviour, IStateManager {

    [SerializeField] private float minYAwayAsteroidSpawn; //minimum y distance to spawn asteroid away from current one
    [SerializeField] private float maxYAwayAsteroidSpawn; //maximum y distance to spawn asteroid away from current one
    [SerializeField] private float xAreaAsteroidSpawn; //x distance to vary spawning asteroid
    [SerializeField] private GameObject rocketPrefab; // prefab used to instantiate rocket
    [SerializeField] private Sprite[] rocketSprites; // Sprites for rockets
    [SerializeField] private GameObject asteroidPrefab; // prefab used to instantiate asteroid
    [SerializeField] private Sprite[] asteroidSprites; // Sprites for asteroids
    [SerializeField] private GameObject gameOverDialogue; // The game over screen
    [SerializeField] private GameObject scoreText; // The object displaying the score
    [SerializeField] private GameObject highscoreText; // The object that displays the highscore

    private IRocket rocket; // The rocket the player controls
    private Queue<IAsteroid> asteroids; // The first asteroid on screen
    private GameObject mainCamera; // The camera that follows the player
    private IScore scoreScript; // Script to access the score
    private IInputManager inputManager; // Script that manages player input
    private ITitleUi titleUiScript; // The screen that accesses the title UI
    private ICamera cameraScript; // The script that controls how the camera moves
    private bool gameHasBegun; // Detects whether or not the game has started

    // Use this for initialization
    void Start()
    {
        rocket = GameObject.Find("Rocket").GetComponent<RocketScript>();
        asteroids = new Queue<IAsteroid>();
        foreach(GameObject asteroidObject in GameObject.FindGameObjectsWithTag("Asteroid"))
        {
            asteroids.Enqueue(asteroidObject.GetComponent<AsteroidScript>());
        }
        mainCamera = GameObject.Find("Main Camera");
        scoreScript = GameObject.Find("Score").GetComponent<ScoreScript>();
        inputManager = gameObject.GetComponent<InputManager>();
        titleUiScript = GameObject.Find("Title").GetComponent<TitleUiScript>();
        cameraScript = GameObject.Find("Main Camera").GetComponent<CameraScript>();

        gameOverDialogue.SetActive(false);
        gameHasBegun = false;
    }

    // Update is called once per frame
    void Update () {
        // Listen to input to launch the rocket
        if(gameHasBegun && inputManager.GetRocketInput())
        {
            rocket.LaunchRocket();
        }
    }

    /// <summary>
    /// Called whenever the rocket fails to make a proper landing on an asteroid
    /// </summary>
    public void RegisterFailedLanding()
    {
        // Display game over screen, check to see if current score is a new highscore
        // if so, updates highscore
        scoreText.GetComponent<Text>().text = scoreScript.GetScore().ToString();
        scoreScript.CheckIfBest(scoreScript.GetScore());
        highscoreText.GetComponent<Text>().text = scoreScript.GetHighscore().ToString();
        scoreScript.FadeOut();
        gameOverDialogue.SetActive(true);

        // Destroy the rocket at the end so no null pointer expections in the code above
        rocket.DestroyInstance();
    }
    
    /// <summary>
    /// Called whenever the rocket successfully lands on an asteroid.
    /// </summary>
    /// <param name="collidedAsteroid"> The asteroid object that the rocket collided with</param>
    public void RegisterSuccessfulLanding(IAsteroid collidedAsteroid)
    {
        // Move the camera updwards
        cameraScript.MoveCameraUntil(collidedAsteroid.GetPosition());
        
        // Create a new asteroid and delete the old one (if there's more than 1 before creation)
        float xSpawn = xAreaAsteroidSpawn / 2;
        Vector3 newAsteroidPos = collidedAsteroid.GetPosition();
        newAsteroidPos.y += Random.Range(minYAwayAsteroidSpawn, maxYAwayAsteroidSpawn);
        newAsteroidPos.x = Random.Range(-xSpawn, xSpawn);

        // This creates a new asteroid after successfully landing on one
        // Additionally it deletes the first landed asteroid if more than
        // two exist.
        IAsteroid newAsteroid = CreateAsteroid(newAsteroidPos);
        asteroids.Enqueue(newAsteroid);
        if (asteroids.Count > 2)
        {
            asteroids.Dequeue().DestroyInstance();
        }

        // Update score
        scoreScript.UpdateScore();
        int score = scoreScript.GetScore();
        rocket.UpdateRotationSpeed(score);
        foreach (IAsteroid asteroidObject in asteroids)
        {
            asteroidObject.UpdateRotationSpeed(score);
        }
    }
    
    /// <summary>
    /// Resets the settings so a new game can begin
    /// </summary>
    public void TriggerNewGame()
    {
        // Reset camera
        mainCamera.transform.localPosition = new Vector3(0, 66, -930);

        // Reset rocket
        IRocket newRocket = CreateRocket(rocketPrefab.transform.position);
        rocket = newRocket;

        // Reset asteroids
        IAsteroid newAsteroid = CreateAsteroid(asteroidPrefab.transform.position);
        foreach (IAsteroid asteroidInstance in asteroids)
        {
            asteroidInstance.DestroyInstance();
        }
        asteroids.Clear();
        asteroids.Enqueue(newAsteroid);

        // This resets the score and prepares the next game.
        scoreScript.ResetScore();
        int score = scoreScript.GetScore();
        rocket.UpdateRotationSpeed(score);
        newAsteroid.UpdateRotationSpeed(score);
        scoreScript.FadeIn();
        gameOverDialogue.SetActive(false);
    }

    /// <summary>
    /// Called at the start of the program and
    /// begins the game by fading out the
    /// title and play button
    /// </summary>
    public void BeginGame()
    {
        scoreScript.FadeIn();
        titleUiScript.FadeOut();
        gameHasBegun = true;
    }

    /// <summary>
    /// Creates new rocket with random sprite
    /// </summary>
    private IRocket CreateRocket(Vector3 position)
    {
        GameObject newRocket = Instantiate(rocketPrefab);
        newRocket.transform.position = position;
        newRocket.transform.rotation = new Quaternion(0, 0, 0, 0);
        newRocket.name = "Rocket";
        newRocket.GetComponent<SpriteRenderer>().sprite = rocketSprites[Random.Range(0, rocketSprites.Length)];
        return newRocket.GetComponent<RocketScript>();
    }

    /// <summary>
    /// Creates new asteroid with random sprite
    /// </summary>
    private IAsteroid CreateAsteroid(Vector3 position)
    {
        GameObject newAsteroid = Instantiate(asteroidPrefab);
        newAsteroid.transform.position = position;
        newAsteroid.name = "Asteroid";
        newAsteroid.GetComponent<SpriteRenderer>().sprite = asteroidSprites[Random.Range(0, asteroidSprites.Length)];

        return newAsteroid.GetComponent<AsteroidScript>();
    }
}
