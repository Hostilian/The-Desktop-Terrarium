using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Desktop.Commands;

namespace Terrarium.Tests.Commands;

/// <summary>
/// Tests for RelayCommand to ensure command pattern works correctly.
/// </summary>
[TestClass]
public class RelayCommandTests
{
    [TestMethod]
    public void Execute_CallsAction()
    {
        bool executed = false;
        var command = new RelayCommand(() => executed = true);

        command.Execute(null);

        Assert.IsTrue(executed);
    }

    [TestMethod]
    public void CanExecute_WithNoCondition_ReturnsTrue()
    {
        var command = new RelayCommand(() => { });

        bool canExecute = command.CanExecute(null);

        Assert.IsTrue(canExecute);
    }

    [TestMethod]
    public void CanExecute_WithCondition_ReturnsConditionResult()
    {
        bool condition = false;
        var command = new RelayCommand(() => { }, () => condition);

        Assert.IsFalse(command.CanExecute(null));

        condition = true;
        Assert.IsTrue(command.CanExecute(null));
    }

    [TestMethod]
    public void Constructor_WithNullAction_ThrowsException()
    {
        try
        {
            new RelayCommand(null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    [TestMethod]
    public void GenericExecute_CallsActionWithParameter()
    {
        string? result = null;
        var command = new RelayCommand<string>(s => result = s);

        command.Execute("test");

        Assert.AreEqual("test", result);
    }

    [TestMethod]
    public void GenericCanExecute_WithCondition_ReturnsConditionResult()
    {
        var command = new RelayCommand<int>(x => { }, x => x > 0);

        Assert.IsTrue(command.CanExecute(5));
        Assert.IsFalse(command.CanExecute(-1));
    }
}


