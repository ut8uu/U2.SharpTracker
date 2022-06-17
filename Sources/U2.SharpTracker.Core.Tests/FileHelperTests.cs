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

using System.Text;
using Microsoft.VisualStudio.TestPlatform.Utilities.Helpers;
using Xunit;

namespace U2.SharpTracker.Core.Tests;

public sealed class FileHelperTests
{
    [Fact]
    public void CanReadFile()
    {
        var tempDirectory = new TempDirectory();
        FileSystemHelper.GetDatabaseFolderFunc = () => tempDirectory.TempPath;

        var filePath = Path.Combine(tempDirectory.TempPath, "test.txt");
        var content = Encoding.ASCII.GetBytes("mama");
        File.WriteAllBytes(filePath, content);

        var loadedContent = FileSystemHelper.ReadFile(filePath);
        Assert.True(loadedContent.SequenceEqual(content));
    }

    [Fact]
    public void CanWriteFile()
    {
        var tempDirectory = new TempDirectory();
        FileSystemHelper.GetDatabaseFolderFunc = () => tempDirectory.TempPath;

        var filePath = Path.Combine(tempDirectory.TempPath, "test.txt");
        var content = Encoding.ASCII.GetBytes("mama");

        FileSystemHelper.WriteFile(filePath, content);
        var loadedContent = File.ReadAllBytes(filePath);
        Assert.True(loadedContent.SequenceEqual(content));
    }
}
