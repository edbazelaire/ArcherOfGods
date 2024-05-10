using System;
using Tools;

public class CharacterSelectionWindow: MObject
{
    #region Members

    CharacterSelectionUI m_CharacterSelection;

    public bool IsOpened => gameObject.activeInHierarchy;

    #endregion


    #region Init & End

    protected override void FindComponents()
    {
        m_CharacterSelection = Finder.FindComponent<CharacterSelectionUI>(gameObject);
    }

    protected override void SetUpUI()
    {
        base.SetUpUI();

        m_CharacterSelection.Initialize();
    }

    #endregion


    #region Open / Close

    public void Open(bool instant = false)
    {
        gameObject.SetActive(true);

        // ==========================================
        // TODO : Animation
    }

    public void Close(bool instant = false)
    {
        if (!IsOpened)
            return;

        if (instant)
        {
            gameObject.SetActive(false);
            return;
        }

        // ==========================================
        // TODO : Animation
        gameObject.SetActive(false);
        // ==========================================
    }

    #endregion
}