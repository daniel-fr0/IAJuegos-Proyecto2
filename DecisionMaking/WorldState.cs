using UnityEngine;

public class WorldState : MonoBehaviour
{
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

		if (playerHealth <= 0 || allyHealth <= 0)
		{
			GameOver();
		}
	}

	private void GameOver()
	{
		Time.timeScale = 0;
		GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
		
		GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 60, 400, 120), "¡Perdiste!", titleFont);
		
		if (playerHealth <= 0)
		{
			GUI.Label(new Rect(Screen.width / 2 - 120, Screen.height / 2 + 30, 200, 60), "¡El jugador ha muerto!", largeFont);
			return;
		}

		if (allyHealth <= 0)
		{
			GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 30, 200, 60), "¡El compañero ha muerto!", largeFont);
		}
	}
}