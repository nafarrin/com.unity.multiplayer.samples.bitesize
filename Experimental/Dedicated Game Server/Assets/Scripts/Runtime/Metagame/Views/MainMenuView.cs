using UnityEngine.UIElements;
namespace Unity.Template.Multiplayer.NGO.Runtime
{
    internal class MainMenuView : View<MetagameApplication>
    {
        Button m_FindMatchButton;
        Button m_JoinDirectIPButton;
        Button m_QuitButton;
        Label m_TitleLabel;
        VisualElement root;

        void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;

            m_FindMatchButton = root.Q<Button>("findMatchButton");
            m_FindMatchButton.RegisterCallback<ClickEvent>(OnClickFindMatch);

            m_JoinDirectIPButton = root.Q<Button>("joinDirectIPButton");
            m_JoinDirectIPButton.RegisterCallback<ClickEvent>(OnClickJoinDirectIP);

            m_QuitButton = root.Q<Button>("quitButton");
            m_QuitButton.RegisterCallback<ClickEvent>(OnClickQuit);

            m_TitleLabel = root.Query<Label>("titleLabel");
            m_TitleLabel.text = "Game title";
        }

        void OnDisable()
        {
            m_FindMatchButton.UnregisterCallback<ClickEvent>(OnClickFindMatch);
            m_QuitButton.UnregisterCallback<ClickEvent>(OnClickQuit);
            m_JoinDirectIPButton.UnregisterCallback<ClickEvent>(OnClickJoinDirectIP);
        }

        void OnClickFindMatch(ClickEvent evt)
        {
            Broadcast(new EnterMatchmakerQueueEvent("Standard"));
        }

        void OnClickJoinDirectIP(ClickEvent evt)
        {
            Broadcast(new EnterIPConnectionEvent());
        }

        void OnClickQuit(ClickEvent evt)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}