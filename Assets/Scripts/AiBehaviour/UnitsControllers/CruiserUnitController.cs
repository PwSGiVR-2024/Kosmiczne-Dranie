using UnityEngine;

public class CruiserUnitController : AiController
{
    [SerializeField] private WeaponController kin_1;
    [SerializeField] private WeaponController kin_2;
    [SerializeField] private WeaponController kin_3;
    [SerializeField] private WeaponController kin_4;
    [SerializeField] private WeaponController las_1;
    [SerializeField] private WeaponController las_2;
    [SerializeField] private WeaponController las_3;
    [SerializeField] private WeaponController las_4;
    [SerializeField] private WeaponController roc_1;
    [SerializeField] private WeaponController roc_2;
    [SerializeField] private WeaponController pointDef_1;
    [SerializeField] private WeaponController pointDef_2;


    [SerializeField] private AiController targetCtrl;
    public int healthCtrl;

    protected override void AdditionalInit()
    {
        if (kin_1) kin_1.Init(this);
        if (kin_2) kin_2.Init(this);
        if (kin_3) kin_3.Init(this);
        if (kin_4) kin_4.Init(this);
        if (las_1) las_1.Init(this);
        if (las_2) las_2.Init(this);
        if (las_4) las_4.Init(this);
        if (las_3) las_3.Init(this);
        if (roc_1) roc_1.Init(this);
        if (roc_2) roc_2.Init(this);
        if (pointDef_1) pointDef_1.Init(this);
        if (pointDef_2) pointDef_2.Init(this);
    }

    protected override void BeforeDeactivation()
    {

    }

    protected override void CombatState()
    {
        if (Target.distance <= Values.attackDistance)
        {
            if (kin_1 && kin_1.isActiveAndEnabled && kin_1.CheckIfFacingTarget(Target.position))
                kin_1.FireProjectile();

            if (kin_2 && kin_2.isActiveAndEnabled && kin_2.CheckIfFacingTarget(Target.position))
                kin_2.FireProjectile();

            if (kin_3 && kin_3.isActiveAndEnabled && kin_3.CheckIfFacingTarget(Target.position))
                kin_3.FireProjectile();

            if (kin_4 && kin_4.isActiveAndEnabled && kin_4.CheckIfFacingTarget(Target.position))
                kin_4.FireProjectile();

            if (las_1 && las_1.isActiveAndEnabled && las_1.CheckIfFacingTarget(Target.position))
                las_1.FireProjectile();

            if (las_2 && las_2.isActiveAndEnabled && las_2.CheckIfFacingTarget(Target.position))
                las_2.FireProjectile();

            if (las_3 && las_3.isActiveAndEnabled && las_3.CheckIfFacingTarget(Target.position))
                las_3.FireProjectile();

            if (las_4 && las_1.isActiveAndEnabled && las_4.CheckIfFacingTarget(Target.position))
                las_4.FireProjectile();

            if (roc_1 && roc_1.isActiveAndEnabled)
                roc_1.FireProjectile();

            if (roc_2 && roc_2.isActiveAndEnabled)
                roc_2.FireProjectile();
        }

        if (Target.distance <= Values.attackDistance * 0.5f)
        {
            if (pointDef_1 && pointDef_1.isActiveAndEnabled && pointDef_1.CheckIfFacingTarget(Target.position))
                pointDef_1.FireProjectile();

            if (pointDef_2 && pointDef_2.isActiveAndEnabled && pointDef_2.CheckIfFacingTarget(Target.position))
                pointDef_2.FireProjectile();
        }
    }

    protected override void IdleState()
    {

    }

    protected override void MovingState()
    {
        if (Agent.hasPath && Agent.remainingDistance <= 1f)
            SetIdleState();
    }

    protected override void OnTargetPositionChanged()
    {
        SetCombatState();

        if (kin_1) kin_1.RotateWeaponAtPosition(Target.position);
        if (kin_2) kin_2.RotateWeaponAtPosition(Target.position);
        if (kin_3) kin_3.RotateWeaponAtPosition(Target.position);
        if (kin_4) kin_4.RotateWeaponAtPosition(Target.position);
        if (las_1) las_1.RotateWeaponAtPosition(Target.position);

        if (Target.distance > Values.attackDistance)
        {
            Agent.isStopped = false;
            Agent.SetDestination(Target.position);
        }

        else
            Agent.isStopped = true;
    }

    protected override void RetreatState()
    {

    }

    protected override void UpdateOperations()
    {
        base.UpdateOperations();
        healthCtrl = Health;
        Target.TryLockTarget(out targetCtrl);
    }

    public override void SetCombatState()
    {
        base.SetCombatState();
    }
    public override void SetIdleState()
    {
        base.SetIdleState();

        Agent.isStopped = false;
    }

    public override void SetMovingState(Vector3 pos)
    {
        base.SetMovingState(pos);

        Agent.isStopped = false;
    }
}
