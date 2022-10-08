using UnityEngine;

// TODO: useful methods, but unfortunately they don't seem to work properly.
// Simply comparing the gameobject's layer to see if its equivalent to the `LayerMask.NameToLayer()` that I am checking
// returns true while both of these methods say its not. Fix at a later time.

public static class LayerMaskExtensions
{
    public static bool IsInLayerMask(this LayerMask mask, int layer) 
    {
        return (mask & (1 << layer)) != 0;
    }
         
    public static bool IsInLayerMask(this LayerMask mask, GameObject obj) 
    {
        return (mask & (1 << obj.layer)) != 0;
    }
}
