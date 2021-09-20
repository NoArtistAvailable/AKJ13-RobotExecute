using System;
using UnityEngine;
using UnityEngine.Events;

public class WinCheck : MonoBehaviour
{
    static WinCheck _instance;// = new Lazy<PathPlaner>(FindObjectOfType<PathPlaner>);

    public static WinCheck Instance
    {
        get
        {
            if (!_instance) _instance = FindObjectOfType<WinCheck>();
            return _instance;
        }
    }
    
    public float radius = 5f;

    public Robot.Faction Check()
    {
        var colliders = Physics.OverlapSphere(transform.position, radius);
        int factionA = 0;
        int factionB = 0;
        int factionC = 0; 
        int factionD = 0;
        foreach (var collider in colliders)
        {
            var robot = collider.GetComponentInParent<Robot>();
            if (!robot) continue;
            if (!robot.living) continue;
            switch (robot.faction)
            {
                case Robot.Faction.A:
                    factionA++;
                    break;
                case Robot.Faction.B:
                    factionB++;
                    break;
                case Robot.Faction.C:
                    factionC++;
                    break;
                case Robot.Faction.D:
                    factionD++;
                    break;
            }
        }
        if (factionA >= factionB && factionA >= factionC && factionA >= factionD) return Robot.Faction.A;
        if (factionB >= factionC && factionB >= factionD) return Robot.Faction.B;
        if (factionC >= factionD) return Robot.Faction.C;
        return Robot.Faction.D;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    
}
