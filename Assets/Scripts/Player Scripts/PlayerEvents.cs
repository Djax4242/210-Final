using System;
using UnityEngine;

public static class PlayerEvents
{
    public static event Action OnBoomerangThrown;
    public static void BoomerangThrown() => OnBoomerangThrown?.Invoke();
    public static event Action OnBoomerangCollected;
    public static void BoomerangCollected() => OnBoomerangCollected?.Invoke();
}
