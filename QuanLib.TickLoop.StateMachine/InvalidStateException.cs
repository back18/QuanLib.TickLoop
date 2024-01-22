using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.TickLoop.StateMachine
{
    public class InvalidStateException : Exception
    {
        public InvalidStateException() : base(DefaultMessage) { }

        public InvalidStateException(string? message) : base(message) { }

        public InvalidStateException(object? sourceState)
        {
            SourceState = sourceState;
        }

        public InvalidStateException(object? sourceState, string? message) : base(message)
        {
            SourceState = sourceState;
        }

        public InvalidStateException(object sourceState, object? targetState)
        {
            ArgumentNullException.ThrowIfNull(sourceState, nameof(sourceState));

            SourceState = sourceState;
            TargetState = targetState;
        }

        public InvalidStateException(object sourceState, object? targetState, string? message) : base(message)
        {
            ArgumentNullException.ThrowIfNull(sourceState, nameof(sourceState));

            SourceState = sourceState;
            TargetState = targetState;
        }

        public object? SourceState { get; }

        public object? TargetState { get; }

        protected const string DefaultMessage = "当前状态下无法完成此操作";

        public override string Message
        {
            get
            {
                string? s = base.Message;
                string message;
                if (SourceState is not null && TargetState is not null)
                    message = $"无法从“{SourceState}”状态切换到“{TargetState}”状态";
                else if (SourceState is not null)
                    message = $"当前状态下无法切换到“{TargetState}”状态";
                else
                    return s ?? string.Empty;
                if (s is null)
                    return message;
                else
                    return s + Environment.NewLine + message;
            }
        }
    }
}
