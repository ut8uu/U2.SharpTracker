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

using System.ComponentModel.DataAnnotations;
using U2.SharpTracker.Core;

namespace U2.SharpTracker.Web.Models;

public sealed class TrackerTopic
{
    [Key]
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public TrackerBranch Branch { get; set; }
    public UrlLoadStatusCode StatusCode { get; set; }
}