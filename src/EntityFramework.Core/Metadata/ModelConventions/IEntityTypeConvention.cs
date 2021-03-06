// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    public interface IEntityTypeConvention
    {
        void Apply([NotNull] InternalEntityBuilder entityBuilder);
    }
}
