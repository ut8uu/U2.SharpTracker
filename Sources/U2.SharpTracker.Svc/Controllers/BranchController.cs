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

using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using U2.SharpTracker.Core;

namespace U2.SharpTracker.Svc.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BranchController : ControllerBase
    {
        private readonly ISharpTrackerService _trackerService;

        public BranchController(ISharpTrackerService trackerService)
        {
            _trackerService = trackerService;
        }

        [HttpPut("CreateBranch")]
        public async Task CreateBranchAsync(BranchDto branch, CancellationToken cancellationToken)
        {
            await _trackerService.AddBranchAsync(branch, cancellationToken);
        }

        [HttpGet("ListBranches")]
        public ActionResult<IEnumerable<BranchInfo>> ListBranches(Guid parentId, CancellationToken token)
        {
            var branches = _trackerService.GetBranchesAsync(parentId, token);
            var list = branches.Result.Select(b => new BranchInfo(b)).ToList();
            return Ok(list);
        }

        [HttpDelete("DeleteBranch")]
        public async Task DeleteBranch(Guid id, CancellationToken cancellationToken)
        {
            await _trackerService.DeleteBranchAsync(id, cancellationToken);
        }

        [HttpGet("GetBranch")]
        public async Task<BranchInfo> GetBranch(Guid id, CancellationToken cancellationToken)
        {
            var branch = await _trackerService.GetBranchAsync(id, cancellationToken);
            var result = new BranchInfo(branch);
            var topics = _trackerService.GetTopicsAsync(branch.Id, cancellationToken);
            await foreach (var topic in topics)
            {
                result.Topics.Add(new TopicShortInfo(topic));
            }

            return result;
        }
    }
}
