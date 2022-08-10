using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace GContacts
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class GroundContact : MonoBehaviour
    {
        #if UNITY_EDITOR
        [SerializeField] private bool m_UseDebugLog;
        #endif

        [Header("Ground Settings")] 
        [SerializeField] private LayerMask m_GroundLayer;
        [SerializeField] private string[] m_GroundTag;

        [Header("Collide Setting")] 
        [SerializeField] private Vector3 m_Origin;
        [SerializeField, Range(0.0f, 10.0f)] private float m_Radius;
        [SerializeField, Range(0.0f, 1.0f)] private float m_Distance;
        [SerializeField] private QueryTriggerInteraction m_Interaction;
        [SerializeField, Range(0, 20)] private int ContactSize;
        
        private Rigidbody m_Rigidbody;
        private Collider[] m_Collide;
        private RaycastHit[] m_Hits;

        private bool m_isGround;
        public bool isGround => m_isGround;
        private float m_Angle;
        public float Angle => m_Angle;
        private Vector3 m_Normal;
        public Vector3 Normal => m_Normal;
        

        private void Start()
        {
            m_Collide = new Collider[ContactSize];
            m_Hits = new RaycastHit[ContactSize];
            if (!TryGetComponent(out m_Rigidbody)) throw new NullReferenceException();
        }
#if UNITY_EDITOR
        private void FixedUpdate()
        {
            OnGround();
            if (m_UseDebugLog) Debug.Log($"{isGround}, {Angle}, {Normal}");
            Debug.DrawRay(m_Rigidbody.position, Normal * 5, Color.blue, 0.1f);
        }
#endif

        private void InitValue()
        {
            m_isGround = false;
            m_Angle = 0.0f;
            m_Normal = Vector3.zero;
            Array.Clear(m_Collide, 0, m_Collide.Length);
            Array.Clear(m_Hits, 0, m_Hits.Length);
        }

        private void SetValue(bool isGround, float angle, Vector3 normal)
        {
            m_isGround = isGround;
            m_Angle = angle;
            m_Normal = normal;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnGround()
        {
            InitValue();
            Vector3 playerPos = m_Rigidbody.position + m_Origin;
            
            switch (Physics.CheckSphere(playerPos, m_Radius, m_GroundLayer, m_Interaction))
            {
                case true:
                    Physics.OverlapSphereNonAlloc(playerPos, m_Radius, m_Collide, m_GroundLayer, m_Interaction);
                    foreach (var _collider in m_Collide)
                    {
                        if(_collider == null) continue;
                        foreach (var _tag in m_GroundTag)
                        {
                            SetValue(_collider.transform.CompareTag(_tag), Vector3.Angle(Vector3.up, _collider.transform.up), _collider.transform.up);
                            Debug.DrawLine(Vector3.zero, _collider.transform.up, Color.magenta, 0.1f);
                        }
                    }
                    break;
                case false:
                    Physics.SphereCastNonAlloc(playerPos, m_Radius, Vector3.down, m_Hits, m_Distance, m_GroundLayer, m_Interaction);
                    foreach (var _hit in m_Hits)
                    {
                        if (_hit.transform == null) continue;
                        foreach (var _tag in m_GroundTag)
                            SetValue(_hit.transform.CompareTag(_tag), Vector3.Angle(Vector3.up, _hit.normal), _hit.normal);
                    }
                    break;
            }
        }
    }
}

public static class RayExtends
{
    public static Ray Set(this Ray self, Vector3 origin, Vector3 direction)
    {
        self.origin = origin;
        self.direction = direction;
        return self;
    }
}