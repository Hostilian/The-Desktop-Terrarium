using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terrarium.Desktop.ViewModels.Base;

namespace Terrarium.Tests.ViewModels.Base;

/// <summary>
/// Tests for ViewModelBase to ensure INotifyPropertyChanged works correctly.
/// </summary>
[TestClass]
public class ViewModelBaseTests
{
    private class TestViewModel : ViewModelBase
    {
        private string _testProperty = "";
        private int _counter;

        public string TestProperty
        {
            get => _testProperty;
            set => SetProperty(ref _testProperty, value);
        }

        public int Counter
        {
            get => _counter;
            set => SetProperty(ref _counter, value);
        }

        public void RaisePropertyChangedManually(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
    }

    [TestMethod]
    public void SetProperty_WithDifferentValue_RaisesPropertyChanged()
    {
        var viewModel = new TestViewModel();
        bool eventRaised = false;
        string? propertyName = null;

        viewModel.PropertyChanged += (s, e) =>
        {
            eventRaised = true;
            propertyName = e.PropertyName;
        };

        viewModel.TestProperty = "new value";

        Assert.IsTrue(eventRaised);
        Assert.AreEqual(nameof(viewModel.TestProperty), propertyName);
    }

    [TestMethod]
    public void SetProperty_WithSameValue_DoesNotRaisePropertyChanged()
    {
        var viewModel = new TestViewModel { TestProperty = "initial" };
        bool eventRaised = false;

        viewModel.PropertyChanged += (s, e) => eventRaised = true;
        viewModel.TestProperty = "initial";

        Assert.IsFalse(eventRaised);
    }

    [TestMethod]
    public void SetProperty_UpdatesBackingField()
    {
        var viewModel = new TestViewModel();

        viewModel.TestProperty = "test value";

        Assert.AreEqual("test value", viewModel.TestProperty);
    }

    [TestMethod]
    public void SetProperty_ReturnsTrue_WhenValueChanged()
    {
        var viewModel = new TestViewModel();
        viewModel.TestProperty = "initial";

        // Can't directly test return value through property setter
        // but can verify through event
        bool changed = false;
        viewModel.PropertyChanged += (s, e) => changed = true;
        viewModel.TestProperty = "new";

        Assert.IsTrue(changed);
    }

    [TestMethod]
    public void OnPropertyChanged_RaisesEvent()
    {
        var viewModel = new TestViewModel();
        bool eventRaised = false;
        string? propertyName = null;

        viewModel.PropertyChanged += (s, e) =>
        {
            eventRaised = true;
            propertyName = e.PropertyName;
        };

        viewModel.RaisePropertyChangedManually("TestProperty");

        Assert.IsTrue(eventRaised);
        Assert.AreEqual("TestProperty", propertyName);
    }

    [TestMethod]
    public void SetProperty_WorksWithValueTypes()
    {
        var viewModel = new TestViewModel();
        bool eventRaised = false;

        viewModel.PropertyChanged += (s, e) => eventRaised = true;
        viewModel.Counter = 42;

        Assert.IsTrue(eventRaised);
        Assert.AreEqual(42, viewModel.Counter);
    }
}


