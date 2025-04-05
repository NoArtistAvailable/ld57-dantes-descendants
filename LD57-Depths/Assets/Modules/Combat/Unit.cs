using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LD57
{
    [Serializable]
    public class Unit
    {
        public string name;
        public int seed = 0;
        public Texture2D faceTexture;

        public float baseHealth = 10;
        public float Health => cards.OfType<IManipulateHealth>()
            .Aggregate(baseHealth, (current, card) => card.ManipulateHealth(current));
        
        public float baseSpeed = 1;
        public float Speed => cards.OfType<IManipulateSpeed>()
            .Aggregate(baseHealth, (current, card) => card.ManipulateSpeed(current));
        
        public float basePower = 1;
        public float Power => cards.OfType<IManipulatePower>()
            .Aggregate(baseHealth, (current, card) => card.ManipulatePower(current));
        
        public List<Card> cards = new List<Card>();

        public Unit(string name, int seed)
        {
            this.name = name;
            this.seed = seed;
        }
        
    }

    public interface IManipulateHealth
    {
        public float ManipulateHealth(float value);
    }
    public interface IManipulateSpeed
    {
        public float ManipulateSpeed(float value);
    }
    public interface IManipulatePower
    {
        public float ManipulatePower(float value);
    }
}