using UnityEngine;
// SOMEONE ELSE PLEASE LOOK AT THIS
public class PlayerWobble : MonoBehaviour
{
    [Header("Body Target")]
    public Transform wobbleTarget;

    [Header("Squash & stretch")]
    public float stretchAmount = 0.5f;
    public float squashAmount = 0.5f;
    public float landingSquashAmount = 1f;
    public float landingRecoverTime = 0.5f;
    public float scaleSmoothTime = 0.5f;
    public float scaleClampMin = 0.75f;
    public float scaleClampMax = 1.25f;
    public float referenceSpeed = 5f;

    [Header("Vertex jelly (original script)")]
    public float Intensity = 1f;
    public float Mass = 2f;
    public float stiffness = 0.25f;
    public float damping = 0.75f;

    private Transform _target;
    private Mesh _originalMesh;
    private Mesh _meshClone;
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private JellyVertex[] _jv;
    private Rigidbody _rb;

    private Vector3 _lastMoveDir;
    private bool _hasLastDir;
    private float _turnRate;
    private Vector3 _wobbleScale = Vector3.one;
    private Vector3 _scaleVelocity;
    private Vector3 _initialLocalScale;

    // player movement information
    private float _lastVelocityY;
    private float _landingSquashTimer;
    private bool _vertexJellyReady;

    void Start()
    {
        _rb = GetComponentInParent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogWarning("No rigidbody detected");
            enabled = false;
            return;
        }

        _target = wobbleTarget != null ? wobbleTarget : transform;
        if (_target == _rb.transform)
        {
            Debug.LogWarning("Wobble Target is assigned");
            enabled = false;
            return;
        }

        _meshFilter = _target.GetComponent<MeshFilter>();
        _meshRenderer = _target.GetComponent<MeshRenderer>();
        if (_meshFilter == null || _meshRenderer == null)
        {
            Debug.LogWarning(" No MeshFilter/MeshRenderer on target");
        }
        
        // Vertex Jelly stuff
        else
        {
            _originalMesh = _meshFilter.sharedMesh;
            _meshClone = Instantiate(_originalMesh);
            _meshFilter.sharedMesh = _meshClone;
            _jv = new JellyVertex[_meshClone.vertexCount];
            for (int i = 0; i < _jv.Length; i++)
            {
                _jv[i] = new JellyVertex(i, _target.TransformPoint(_originalMesh.vertices[i]));
            }
            _vertexJellyReady = true;
        }

