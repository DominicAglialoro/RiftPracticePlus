namespace RiftCommon;

public readonly struct VibeData {
    public readonly int MaxVibeBonus;
    public readonly Activation[] SingleVibeActivations;
    public readonly Activation[] DoubleVibeActivations;

    public VibeData(int maxVibeBonus, Activation[] singleVibeActivations, Activation[] doubleVibeActivations) {
        MaxVibeBonus = maxVibeBonus;
        SingleVibeActivations = singleVibeActivations;
        DoubleVibeActivations = doubleVibeActivations;
    }

    public bool TryGetActivationAt(double time, bool isDouble, out Activation activation) {
        var activations = isDouble ? DoubleVibeActivations : SingleVibeActivations;
        int min = 0;
        int max = activations.Length - 1;

        while (max >= min) {
            int mid = (min + max) / 2;

            if (activations[mid].MinStartTime > time)
                max = mid - 1;
            else if (activations[mid].MaxStartTime <= time)
                min = mid + 1;
            else {
                activation = activations[mid];

                return true;
            }
        }

        activation = default;

        return false;
    }
}