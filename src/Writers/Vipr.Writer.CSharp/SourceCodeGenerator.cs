// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Vipr.Core;
using Vipr.Core.CodeModel;

namespace Vipr.Writer.CSharp
{
    internal class SourceCodeGenerator
    {

        internal SourceCodeGenerator(ServiceType serviceType)
        {
        }

        internal IEnumerable<TextFile> Generate(CSharpProject project)
        {

            return Write(project);
        }
        
        private IEnumerable<TextFile> Write(CSharpProject project)
        {
            foreach (var @namespace in project.Namespaces)
            {
                foreach (var file in Write(@namespace))
                {
                    yield return file;
                }
            }
        }

        private IEnumerable<TextFile> Write(Namespace @namespace)
        {
            foreach (var @enum in @namespace.Enums)
            {
                yield return new CSharpTextFile(@enum);
            }

            foreach (var @class in @namespace.Classes)
            {
                yield return new CSharpTextFile(@class);
            }

            foreach (var @interface in @namespace.Interfaces)
            {
                yield return new CSharpTextFile(@interface);
            }
        }
    }
}