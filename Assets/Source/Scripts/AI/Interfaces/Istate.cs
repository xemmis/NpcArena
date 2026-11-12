using System;
using UnityEngine;

namespace AI.Interface
{
    public interface IState
    {
        void Enter(INpcController Self);
        void Update(INpcController Self);
        void Exit(INpcController Self);
    }
}
