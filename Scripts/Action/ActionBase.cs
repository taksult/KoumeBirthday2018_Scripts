using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class ActionBase : MonoBehaviour
    {
        public bool isActing = false;
        public float ActDuration { get; set; }
        protected Vector2 center = new Vector2(0.5f * Constant.Step, 0.5f * Constant.Step);

        protected float lastAct;

        public bool IsActing
        {
            get;
            set;
        }
    }
}
