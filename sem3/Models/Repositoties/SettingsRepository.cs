using System;
using System.Collections.Generic;
using System.Linq;
using sem3.Models.Entities;

namespace sem3.Models.Repositories
{
    public class SettingsRepository : IDisposable
    {
        private readonly OnlineRechargeDBEntities _context;

        public SettingsRepository()
        {
            _context = new OnlineRechargeDBEntities();
        }

        public List<SystemSetting> GetAll()
        {
            return _context.SystemSettings.ToList();
        }

        public SystemSetting GetByKey(string key)
        {
            return _context.SystemSettings.Find(key);
        }

        public string GetValue(string key)
        {
            var item = _context.SystemSettings.Find(key);
            return item != null ? item.SettingValue : "";
        }

        public void Update(SystemSetting model)
        {
            var item = _context.SystemSettings.Find(model.SettingKey);
            if (item != null)
            {
                item.SettingValue = model.SettingValue;
                _context.SaveChanges();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}