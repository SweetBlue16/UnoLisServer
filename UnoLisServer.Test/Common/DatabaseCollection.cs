using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnoLisServer.Test.Common
{
    /// <summary>
    /// Class to avoid paralelism between Repository Classes
    /// </summary>
    [CollectionDefinition("DatabaseTests")]
    public class DatabaseCollection : ICollectionFixture<object>
    {

    }
}
