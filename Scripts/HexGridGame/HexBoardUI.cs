using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using Unity.Netcode;
using UnityEngine;
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
    float[] m_StartYAngles = { 0f, 270f, 180f, 90f };

    void OnEnable()
    {
        for (int i = 0; i < m_StartReceiver.Length; ++i)
        {
            float yAngle = m_StartYAngles[i];
            m_StartReceiver[i].selectEntered.AddListener(args => StartGame(yAngle));
        }
    }

    void OnDisable()
    {
        for (int i = 0; i < m_StartReceiver.Length; ++i)
        {
            m_StartReceiver[i].selectEntered.RemoveListener(args => StartGame(m_StartYAngles[i]));
        }
    }

    public void SetStartButtonsActive(bool state)
    {
        for (int i = 0; i < m_StartReceiver.Length; ++i)
        {
            m_StartReceiver[i].enabled = state;
        }
    }

    async void StartGame(float rotateYAngle)
    {
        //m_Board.ClaimBoardStateOwnership();
        m_Board.boardRotation.Value = rotateYAngle;
        m_NetworkGameManager.StartGame();
        await DelayDeactivateUniTask();
        //Sequence sequence = DOTween.Sequence();
        //sequence.SetDelay(0.5f);
        //sequence.AppendCallback(() =>
        //{
        //    foreach (var receiver in m_StartReceiver)
        //    {
        //        receiver.gameObject.SetActive(false);
        //    }
        //});
        //sequence.Play();

        //m_Board.boardRotation.Value = rotateYAngle;

        //if (NetworkManager.Singleton.IsServer && NetworkManager.Singleton.ConnectedClients.Count == 1)
        //{

        //    Debug.Log("Starting game with one player. Starting Human vs AI.");
        //    m_Board.StartGame(VersusGameMode.HumanVsAI, true);
        //}
        //else
        //{
        //    Debug.Log("Starting game with two players. Starting Human vs AI.");
        //    m_Board.StartGame(VersusGameMode.HumanVsHuman, true);
        //}
    }

    async UniTask DelayDeactivateUniTask()
    {
        await UniTask.Delay(500);
        foreach (var receiver in m_StartReceiver)
        {
            receiver.gameObject.SetActive(false);
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
