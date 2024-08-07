using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigFileAssistant_v1
{
    public class Command
    {
        public string Action { get; }
        public int? Index { get; }
        public int Value { get; }

        public Command(string action, int value, int? index = null)
        {
            Action = action;
            Value = value;
            Index = index;
        }
    }
    public class CommandHistoryManager
    {
        private readonly Stack<Command> _undoStack = new Stack<Command>();
        private readonly Stack<Command> _redoStack = new Stack<Command>();

        public void Add(Command command)
        {
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        public Command Undo()
        {
            if (_undoStack.Count == 0)
                return null;

            var command = _undoStack.Pop();
            _redoStack.Push(command);
            return command;
        }

        public Command Redo()
        {
            if (_redoStack.Count == 0)
                return null;

            var command = _redoStack.Pop();
            _undoStack.Push(command);
            return command;
        }
    }
}
