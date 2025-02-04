using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityLabs.Slices.Games.Chess;

public class HexBoardUI : MonoBehaviour, IGameUI
{
    [Inject]
    HexGridManager m_Board;

    [Inject]
    NetworkGameManager m_NetworkGameManager;

    [SerializeField]
    XRBaseInteractable[] m_StartReceiver = null;

    [SerializeField]
    Button[] m_StartButtons = null;

    [SerializeField]
    float[] m_StartYAngles = { 0f, 270f, 180f, 90f };

    void OnEnable()
    {
        for (int i = 0; i < m_StartReceiver.Length; ++i)
        {
            float yAngle = m_StartYAngles[i];
            m_StartReceiver[i].selectEntered.AddListener(args => StartGame(yAngle));
        }

        for (int i = 0; i < m_StartButtons.Length; ++i)
        {
            float yAngle = m_StartYAngles[i];
            m_StartButtons[i].onClick.AddListener(() => StartGame(yAngle));
        }
    }

    void OnDisable()
    {
        for (int i = 0; i < m_StartReceiver.Length; ++i)
        {
            m_StartReceiver[i].selectEntered.RemoveListener(args => StartGame(m_StartYAngles[i]));
        }

        for (int i = 0; i < m_StartButtons.Length; ++i)
        {
            m_StartButtons[i].onClick.RemoveListener(() => StartGame(m_StartYAngles[i]));
        }
    }

    public void SetStartButtonsActive(bool state)
    {
        for (int i = 0; i < m_StartReceiver.Length; ++i)
        {
            m_StartReceiver[i].enabled = state;
        }

        for (int i = 0; i < m_StartButtons.Length; ++i)
        {
            m_StartButtons[i].interactable = state;
        }
    }

    async void StartGame(float rotateYAngle)
    {
        m_Board.boardRotation.Value = rotateYAngle;
        m_NetworkGameManager.StartGame();
        await DelayDeactivateUniTask();
    }

    async UniTask DelayDeactivateUniTask()
    {
        await UniTask.Delay(500);
        foreach (var receiver in m_StartReceiver)
        {
            receiver.gameObject.SetActive(false);
        }
        foreach (var button in m_StartButtons)
        {
            button.gameObject.SetActive(false);
        }
    }

    public void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 100), "StartGame"))
        {
            StartGame(m_StartYAngles[0]);
        }
    }
}
