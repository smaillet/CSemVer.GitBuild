// -----------------------------------------------------------------------
// <copyright file="GenerateHeaderTests.cs" company="Ubiquity.NET Contributors">
// Copyright (c) Ubiquity.NET Contributors. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Build.Evaluation;
using Microsoft.Build.Utilities.ProjectCreation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Ubiquity.NET.Versioning.Build.Tasks.UT.Support;

namespace Ubiquity.NET.Versioning.Build.Tasks.UT
{
    [TestClass]
    public class GenerateHeaderTests
    {
        [TestMethod]
        public void CreateVcxProj( )
        {
            using var collection = new ProjectCollection(/*globalProperties*/);

            // TEMP - create a VCXproj to validate it is working correctly
            var x = ProjectCreator.Templates.VcxProj(collection);
            var xml = x.Project.Xml;
        }
    }
}
