﻿using PSol.Data.Models;

namespace PSol.Data.Repositories.Interfaces
{
    public interface IStarRepository
    {
        Star LoadStar(string id);
    }
}
