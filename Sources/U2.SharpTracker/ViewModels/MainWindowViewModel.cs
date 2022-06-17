/* 
 * This file is part of the U2.SharpTracker distribution
 * (https://github.com/ut8uu/U2.SharpTracker).
 * 
 * Copyright (c) 2022 Sergey Usmanov.
 * 
 * This program is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU General Public License as published by  
 * the Free Software Foundation, version 3.
 *
 * This program is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License 
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Controls;
using DynamicData;

namespace U2.SharpTracker.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _selectedThread = null;
        public string StatusText { get; set; } = "Ready";
        public string Title { get; set; }
        public Window Owner { get; set; }

        public string SelectedThread
        {
            get => _selectedThread;
            set
            {
                if (_selectedThread != value)
                {
                    SelectThread(value);
                }
                _selectedThread = value;
            }
        }

        private void SelectThread(string value)
        {

        }

        public ObservableCollection<string> ThreadsList { get; } = new();
        public ObservableCollection<KeyValuePair<string, string>> ThreadInfo { get; } = new();

        public void ExecuteExitCommand()
        {
            Owner?.Close();
        }

    }

    public sealed class DesignMainWindowViewModel : MainWindowViewModel
    {
        public DesignMainWindowViewModel()
        {
            Title = "RuTracker Parser";
            ThreadsList.AddRange(new[] { "Thread 1", "Thread 2" });
            SelectedThread = "Thread 1";
            ThreadInfo.AddRange(
                new[]
                {
                    new KeyValuePair<string, string>("Index", "1001"),
                    new KeyValuePair<string, string>("Url", "https://rutracker.org/1001"),
                    new KeyValuePair<string, string>("Status", "Connecting"),
                });
        }
    }
}
