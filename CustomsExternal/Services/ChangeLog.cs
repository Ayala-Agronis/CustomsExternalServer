using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomsExternal.Services
{
    public class ChangeLogService
    {
        private CustomsExternalEntities db = new CustomsExternalEntities();

        public void LogChange(ChangeLog changeLog)
        {           
            db.ChangeLog.Add(changeLog);
            db.SaveChanges();
        }
    }
}