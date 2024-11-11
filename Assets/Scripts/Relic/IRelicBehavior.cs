using System;
using System.Collections.Generic;
using R3;

public interface IRelicBehavior
{
    void ApplyEffect(RelicUI relicUI);
    void RemoveEffect();
}