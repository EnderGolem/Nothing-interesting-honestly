﻿using AiCup22.Custom;
using AiCup22.Model;

namespace AiCup22
{
    public class GeneralBrain:Brain
    {
        private LootingBrain _lootingBrain;

        public GeneralBrain()
        {
            _lootingBrain = new LootingBrain();
        }

        public override UnitOrder Process(Perception perception)
        {
            return _lootingBrain.Process(perception);
        }
    }
}