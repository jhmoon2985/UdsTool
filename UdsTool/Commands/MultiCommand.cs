// Commands/MultiCommand.cs
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace UdsTool.Commands
{
    public class MultiCommand : ICommand
    {
        private readonly List<ICommand> _commands = new List<ICommand>();
        private readonly Func<ICommand> _commandSelector;

        public MultiCommand(params ICommand[] commands)
        {
            _commands.AddRange(commands);
        }

        public MultiCommand(Func<ICommand> commandSelector)
        {
            _commandSelector = commandSelector ?? throw new ArgumentNullException(nameof(commandSelector));
        }

        public bool CanExecute(object parameter)
        {
            if (_commandSelector != null)
            {
                ICommand selectedCommand = _commandSelector();
                return selectedCommand != null && selectedCommand.CanExecute(parameter);
            }

            // 모든 명령이 실행 가능한지 확인
            foreach (var command in _commands)
            {
                if (command.CanExecute(parameter))
                    return true;
            }

            return false;
        }

        public void Execute(object parameter)
        {
            if (_commandSelector != null)
            {
                ICommand selectedCommand = _commandSelector();
                if (selectedCommand != null && selectedCommand.CanExecute(parameter))
                {
                    selectedCommand.Execute(parameter);
                }
                return;
            }

            // 실행 가능한 첫 번째 명령 실행
            foreach (var command in _commands)
            {
                if (command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                    break;
                }
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}