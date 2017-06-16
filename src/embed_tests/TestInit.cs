using System;
using NUnit.Framework;
using Python.Runtime;

namespace Python.EmbeddingTest
{
    public class TestInit
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            PythonEngine.Initialize();
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            PythonEngine.Shutdown();
        }

        [Test]
        public static void RefCountTest()
        {
            var locals = new PyDict();

            PythonEngine.Exec(@"
import clr, System
class Kalle(System.Object):
    __namespace__ = ""PyTest""
    def __init__(self, x):
        print(""__init__({})"".format(x))
        self._x = x

k = Kalle(1)
a = k._x
        ", null, locals.Handle);

            object result;
            Converter.ToManagedValue(locals.GetItem("a").Handle, typeof(Int64), out result, false);
            Assert.AreEqual(1, result);
        }
    }
}
