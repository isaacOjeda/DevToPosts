namespace ApiBackgroundChannels.Jobs;

// Enum para los tipos de comandos
public enum CommandType
{
    Start,
    Stop,
    GetStatus
}

// Clase que representa un comando para el background job
public class JobCommand
{
    public CommandType Type { get; set; }
    public TaskCompletionSource<JobStatus>? ResponseTask { get; set; }
}

// Clase que representa el estado del job
public class JobStatus
{
    public bool IsRunning { get; set; }
    public int ExecutionCount { get; set; }
    public DateTime? LastExecutionTime { get; set; }
    public string Message { get; set; } = string.Empty;
}