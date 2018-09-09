﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using PSol.Data.Models;
using PSol.Data.Repositories.Interfaces;

namespace PSol.Data.Repositories
{
    public class MobRepository : IMobRepository
    {
        private readonly PSolDataContext _context;

        public MobRepository(PSolDataContext context)
        {
            _context = context;
        }

        public Mob GetMobById(string id)
        {
            return _context.Mobs.FirstOrDefault(m => m.Id == id);
        }

        public ICollection<Mob> GetAllMobs()
        {
            return _context.Mobs.Include(i => i.MobType).Include(i => i.MobType.Star).ToList();
        }

        public ICollection<Mob> GetAllDeadMobs()
        {
            return _context.Mobs.Where(m => !m.Alive).ToList();
        }

        public Mob Add(Mob mob)
        {
            _context.Mobs.AddOrUpdate(mob);
            _context.SaveChanges();
            return mob;
        }

        public void SaveMob(Mob mob)
        {
            var dbMob = GetMobById(mob.Id);
            _context.Entry(dbMob).CurrentValues.SetValues(mob);
            _context.SaveChanges();
        }

        public ICollection<MobType> GetAllMobTypes()
        {
            return _context.MobTypes.ToList();
        }
    }
}