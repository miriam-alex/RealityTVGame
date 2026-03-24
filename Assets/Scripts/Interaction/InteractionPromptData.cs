public readonly struct InteractionPromptData
{
    public readonly string PrimaryAction;
    public readonly string SecondaryAction;
    public readonly bool HasSecondary;

    public InteractionPromptData(string primaryAction, string secondaryAction, bool hasSecondary)
    {
        PrimaryAction = primaryAction;
        SecondaryAction = secondaryAction;
        HasSecondary = hasSecondary;
    }

    public static InteractionPromptData PrimaryOnly(string primaryAction)
        => new InteractionPromptData(primaryAction, string.Empty, false);

    public static InteractionPromptData PrimaryAndSecondary(string primaryAction, string secondaryAction)
        => new InteractionPromptData(primaryAction, secondaryAction, true);
}
