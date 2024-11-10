public interface Action
{
	void OnStateEnter();
	void OnStateExit();
	void Save();
	void Load();
}