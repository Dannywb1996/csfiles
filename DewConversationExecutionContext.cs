using DewInternal;
using UnityEngine;

public class DewConversationExecutionContext
{
	public Coroutine coroutine;

	public int currentLineIndex;

	public string currentKey;

	public ConversationData currentData;

	public bool waitingForUserInput;

	public int userInput;
}
