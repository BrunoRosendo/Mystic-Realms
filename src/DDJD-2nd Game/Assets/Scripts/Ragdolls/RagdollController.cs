using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    private Damageable _damageable;
    private Rigidbody[] _rigidbodies; //Rig rigibodies
    private Collider[] _colliders; //Rig colliders

    [SerializeField]
    private Collider _collider; //Collider to aid in ragdoll get up

    private Collider _triggerCollider; //Main Collider
    private Rigidbody _rb; //Main rigidbody
    private Animator _animator;

    [SerializeField]
    private float _knockbackForceMultiplier = 3f;

    [SerializeField]
    private bool _deactivateBones = false;

    [SerializeField]
    private List<string> _animationsLoaded = new List<string>();
    private Dictionary<string, BoneTransform[]> _animationInitialBones =
        new Dictionary<string, BoneTransform[]>();

    private Transform[] _bones;

    public Transform[] Bones
    {
        get { return _bones; }
    }

    private Transform _hipsBone;

    public Transform HipsBone
    {
        get { return _hipsBone; }
    }

    private bool _isRagdollActive;
    public bool IsRagdollActive
    {
        get { return _isRagdollActive; }
    }

    private bool _originalIsTrigger;

    [SerializeField]
    private bool _setBoneComponentsOnAwake = true;

    // List of objects that damaged this ragdoll in this frame.
    private List<GameObject> damageInteractions;

    // Start is called before the first frame update
    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rigidbodies = GetComponentsInChildren<Rigidbody>();
        _damageable = GetComponent<Damageable>();
        //Remove main rigidbody from array
        List<Rigidbody> rigidbodies = new List<Rigidbody>(_rigidbodies);
        rigidbodies.Remove(_rb);
        _rigidbodies = rigidbodies.ToArray();

        _colliders = GetComponentsInChildren<Collider>();

        //Remove colliders that contains Hitbox
        List<Collider> colliders = new List<Collider>(_colliders);
        foreach (Collider collider in _colliders)
        {
            if (collider.gameObject.GetComponent<Hitbox>() != null)
            {
                colliders.Remove(collider);
            }
        }
        _colliders = colliders.ToArray();

        _triggerCollider = GetComponent<Collider>();
        _animator = GetComponent<Animator>();
        _hipsBone = _animator.GetBoneTransform(HumanBodyBones.Hips);
        _bones = GetRagdollTransforms();
        foreach (string animation in _animationsLoaded)
        {
            PopulateAnimationBones(animation);
        }
        _originalIsTrigger = _collider.isTrigger;
        _isRagdollActive = false;
        damageInteractions = new List<GameObject>();
    }

    public void Start()
    {
        if (_setBoneComponentsOnAwake)
        {
            foreach (Rigidbody rb in _rigidbodies)
            {
                BoneComponent boneComponent = rb.GetComponent<BoneComponent>();
                if (boneComponent == null)
                {
                    boneComponent = rb.gameObject.AddComponent<BoneComponent>() as BoneComponent;
                }
                boneComponent.SetRagdollController(this);
            }
        }

        DeactivateRagdoll();
    }

    public void AddDamageInteraction(GameObject gameObject)
    {
        damageInteractions.Add(gameObject);
        StartCoroutine(ReleaseDamageInteraction(gameObject));
    }

    public bool CanDamage(GameObject gameObject)
    {
        return !damageInteractions.Contains(gameObject);
    }

    public Transform GetRagdollTransform()
    {
        return _hipsBone.transform;
    }

    public void ActivateRagdoll()
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
        }
        foreach (Collider col in _colliders)
        {
            col.enabled = true;
        }
        _collider.enabled = false;
        _triggerCollider.enabled = false;
        _rb.isKinematic = true;
        _animator.enabled = false;
        _isRagdollActive = true;
    }

    public float GetVelocity()
    {
        return _rigidbodies[0].velocity.magnitude;
    }

    public Transform[] GetRagdollTransforms()
    {
        return _hipsBone.GetComponentsInChildren<Transform>();
    }

    public void DeactivateRagdoll()
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }
        foreach (Collider col in _colliders)
        {
            if (_deactivateBones)
            {
                col.enabled = false;
            }
            col.isTrigger = false;
        }
        _collider.enabled = true;
        _triggerCollider.enabled = true;
        _triggerCollider.isTrigger = true;
        _collider.isTrigger = _originalIsTrigger;
        _rb.isKinematic = false;
        _animator.enabled = true;
        _isRagdollActive = false;
    }

    public void PushRagdoll(int force, Vector3 hitPoint, Vector3 hitDirection)
    {
        //Vector3 direction = hitPoint - transform.position;
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();

        //Get rigidbody closest to hitpoint
        Rigidbody closestRb = rigidbodies[0];
        float closestDistance = Vector3.Distance(rigidbodies[0].transform.position, hitPoint);
        float totalForce = force * _knockbackForceMultiplier;
        foreach (Rigidbody rb in rigidbodies)
        {
            float distance = Vector3.Distance(rb.transform.position, hitPoint);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestRb = rb;
            }
            rb.AddForce(totalForce * 0.2f * hitDirection, ForceMode.Impulse);
        }

        //Add force to closest rigidbody
        closestRb.AddForce(hitDirection * totalForce, ForceMode.Impulse);
    }

    public void AlignPositionWithHips(string animationName)
    {
        Vector3 originalPosition = _hipsBone.position;
        transform.position = _hipsBone.position;

        BoneTransform hipsAnimationBone = _animationInitialBones[animationName][0];
        Vector3 positionOffset = hipsAnimationBone.Position;
        positionOffset.y = 0;
        positionOffset = transform.rotation * positionOffset;
        transform.position -= positionOffset;

        if (Physics.Raycast(_hipsBone.position, Vector3.down, out RaycastHit hit, 1f))
        {
            transform.position = new Vector3(
                transform.position.x,
                hit.point.y,
                transform.position.z
            );
        }

        _hipsBone.position = originalPosition;
    }

    public void AlignRotationWithHips()
    {
        Vector3 originalPosition = _hipsBone.position;
        Quaternion originalHipsRotaiton = _hipsBone.rotation;

        Vector3 desiredDirection = _hipsBone.up * 1f;
        desiredDirection.y = 0;

        bool isfacingUp = _hipsBone.forward.y > 0;
        if (isfacingUp)
        {
            desiredDirection *= -1;
        }
        desiredDirection.Normalize();

        Quaternion fromToRotation = Quaternion.FromToRotation(transform.forward, desiredDirection);
        transform.rotation *= fromToRotation;

        _hipsBone.position = originalPosition;
        _hipsBone.rotation = originalHipsRotaiton;
    }

    public void PopulateAnimationBones(string animationName)
    {
        Vector3 positionBeforeSampling = transform.position;
        Quaternion rotationBeforeSampling = transform.rotation;
        foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animationName)
            {
                clip.SampleAnimation(gameObject, 0f);
                break;
            }
        }

        BoneTransform[] boneTransforms = new BoneTransform[_bones.Length];
        PopulateBoneTransforms(boneTransforms);
        _animationInitialBones.Add(animationName, boneTransforms);

        //Need to reset the position and rotation of the character after sampling
        transform.position = positionBeforeSampling;
        transform.rotation = rotationBeforeSampling;
    }

    private void PopulateBoneTransforms(BoneTransform[] boneTransforms)
    {
        for (int i = 0; i < boneTransforms.Length; i++)
        {
            Transform bone = _bones[i].transform;
            boneTransforms[i] = new BoneTransform(bone.localPosition, bone.localRotation);
        }
    }

    public BoneTransform[] GetAnimationInitialBones(string animationName)
    {
        return _animationInitialBones[animationName];
    }

    private IEnumerator ReleaseDamageInteraction(GameObject gameObject)
    {
        yield return 0;
        //Remove from next frame
        if (gameObject != null)
        {
            damageInteractions.Remove(gameObject);
        }
        else
        {
            // Remove nulls
            damageInteractions.RemoveAll(item => item == null);
        }
    }

    public Damageable Damageable
    {
        get { return _damageable; }
    }

    public void SetPositionToRagdoll()
    {
        _hipsBone.parent = null;
        transform.position = _hipsBone.position;
        _hipsBone.parent = transform;
    }
}
