﻿/* 
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

namespace U2.SharpTracker.Core
{
    public sealed class TrackerSvcSettings : ITrackerSvcSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string Strategy { get; set; }
    }

    public interface ITrackerSvcSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string Strategy { get; set; }
    }
}
