public enum ThePetEnergyRecoveryProfile
{
    Idle,
    Happy,
    Sad,
    Hungry,
    Sleep
}

public static class ThePetEnergyRecoveryProfileResolver
{
    public static bool TryResolveFromState(EntityState<ThePet> state, out ThePetEnergyRecoveryProfile profile)
    {
        profile = ThePetEnergyRecoveryProfile.Idle;
        if (state == null)
        {
            return false;
        }

        if (state is IdleState)
        {
            profile = ThePetEnergyRecoveryProfile.Idle;
            return true;
        }

        if (state is HappyState || state is LyingState)
        {
            profile = ThePetEnergyRecoveryProfile.Happy;
            return true;
        }

        if (state is SadState)
        {
            profile = ThePetEnergyRecoveryProfile.Sad;
            return true;
        }

        if (state is HungryState)
        {
            profile = ThePetEnergyRecoveryProfile.Hungry;
            return true;
        }

        if (state is SleepState)
        {
            profile = ThePetEnergyRecoveryProfile.Sleep;
            return true;
        }

        return false;
    }

    public static bool IsTemporaryState(EntityState<ThePet> state)
    {
        return state is DragState
            || state is StretchState
            || state?.GetType().Name == "TimerState";
    }

    public static ThePetEnergyRecoveryProfile InferFromStats(ThePetStats stats)
    {
        if (stats == null)
        {
            return ThePetEnergyRecoveryProfile.Idle;
        }

        if (stats.satiety < 50f)
        {
            return ThePetEnergyRecoveryProfile.Hungry;
        }

        if (stats.energy < 50f)
        {
            return ThePetEnergyRecoveryProfile.Sleep;
        }

        if (stats.happiness >= 50f)
        {
            return ThePetEnergyRecoveryProfile.Happy;
        }

        return ThePetEnergyRecoveryProfile.Sad;
    }
}
