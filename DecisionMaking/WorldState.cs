using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldState : MonoBehaviour
{
	public RectTransform goal;
	private Vector3 leftBottom;
	private Vector3 rightTop;
	public Sprite heartSprite;
	public Sprite playerSprite;
	public Sprite allySprite;
	public Kinematic player;
	public Kinematic ally;
	public float playerMaxHealth;
	public float allyMaxHealth;
	private float _playerHealth;
	public float playerHealth
	{
		get { return _playerHealth; }
		set
		{
			_playerHealth = Mathf.Clamp(value, 0, playerMaxHealth);
		}
	}
	private float _allyHealth;
	public float allyHealth
	{
		get { return _allyHealth; }
		set
		{
			_allyHealth = Mathf.Clamp(value, 0, allyMaxHealth);
		}
	}
	[HideInInspector]
	public bool playerInvulnerable;
	[HideInInspector]
	public bool allyInvulnerable;
	public float invulnerabilityDuration = 2.0f;
	private Renderer playerRenderer;
	private Renderer allyRenderer;
	public int largeFontSize = 30;
	public int titleFontSize = 100;
	private GUIStyle largeFont;
	private GUIStyle titleFont;
	private GUIStyle buttonStyle;

	#region Singleton
	public static WorldState instance;
	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(this);
		}
	}
	#endregion

	// Add game state variables
	private bool gameEnded = false;
	private bool playerWon = false;

	private void Start()
	{
		playerHealth = playerMaxHealth;
		allyHealth = allyMaxHealth;
		playerRenderer = player.gameObject.GetComponentInChildren<Renderer>();
		allyRenderer = ally.gameObject.GetComponentInChildren<Renderer>();
		
		largeFont = new GUIStyle();
		largeFont.fontSize = largeFontSize;
		largeFont.normal.textColor = Color.white;

		titleFont = new GUIStyle();
		titleFont.fontSize = titleFontSize;
		titleFont.normal.textColor = Color.white;

		leftBottom = new Vector3(goal.rect.xMin, goal.rect.yMin, 0) + goal.transform.position;
		rightTop = new Vector3(goal.rect.xMax, goal.rect.yMax, 0) + goal.transform.position;
	}

	public void SetInvulnerability(Kinematic character)
	{
		if (character == player)
		{
			playerInvulnerable = true;
			Invoke("ResetPlayerInvulnerability", invulnerabilityDuration);
		}
		else if (character == ally)
		{
			allyInvulnerable = true;
			Invoke("ResetAllyInvulnerability", invulnerabilityDuration);
		}
	}

	private void ResetPlayerInvulnerability()
	{
		playerInvulnerable = false;
	}

	private void ResetAllyInvulnerability()
	{
		allyInvulnerable = false;
	}

	private void Update()
	{
		if (playerHealth <= 0)
		{
			return;
		}
		if (allyHealth <= 0)
		{
			return;
		}

		if (playerInvulnerable)
		{
			// make player blink red
			playerRenderer.material.color = playerRenderer.material.color == Color.red ? Color.white : Color.red;
		}
		else
		{
			playerRenderer.material.color = Color.white;
		}

		if (allyInvulnerable)
		{
			// make ally blink red
			allyRenderer.material.color = allyRenderer.material.color == Color.red ? Color.white : Color.red;
		}
		else
		{
			allyRenderer.material.color = Color.white;
		}
	}

	private void OnGUI()
	{
		// Initialize button style here if it hasn't been created yet
		if (buttonStyle == null)
		{
			buttonStyle = new GUIStyle(GUI.skin.button);
			buttonStyle.fontSize = largeFontSize;
			buttonStyle.normal.textColor = Color.black; // Changed to black for better visibility
			buttonStyle.hover.textColor = Color.gray;
		}

		// Draw sprites and hearts only if game hasn't ended
		if (!gameEnded)
		{
			// Draw sprites
			GUI.DrawTexture(new Rect(15, 100, 80, 80), playerSprite.texture);
			GUI.DrawTexture(new Rect(15, 200, 80, 80), allySprite.texture);
			
			// Draw hearts
			for (int i = 1; i <= playerHealth; i++)
			{
				GUI.DrawTexture(new Rect(15 + i * 80, 100, 80, 80), heartSprite.texture);
			}
			for (int i = 1; i <= allyHealth; i++)
			{
				GUI.DrawTexture(new Rect(15 + i * 80, 200, 80, 80), heartSprite.texture);
			}

			// Check win/lose conditions
			if (playerHealth <= 0 || allyHealth <= 0)
			{
				gameEnded = true;
				playerWon = false;
				Time.timeScale = 0;
			}
			else if (ally.position.x >= leftBottom.x && ally.position.x <= rightTop.x &&
					 ally.position.y >= leftBottom.y && ally.position.y <= rightTop.y)
			{
				gameEnded = true;
				playerWon = true;
				Time.timeScale = 0;
			}
		}

		// Show game over or win screen
		if (gameEnded)
		{
			if (playerWon)
			{
				Win();
			}
			else
			{
				GameOver();
			}
		}
	}

	private void GameOver()
	{
		// Semi-transparent black background
		GUI.color = new Color(0, 0, 0, 0.8f);
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
		GUI.color = Color.white;
		
		// Game over text
		GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 60, 400, 120), "¡Perdiste!", titleFont);
		
		if (playerHealth <= 0)
		{
			GUI.Label(new Rect(Screen.width / 2 - 120, Screen.height / 2 + 50, 240, 60), "¡El jugador ha muerto!", largeFont);
		}
		else if (allyHealth <= 0)
		{
			GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 50, 300, 60), "¡El compañero ha muerto!", largeFont);
		}

		// Restart button
		if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 120, 200, 50), "Reiniciar", buttonStyle))
		{
			RestartGame();
		}

		// Quit button
		if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 180, 200, 50), "Salir", buttonStyle))
		{
			QuitGame();
		}
	}

	private void Win()
	{
		// Semi-transparent black background
		GUI.color = new Color(0, 0, 0, 0.8f);
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
		GUI.color = Color.white;
		
		// Win text
		GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 60, 400, 120), "¡Ganaste!", titleFont);
		GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height / 2 + 50, 440, 60), "¡El compañero ha llegado a la salida!", largeFont);

		// Restart button
		if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 120, 300, 50), "Jugar de nuevo", buttonStyle))
		{
			RestartGame();
		}

		// Quit button
		if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 180, 200, 50), "Salir", buttonStyle))
		{
			QuitGame();
		}
	}

	private void RestartGame()
	{
		// Reset time scale to normal
		Time.timeScale = 1;
		
		// Reload the current scene
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	private void QuitGame()
	{
		// Reset time scale to normal
		Time.timeScale = 1;
		
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}
}