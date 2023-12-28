using System;
using Tools;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Game.Spells
{
    public class Arrow : MonoBehaviour
    {
        #region Members

        const string    c_TargetHight = "TargetHight"; 

        public float    Speed = 1f;
        public int      Damage = 10;

        Rigidbody2D     m_Rigidbody;

        Controller      m_Owner;
        Vector3         m_Target;
        Vector3         m_TargetLookAt;
        Vector3         m_Direction;

        #endregion

        
        #region Init & End

        public void Initialize(Controller owner)
        {
            m_Owner = owner;
            m_Rigidbody = GetComponent<Rigidbody2D>();

            // set target as first enemy
            SetTarget(GameManager.Instance.GetEnemy(owner.Team).transform.position);
        }

        void End()
        {
            Destroy(gameObject);
        }

        #endregion


        #region Inherited Manipulators  

        private void Update()
        {
            if (m_Target != m_TargetLookAt && Math.Abs(transform.position.y - m_TargetLookAt.y) < 0.1)
                SetCurrentTarget(m_Target);

            LookAt(m_TargetLookAt);
            m_Direction = (m_TargetLookAt - transform.position).normalized;
            transform.position += m_Direction * Speed * Time.deltaTime;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log("OnCollisionEnter2D with " + collision.gameObject.name);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {

            }
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Controller controller = collision.gameObject.GetComponent<Controller>();
                if (controller.Team == m_Owner.Team)
                    return;

                controller.Life.Hit(Damage);
            }
            else 
            { 
                return; 
            }

            End();
        }

        #endregion


        #region Public Manipulators

        public void SetTarget(Vector3 target)
        {
            m_Target = target;
            SetCurrentTarget(GameManager.Instance.TargetHight.transform.position);
        }

        #endregion


        #region Private Manipulators

        void SetCurrentTarget(Vector3 target)
        {
            m_TargetLookAt = target;
        }

        #endregion


        #region Private Accessors

        void LookAt(Vector3 target)
        {
            Vector3 diff = target - transform.position;
            diff.Normalize();
            float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, rot_z);
        }

        #endregion
    }
}