using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICastSpell
{
    Source Source { get; }
    void OnSpellCastStarted();
    void OnSpellCastFinished();
}
