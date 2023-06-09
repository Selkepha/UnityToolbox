using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitboxResponder
{
    void CollidingWith(Collider2D collider);
}
