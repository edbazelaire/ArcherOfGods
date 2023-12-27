using UnityEngine;

public class Movement : MonoBehaviour
{
    public float InitialSpeed;

    int m_MoveX = 0;
    float m_SpeedFactor = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckInputs();
        UpdateMovement();
    }

    #region Update Methods

    /// <summary>
    /// Check if movement inputs have beed pressed
    /// </summary>
    void CheckInputs()
    {
        m_MoveX = 0;

        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A))
            m_MoveX = -1;
        else if (Input.GetKey(KeyCode.D))
            m_MoveX = 1;
    }     
    
    /// <summary>
    /// Apply speed on position
    /// </summary>
    void UpdateMovement()
    {
        transform.position += new Vector3(m_MoveX * Speed * Time.deltaTime, 0f, 0f);
    }

    #endregion


    #region Dependent Attributes

    public float Speed
    {
        get { return InitialSpeed * m_SpeedFactor; }
    }

    #endregion
}
