﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Vipr.Writer.CSharp
{
    public class GeneratedEdmModelGetInstanceMethod : Method
    {
        public GeneratedEdmModelGetInstanceMethod()
        {
            Visibility = Visibility.Public;
            IsStatic = true;
            Name = "GetInstance";
            ReturnType = new Type(new Identifier("global::Microsoft.OData.Edm", "IEdmModel"));
        }
    }
}