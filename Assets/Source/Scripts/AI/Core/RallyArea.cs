namespace AI.Core
{
    using UnityEngine;

    // Куда идут NPC, когда рядом нет врагов.
    // Roam = false: дойти до случайной точки в круге и остаться там.
    // Roam = true:  дойдя, выбрать новую случайную точку - толпа бродит без остановки.
    public readonly struct RallyArea
    {
        public readonly Vector3 Center;
        public readonly float Radius;
        public readonly bool Roam;

        public RallyArea(Vector3 center, float radius, bool roam)
        {
            Center = center;
            Radius = radius;
            Roam = roam;
        }
    }
}
