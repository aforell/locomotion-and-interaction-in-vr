using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.PoseDetection;
using UnityEngine;

public interface Movable
{
    // other : Object to attach to
    public void attach (Transform other, GrapplingHook gp);

    public void detach (GrapplingHook gp);
}