        // initializes values to the specific player assigned using the target and rigidbody information
        _initialLocalScale = _target.localScale;
        _lastVelocityY = _rb.linearVelocity.y;
    }

    void Update()
    {
        // if rigidbody doesn't exist or target doesn't exist
        if (_rb == null || _target == null)
        {
            return;
        }

        _target.localRotation = Quaternion.identity;
        
         // gets velocity from rigidbody and other physics variables
        Vector3 vel = _rb.linearVelocity;
        float speed = new Vector3(vel.x, 0f, vel.z).magnitude;
        Vector3 moveDir;
        if (speed > 0.01f)
        {
            moveDir = new Vector3(vel.x, 0f, vel.z).normalized;
        }
        else
        {
            moveDir = _lastMoveDir;
        }

        // _turnRate also has an affect on how object is squashed/stretched
        if (_hasLastDir && moveDir.sqrMagnitude > 0.01f)
        {
            float angle = Vector3.SignedAngle(_lastMoveDir, moveDir, Vector3.up);
            _turnRate = Mathf.Lerp(_turnRate, Mathf.Clamp(angle / Mathf.Max(Time.deltaTime, 0.001f), -720f, 720f), 10f * Time.deltaTime);
        }
        else
        {
            _turnRate = Mathf.Lerp(_turnRate, 0f, 10f * Time.deltaTime);
        }

        if (moveDir.sqrMagnitude > 0.01f)
        {
            _lastMoveDir = moveDir;
        }
        _hasLastDir = true;

        float speedFactor = referenceSpeed > 0.01f ? Mathf.Clamp01(speed / referenceSpeed) : 0f;

        bool wasFalling = _lastVelocityY < -0.5f;
        bool nowLanded = vel.y >= -0.2f && wasFalling;
        _lastVelocityY = vel.y;
        if (nowLanded)
        {
            _landingSquashTimer = landingRecoverTime;
        }

        float landingT = _landingSquashTimer > 0 ? (landingRecoverTime - _landingSquashTimer) / landingRecoverTime : 1f;
        if (_landingSquashTimer > 0)
        {
            _landingSquashTimer -= Time.deltaTime;
            if (_landingSquashTimer < 0) _landingSquashTimer = 0;
        }

        Vector3 targetWobble = Vector3.one;

        // alters the squash upon landing
        if (landingT < 1f)
        {
            float squash = landingSquashAmount * (1f - landingT);
            targetWobble.y = 1f - squash;
            targetWobble.x = 1f + squash * 0.5f;
            targetWobble.z = 1f + squash * 0.5f;
        }

        // alters stretch and squash in XYZ direction according to the players movement
        if (speed > 0.1f)
        {
            float turnMag = Mathf.Abs(_turnRate);
            float stretch = stretchAmount * speedFactor * (1f - 0.4f * Mathf.Clamp01(turnMag * 0.02f));
            targetWobble.y *= (1f - stretch);
            targetWobble.x *= (1f + stretch * 0.5f);
            targetWobble.z *= (1f + stretch * 0.5f);
            float squash = squashAmount * speedFactor * Mathf.Clamp01(turnMag * 0.015f);
            targetWobble.y += squash;
            targetWobble.x -= squash * 0.5f;
            targetWobble.z -= squash * 0.5f;
        }

        _wobbleScale.x = Mathf.SmoothDamp(_wobbleScale.x, targetWobble.x, ref _scaleVelocity.x, scaleSmoothTime);
        _wobbleScale.y = Mathf.SmoothDamp(_wobbleScale.y, targetWobble.y, ref _scaleVelocity.y, scaleSmoothTime);
        _wobbleScale.z = Mathf.SmoothDamp(_wobbleScale.z, targetWobble.z, ref _scaleVelocity.z, scaleSmoothTime);
        float cx = Mathf.Clamp(_wobbleScale.x, scaleClampMin, scaleClampMax);
        float cy = Mathf.Clamp(_wobbleScale.y, scaleClampMin, scaleClampMax);
        float cz = Mathf.Clamp(_wobbleScale.z, scaleClampMin, scaleClampMax);
        _target.localScale = new Vector3(_initialLocalScale.x * cx, _initialLocalScale.y * cy, _initialLocalScale.z * cz);
    }

    // Vertex Jelly script
    // ensures that the mesh deforms and the "rest 
    void FixedUpdate()
    {
        if (!_vertexJellyReady || _meshClone == null || _jv == null || _meshRenderer == null || _target == null) return;

        Vector3 vel = _rb != null ? _rb.linearVelocity : Vector3.zero;
        Vector3 moveDir = new Vector3(vel.x, 0f, vel.z);
        float speed = moveDir.magnitude;
        if (speed > 0.01f) moveDir /= speed;
        float moveStretch = Mathf.Clamp01(speed / Mathf.Max(referenceSpeed, 0.1f)) * 0.08f;
        float dt = Time.fixedDeltaTime;

        Vector3[] verts = _originalMesh.vertices;
        for (int i = 0; i < _jv.Length; i++)
        {
            Vector3 target = _target.TransformPoint(verts[_jv[i].ID]);
            target += moveDir * moveStretch;
            float intensity = (1f - (_meshRenderer.bounds.max.y - target.y) / Mathf.Max(_meshRenderer.bounds.size.y, 0.001f)) * Intensity;
            _jv[i].Shake(target, Mass, stiffness, damping, dt);
            target = _target.InverseTransformPoint(_jv[i].Position);
            verts[_jv[i].ID] = Vector3.Lerp(verts[_jv[i].ID], target, intensity);
        }
        _meshClone.vertices = verts;
        _meshClone.RecalculateNormals();
        _meshClone.RecalculateBounds();
    }

    public class JellyVertex
    {
        public int ID;
        public Vector3 Position;
        public Vector3 velocity;
        public Vector3 Force;

        public JellyVertex(int id, Vector3 position)
        {
            ID = id;
            Position = position;
        }

        // Causes jiggle on PLayer prefab
        public void Shake(Vector3 target, float m, float s, float d, float dt)
        {
            Force = (target - Position) * s;
            velocity += (Force / m) * dt;
            velocity *= Mathf.Pow(d, dt);
            Position += velocity * dt;
            if (velocity.sqrMagnitude < 1e-6f && (target - Position).sqrMagnitude < 1e-6f)
                Position = target;
        }
    }
}
