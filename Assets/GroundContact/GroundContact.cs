using System.Text;
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
        [SerializeField] private Vector3 m_NormalOrigin;
        [SerializeField, Range(0.0f, 10.0f)] private float m_Radius;
        [SerializeField, Range(0.0f, 1.0f)] private float m_Distance;
        [SerializeField, Range(0.0f, 5.0f)] private float m_NormalDistance;
        [SerializeField] private QueryTriggerInteraction m_Interaction;
        [SerializeField, Range(0, 20)] private int m_BufferSize;

        private RaycastHit[] m_NormalHit;
        private Rigidbody m_Rigidbody;
        private Collider[] m_Collide;
        private RaycastHit[] m_Hits;
        private GContactResult m_Result;
        private GContactResult m_Prev_Result;
        public GContactResult Result => m_Result;
        public GContactResult Prev_Result => m_Prev_Result;

        private void Start() {
            m_Collide = new Collider[m_BufferSize];
            m_Hits = new RaycastHit[m_BufferSize];
            m_NormalHit = new RaycastHit[1];
            if (m_BufferSize <= 0) throw new StackOverflowException("Buffer size is less than 0.");
            if (!TryGetComponent(out m_Rigidbody)) throw new NullReferenceException();
        }

#if UNITY_EDITOR
        private void FixedUpdate() {
            if (!m_UseDebugLog) return;
            Debug.Log(m_Result.ToString());
            Debug.DrawRay(m_Rigidbody.position, m_Result.normal * 5, Color.blue, 0.1f);
        }
#endif

        private void InitValue() {
            m_Result = GContactResult.Zero;
            Array.Clear(m_Collide, 0, m_Collide.Length);
            Array.Clear(m_Hits, 0, m_Hits.Length);
        }

        /// <summary>
        /// 値の代入
        /// </summary>
        /// <param name="isGround"></param>
        /// <param name="angle"></param>
        /// <param name="normal"></param>
        private void SetValue(bool isGround, float angle, Vector3 normal) {
            m_Result = new GContactResult(isGround, angle, normal);
        }

        /// <summary>
        /// 接地判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnGround() {
            m_Prev_Result = m_Result;
            InitValue();
            Vector3 playerPos = m_Rigidbody.position + m_Origin;
            Vector3 normal = GetNormal(playerPos);
            switch (Physics.CheckSphere(playerPos, m_Radius, m_GroundLayer, m_Interaction)) {
                case true: {
                        var result = Overlap(playerPos, normal);
                        SetValue(result.hit, result.angle, result.normal);
                    }
                    break;
                case false: {
                        var result = NoOverlap(playerPos, normal);
                        SetValue(result.hit, result.angle, result.normal);
                    }
                    break;
            }
        }

        /// <summary>
        /// 地面の法線を取得
        /// </summary>
        /// <param name="StartPos"></param>
        /// <returns></returns>
        private Vector3 GetNormal(Vector3 StartPos) {
            Physics.RaycastNonAlloc(StartPos + m_NormalOrigin, Vector3.down, m_NormalHit, m_NormalDistance, m_GroundLayer, m_Interaction);
            return m_NormalHit[0].normal;
        }

        /// <summary>
        /// 重なってる場合
        /// </summary>
        /// <param name="StartPos"></param>
        /// <param name="Normal"></param>
        private (bool hit, float angle, Vector3 normal) Overlap(Vector3 StartPos, Vector3 Normal) {
            Physics.OverlapSphereNonAlloc(StartPos, m_Radius, m_Collide, m_GroundLayer, m_Interaction);

            foreach (var _collider in m_Collide) {
                if (_collider == null) continue;
                foreach (var _tag in m_GroundTag)
                    return (_collider.transform.CompareTag(_tag), Vector3.Angle(Vector3.up, Normal), Normal);
            }

            return (false, 0, Vector3.up);
        }

        /// <summary>
        /// 重なってない場合
        /// </summary>
        /// <param name="StartPos"></param>
        /// <param name="Normal"></param>
        private (bool hit, float angle, Vector3 normal) NoOverlap(Vector3 StartPos, Vector3 Normal) {
            Physics.SphereCastNonAlloc(StartPos, m_Radius, Vector3.down, m_Hits, m_Distance, m_GroundLayer, m_Interaction);

            foreach (var _hit in m_Hits) {
                if (_hit.transform == null) continue;
                foreach (var _tag in m_GroundTag)
                    return (_hit.transform.CompareTag(_tag), Vector3.Angle(Vector3.up, Normal), Normal);
            }
            return (false, 0, Vector3.up);
        }
    }

    public readonly struct GContactResult
    {
        public readonly bool isGround;
        public readonly float angle;
        public readonly Vector3 normal;
        public static GContactResult Zero = new GContactResult();

        public GContactResult(bool isGround = false, float angle = 0.0f, Vector3 normal = default) {
            this.isGround = isGround;
            this.angle = angle;
            this.normal = normal;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("isGround ").Append(isGround).Append(" : ");
            sb.Append("angle ").Append(angle).Append(" : ");
            sb.Append("normal ").Append(normal);
            return sb.ToString();
        }
    }
}