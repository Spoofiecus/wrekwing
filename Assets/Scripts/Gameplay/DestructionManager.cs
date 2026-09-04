using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WreckWing.Core;

namespace WreckWing.Gameplay
{
  public enum StructureMaterial { Glass, Wood, Concrete, Steel }
  public enum DebrisMaterial { Concrete, Glass, Wood, Metal }

  /// <summary>Handles structure destruction, chain reactions, debris, and progress.</summary>
  public class DestructionManager : MonoBehaviour
  {
    public static DestructionManager Instance { get; private set; }

    [Header("Destruction Settings")]
    [SerializeField] private float chainReactionDelay = 0.3f;
    [SerializeField] private float debrisLifetime = 5f;
    [SerializeField] private float glassBreakThreshold = 10f;
    [SerializeField] private float woodBreakThreshold = 25f;
    [SerializeField] private float concreteBreakThreshold = 50f;
    [SerializeField] private float steelBreakThreshold = 100f;
    [SerializeField] private GameObject debrisPrefab;

    private readonly List<DestructibleStructure> _destructibleObjects = new List<DestructibleStructure>();
    private int _totalStructuresDestroyed;
    private float _totalDestructionPercent;

    public event System.Action<DestructibleStructure, float> OnStructureDestroyed;
    public event System.Action<float> OnDestructionPercentChanged;
    public event System.Action<int> OnTotalStructuresChanged;

    public float TotalDestructionPercent => _totalDestructionPercent;
    public int TotalStructuresDestroyed => _totalStructuresDestroyed;

    private void Awake()
    {
      if (Instance != null && Instance != this) { Destroy(gameObject); return; }
      Instance = this;
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }

    public void RegisterStructure(DestructibleStructure structure)
    {
      if (structure != null && !_destructibleObjects.Contains(structure)) _destructibleObjects.Add(structure);
    }

    public void UnregisterStructure(DestructibleStructure structure)
    {
      if (structure != null) _destructibleObjects.Remove(structure);
    }

    public void ProcessImpact(Vector3 impactPoint, float impactForce, float explosionRadius)
    {
      StartCoroutine(ProcessImpactCoroutine(impactPoint, impactForce, explosionRadius));
    }

    private IEnumerator ProcessImpactCoroutine(Vector3 impactPoint, float impactForce, float explosionRadius)
    {
      List<DestructibleStructure> list = GetStructuresInRadius(impactPoint, explosionRadius);
      list.Sort((a, b) => (a.transform.position - impactPoint).sqrMagnitude
        .CompareTo((b.transform.position - impactPoint).sqrMagnitude));

      foreach (DestructibleStructure s in list)
      {
        if (s == null || s.IsDestroyed) continue;
        float force = impactForce * (1f - Vector3.Distance(s.transform.position, impactPoint) / explosionRadius);
        if (force < s.BreakThreshold) continue;

        s.BreakStructure(force);
        _totalStructuresDestroyed++;
        CalculateTotalDestructionPercent();
        OnStructureDestroyed?.Invoke(s, force);
        OnTotalStructuresChanged?.Invoke(_totalStructuresDestroyed);
        OnDestructionPercentChanged?.Invoke(_totalDestructionPercent);
        yield return new WaitForSeconds(chainReactionDelay);
      }
    }

    public List<DestructibleStructure> GetStructuresInRadius(Vector3 center, float radius)
    {
      return _destructibleObjects.FindAll(s => s != null && (s.transform.position - center).sqrMagnitude <= radius * radius);
    }

    public float CalculateTotalDestructionPercent()
    {
      if (_destructibleObjects.Count == 0) { _totalDestructionPercent = 0f; return 0f; }
      int destroyed = _destructibleObjects.FindAll(s => s != null && s.IsDestroyed).Count;
      _totalDestructionPercent = (float)destroyed / _destructibleObjects.Count * 100f;
      return _totalDestructionPercent;
    }

    public void SpawnDebris(Vector3 position, Vector3 force, DebrisMaterial material)
    {
      GameObject debris = ObjectPooler.Instance?.SpawnFromPool("Debris", position, Quaternion.identity);
      if (debris == null) return;
      Rigidbody rb = debris.GetComponent<Rigidbody>();
      if (rb != null)
      {
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
      }
      debris.GetComponent<DebrisObject>()?.Initialize(material, debrisLifetime);
    }

    public float GetBreakThreshold(StructureMaterial material)
    {
      if (material == StructureMaterial.Glass) return glassBreakThreshold;
      if (material == StructureMaterial.Wood) return woodBreakThreshold;
      if (material == StructureMaterial.Concrete) return concreteBreakThreshold;
      return steelBreakThreshold;
    }

    public void ResetDestruction()
    {
      StopAllCoroutines();
      _totalStructuresDestroyed = 0;
      _totalDestructionPercent = 0f;
      foreach (DestructibleStructure s in _destructibleObjects)
      {
        if (s != null) s.ResetStructure();
      }
      OnTotalStructuresChanged?.Invoke(_totalStructuresDestroyed);
      OnDestructionPercentChanged?.Invoke(_totalDestructionPercent);
    }
  }
}
