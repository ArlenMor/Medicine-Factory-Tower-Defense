namespace _Project.Code.Scripts.Data.TaskData
{
    public static class TaskResultTypeExtensions
    {
        public static string ToDisplayString(this TaskResultType type) => type switch
        {
            TaskResultType.ClassicOfficeChair => "Office Chair",
            TaskResultType.FactoryChair => "Factory Chair",
            TaskResultType.SimpleErgoChair => "Simple Ergo Chair",
            TaskResultType.BoxChair => "Box Chair",
            TaskResultType.ControlChair => "Control Chair",
            TaskResultType.ReinforcedSeat => "Reinforced Seat",
            TaskResultType.MetalFrameChair => "Metal Frame Chair",
            TaskResultType.CommandSeat => "Command Seat",
            TaskResultType.PaddedTechChair => "Padded Tech Chair",
            TaskResultType.IndustrialConsoleChair => "Industrial Console Chair",
            _                    => "—"
        };
    }
}
