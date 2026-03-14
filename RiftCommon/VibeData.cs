namespace RiftCommon;

public record struct VibeData(int MaxVibeBonus, Activation[] SingleVibeActivations, Activation[] DoubleVibeActivations) {
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