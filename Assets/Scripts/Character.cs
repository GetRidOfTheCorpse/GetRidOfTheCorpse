using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    interface Character
    {
        Transform GetTransform();
        BoundingCircle GetBoundingCircle();

    }
}
