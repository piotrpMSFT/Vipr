// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Vipr.Writer.CSharp
{
    public class Identifier
    {
        public Identifier(string @namespace, string name)
        {
            Name = name;
            Namespace = @namespace;
        }

        public string Name { get; }

        public string Namespace { get; }

        public string FullName => Namespace == null
            ? Name
            : Namespace + "." + Name;

        public override string ToString()
        {
            return FullName;
        }

        public static Identifier Task { get { return new Identifier("global::System.Threading.Tasks", "Task"); } }
    }
}
