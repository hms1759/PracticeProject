using practiceAuthTable.Core.IService;
using practiceAuthTable.Core.Models;
using Shared.Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace practiceAuthTable.Core.Service
{
  public  class TestingService : Service<Items>, ITestingService
    {
    }
}
