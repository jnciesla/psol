﻿using System;
using System.Collections.Generic;
using System.Text;
using Bindings;
using PSol.Data.Models;

namespace Bindings
{
    internal class Types
    {
        // Switch to a collection?
        public static User[] Player = new User[Constants.MAX_PLAYERS];
        public static User Default = new User();
    }
}
