using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Models
{
    class Polus
    {
        public PolusShipStatus shipStatus;
        public GameObject gameObject;

        public Polus(PolusShipStatus shipStatus)
        {
            this.shipStatus = shipStatus;
            this.gameObject = GameObject.Find("PolusShip(Clone)");
        }
    }
}
