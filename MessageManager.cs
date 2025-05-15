using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageManager : ManagerBase<MessageManager>
{
	public TextMeshProUGUI contentText;

	public CanvasGroup canvasGroup;

	public List<GameObject> buttons;

	private DewMessageSettings _currentMessage;

	private Queue<DewMessageSettings> _messageQueue = new Queue<DewMessageSettings>();

	public bool isShowingMessage => _currentMessage != null;

	private void Start()
	{
		UpdateVisibility();
	}

	public void ShowMessage(DewMessageSettings msg)
	{
		_messageQueue.Enqueue(msg);
		if (!isShowingMessage)
		{
			DequeueMessage();
		}
	}

	public void ShowMessageLocalized(string id)
	{
		ShowMessage(new DewMessageSettings
		{
			rawContent = DewLocalization.GetUIValue(id)
		});
	}

	private void DequeueMessage()
	{
		if (_messageQueue.Count <= 0)
		{
			return;
		}
		_currentMessage = _messageQueue.Dequeue();
		contentText.text = _currentMessage.rawContent;
		for (int i = 0; i < buttons.Count; i++)
		{
			int power = Mathf.RoundToInt(Mathf.Pow(2f, i));
			buttons[i].SetActive(((uint)_currentMessage.buttons & (uint)power) != 0);
		}
		UpdateVisibility();
		Dew.CallDelayed(delegate
		{
			for (int j = 0; j < buttons.Count; j++)
			{
				if (buttons[j].activeInHierarchy)
				{
					int num = Mathf.RoundToInt(Mathf.Pow(2f, j));
					if (_currentMessage.defaultButton == (DewMessageSettings.ButtonType)num)
					{
						ManagerBase<GlobalUIManager>.instance.SetFocus(buttons[j].GetComponent<IGamepadFocusable>());
						return;
					}
				}
			}
			ManagerBase<GlobalUIManager>.instance.SetFocusOnFirstFocusable(canvasGroup.gameObject);
		});
	}

	public void CloseMessageOk()
	{
		CloseMessage(DewMessageSettings.ButtonType.Ok);
	}

	public void CloseMessageYes()
	{
		CloseMessage(DewMessageSettings.ButtonType.Yes);
	}

	public void CloseMessageNo()
	{
		CloseMessage(DewMessageSettings.ButtonType.No);
	}

	public void CloseMessageCancel()
	{
		CloseMessage(DewMessageSettings.ButtonType.Cancel);
	}

	public void CloseMessage(DewMessageSettings.ButtonType button)
	{
		if (_currentMessage == null)
		{
			return;
		}
		if (_currentMessage.IsValid())
		{
			try
			{
				_currentMessage.onClose?.Invoke(button);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		for (int i = 0; i < buttons.Count; i++)
		{
			buttons[i].SetActive(value: false);
		}
		_currentMessage = null;
		if (_messageQueue.Count <= 0)
		{
			UpdateVisibility();
		}
		else
		{
			DequeueMessage();
		}
	}

	private void UpdateVisibility()
	{
		if (isShowingMessage)
		{
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
			canvasGroup.alpha = 1f;
		}
		else
		{
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
			canvasGroup.alpha = 0f;
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (isShowingMessage && !_currentMessage.IsValid())
		{
			CloseMessage(DewMessageSettings.ButtonType.Cancel);
		}
	}
}
