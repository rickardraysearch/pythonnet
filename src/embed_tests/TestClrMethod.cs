using System;
using NUnit.Framework;
using Python.Runtime;

namespace Python.EmbeddingTest
{
    public class TestClrMethod
    {
        private IntPtr globals;

        [OneTimeSetUp]
        public void SetUp()
        {
            PythonEngine.Initialize();
            globals = Runtime.Runtime.PyDict_New();
            Runtime.Runtime.PyDict_SetItemString(globals, "__builtins__", Runtime.Runtime.PyEval_GetBuiltins());
            Runtime.Runtime.PyDict_SetItemString(globals, "clr", ImportHook.GetCLRModule());
            Runtime.Runtime.PyDict_SetItemString(globals, "ExampleClrClass", CreateTestClass().Handle);
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            PythonEngine.Shutdown();
        }

        [Test]
        public void SetAndGetPropertyTest()
        {
            dynamic a = InstantiateTestClass();
            Assert.AreEqual(3, (int) a.X);
            Assert.AreEqual(3 * 2, (int) a.Y);
            a.X = 4;
            Assert.AreEqual(4, (int) a.X);
            Assert.AreEqual(4 * 2, (int) a.Y);
        }

        [Test]
        public void CallMethodTest()
        {
            dynamic a = InstantiateTestClass();
            Assert.AreEqual(4 * 2, (int) a.test(4));
            Assert.AreEqual(5 * 2, (int) a.test(5));
        }

        private PyObject CreateTestClass()
        {
            var locals = new PyDict();
            PythonEngine.Exec(@"
import System

class ExampleClrClass(System.Object):
    __namespace__ = ""PyTest""
    def __init__(self):
        self._x = 3
    @clr.clrmethod(int, [int])
    def test(self, x):
        return x*2
    
    def get_X(self):
        return self._x
    def set_X(self, value):
        self._x = value
    X = clr.clrproperty(int, get_X, set_X)

    @clr.clrproperty(int)
    def Y(self):
        return self._x * 2
",
                globals, locals.Handle);

            return locals.GetItem("ExampleClrClass");
        }

        private PyObject InstantiateTestClass()
        {
            return PythonEngine.Eval(@"ExampleClrClass()", globals);
        }
    }
}
